using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class IntegrationTestClient
{
    static async Task Main()
    {
        // Change this to match your listener's address and port
        string baseUrl = "http://localhost:8888/";

        using var client = new HttpClient();

        // REST GET request
        var restGetUrl = baseUrl + "Countries";
        var restBody = @"{
                            ""countryRequest"": {
                                ""code"": ""CA""
                            }
                        }";
        var restContent = new StringContent(restBody, Encoding.UTF8, "application/json");
        var restGetResponse = await client.PostAsync(restGetUrl, restContent);
        Console.WriteLine($"REST POST: {restGetUrl} => {restGetResponse.StatusCode}");
        Console.WriteLine(await restGetResponse.Content.ReadAsStringAsync());

        // SOAP request
        var soapUrl = baseUrl + "ProfileService";
        var soapEnvelope = @"<?xml version=""1.0"" encoding=""utf-8""?>
                            <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                              <soap:Body>
                                <GetProfileRequest>
                                  <profileType>Personal</profileType>
                                  <profileId>1000002</profileId>
                                </GetProfileRequest>
                              </soap:Body>
                            </soap:Envelope>";
        var soapContent = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        var soapResponse = await client.PostAsync(soapUrl, soapContent);
        Console.WriteLine($"SOAP POST: {soapUrl} => {soapResponse.StatusCode}");
        Console.WriteLine(await soapResponse.Content.ReadAsStringAsync());

        // UNKNOWN content type request (to trigger MessageBox)
        //var unknownUrl = baseUrl + "api/test";
        //var unknownContent = new StringContent("This is a plain text request.", Encoding.UTF8, "text/plain");
        //var unknownResponse = await client.PostAsync(unknownUrl, unknownContent);
        //Console.WriteLine($"UNKNOWN POST: {unknownUrl} => {unknownResponse.StatusCode}");
        //Console.WriteLine(await unknownResponse.Content.ReadAsStringAsync());
    }
}