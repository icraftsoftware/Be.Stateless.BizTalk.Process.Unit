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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using AutoFixture;
using AutoFixture.Kernel;
using Be.Stateless.BizTalk.Component;
using Be.Stateless.BizTalk.Component.Extensions;
using Be.Stateless.Extensions;
using Be.Stateless.Linq.Extensions;
using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Message.Interop;
using Moq;

namespace Be.Stateless.BizTalk.Unit.Component
{
	/// <summary>
	/// This base class provides utility methods as well as non-regression test cases for <see cref="PipelineComponent"/>-derived classes.
	/// </summary>
	/// <typeparam name="T">
	/// The <see cref="PipelineComponent"/> derived class to test.
	/// </typeparam>
	public abstract class PipelineComponentFixtureBase<T> where T : PipelineComponent, new()
	{
		/// <summary>
		/// All the properties of <see cref="PipelineComponent"/> <typeparamref name="T"/> that are to be read and written to an <see cref="IPropertyBag"/>.
		/// </summary>
		/// <remarks>
		/// Any property that is <c>public</c> and optionally qualified by a <see cref="BrowsableAttribute">[Browsable(true)]</see> attribute is considered to
		/// be a configurable <see cref="PipelineComponent"/> property. Properties explicitly qualified by an <see
		/// cref="BrowsableAttribute">[Browsable(false)]</see> attribute are ignored.
		/// </remarks>
		protected virtual IEnumerable<PropertyInfo> ConfigurableProperties
		{
			get
			{
				// all public properties that are either [Browsable(true)] qualified or not qualified at all
				// (.All() is true if the collection of BrowsableAttribute is empty)
				return typeof(T).GetProperties()
					.Where(
						propertyInfo => propertyInfo.GetCustomAttributes(typeof(BrowsableAttribute), true)
							.Cast<BrowsableAttribute>()
							.All(ba => ba.Browsable))
					.ToArray();
			}
		}

		protected MessageMock MessageMock { get; private set; }

		protected Mock<IPipelineContext> PipelineContextMock { get; private set; }

		/// <summary>
		/// <see cref="PipelineComponentFixtureBase{T}"/> initialization to be called either by an xUnit fixture's constructor or a NUnit fixture's SetUp
		/// method.
		/// </summary>
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

		/// <summary>
		/// Ensure all	<see cref="ConfigurableProperties"/> can be converted back and forth to a string in order to be both readable from and writable to an
		/// <see cref="IPropertyBag"/>.
		/// </summary>
		protected void VerifyAllPropertiesSupportsRoundTripStringConversion()
		{
			var fixture = new Fixture();

			ConfigurableProperties.ForEach(
				p => {
					var converter = TypeDescriptor.GetConverter(p.PropertyType);
					var value = new SpecimenContext(fixture).Resolve(p);

					if (!converter.CanConvertTo(typeof(string)))
						throw new NotSupportedException(
							$"{converter.GetType().Name} TypeConverter cannot convert a {p.PropertyType.Name} to a string. "
							+ $"{p.PropertyType.Name} must have a dedicated TypeConverter that supports back-and-forth string conversions.");
					var serializedValue = (string) converter.ConvertTo(value, typeof(string));
					if (serializedValue.IsNullOrEmpty())
						throw new NotSupportedException(
							$"The conversion to string made by {converter.GetType().Name} of the {p.PropertyType.Name} {p.Name} property returned null for {value}.");

					if (!converter.CanConvertFrom(typeof(string)))
						throw new NotSupportedException(
							$"{converter.GetType().Name} TypeConverter cannot convert a string to a {p.PropertyType.Name}. "
							+ $"{p.PropertyType.Name} must have a dedicated TypeConverter that supports back-and-forth string conversions.");
					var deserializedValue = converter.ConvertFrom(serializedValue);
					if (deserializedValue == null)
						throw new NotSupportedException(
							$"The conversion from string made by {converter.GetType().Name} of the {p.PropertyType.Name} {p.Name} property returned null for {serializedValue}.");

					if (!value.Equals(deserializedValue))
						throw new NotSupportedException(
							$"{converter.GetType().Name} TypeConverter could not perform a successful roundtrip conversion of {p.PropertyType.Name} {p.Name} for {value}. "
							+ $"The failure to validate a successful roundtrip conversion could also be due to the fact that {p.PropertyType.Name} does not implement IEquatable<T>.");
				});
		}

