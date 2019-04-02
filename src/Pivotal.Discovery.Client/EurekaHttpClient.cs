//
// Copyright 2015 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using STT = Steeltoe.Discovery.Eureka.Transport;
using Steeltoe.Discovery.Eureka;

namespace Pivotal.Discovery.Client
{
    public class EurekaHttpClient : STT.EurekaHttpClient
    {
        public EurekaHttpClient(IEurekaClientConfig config, ILoggerFactory logFactory = null) : base(config, logFactory)
        {
        }

        protected override HttpRequestMessage GetRequestMessage(HttpMethod method, Uri requestUri)
        {
            var request = new HttpRequestMessage(method, requestUri);
            var config = _config as EurekaClientOptions;
            if (config != null && !string.IsNullOrEmpty(config.AccessTokenUri))
            {
                Task<string> task = GetAccessToken();
                task.Wait();

                var accessToken = task.Result;
                if (accessToken != null)
                {
                    AuthenticationHeaderValue auth = new AuthenticationHeaderValue("Bearer", accessToken);
                    request.Headers.Authorization = auth;
                }
            }

            foreach (var header in _headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            request.Headers.Add("Accept", "application/json");
            return request;
        }


        internal async Task<string> GetAccessToken()
        {
            var config = _config as EurekaClientOptions;
   
            var request = new HttpRequestMessage(HttpMethod.Post, config.AccessTokenUri);
            HttpClient client = GetHttpClient(config);
#if NET452
            // If certificate validation is disabled, inject a callback to handle properly
            RemoteCertificateValidationCallback prevValidator = null;
            SecurityProtocolType prevProtocols = (SecurityProtocolType) 0;
            ConfigureCertificateValidatation(out prevProtocols, out prevValidator);
#endif      

            AuthenticationHeaderValue auth = new AuthenticationHeaderValue("Basic", GetEncoded(config.ClientId, config.ClientSecret));
            request.Headers.Authorization = auth;

            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" }
            });

            try
            {
                using (client)
                {
                    using (HttpResponseMessage response = await client.SendAsync(request))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            _logger?.LogInformation("Eureka Server returned status: {0} while obtaining access token from: {1}",
                                response.StatusCode, config.AccessTokenUri);
                            return null;
                        }

                        var payload = JObject.Parse(await response.Content.ReadAsStringAsync());
                        var token = payload.Value<string>("access_token");
                        return token;
                    }
                }
            }
            catch (Exception e)
            {
                _logger?.LogError("Eureka Server exception: {0} ,obtaining access token from: {1}", e, config.AccessTokenUri);
            }
#if NET452
            finally
            {
                RestoreCertificateValidation(prevProtocols, prevValidator);
            }
#endif
            return null;
        }
        internal protected string GetEncoded(string user, string password)
        {
            if (user == null)
                user = string.Empty;
            if (password == null)
                password = string.Empty;
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(user + ":" + password));
        }

    }
}
