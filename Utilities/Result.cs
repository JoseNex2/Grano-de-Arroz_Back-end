using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class Result<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Response { get; set; }
        public int Code { get; set; }

        public static Result<T> Ok(int code, T response, string message = "")
        {
            return new Result<T>() { Code = code, Success = true, Response = response, Message = message };
        }

        public static Result<T> Fail(int code, T response ,string message)
        {
            return new Result<T>() { Code = code, Success = false, Response = response, Message = message };
        }
    }
}
