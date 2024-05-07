using System.Net;

namespace LogAnalyzerLibrary.Model
{
    public class ApiResponse
    {
        public HttpStatusCode Code { get; set; } = HttpStatusCode.BadRequest;
        public OutResponse OutResponses { get; set; } = new OutResponse();
    }
    public class OutResponse
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = "Invalid Parameters Passed";
        public object Data { get; set; }
    }
}
