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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using Be.Stateless.BizTalk.Management;
using Be.Stateless.BizTalk.Operations.Extensions;
using Be.Stateless.Linq.Extensions;
using log4net;
using Microsoft.BizTalk.Operations;

namespace Be.Stateless.BizTalk.Unit.Process
{
	[SuppressMessage("ReSharper", "UnusedMember.Global")]
	[SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
	public abstract class ProcessFixtureBase
	{
		protected virtual IEnumerable<Type> AllDependantOrchestrationTypes => DependantOrchestrationTypes;

		protected IEnumerable<MessageBoxServiceInstance> BizTalkServiceInstances => BizTalkOperationsExtensions.GetRunningOrSuspendedServiceInstances();

		/// <summary>
		/// Ordered list of orchestration types that this process depends upon.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Dependant orchestrations will be started before any test method of this process is run; notice that they will be
		/// started in the order in which they were given. Likewise, dependant orchestrations will be unenlisted after all of the
		/// test methods of this process has run; notice that they will be unenlisted in the reverse order in which they were
		/// given.
		/// </para>
		/// <para>
		/// Any service instance of any one of the dependant orchestrations will be terminated after each test method of this
		/// process has run should the service instance be suspended or still be running.
		/// </para>
		/// </remarks>
		protected virtual IEnumerable<Type> DependantOrchestrationTypes => Enumerable.Empty<Type>();

		/// <summary>
		/// Input folders where input files are dropped.
		/// </summary>
		/// <remarks>
		/// <para>
		/// All folders, whether <see cref="SystemInputFolders"/>, <see cref="SystemOutputFolders"/>, <see cref="InputFolders"/>,
		/// or <see cref="OutputFolders"/> will always be created if necessary. Besides all output folders, that is <see
		/// cref="SystemOutputFolders"/> and <see cref="OutputFolders"/>, will be emptied immediately before each test is run,
		/// while all input folders, that is <see cref="SystemInputFolders"/> and <see cref="InputFolders"/>, will be emptied
		/// immediately after each test has run.
		/// </para>
		/// <para>
		/// It is not required that an override call its base class overridden <see cref="InputFolders"/> member; that is why the
		/// <see cref="SystemInputFolders"/> has been made available in the first place: to limit the burden on the unit test
		/// developers.
		/// </para>
		/// </remarks>
		protected virtual IEnumerable<string> InputFolders => Enumerable.Empty<string>();

		/// <summary>
		/// Output folders where output files are dropped.
		/// </summary>
		/// <remarks>
		/// <para>
		/// All folders, whether <see cref="SystemInputFolders"/>, <see cref="SystemOutputFolders"/>, <see cref="InputFolders"/>,
		/// or <see cref="OutputFolders"/> will always be created if necessary. Besides all output folders, that is <see
		/// cref="SystemOutputFolders"/> and <see cref="OutputFolders"/>, will be emptied immediately before each test is run,
		/// while all input folders, that is <see cref="SystemInputFolders"/> and <see cref="InputFolders"/>, will be emptied
		/// immediately after each test has run.
		/// </para>
		/// <para>
		/// It is not required that an override call its base class overridden <see cref="OutputFolders"/> member; that is why
		/// the <see cref="SystemOutputFolders"/> has been made available in the first place: to limit the burden on the unit
		/// test developers.
		/// </para>
		/// </remarks>
		protected virtual IEnumerable<string> OutputFolders => Enumerable.Empty<string>();

		[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
		protected DateTime StartTime { get; private set; }

		/// <summary>
		/// Input system (e.g. Claim Check) folders where input files are dropped.
		/// </summary>
		/// <remarks>
		/// <para>
		/// All folders, whether <see cref="SystemInputFolders"/>, <see cref="SystemOutputFolders"/>, <see cref="InputFolders"/>,
		/// or <see cref="OutputFolders"/> will always be created if necessary. Besides all output folders, that is <see
		/// cref="SystemOutputFolders"/> and <see cref="OutputFolders"/>, will be emptied immediately before each test is run,
		/// while all input folders, that is <see cref="SystemInputFolders"/> and <see cref="InputFolders"/>, will be emptied
		/// immediately after each test has run.
		/// </para>
		/// <para>
		/// Any override must call its base class overridden <see cref="SystemInputFolders"/> member so that not any system
		/// folder is ever missing in the list.
		/// </para>
		/// </remarks>
		protected virtual IEnumerable<string> SystemInputFolders => Enumerable.Empty<string>();

		/// <summary>
		/// Output system (e.g. Claim Check) folders where output files are dropped.
		/// </summary>
		/// <remarks>
		/// <para>
		/// All folders, whether <see cref="SystemInputFolders"/>, <see cref="SystemOutputFolders"/>, <see cref="InputFolders"/>,
		/// or <see cref="OutputFolders"/> will always be created if necessary. Besides all output folders, that is <see
		/// cref="SystemOutputFolders"/> and <see cref="OutputFolders"/>, will be emptied immediately before each test is run,
		/// while all input folders, that is <see cref="SystemInputFolders"/> and <see cref="InputFolders"/>, will be emptied
		/// immediately after each test has run.
		/// </para>
		/// <para>
		/// Any override must call its base class overridden <see cref="SystemOutputFolders"/> member so that not any system
		/// folder is ever missing in the list.
		/// </para>
		/// </remarks>
		protected virtual IEnumerable<string> SystemOutputFolders => Enumerable.Empty<string>();

		private IEnumerable<string> AllFolders => AllInputFolders.Concat(AllOutputFolders);

		private IEnumerable<string> AllInputFolders => SystemInputFolders.Concat(InputFolders);

		private IEnumerable<string> AllOutputFolders => SystemOutputFolders.Concat(OutputFolders);

		[SuppressMessage("ReSharper", "InvertIf")]
		protected void InitializeFixture()
		{
			AllFolders.ForEach(
				d => {
					if (!Directory.Exists(d))
					{
						_logger.Debug($"Creating folder '{d}'.");
						Directory.CreateDirectory(d);
					}
				});
			AllDependantOrchestrationTypes.ForEach(ot => { new Orchestration(ot).EnsureStarted(); });
		}

		protected void Initialize()
		{
			_logger.Debug("Emptying output folders.");
			CleanFolders(AllOutputFolders);

			StartTime = DateTime.UtcNow;
		}

		protected void Terminate()
		{
			_logger.Debug("Terminating uncompleted BizTalk service instances.");
			TerminateUncompletedBizTalkServiceInstances();

			_logger.Debug("Emptying input folders.");
			CleanFolders(AllInputFolders);
		}

		protected void TerminateFixture()
		{
			AllDependantOrchestrationTypes.Reverse().ForEach(ot => { new Orchestration(ot).EnsureUnenlisted(); });
		}

		/// <summary>
		/// Terminate all BizTalk service instances that are still running or that have been suspended.
		/// </summary>
		[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
		protected void TerminateUncompletedBizTalkServiceInstances()
		{
			BizTalkOperationsExtensions.TerminateUncompletedBizTalkServiceInstances();
		}

		private void CleanFolders(IEnumerable<string> folders)
		{
			folders.ForEach(
				d => {
					_logger.DebugFormat("Emptying folder '{0}'.", d);
					Directory.GetFiles(d).ForEach(
						f => {
							var attempts = 0;
							while (File.Exists(f))
							{
								try
								{
									attempts++;
									_logger.DebugFormat("Attempting to delete file '{0}'.", f);
									File.Delete(f);
								}
								catch (Exception exception)
								{
									_logger.DebugFormat("Exception encountered while attempting to delete file '{0}'. {1}", f, exception);
									if (attempts == 5) throw;
									Thread.Sleep(TimeSpan.FromSeconds(1));
								}
							}
						});
					CleanFolders(Directory.GetDirectories(d));
				});
		}

		private static readonly ILog _logger = LogManager.GetLogger(typeof(ProcessFixtureBase));
	}
}
