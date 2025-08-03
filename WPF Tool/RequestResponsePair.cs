public class RequestResponsePair
{
    public string RequestSummary { get; set; }
    public string RequestBody { get; set; }
    public string ResponseSummary { get; set; }
    public string ResponseBody { get; set; }
    public string MockFileSource { get; set; }
    public int StatusCode { get; set; }
}