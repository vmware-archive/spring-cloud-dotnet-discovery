// Copyright 2017 the original author or authors.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Steeltoe.Common.Http;
using Steeltoe.Discovery.Eureka;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Pivotal.Discovery.Eureka
{
    public class PivotalEurekaHttpClient : Steeltoe.Discovery.Eureka.Transport.EurekaHttpClient
    {
        private const int DEFAULT_GETACCESSTOKEN_TIMEOUT = 10000; // Milliseconds
        private static readonly char[] COLON_DELIMIT = new char[] { ':' };

        private IOptionsMonitor<EurekaClientOptions> _configOptions;

        protected override IEurekaClientConfig Config
        {
            get
            {
                return _configOptions.CurrentValue;
            }
        }

        public PivotalEurekaHttpClient(IOptionsMonitor<EurekaClientOptions> config, ILoggerFactory logFactory = null)
        {
            _config = null;
            _configOptions = config ?? throw new ArgumentNullException(nameof(config));
            Initialize(new Dictionary<string, string>(), logFactory);
        }

        internal string FetchAccessToken()
        {
            var config = Config as EurekaClientOptions;
            if (config == null || string.IsNullOrEmpty(config.AccessTokenUri))
            {
                return null;
            }

            return HttpClientHelper.GetAccessToken(
                config.AccessTokenUri,
                config.ClientId,
                config.ClientSecret,
                DEFAULT_GETACCESSTOKEN_TIMEOUT,
                config.ValidateCertificates).Result;
        }

        protected override HttpRequestMessage GetRequestMessage(HttpMethod method, Uri requestUri)
        {
            string rawUri = requestUri.GetComponents(UriComponents.HttpRequestUrl, UriFormat.Unescaped);
            string rawUserInfo = requestUri.GetComponents(UriComponents.UserInfo, UriFormat.Unescaped);
            var request = new HttpRequestMessage(method, rawUri);

            if (!string.IsNullOrEmpty(rawUserInfo) && rawUserInfo.Contains(":"))
            {
                string[] userInfo = GetUserInfo(rawUserInfo);
                if (userInfo.Length >= 2)
                {
                    request = HttpClientHelper.GetRequestMessage(method, rawUri, userInfo[0], userInfo[1]);
                }
            }
            else
            {
                request = HttpClientHelper.GetRequestMessage(method, rawUri, FetchAccessToken);
            }

            foreach (var header in _headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }

            request.Headers.Add("Accept", "application/json");
            return request;
        }

        private string[] GetUserInfo(string userInfo)
        {
            string[] result = null;
            if (!string.IsNullOrEmpty(userInfo))
            {
                result = userInfo.Split(COLON_DELIMIT);
            }

            return result;
        }
    }
}
