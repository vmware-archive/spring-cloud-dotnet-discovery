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

using Steeltoe.Discovery.Eureka;
using System;
using System.Net.Http;
using Xunit;

namespace Pivotal.Discovery.EurekaBase.Test
{
    public class PivotalEurekaHttpClientTest
    {
        [Fact]
        public void GetRequestMessage_No_Auth_When_Creds_Not_In_Url()
        {
            // arrange
            var clientOptions = new EurekaClientOptions { ServiceUrl = "https://boo:123/eureka/" };
            var optionsMonitor = new TestOptionMonitorWrapper<EurekaClientOptions>(clientOptions);
            var client = new TestPivotalEurekaHttpClient(optionsMonitor);

            // act
            var result = client.PublicGetRequestMessage(HttpMethod.Post, new Uri(clientOptions.EurekaServerServiceUrls));

            // assert
            Assert.Equal(HttpMethod.Post, result.Method);
            Assert.Equal(new Uri("https://boo:123/eureka/"), result.RequestUri);
            Assert.False(result.Headers.Contains("Authorization"));
        }

        [Fact]
        public void GetRequestMessage_Adds_Auth_When_Creds_In_Url()
        {
            // arrange
            var clientOptions = new EurekaClientOptions { ServiceUrl = "https://user:pass@boo:123/eureka/" };
            var optionsMonitor = new TestOptionMonitorWrapper<EurekaClientOptions>(clientOptions);
            var client = new TestPivotalEurekaHttpClient(optionsMonitor);

            // act
            var result = client.PublicGetRequestMessage(HttpMethod.Post, new Uri(clientOptions.EurekaServerServiceUrls));

            // assert
            Assert.Equal(HttpMethod.Post, result.Method);
            Assert.Equal(new Uri("https://boo:123/eureka/"), result.RequestUri);
            Assert.True(result.Headers.Contains("Authorization"));
        }
    }
}
