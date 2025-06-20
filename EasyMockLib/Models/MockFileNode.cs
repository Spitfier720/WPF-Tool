﻿using EasyMockLib.MatchingPolicies;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using WPF_Tool;

namespace EasyMockLib.Models
{
    public class MockFileNode
    {
        public MockFileNode()
        {
            Nodes = new List<MockNode>();
        }

        public string MockFile { get; set; }
        public List<MockNode> Nodes { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> RestServiceMatchingConfig { get; set; }
        public Dictionary<string, Dictionary<string, List<string>>> SoapServiceMatchingConfig { get; set; }


        public MockNode GetMock(ServiceType serviceType, string service, 
            string method, string requestContent, 
            Dictionary<string, Dictionary<string, List<string>>> soapMatchingElements)
        {
            var mocks = this.Nodes.Where(m =>
            m.Request.RequestType == serviceType &&
            m.MethodName.Equals(method, StringComparison.OrdinalIgnoreCase) &&
            MatchUrl(m, service, method));

            if (mocks.Any())
            {
                if (mocks.Count() == 1)
                {
                    return mocks.ElementAt(0);
                }
                if (serviceType == ServiceType.REST)
                {
                    if(RestServiceMatchingConfig.ContainsKey(service) &&
                       RestServiceMatchingConfig[service].ContainsKey(method) &&
                       RestServiceMatchingConfig[service][method].Count() > 0)
                    {
                        var restMatchingPolicy = new RestRequestValueMatchingPolicy();
                        var mock = restMatchingPolicy.Apply(requestContent, mocks, RestServiceMatchingConfig[service][method]);
                        if (mock != null)
                        {
                            return mock;
                        }
                    }

                    IMatchingPolicy matchingPolicy;
                    if (method.Equals(HttpMethod.Get.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        matchingPolicy = new HttpGetMatchingPolicy();
                        return matchingPolicy.Apply(service, mocks);
                    }
                    JObject jRequest = JObject.Parse(requestContent);
                    foreach (var mock in mocks)
                    {
                        if (JToken.DeepEquals(jRequest, (JObject)mock.Request.RequestBody.ContentObject))
                        {
                            return mock;
                        }
                    }
                    return mocks.FirstOrDefault();
                }
                else if (serviceType == ServiceType.SOAP)
                {

                    if (SoapServiceMatchingConfig.ContainsKey(service) &&
                        SoapServiceMatchingConfig[service].ContainsKey(method) &&
                        SoapServiceMatchingConfig[service][method].Count() > 0)
                    {
                        var soapMatchingPolicy = new SoapRequestValueMatchingPolicy();
                        var mock = soapMatchingPolicy.Apply(requestContent, mocks, SoapServiceMatchingConfig[service][method]);
                        if (mock != null)
                        {
                            return mock;
                        }
                    }

                    XElement xRequestContent = XElement.Parse(requestContent);
                    List<string> elementsToCompare;
                    if (soapMatchingElements.ContainsKey(service) && soapMatchingElements[service].ContainsKey(method))
                    {
                        elementsToCompare = soapMatchingElements[service][method];
                    }
                    else
                    {
                        elementsToCompare = new List<string>() { method + "Request" };
                    }
                    foreach (var mock in mocks)
                    {
                        XElement xMockRequest = (XElement)mock.Request.RequestBody.ContentObject;
                        bool match = true;
                        foreach (var elementName in elementsToCompare)
                        {
                            var element1 = xRequestContent.Descendants().Where(e => e.Name.LocalName == elementName).FirstOrDefault();
                            var element2 = xMockRequest.Descendants().Where(e => e.Name.LocalName == elementName).FirstOrDefault();
                            if (!XNode.DeepEquals(element1, element2))
                            {
                                match = false; break;
                            }
                        }
                        if (match)
                        {
                            return mock;
                        }
                    }
                    return mocks.FirstOrDefault();
                }
                return mocks.First();
            }
            return null;
        }

        private bool MatchUrl(MockNode mock, string url, string method)
        {
            if (method.Equals(HttpMethod.Get.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return mock.Request.ServiceName.EndsWith(UriPath(url, method), StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                return mock.Url.EndsWith(UriPath(url, method), StringComparison.OrdinalIgnoreCase);
            }
        }

        private string UriPath(string pathAndQuery, string method)
        {
            if (method.Equals(HttpMethod.Get.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (pathAndQuery.IndexOf("?") == -1)
                {
                    return pathAndQuery;
                }
                else
                {
                    return pathAndQuery.Substring(0, pathAndQuery.IndexOf("?"));
                }
            }
            else
            {
                return pathAndQuery;
            }
        }
    }
}
