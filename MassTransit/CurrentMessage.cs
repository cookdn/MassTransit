// Copyright 2007-2008 The Apache Software Foundation.
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
namespace MassTransit
{
	using System;
	using Internal;

	public static class CurrentMessage
	{
		public static IInboundMessageHeaders Headers
		{
			get { return InboundMessageHeaders.Current; }
		}

		/// <summary>
		/// Respond to the current inbound message with either a send to the ResponseAddress or a
		/// Publish on the bus that received the message
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message">The message to send/publish</param>
		public static void Respond<T>(T message) where T : class
		{
			var headers = Headers;

			if (headers.ResponseAddress != null)
			{
				headers.GetResponseEndpoint().Send(message);
			}
			else
			{
				headers.Bus.Publish(message);
			}
		}


		/// <summary>
		/// Respond to the current inbound message with either a send to the ResponseAddress or a
		/// Publish on the bus that received the message
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="message">The message to send/publish</param>
		/// <param name="contextAction">The action to setup the context on the outbound message</param>
		public static void Respond<T>(T message, Action<IOutboundMessage> contextAction) where T : class
		{
			var context = InboundMessageHeaders.Current;

			if (context.ResponseAddress != null)
			{
				context.GetResponseEndpoint().Send(message, contextAction);
			}
			else
			{
				context.Bus.Publish(message, contextAction);
			}
		}

		/// <summary>
		/// Puts the message back on the input queue so that it can be processed again later
		/// </summary>
		public static void RetryLater()
		{
			var context = InboundMessageHeaders.Current;

			if (context.Message == null)
				throw new InvalidOperationException("RetryLater can only be called when a message is being consumed");

			context.Bus.Endpoint.Send(context.Message, x =>
				{
					x.SetSourceAddress(context.SourceAddress);
					x.SetDestinationAddress(context.DestinationAddress);
					x.SendResponseTo(context.ResponseAddress);
					x.SendFaultTo(context.FaultAddress);
					x.SetRetryCount(context.RetryCount + 1);
				});
		}

		private static IEndpoint GetFaultEndpoint(this IInboundMessageHeaders headers)
		{
			if (headers.FaultAddress == null)
				throw new InvalidOperationException("No fault address was contained in the message");

			return headers.Bus.EndpointFactory.GetEndpoint(headers.FaultAddress);
		}

		private static IEndpoint GetResponseEndpoint(this IInboundMessageHeaders headers)
		{
			if (headers.ResponseAddress == null)
				throw new InvalidOperationException("No response address was contained in the message");

			return headers.Bus.EndpointFactory.GetEndpoint(headers.ResponseAddress);
		}
	}
}