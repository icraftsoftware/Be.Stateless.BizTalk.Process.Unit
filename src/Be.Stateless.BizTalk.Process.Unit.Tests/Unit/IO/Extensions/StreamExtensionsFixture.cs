#region Copyright & License

// Copyright © 2012 - 2021 François Chabot
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
using System.Xml.Linq;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.IO;
using Be.Stateless.IO.Extensions;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.BizTalk.Unit.IO.Extensions
{
	public class StreamExtensionsFixture
	{
		[Fact]
		public void InjectDateTimeAttribute()
		{
			var sut = new StringStream(XDocument.Parse("<root />").ToString(SaveOptions.DisableFormatting));

			var stream = sut.InjectAttribute(SBMessagingProperties.EnqueuedTimeUtc, DateTime.Parse("2022-02-24T11:00:00.123456z"));

			stream.ReadToEnd().Should().Be("<root EnqueuedTimeUtc=\"2022-02-24T12:00:00.1234560+01:00\" />");
		}

		[Fact]
		public void InjectIntegerAttribute()
		{
			var sut = new StringStream(XDocument.Parse("<root />").ToString(SaveOptions.DisableFormatting));

			var stream = sut.InjectAttribute(BtsProperties.RetryCount, 13);

			stream.ReadToEnd().Should().Be("<root RetryCount=\"13\" />");
		}

		[Fact]
		public void InjectStringAttribute()
		{
			var sut = new StringStream(XDocument.Parse("<root />").ToString(SaveOptions.DisableFormatting));

			var stream = sut.InjectAttribute(SBMessagingProperties.CorrelationId, "correlationId");

			stream.ReadToEnd().Should().Be("<root CorrelationId=\"correlationId\" />");
		}
	}
}
