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

        public MockNode GetMock(ServiceType serviceType, string url, 
            string method, string requestContent, IMatchingPolicy matchingPolicy)
        {
            var mocks = this.Nodes.Where(m =>
            m.ServiceType == serviceType &&
            m.MethodName.Equals(method, StringComparison.OrdinalIgnoreCase) &&
            MatchUrl(m, url, method));

            if (mocks.Any())
            {
                if (mocks.Count() == 1)
                {
                    return mocks.ElementAt(0);
                }

                MockNode? matchingMock = matchingPolicy.Apply(requestContent, mocks, url, method);
                if (matchingMock != null)
                {
                    return matchingMock;
                }
            }
            return null;
        }

        private bool MatchUrl(MockNode mock, string url, string method)
        {
            return mock.Url.EndsWith(UriPath(url, method), StringComparison.OrdinalIgnoreCase);
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
