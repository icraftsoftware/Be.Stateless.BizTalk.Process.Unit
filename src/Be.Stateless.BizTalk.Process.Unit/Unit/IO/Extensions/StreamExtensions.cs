#region Copyright & License

// Copyright © 2012 - 2022 François Chabot
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
using System.Xml;
using Be.Stateless.BizTalk.ContextProperties;
using Be.Stateless.Xml.Extensions;
using Microsoft.XLANGs.BaseTypes;

namespace Be.Stateless.BizTalk.Unit.IO.Extensions
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Extension class.")]
	public static class StreamExtensions
	{
		public static Stream InjectAttribute<T>(this Stream stream, MessageContextProperty<T, string> property, string value)
			where T : MessageContextPropertyBase, new()
		{
			return stream.InjectAttribute(property.Name, value);
		}

		public static Stream InjectAttribute<T, TR>(this Stream stream, MessageContextProperty<T, TR> property, TR value)
			where T : MessageContextPropertyBase, new()
		{
			return stream.InjectAttribute(property.Name, value.ToString());
		}

		public static Stream InjectAttribute<T>(this Stream stream, MessageContextProperty<T, DateTime> property, DateTime value)
			where T : MessageContextPropertyBase, new()
		{
			return stream.InjectAttribute(property.Name, value.ToString("o"));
		}

		private static Stream InjectAttribute(this Stream stream, string name, string value)
		{
			var document = new XmlDocument();
			document.Load(stream);
			document.DocumentElement!.Attributes.Append(document.CreateAttribute(name)).Value = value;
			return document.AsStream();
		}
	}
}
