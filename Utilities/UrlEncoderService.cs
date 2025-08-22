using System.Web;

namespace Utilities
{
    public interface IUrlEncoderService
    {
        string Encode(string value);
    }
    public class UrlEncoderService : IUrlEncoderService
    {
        public UrlEncoderService() { }
        public string Encode(string value)
        {
            return HttpUtility.UrlEncode(value);
        }
    }
}