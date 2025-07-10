using EasyMockLib.MatchingPolicies;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

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

        public MockNode GetMock(ServiceType serviceType, string service, 
            string method, string requestContent, IMatchingPolicy matchingPolicy)
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

                MockNode? matchingMock = matchingPolicy.Apply(requestContent, mocks, service, method);
                if (matchingMock != null)
                {
                    return matchingMock;
                }

                else
                {
                    // If no matching mock found, return the first one as a fallback
                    return mocks.First();
                }
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