		/// <summary>
		/// Ensure all <see cref="ConfigurableProperties"/> are loaded from an <see cref="IPropertyBag"/>, it mainly protects pipeline component authors from
		/// stupid copy/paste mistakes in <see cref="PipelineComponent"/>-derived classes.
		/// </summary>
		protected void VerifyAllPropertiesAreLoadedFromPropertyBag()
		{
			var propertyBag = new Mock<IPropertyBag>().Object;

			var sut = new T();
			sut.Load(propertyBag, 0);

			ConfigurableProperties
				.ForEach(
					p => Mock.Get(propertyBag).Verify(
						pb => pb.Read(p.Name, out It.Ref<object>.IsAny, 0),
						Times.Once,
						$"{p.Name} property has not been read from IPropertyBag. Apply a [Browsable(false)] attribute to the property if it is intended."));
		}

		/// <summary>
		/// Ensure <see cref="ConfigurableProperties"/> are saved to an <see cref="IPropertyBag"/>, it mainly protects pipeline component authors from stupid
		/// copy/paste mistakes in <see cref="PipelineComponent"/>-derived classes.
		/// </summary>
		protected void VerifyAllPropertiesAreSavedToPropertyBag()
		{
			var propertyBag = new Mock<IPropertyBag>().Object;

			var sut = new T();
			sut.Save(propertyBag, true, true);

			ConfigurableProperties
				.ForEach(
					p => Mock.Get(propertyBag).Verify(
						pb => pb.Write(p.Name, ref It.Ref<object>.IsAny),
						Times.Once,
						$"{p.Name} property has not been written to IPropertyBag. Apply a [Browsable(false)] attribute to the property if it is intended. "
						+ "Notice that, for a string property, an empty string value should always be written even if it is null or empty."));
		}

		/// <summary>
		/// Ensure <see cref="PipelineComponent"/>-derived class does not break the implementation of the <see
		/// cref="Microsoft.BizTalk.Component.Interop.IComponent.Execute"/> method from its base class.
		/// </summary>
		protected void VerifyExecuteReturnsImmediatelyWhenMessageIsNull()
		{
			var sut = new Mock<T> { CallBase = true };

			var resultMessage = sut.Object.Execute(null, null);
			if (resultMessage != null)
				throw new NotSupportedException($"{nameof(PipelineComponent)} {typeof(T).Name} did not return a null message although it was given a null one.");

			sut.Verify(pc => pc.ExecuteCore(It.IsAny<IPipelineContext>(), It.IsAny<IBaseMessage>()), Times.Never());
		}

		/// <summary>
		/// Ensure <see cref="PipelineComponent"/>-derived class does not break the implementation of the <see
		/// cref="Microsoft.BizTalk.Component.Interop.IComponent.Execute"/> method from its base class.
		/// </summary>
		protected void VerifyExecuteThrowsWhenPipelineContextIsNull()
		{
			try
			{
				var sut = new T { Enabled = false };
				sut.Execute(null, MessageMock.Object);
				throw new NotSupportedException($"{nameof(PipelineComponent)} {typeof(T).Name} did not throw when given a null pipelineContext.");
			}
			catch (ArgumentNullException exception)
			{
				if (exception.ParamName != "pipelineContext")
					throw new NotSupportedException($"{nameof(PipelineComponent)} {typeof(T).Name} did not throw when given a null pipelineContext.", exception);
			}
		}

		/// <summary>
		/// Ensure <see cref="PipelineComponent"/>-derived class does not break the implementation of the <see
		/// cref="Microsoft.BizTalk.Component.Interop.IComponent.Execute"/> method from its base class.
		/// </summary>
		protected void VerifyExecuteCoreIsSkippedWhenPipelineComponentIsNotEnabled()
		{
			var sut = new Mock<T> { CallBase = true };
			sut.Object.Enabled = false;

			var resultMessage = sut.Object.Execute(PipelineContextMock.Object, MessageMock.Object);
			if (!ReferenceEquals(resultMessage, MessageMock.Object))
				throw new NotSupportedException(
					$"{nameof(PipelineComponent)} {typeof(T).Name} did not return the same message it was given although the pipeline component was not enabled.");

			sut.Verify(pc => pc.ExecuteCore(It.IsAny<IPipelineContext>(), It.IsAny<IBaseMessage>()), Times.Never());
		}
	}
}
