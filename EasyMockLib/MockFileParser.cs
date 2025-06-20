using EasyMockLib.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyMockLib
{
    public class MockFileParser
    {
        private const string DevSoapRequestPattern = "^\\d{2}:\\d{2}:\\d{2}\\.\\d+\\s+(http\\S+)\\s+(\\w+)\\s+Request$";
        private const string DevSoapResponsePattern = "^\\d{2}:\\d{2}:\\d{2}\\.\\d+\\s+(http\\S+)\\s+(\\w+)\\s+Response$";
        private const string DevRestRequestPattern = "^\\d{2}:\\d{2}:\\d{2}\\.\\d+\\s+(\\w+)\\s+(http\\S+)\\s+Request$";
        private const string DevRestResponsePattern = "^\\d{2}:\\d{2}:\\d{2}\\.\\d+\\s+(\\w+)\\s+(http\\S+)\\s+Response$";
        private const string SoapEnvSingleLineStartPattern = "^<[a-zA-Z-]+:Envelope\\s+[^>]+>.*</[a-zA-Z-]+:Envelope>$";
        private const string SoapEnvStartPattern = "^<[a-zA-Z-]+:Envelope\\s+[^>]+>";
        private const string SoapEnvEndPattern = "</[a-zA-Z-]+:Envelope>$";
        private const string RestRequestBodyPattern = "^RequestBody:\\s+(.*)$";
        private const string RestResponseBodyPattern = "^ResponseBody:\\s+(.*)$";
        private const string xml = @"<?xml version=""1.0"" encoding=""utf-8""?>";
        private readonly XNamespace soap = "http://schemas.xmlsoap.org/soap/envelope/";
        private int lineNumber = 0;

        public MockFileNode Parse(string filePath)
        {
            MockFileNode root = new MockFileNode()
            {
                MockFile = filePath
            };
            lineNumber = 0;
            List<(string MethodName, string Url, Request Request)> pendingRequests = new List<(string, string, Request)>();
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = ReadLine(reader)) != null)
                {
                    Match m = Regex.Match(line, DevSoapRequestPattern);
                    if (m.Success)
                    {
                        pendingRequests.Add(ProcessSoapRequest(reader, m, line));
                        continue;
                    }
                    m = Regex.Match(line, DevSoapResponsePattern);
                    if (m.Success && pendingRequests.Count > 0)
                    {
                        root.Nodes.Add(ProcessSoapResponse(reader, m, pendingRequests, line));
                        continue;
                    }
                    m = Regex.Match(line, DevRestRequestPattern);
                    if (m.Success)
                    {
                        // Found a REST request
                        pendingRequests.Add(ProcessRestRequest(reader, m));
                        continue;
                    }
                    m = Regex.Match(line, DevRestResponsePattern);
                    if (m.Success)
                    {
                        // Found a REST response
                        root.Nodes.Add(ProcessRestResponse(reader, m, pendingRequests));
                        continue;
                    }
                }
                ;
            }
            foreach (var requestTuple in pendingRequests)
            {
                root.Nodes.Add(new MockNode() { Request = requestTuple.Request, Response = null });
            }
            return root;
        }

        private (string MethodName, string Url, Request Request) ProcessSoapRequest(StreamReader reader, Match m, string line)
        {
            var url = m.Groups[2].Value;
            var methodName = m.Groups[1].Value;
            Request req = new Request()
            {
                RequestType = ServiceType.SOAP,
                ServiceName = m.Groups[1].Value.Substring(m.Groups[1].Value.LastIndexOf('/') + 1),
            };
            req.RequestBody = ReadSoapBlock(reader, line);
            methodName = GetSoapAction(req.RequestBody.Content);
            return (methodName, url, req);
        }
        private string GetSoapAction(string soapEnv)
        {
            XDocument doc = XDocument.Parse(soapEnv);
            return Regex.Replace(doc.Descendants(soap + "Body").First().Elements().First().Name.LocalName, "Request$", "");
        }
        private MockNode ProcessSoapResponse(StreamReader reader, Match m, List<(string MethodName, string Url, Request Request)> pendingRequests, string line)
        {
            var url = m.Groups[2].Value;
            var methodName = m.Groups[1].Value;
            var requestTuple = pendingRequests.LastOrDefault(r => r.Url == url && r.MethodName == methodName);
            var response = new Response() { StatusCode = HttpStatusCode.OK, ResponseBody = new Body() };
            response.ResponseBody = ReadSoapBlock(reader, line);
            response.ResponseBody.Content = xml + "\r\n" + response.ResponseBody.Content;
            pendingRequests.Remove(requestTuple);
            return new MockNode() {
                Url = url.Replace("http://localhost:8888", "").Trim('/'),
                MethodName = methodName,
                Request = requestTuple.Request, 
                Response = response 
            };
        }
        private (string MethodName, string Url, Request Request) ProcessRestRequest(StreamReader reader, Match m)
        {
            var url = m.Groups[2].Value;
            var methodName = m.Groups[1].Value;
            Request req = new Request()
            {
                RequestType = ServiceType.REST,
                ServiceName = (new Uri(m.Groups[2].Value)).AbsolutePath,
            };
            if (!methodName.Equals("GET", StringComparison.OrdinalIgnoreCase))
            {
                req.RequestBody = ReadRestRequestBlock(reader);
            }
            return (methodName, url, req);
        }
        private MockNode ProcessRestResponse(StreamReader reader, Match m, List<(string MethodName, string Url, Request Request)> pendingRequests)
        {
            var url = m.Groups[2].Value;
            var methodName = m.Groups[1].Value;
            var requestTuple = pendingRequests.LastOrDefault(r => r.Url == url && r.MethodName == methodName);
            var response = ReadRestResponseBlock(reader);
            pendingRequests.Remove(requestTuple);
            return new MockNode() {
                Url = url.Replace("http://localhost:8888", "").Trim('/'),
                MethodName = methodName,
                Request = requestTuple.Request, 
                Response = response 
            };
        }
        private Body ReadRestRequestBlock(StreamReader reader)
        {
            var body = new Body();
            string line;
            while ((line = ReadLine(reader)) != null && !Regex.IsMatch(line, RestRequestBodyPattern))
            {
            }
            if (line == null)
            {
                throw new Exception($"Cannot find REST RequestBody pattern {RestRequestBodyPattern}");
            }
            body.Content = ExtractRestBody(reader, ref line, RestRequestBodyPattern);
            body.ContentObject = JObject.Parse(body.Content);
            return body;
        }
        private Response ReadRestResponseBlock(StreamReader reader)
        {
            string line;
            var response = new Response()
            {
                StatusCode = HttpStatusCode.Unused,
            };
            while ((line = ReadLine(reader)) != null)
            {
                Match m;
                if (response.StatusCode == HttpStatusCode.Unused)
                {
                    m = Regex.Match(line, "^StatusCode:(\\w+)$");
                    if (m.Success)
                    {
                        response.StatusCode = (HttpStatusCode)Enum.Parse(typeof(HttpStatusCode), m.Groups[1].Value);
                        continue;
                    }
                }
                m = Regex.Match(line, RestResponseBodyPattern);
                if (m.Success)
                {
                    response.ResponseBody = new Body
                    {
                        Content = ExtractRestBody(reader, ref line, RestResponseBodyPattern),
                    };
                    if (response.StatusCode == HttpStatusCode.Unused)
                    {
                        response.StatusCode = HttpStatusCode.OK;
                    }
                }
                if ((response.StatusCode != HttpStatusCode.Unused || response.ResponseBody != null) &&
                (line == null || Regex.IsMatch(line, "^\\s*$")))
                {
                    return response;
                }
            }
            throw new Exception($"Cannot find extract pattern {RestResponseBodyPattern}");
        }
        private string ExtractRestBody(StreamReader reader, ref string line, string pattern)
        {
            Match m = Regex.Match(line, pattern);
            if (m.Success)
            {
                if (m.Groups[1].Value.StartsWith("{") && m.Groups[1].Value.EndsWith("}"))
                {
                    // Single line JSON
                    var jobject = JObject.Parse(m.Groups[1].Value);
                    return jobject.ToString();
                }
                else if (m.Groups[1].Value.StartsWith("[") && m.Groups[1].Value.EndsWith("]"))
                {
                    // Single line JSON
                    var jarray = JArray.Parse(m.Groups[1].Value);
                    return jarray.ToString();
                }
                else
                {
                    StringBuilder sbJson = new StringBuilder(m.Groups[1].Value);
                    while ((line = ReadLine(reader)) != null)
                    {
                        if (Regex.IsMatch(line, "^\\s*$"))
                        {
                            if (sbJson[0] == '[')
                            {
                                JArray jarray = JArray.Parse(sbJson.ToString());
                                return jarray.ToString();
                            }
                            else if (sbJson[0] == '{')
                            {
                                JObject jobject = JObject.Parse(sbJson.ToString());
                                return jobject.ToString();
                            }
                            else
                            {
                                sbJson.Clear();
                                return sbJson.ToString();
                            }
                        }
                        sbJson.AppendLine(line);
                    }
                    var json = sbJson.ToString().Trim();
                    if (json.Length > 0)
                    {
                        if (json[0] == '[')
                        {
                            JArray jarray = JArray.Parse(json);
                            return json.ToString();
                        }
                        else if (json[0] == '{')
                        {
                            JObject jobject = JObject.Parse(json);
                            return jobject.ToString();
                        }
                        else
                        {
                            throw new Exception($"Invalid Json Object");
                        }
                    }
                    else
                    {
                        return json;
                    }
                }
            }
            else
            {
                throw new Exception($"Cannot match extract pattern {pattern}");
            }
        }

        private Body ReadSoapBlock(StreamReader reader, string line)
        {
            var body = new Body();
            while (line != null)
            {
                if (Regex.IsMatch(line, SoapEnvSingleLineStartPattern))
                {
                    var xml0 = XElement.Parse(line);
                    body.Content = line;
                    body.ContentObject = xml0.Descendants(soap + "Body").First().Elements().First();
                    return body;
                }
                else if (Regex.IsMatch(line, SoapEnvStartPattern)) break;
                else
                {
                    line = ReadLine(reader);
                }
            }
            if (line == null)
            {
                throw new Exception($"Cannot find startPattern {SoapEnvStartPattern}");
            }
            StringBuilder blocks = new StringBuilder(line);
            blocks.AppendLine();
            while (line != null && !Regex.IsMatch(line, SoapEnvEndPattern))
            {
                line = ReadLine(reader);
                blocks.AppendLine(line);
            }
            if (line == null)
            {
                throw new Exception($"Cannot find endPattern {SoapEnvEndPattern}");
            }
            var xml = XElement.Parse(blocks.ToString());
            body.Content = blocks.ToString();
            body.ContentObject = xml.Descendants(soap + "Body").First().Elements().First();
            return body;
        }
        private string ReadLine(StreamReader reader)
        {
            string line = reader.ReadLine();
            lineNumber++;
            return line;
        }
    }
}
