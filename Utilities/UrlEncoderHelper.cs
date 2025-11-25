using System.Web;

namespace Utilities
{
    public interface IUrlEncoderHelper
    {
        string Encode(string value);
    }
    public class UrlEncoderHelper : IUrlEncoderHelper
    {
        public UrlEncoderHelper() { }
        public string Encode(string value)
        {
            return HttpUtility.UrlEncode(value);
        }
    }
}