﻿using System.Text.Json.Nodes;
using System.Net;

namespace Appsec_webapp
{
    public class Recaptcha
    {
        public static async Task<bool> verifyCaptcha(string recaptchaResponse, string secretKey, string verificationUrl)
        {
            using (var client = new HttpClient())
            {
                var content = new MultipartFormDataContent();
                var sanitizedRecaptchaResponse = WebUtility.HtmlEncode(recaptchaResponse);
                content.Add(new StringContent(sanitizedRecaptchaResponse), "response");
                content.Add(new StringContent(secretKey), "secret");

                var response = await client.PostAsync(verificationUrl, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    
                    var jsonResp = JsonNode.Parse(responseString);
                    if (jsonResp != null)
                    {
                        var success = ((bool?)jsonResp["success"]);
                        if (success != null && success == true)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
