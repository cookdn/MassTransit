﻿// Copyright 2007-2015 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace MassTransit.QuartzIntegration.Tests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;


    [TestFixture]
    public class Specifying_an_event_in_the_past :
        QuartzInMemoryTestFixture
    {
        [Test]
        public async void Should_handle_now_properly()
        {
            Task<ConsumeContext<A>> handler = SubscribeHandler<A>();

            ISendEndpoint endpoint = await Bus.GetSendEndpoint(new Uri("loopback://localhost/quartz"));

            await endpoint.ScheduleSend(Bus.Address, DateTime.UtcNow, new A {Name = "Joe"});

            await handler;
        }

        [Test]
        public async void Should_handle_slightly_in_the_future_properly()
        {
            Task<ConsumeContext<A>> handler = SubscribeHandler<A>();

            ISendEndpoint endpoint = await Bus.GetSendEndpoint(new Uri("loopback://localhost/quartz"));

            await endpoint.ScheduleSend(Bus.Address, DateTime.UtcNow + TimeSpan.FromSeconds(2), new A {Name = "Joe"});

            await handler;
        }

        [Test]
        public async void Should_include_message_headers()
        {
            Task<ConsumeContext<A>> handler = SubscribeHandler<A>();

            ISendEndpoint endpoint = await Bus.GetSendEndpoint(new Uri("loopback://localhost/quartz"));

            Guid requestId = Guid.NewGuid();
            Guid correlationId = Guid.NewGuid();
            await endpoint.ScheduleSend(Bus.Address, DateTime.UtcNow, new A {Name = "Joe"}, Pipe.Execute<SendContext>(x =>
            {
                x.FaultAddress = Bus.Address;
                x.ResponseAddress = InputQueueAddress;
                x.RequestId = requestId;
                x.CorrelationId = correlationId;

                x.Headers.Set("Hello", "World");
            }));

            ConsumeContext<A> context = await handler;

            Assert.AreEqual(Bus.Address, context.FaultAddress);
            Assert.AreEqual(InputQueueAddress, context.ResponseAddress);
            Assert.IsTrue(context.RequestId.HasValue);
            Assert.AreEqual(requestId, context.RequestId.Value);
            Assert.IsTrue(context.CorrelationId.HasValue);
            Assert.AreEqual(correlationId, context.CorrelationId.Value);

            object value;
            Assert.IsTrue(context.Headers.TryGetHeader("Hello", out value));
            Assert.AreEqual("World", value);
        }

        [Test]
        public async void Should_properly_send_the_message()
        {
            Task<ConsumeContext<A>> handler = SubscribeHandler<A>();

            ISendEndpoint endpoint = await Bus.GetSendEndpoint(new Uri("loopback://localhost/quartz"));

            await endpoint.ScheduleSend(Bus.Address, DateTime.UtcNow + TimeSpan.FromHours(-1), new A {Name = "Joe"});

            await handler;
        }


        class A
        {
            public string Name { get; set; }
        }
    }
}