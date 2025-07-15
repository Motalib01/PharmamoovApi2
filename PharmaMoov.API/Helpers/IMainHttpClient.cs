//using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PharmaMoov.API.Helpers
{
    public interface IMainHttpClient
    {
        string PostHttpClientRequest(string _rEndPoint, object _content, int? _platform);
        string GetHttpClientRequest(string requestEndPoint);
        //IRestResponse DeliveryJobPostRequest(string iEndpoint, Method iMethod,
        //                                            List<KeyValuePair<string, string>> iHeaders,
        //                                            List<KeyValuePair<string, string>> iQueryParameters = null,
        //                                            List<KeyValuePair<string, string>> iBodyParameters = null);
        //IRestResponse DeliveryJobPostRequestText();
    }
}
