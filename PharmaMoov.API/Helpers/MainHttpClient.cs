using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace PharmaMoov.API.Helpers
{
    public class MainHttpClient : IMainHttpClient
    {
        private string FirebaseLink = "";
        private string FirebaseAPIKeyIOS = "";
        private string iOSSenderID = "";
        private string FirebaseAPIKeyAndroid = "";
        private string androidSenderKey = "";
        private APIConfigurationManager Configuration { get; }

        private IHttpContextAccessor _httpCtxtAcc;
        private ISession _session => _httpCtxtAcc.HttpContext.Session;

        public MainHttpClient(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, APIConfigurationManager _config)
        {
            Configuration = _config;
            FirebaseLink = Configuration.PNConfig.FireBLink;
            FirebaseAPIKeyIOS = Configuration.PNConfig.IOSFireBKey;
            iOSSenderID = Configuration.PNConfig.IOSSenderID;
            FirebaseAPIKeyAndroid = Configuration.PNConfig.AndroidFireBKey;
            androidSenderKey = Configuration.PNConfig.AndroidSenderID;
            _httpCtxtAcc = httpContextAccessor;
        }

        public MainHttpClient(APIConfigurationManager _config) {
            Configuration = _config;
        }

        public string PostHttpClientRequest(string requestEndPoint, object content, int? platform)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(FirebaseLink);

                client.DefaultRequestHeaders.Clear();

                if (platform == 1) //iOS
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + FirebaseAPIKeyIOS);
                    client.DefaultRequestHeaders.Add("Sender", iOSSenderID);
                }
                else if (platform == 2) //android
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + FirebaseAPIKeyAndroid);
                    client.DefaultRequestHeaders.Add("Sender", androidSenderKey);
                }
                else //welp
                {
                    //nothing to see here
                }


                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = client.PostAsJsonAsync(requestEndPoint, content).Result;
                //Res.Content = new StringContent()
                return Res.Content.ReadAsStringAsync().Result;

            }
        }

        public async Task<string> PostHttpClientRequestAsync(string requestEndPoint, object content, int? platform)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(FirebaseLink);
                client.DefaultRequestHeaders.Clear();

                if (platform == 1) //iOS
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + FirebaseAPIKeyIOS);
                    client.DefaultRequestHeaders.Add("Sender", iOSSenderID);
                }
                else if (platform == 2) //android
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("key", "=" + FirebaseAPIKeyAndroid);
                    client.DefaultRequestHeaders.Add("Sender", androidSenderKey);
                }

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = (await client.PostAsJsonAsync(requestEndPoint, content));

                return Res.Content.ReadAsStringAsync().Result;
            }
        }

        public string GetHttpClientRequest(string requestEndPoint)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(FirebaseLink);

                client.DefaultRequestHeaders.Clear();

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = client.GetAsync(requestEndPoint).Result;
                return Res.Content.ReadAsStringAsync().Result;
            }
        }

        public async Task<string> SendSmsHttpClientRequestAsync(string _smsParameters)
        {
            using (var client = new HttpClient())
            {
                //Passing service base url  
                client.BaseAddress = new Uri(Configuration.SmsConfig.Endpoint);

                client.DefaultRequestHeaders.Clear();

                //Define request data format  
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource GetAllEmployees using HttpClient  
                HttpResponseMessage Res = (await client.GetAsync(_smsParameters));
                return Res.Content.ReadAsStringAsync().Result;
            }
        }



        #region stuart

        //public IRestResponse DeliveryJobPostRequest(string iEndpoint, Method iMethod, 
        //                                            List<KeyValuePair<string, string>> iHeaders, 
        //                                            List<KeyValuePair<string, string>> iQueryParameters = null,
        //                                            List<KeyValuePair<string, string>> iBodyParameters = null)
        //{
            
        //    var client = new RestClient(Configuration.DeliveryJobConfig.BaseUrl + iEndpoint);
        //    var request = new RestRequest(iMethod);
        //    if (iHeaders != null) 
        //    { 
        //        foreach(var header in iHeaders)
        //        {
        //            request.AddHeader(header.Key, header.Value);
        //        }
        //    }
        //    if (iQueryParameters != null)
        //    {
        //        foreach (var parameter in iQueryParameters)
        //        {
        //            request.AddParameter(parameter.Key, parameter.Value, ParameterType.QueryString);
        //        }
        //    }
        //    if (iBodyParameters != null)
        //    {
        //        foreach (var parameter in iBodyParameters)
        //        {
        //            request.AddParameter(parameter.Key, parameter.Value, ParameterType.RequestBody);
        //        }
        //    }

        //    IRestResponse response = client.Execute(request);
        //    return response;
        //}

        //public IRestResponse DeliveryJobPostRequestText()
        //{

        //    var client = new RestClient("https://api.sandbox.stuart.com/https%3A%2F%2Featalyapi.itfaq.net%2Fapi%2FJobUpdatedHook");
        //    var request = new RestRequest(Method.POST);
        //    request.AddHeader("content-type", "application/json");
        //    request.AddParameter("application/json", "{\"event\":\"job\",\"type\":\"create\",\"data\":{\"id\":45153,\"currentDelivery\":null,\"transportType\":null,\"status\":\"scheduled\",\"comment\":null,\"originComment\":null,\"destinationComment\":null,\"jobReference\":\"Order_ID#1234_1\"}}", ParameterType.RequestBody);
        //    IRestResponse response = client.Execute(request);

        //    return response;

        //}

        #endregion
    }
}
