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
            m.Url.Equals(url, StringComparison.OrdinalIgnoreCase));

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
    }
}
