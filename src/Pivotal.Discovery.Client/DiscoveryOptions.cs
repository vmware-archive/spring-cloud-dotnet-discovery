//
// Copyright 2015 the original author or authors.
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
//

using ST = SteelToe.Discovery.Client;

namespace Pivotal.Discovery.Client
{
    public class DiscoveryOptions : ST.DiscoveryOptions
    {
        public new DiscoveryClientType ClientType
        {
            get
            {
                return (DiscoveryClientType)System.Enum.Parse(typeof(DiscoveryClientType), _type);
            }
            set
            {
                _type = System.Enum.GetName(typeof(DiscoveryClientType), value);
            }
        }

        public new IDiscoveryClientOptions ClientOptions
        {
            get
            {
                return (IDiscoveryClientOptions) _clientOptions;
            }
            set
            {
                _clientOptions = value;
            }
        }

        public new IDiscoveryRegistrationOptions RegistrationOptions
        {
            get
            {
                return (IDiscoveryRegistrationOptions) _registrationOptions;
            }
            set
            {
                _registrationOptions = value;
            }
        }
    }

    public enum DiscoveryClientType { EUREKA, UNKNOWN }
}
