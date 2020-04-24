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
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Be.Stateless.BizTalk.Activity.Monitoring.Model;
using NUnit.Framework;

namespace Be.Stateless.BizTalk.Unit
{
	[SuppressMessage("ReSharper", "UnusedType.Global")]
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
	public static class ProcessExtensions
	{
		public static MessagingStep SingleMessagingStep(this Activity.Monitoring.Model.Process process, Func<MessagingStep, bool> predicate)
		{
			return process.SingleMessagingStep(predicate, TrackingRepository.Timeout);
		}

		public static MessagingStep SingleMessagingStep(this Activity.Monitoring.Model.Process process, Func<MessagingStep, bool> predicate, TimeSpan timeout)
		{
			Assert.That(
				() => {
					using (var searchContext = new ActivityContext())
					{
						return searchContext.Processes.Single(p => p.ActivityID == process.ActivityID).MessagingSteps.Where(predicate).SingleOrDefault();
					}
				},
				Is.Not.Null.After((int) timeout.TotalMilliseconds, (int) TrackingRepository.PollingInterval.TotalMilliseconds));

			TrackingRepository.Refresh(process, p => p.MessagingSteps);
			return process.MessagingSteps.Where(predicate).Single();
		}

		public static ProcessingStep SingleProcessingStep(this Activity.Monitoring.Model.Process process, Func<ProcessingStep, bool> predicate)
		{
			return process.SingleProcessingStep(predicate, TrackingRepository.Timeout);
		}

		public static ProcessingStep SingleProcessingStep(this Activity.Monitoring.Model.Process process, Func<ProcessingStep, bool> predicate, TimeSpan timeout)
		{
			Assert.That(
				() => {
					using (var searchContext = new ActivityContext())
					{
						return searchContext.Processes.Single(p => p.ActivityID == process.ActivityID).ProcessingSteps.Where(predicate).SingleOrDefault();
					}
				},
				Is.Not.Null.After((int) timeout.TotalMilliseconds, (int) TrackingRepository.PollingInterval.TotalMilliseconds));

			TrackingRepository.Refresh(process, p => p.ProcessingSteps);
			return process.ProcessingSteps.Where(predicate).Single();
		}
	}
}
