namespace Utilities
{
    public class ResultHelper<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Response { get; set; }
        public int Code { get; set; }

        public static ResultHelper<T> Ok(int code, T response, string message = "")
        {
            return new ResultHelper<T>() { Code = code, Success = true, Response = response, Message = message };
        }

        public static ResultHelper<T> Fail(int code, T response ,string message)
        {
            return new ResultHelper<T>() { Code = code, Success = false, Response = response, Message = message };
        }
    }
}
