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
    public class SoapRequestValueMatchingPolicy : IMatchingPolicy
    {
        public Dictionary<string, Dictionary<string, List<string>>> SoapServiceMatchingConfig { get; set; } = new Dictionary<string, Dictionary<string, List<string>>>();

        public MockNode? Apply(string requestContent, IEnumerable<MockNode> mocks, string service, string method)
        {
            List<string> elementsToCompare = null;

            if (SoapServiceMatchingConfig.ContainsKey(service) &&
                        SoapServiceMatchingConfig[service].ContainsKey(method) &&
                        SoapServiceMatchingConfig[service][method].Count() > 0)
            {
                elementsToCompare = SoapServiceMatchingConfig[service][method];
            }
            else
            {
                return null;
            }

            XElement xRequestContent = XElement.Parse(requestContent);
            foreach (var mock in mocks)
            {
                XElement xMockRequest = (XElement)mock.Request.RequestBody.ContentObject;
                bool match = true;
                foreach (var elementName in elementsToCompare)
                {
                    var element1 = FindNode(xRequestContent, elementName);
                    var element2 = FindNode(xMockRequest, elementName);
                    if (element1 == null || element2 == null)
                    {
                        match = false; break;
                    }
                    else if (!XNode.DeepEquals(element1, element2))
                    {
                        match = false; break;
                    }
                }
                if (match)
                {
                    return mock;
                }
            }
            return null;
        }
        private static XElement? FindNode(XElement root, string elementPath)
        {
            if (string.IsNullOrEmpty(elementPath))
            {
                throw new ArgumentNullException(nameof(elementPath));
            }
            var elementNames = elementPath.Trim().Split('/');
            var elementsLevel1 = root.Descendants().Where(e => e.Name.LocalName.Equals(elementNames[0], StringComparison.OrdinalIgnoreCase));
            foreach (var element in elementsLevel1)
            {
                XElement elementNext = element;
                int i;
                for (i = 1; i < elementNames.Length; i++)
                {
                    elementNext = elementNext.Elements().Where(e => e.Name.LocalName.Equals(elementNames[i], StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                    if (elementNext == null)
                    {
                        break;
                    }
                }
                if (i == elementNames.Length)
                {
                    return elementNext;
                }
            }
            return null;
        }
    }
}
