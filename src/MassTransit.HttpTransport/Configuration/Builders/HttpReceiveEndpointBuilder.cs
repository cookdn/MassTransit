﻿// Copyright 2007-2016 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace MassTransit.HttpTransport.Configuration.Builders
{
    using GreenPipes;
    using MassTransit.Pipeline;


    public class HttpReceiveEndpointBuilder :
        IHttpReceiveEndpointBuilder
    {
        readonly IConsumePipe _consumePipe;

        public HttpReceiveEndpointBuilder(IConsumePipe consumePipe)
        {
            _consumePipe = consumePipe;
        }

        public ConnectHandle ConnectConsumePipe<T>(IPipe<ConsumeContext<T>> pipe) where T : class
        {
            return _consumePipe.ConnectConsumePipe(pipe);
        }

        public ConnectHandle ConnectConsumeMessageObserver<T>(IConsumeMessageObserver<T> observer) where T : class
        {
            return _consumePipe.ConnectConsumeMessageObserver(observer);
        }
    }
}