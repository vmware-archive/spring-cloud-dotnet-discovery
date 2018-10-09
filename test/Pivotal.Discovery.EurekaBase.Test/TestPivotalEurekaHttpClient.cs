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
using Pivotal.Discovery.Eureka;
using Steeltoe.Discovery.Eureka;
using System;
using System.Net.Http;

namespace Pivotal.Discovery.EurekaBase.Test
{
    public class TestPivotalEurekaHttpClient : PivotalEurekaHttpClient
    {
        public TestPivotalEurekaHttpClient(IOptionsMonitor<EurekaClientOptions> config, IEurekaDiscoveryClientHandlerProvider handlerProvider = null, ILoggerFactory logFactory = null)
            : base(config, handlerProvider, logFactory)
        {
        }

        public HttpRequestMessage PublicGetRequestMessage(HttpMethod method, Uri requestUri)
        {
            return GetRequestMessage(method, requestUri);
        }
    }
}
