using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class ResultService<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Response { get; set; }
        public int Code { get; set; }

        public static ResultService<T> Ok(int code, T response, string message = "")
        {
            return new ResultService<T>() { Code = code, Success = true, Response = response, Message = message };
        }

        public static ResultService<T> Fail(int code, T response ,string message)
        {
            return new ResultService<T>() { Code = code, Success = false, Response = response, Message = message };
        }
    }
}
