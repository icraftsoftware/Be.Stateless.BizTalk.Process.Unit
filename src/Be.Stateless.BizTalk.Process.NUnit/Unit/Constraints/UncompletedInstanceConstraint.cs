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
using System.Collections.Generic;
using System.Linq;
using Microsoft.BizTalk.Operations;
using NUnit.Framework.Constraints;

namespace Be.Stateless.BizTalk.Unit.Constraints
{
	public class UncompletedInstanceConstraint : Constraint
	{
		#region Nested Type: MessageBoxServiceInstanceWrapper

		private class MessageBoxServiceInstanceWrapper
		{
			public MessageBoxServiceInstanceWrapper(MessageBoxServiceInstance messageBoxServiceInstance)
			{
				_messageBoxServiceInstance = messageBoxServiceInstance ?? throw new ArgumentNullException(nameof(messageBoxServiceInstance));
			}

			#region Base Class Member Overrides

			public override string ToString()
			{
				return $"Class: {_messageBoxServiceInstance.Class}\r\n"
					+ $"     ServiceType: {_messageBoxServiceInstance.ServiceType}\r\n"
					+ $"     Creation Time: {_messageBoxServiceInstance.CreationTime}\r\n"
					+ $"     Status: {_messageBoxServiceInstance.InstanceStatus}\r\n"
					+ $"     Error: {_messageBoxServiceInstance.ErrorDescription}\r\n";
			}

			#endregion

			private readonly MessageBoxServiceInstance _messageBoxServiceInstance;
		}

		#endregion

		#region Base Class Member Overrides

		public override ConstraintResult ApplyTo<TActual>(TActual actual)
		{
			if (actual is not IEnumerable<MessageBoxServiceInstance> enumerable) return new(this, actual, false);
			var actualCollection = enumerable.Select(serviceInstance => new MessageBoxServiceInstanceWrapper(serviceInstance));
			return new(this, actualCollection, actualCollection.Any());
		}

		public override string Description => "any uncompleted BizTalk service instance.";

		#endregion
	}
}
