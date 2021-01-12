#region Copyright & License

// Copyright © 2012 - 2020 François Chabot
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

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using Be.Stateless.BizTalk.Unit.ServiceModel;
using Be.Stateless.BizTalk.Unit.ServiceModel.Stub;
using Microsoft.BizTalk.XLANGs.BTXEngine;

namespace Be.Stateless.BizTalk.Unit.Process
{
	[SoapStubHostActivator]
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
	public abstract class CompletedSoapedOrchestrationFixture<T> : CompletedOrchestrationFixture<T>, ISoapServiceHostInjection
		where T : BTXService
	{
		#region ISoapServiceHostInjection Members

		public virtual void InjectSoapServiceHost(SoapServiceHost soapServiceHost)
		{
			if (soapServiceHost == null) throw new ArgumentNullException(nameof(soapServiceHost));
			if (soapServiceHost.ServiceType == typeof(SoapStub))
			{
				SoapStub = (SoapStub) soapServiceHost.SingletonInstance;
			}
		}

		#endregion

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Public API.")]
		protected SoapStub SoapStub { get; private set; }
	}
}
