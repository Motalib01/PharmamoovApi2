using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PharmaMoov.API.Helpers
{
    public class PushNotification
    {
        //Code to send push notifications to device
        public static async Task<bool> SendNotifications(string serverKey, string senderId, string to, string title, string body,string fcmUrl, Dictionary<string, string> customValue)
        {
            try
            {
                var customData = new
                {
                    to, // Recipient device token
                    notification = new { title, body, channel_id = "PHARMA_CHANNEL_ID" },
                    data = customValue
                };
                //// Using Newtonsoft.Json
                var jsonBody = JsonConvert.SerializeObject(customData);
                return await SendNotificationToFirebaseServer(serverKey, senderId, fcmUrl, jsonBody);
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public static async Task<bool> SendNotificationToFirebaseServer(string serverKey, string senderId, string fcmUrl, string jsonBody)
        {
            try
            {
                // Get the server key from FCM console
                serverKey = string.Format("key={0}", serverKey);
                // Get the sender id from FCM console
                senderId = string.Format("id={0}", senderId);

                using (var httpRequest = new HttpRequestMessage(HttpMethod.Post, fcmUrl))
                {
                    httpRequest.Headers.TryAddWithoutValidation("Authorization", serverKey);
                    httpRequest.Headers.TryAddWithoutValidation("Sender", senderId);
                    httpRequest.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                    using (var httpClient = new HttpClient())
                    {
                        var result = await httpClient.SendAsync(httpRequest);

                        if (result.IsSuccessStatusCode)
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }
    }
}
