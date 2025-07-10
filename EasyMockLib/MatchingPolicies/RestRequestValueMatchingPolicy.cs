using EasyMockLib.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EasyMockLib.MatchingPolicies
{
    public class RestRequestValueMatchingPolicy : IMatchingPolicy
    {
        public Dictionary<string, Dictionary<string, List<string>>> RestServiceMatchingConfig { get; set; } = new Dictionary<string, Dictionary<string, List<string>>>();

        public MockNode? Apply(string requestContent, IEnumerable<MockNode> mocks, string service, string method)
        {
            List<string> elementsToCompare = null;

            if (RestServiceMatchingConfig.ContainsKey(service) &&
                       RestServiceMatchingConfig[service].ContainsKey(method) &&
                       RestServiceMatchingConfig[service][method].Count() > 0)
            {
                elementsToCompare = RestServiceMatchingConfig[service][method];
            }

            else
            {
                return null;
            }

                JObject jIncomingRequest = JObject.Parse(requestContent);
            foreach (var mock in mocks)
            {
                var jMockRequest = (JObject)mock.Request.RequestBody.ContentObject;
                if (elementsToCompare == null || elementsToCompare.Count == 0)
                {
                    if (JToken.DeepEquals(jIncomingRequest, jMockRequest))
                    {
                        return mock;
                    }
                }
                else
                {
                    bool match = true;
                    foreach (var elementName in elementsToCompare)
                    {
                        var element1 = jIncomingRequest.SelectToken(elementName);
                        var element2 = jMockRequest.SelectToken(elementName);
                        if (!JToken.DeepEquals(element1, element2))
                        {
                            match = false; break;
                        }
                    }
                    if (match)
                    {
                        return mock;
                    }
                }
            }
            return null;
        }
    }
}
