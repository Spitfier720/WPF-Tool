using EasyMockLib.Models;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Xml.Linq;

namespace WPF_Tool_Unit_Testing
{
    public class MockFileNodeTests
    {
        [Fact]
        public void GetMock_ReturnsCorrectMock_ForMatchingRestRequest()
        {
            // Arrange
            var restConfig = new Dictionary<string, Dictionary<string, List<string>>>
            {
                ["Countries/Details"] = new()
                {
                    ["POST"] = new() { "countryRequest.code", "countryRequest.details.population" }
                }
            };
            var soapConfig = new Dictionary<string, Dictionary<string, List<string>>>();
            var mockFileNode = new MockFileNode();

            // Create two mock nodes with different request bodies
            var mock1 = new MockNode
            {
                Request = new Request
                {
                    RequestType = ServiceType.REST,
                    ServiceName = "Countries/Details",
                    RequestBody = new Body
                    {
                        Content = @"{""countryRequest"":{""code"":""CA"",""details"":{""population"":38000000}}}",
                        ContentObject = JObject.Parse(@"{""countryRequest"":{""code"":""CA"",""details"":{""population"":38000000}}}")
                    }
                },
                Response = new Response
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseBody = new Body { Content = @"{""name"":""Canada"",""code"":""CA""}" }
                }
            };

            var mock2 = new MockNode
            {
                Request = new Request
                {
                    RequestType = ServiceType.REST,
                    ServiceName = "Countries/Details",
                    RequestBody = new Body
                    {
                        Content = @"{""countryRequest"":{""code"":""US"",""details"":{""population"":331000000}}}",
                        ContentObject = JObject.Parse(@"{""countryRequest"":{""code"":""US"",""details"":{""population"":331000000}}}")
                    }
                },
                Response = new Response
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseBody = new Body { Content = @"{""name"":""United States"",""code"":""US""}" }
                }
            };

            mockFileNode.Nodes.Add(mock1);
            mockFileNode.Nodes.Add(mock2);

            // The request to match
            string requestContent = @"{""countryRequest"":{""code"":""CA"",""details"":{""population"":38000000}}}";

            // Act
            var result = mockFileNode.GetMock(
                ServiceType.REST,
                "Countries/Details",
                "POST",
                requestContent,
                new Dictionary<string, Dictionary<string, List<string>>>()
            );

            // Assert
            Assert.NotNull(result);
            Assert.Equal("CA", ((JObject)result.Request.RequestBody.ContentObject)["countryRequest"]["code"]);
            Assert.Equal("Canada", JObject.Parse(result.Response.ResponseBody.Content)["name"]);
        }

        [Fact]
        public void GetMock_ReturnsCorrectMock_ForMatchingSoapRequest()
        {
            // Arrange
            var restConfig = new Dictionary<string, Dictionary<string, List<string>>>();
            var soapConfig = new Dictionary<string, Dictionary<string, List<string>>>
            {
                ["ProfileService"] = new()
                {
                    ["GetProfile"] = new() { "profileId" }
                }
            };
            var mockFileNode = new MockFileNode();

            // SOAP request for profileId 1000001
            string request1 = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
                                   <soap:Header>
                                         <version>1.0</version>
                                         <appName>EasyMock Demo</appName>             
                                         <hostName>Demo Workstation</hostName>
                                         <userId>tester</userId>          
                                         <timeStamp>2023–01–20T14:22:07.425</timeStamp>
                                   </soap:Header>    
                                   <soap:Body>
                                      <GetProfileRequest>
                                          <profileType>Personal</profileType>
                                          <profileId>1000001</profileId>          
                                      </GetProfileRequest>
                                   </soap:Body>
                                </soap:Envelope>";

            string response1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	                                <soapenv:Header>
		                                <requestId>e77ee721-7e71-44db-8f84-08fd5040c458</requestId>
	                                </soapenv:Header>
	                                <soapenv:Body>
		                                <GetProfileResponse>
			                                <name>
				                                <namePrefix>Ms.</namePrefix>
				                                <firstName>JOHN</firstName>
				                                <lastName>SMITH</lastName>
			                                </name>
			                                <address>
				                                <streetNumber>100</streetNumber>
				                                <streetName>KING STREET WEST</streetName>
				                                <city>Toronto</city>
				                                <province>Ontario</province>
				                                <country>Canada</country>
				                                <postalCode>M5N3R7</postalCode>
			                                </address>
		                                </GetProfileResponse>
	                                </soapenv:Body>
                                </soapenv:Envelope>";

            // SOAP request for profileId 1000002
            string request2 = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">  
                                   <soap:Header>
                                         <version>1.0</version>
                                         <appName>EasyMock Demo</appName>             
                                         <hostName>Demo Workstation</hostName>
                                         <userId>tester</userId>          
                                         <timeStamp>2023–01–20T14:22:08.425</timeStamp>
                                   </soap:Header>    
                                   <soap:Body>
                                      <GetProfileRequest>
                                          <profileType>Personal</profileType>
                                          <profileId>1000002</profileId>          
                                      </GetProfileRequest>
                                   </soap:Body>
                                </soap:Envelope>";

            string response2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
                                <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
	                                <soapenv:Header>
		                                <requestId>cc422093-a929-4116-af59-96d3db425555</requestId>
	                                </soapenv:Header>
	                                <soapenv:Body>
		                                <GetProfileResponse>
			                                <name>
				                                <namePrefix>Ms.</namePrefix>
				                                <firstName>TERRY</firstName>
				                                <lastName>CLARK</lastName>
			                                </name>
			                                <address>
				                                <streetNumber>1800</streetNumber>
				                                <streetName>YOUNG STREET</streetName>
				                                <city>Toronto</city>
				                                <province>Ontario</province>
				                                <country>Canada</country>
				                                <postalCode>M5N6P8</postalCode>
			                                </address>
		                                </GetProfileResponse>
	                                </soapenv:Body>
                                </soapenv:Envelope>";

            // Add two mock nodes
            mockFileNode.Nodes.Add(new MockNode
            {
                Request = new Request
                {
                    RequestType = ServiceType.SOAP,
                    ServiceName = "ProfileService",
                    RequestBody = new Body
                    {
                        Content = request1,
                        ContentObject = XElement.Parse(request1)
                    }
                },
                Response = new Response
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseBody = new Body { Content = response1 }
                }
            });

            mockFileNode.Nodes.Add(new MockNode
            {
                Request = new Request
                {
                    RequestType = ServiceType.SOAP,
                    ServiceName = "ProfileService",
                    RequestBody = new Body
                    {
                        Content = request2,
                        ContentObject = XElement.Parse(request2)
                    }
                },
                Response = new Response
                {
                    StatusCode = HttpStatusCode.OK,
                    ResponseBody = new Body { Content = response2 }
                }
            });

            // Act: Try to match the first request
            var result = mockFileNode.GetMock(
                ServiceType.SOAP,
                "ProfileService",
                "GetProfile",
                request1,
                soapConfig
            );

            // Assert
            Assert.NotNull(result);
            Assert.Contains("JOHN", result.Response.ResponseBody.Content);

            // Act: Try to match the second request
            var result2 = mockFileNode.GetMock(
                ServiceType.SOAP,
                "ProfileService",
                "GetProfile",
                request2,
                soapConfig
            );

            // Assert
            Assert.NotNull(result2);
            Assert.Contains("TERRY", result2.Response.ResponseBody.Content);
        }
    }
}