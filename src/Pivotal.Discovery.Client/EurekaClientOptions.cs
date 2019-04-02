
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

using ST = Steeltoe.Discovery.Client;

namespace Pivotal.Discovery.Client
{
    public class EurekaClientOptions : ST.EurekaClientOptions, IDiscoveryClientOptions
    {
        public EurekaClientOptions() : base()
        {
        }

        private string _accessTokenUri;
        public string AccessTokenUri
        {
            internal set
            {
                _accessTokenUri = value;
            }
            get
            {
                return _accessTokenUri;
            }

        }

        private string _clientSecret;
        public string ClientSecret
        {
            internal set
            {
                _clientSecret = value;
            }
            get
            {
                return _clientSecret;
            }

        }

        private string _clientId;
        public string ClientId
        {
            internal set
            {
                _clientId = value;
            }
            get
            {
                return _clientId;
            }

        }
        
    }
}

