namespace API.Response {
    public class BaseResponse {
        public int Code { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }
    }
}
