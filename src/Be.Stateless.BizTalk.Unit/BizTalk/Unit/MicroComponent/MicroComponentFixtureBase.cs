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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Be.Stateless.BizTalk.Component.Extensions;
using Be.Stateless.BizTalk.MicroComponent;
using Be.Stateless.BizTalk.Unit.Component;
using Microsoft.BizTalk.Component.Interop;
using Moq;

namespace Be.Stateless.BizTalk.Unit.MicroComponent
{
	/// <summary>
	/// This base class provides utility methods for <see cref="IMicroComponent"/>-derived classes.
	/// </summary>
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
	public abstract class MicroComponentFixtureBase
	{
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "Public API.")]
		protected MessageMock MessageMock { get; private set; }

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API.")]
		protected Mock<IPipelineContext> PipelineContextMock { get; private set; }

		/// <summary>
		/// <see cref="PipelineComponentFixtureBase{T}"/> initialization to be called either by an xUnit fixture's constructor or a NUnit fixture's SetUp
		/// method.
		/// </summary>
		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
		protected void Initialize()
		{
			MessageMock = new MessageMock { DefaultValue = DefaultValue.Mock };
			PipelineContextMock = new Mock<IPipelineContext> { DefaultValue = DefaultValue.Mock };
			// default behavior analogous to actual IPipelineContext implementation
			PipelineContextMock
				.Setup(pc => pc.GetDocumentSpecByType(It.IsAny<string>()))
				.Callback<string>(
					t => throw new COMException(
						$"Finding the document specification by message type \"{t}\" failed. Verify the schema deployed properly.",
						unchecked((int) PipelineContextExtensions.E_SCHEMA_NOT_FOUND)));
		}
	}
}
