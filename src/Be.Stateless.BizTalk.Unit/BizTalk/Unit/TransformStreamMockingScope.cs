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
using System.IO;
using Be.Stateless.BizTalk.Streaming.Extensions;
using Moq;

namespace Be.Stateless.BizTalk.Unit
{
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API")]
	public class TransformStreamMockingScope : IDisposable
	{
		public TransformStreamMockingScope()
		{
			_transformerFactory = StreamExtensions.StreamTransformerFactory;
			Mock = new Mock<ITransformStream>();
			StreamExtensions.StreamTransformerFactory = _ => Mock.Object;
		}

		#region IDisposable Members

		public void Dispose()
		{
			StreamExtensions.StreamTransformerFactory = _transformerFactory;
		}

		#endregion

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Public API")]
		public Mock<ITransformStream> Mock { get; }

		private readonly Func<Stream[], ITransformStream> _transformerFactory;
	}
}
