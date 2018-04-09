﻿using System.Text;
using System.Text.RegularExpressions;
using DynamicPowerShellApi.Exceptions;
using DynamicPowerShellApi.Model;

namespace DynamicPowerShellApi
{
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.IO;
	using System.Linq;
	using System.Management.Automation;
	using System.Management.Automation.Runspaces;
	using System.Threading.Tasks;

	/// <summary>
	/// The PowerShell runner.
	/// </summary>
	public class PowershellRunner : IRunner
	{
		/// <summary>	The asynchronous execution method. </summary>
		/// <remarks>	Anthony, 5/27/2015. </remarks>
		/// <exception cref="ArgumentException">		   	Thrown when one or more arguments have
		/// 												unsupported or illegal values. </exception>
		/// <exception cref="ArgumentNullException">	   	Thrown when one or more required arguments
		/// 												are null. </exception>
		/// <exception cref="PSSnapInException">		   	. </exception>
		/// <exception cref="PowerShellExecutionException">	Thrown when a Power Shell Execution error
		/// 												condition occurs. </exception>
		/// <param name="filename">		 	The filename. </param>
		/// <param name="snapin">		 	The snap in. </param>
		/// <param name="module">		 	The module. </param>
		/// <param name="parametersList">	The parameters List. </param>
		/// <param name="asJob">		 	Run this command as a job. </param>
		/// <returns>	The <see cref="Task"/>. </returns>
		public Task<PowershellReturn> ExecuteAsync(
			string filename, 
			string snapin, 
			string module, 
			IList<KeyValuePair<string, object>> parametersList,
			bool asJob)
		{
			if (string.IsNullOrWhiteSpace(filename))
				throw new ArgumentException("Argument cannot be null, empty or composed of whitespaces only", "filename");
			if (parametersList == null)
				throw new ArgumentNullException("parametersList", "Argument cannot be null");

			// Raise an event so we know what is going on
			try
			{
				var sb = new StringBuilder();

				foreach (KeyValuePair<string, object> kvp in parametersList)
				{
					if (sb.Length > 0)
						sb.Append(";");

					sb.Append(string.Format("{0}:{1}", kvp.Key, kvp.Value));
				}

				DynamicPowershellApiEvents
					.Raise
					.ExecutingPowerShellScript(filename, sb.ToString());
			}
			catch (Exception)
			{
				DynamicPowershellApiEvents
					.Raise
					.ExecutingPowerShellScript(filename, "Unknown");
			}
			
			try
			{
				string strBaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
				//string scriptContent = File.ReadAllText(Path.Combine(strBaseDirectory, Path.Combine("ScriptRepository", filename)));

				RunspaceConfiguration rsConfig = RunspaceConfiguration.Create();

				if (!String.IsNullOrWhiteSpace(snapin))
				{
					PSSnapInException snapInException;
					rsConfig.AddPSSnapIn(snapin, out snapInException);
					if (snapInException != null)
					{
						DynamicPowershellApiEvents
							.Raise
							.SnapinException(snapInException.Message);
						throw snapInException;
					}
				}

				InitialSessionState initialSession = InitialSessionState.CreateDefault2();
				if (!String.IsNullOrWhiteSpace(module))
				{
					DynamicPowershellApiEvents
						.Raise
						.LoadingModule(module);
					initialSession.ImportPSModule(new[] { module });
				}

                using (PowerShell powerShellInstance = PowerShell.Create(initialSession))
                {
                    //powerShellInstance.RunspacePool = RunspaceFactory.CreateRunspacePool(initialSession); //RunspacePoolWrapper.Pool;


                    if (powerShellInstance.Runspace == null)
                    {
                        powerShellInstance.Runspace = RunspaceFactory.CreateRunspace(rsConfig);
                        powerShellInstance.Runspace.Open();
                    }


                    //powerShellInstance.AddCommand("Set-Location").AddParameter("LiteralPath", Path.GetDirectoryName(filename);
                    //powerShellInstance.AddStatement();

                    //ps.AddCommand("Import-Module", true).AddParameter("Name", module);
                    //ps.AddStatement();

                    //powerShellInstance.AddScript(scriptContent);

                    powerShellInstance.AddCommand(filename, true);

                    foreach (var item in parametersList)
                        powerShellInstance.AddParameter(item.Key, item.Value);

                    //TO DO : take charge other output format (CSV, ...)
                    //https://github.com/DataBooster/PS-WebApi/blob/d0e38e9c06d0c21de4b64237dd61ad9e6a040460/src/DataBooster.PSWebApi/PSConfiguration.cs
                    powerShellInstance.AddCommand("ConvertTo-Json").AddParameter("Compress");

                    // invoke execution on the pipeline (collecting output)
                    Collection<PSObject> psOutput = powerShellInstance.Invoke();

                    string sMessage = psOutput == null
                        ? String.Empty
                        : (
                            psOutput.LastOrDefault() != null
                            ? Regex.Replace(psOutput.LastOrDefault().ToString(), @"[^\u0000-\u007F]", string.Empty)
                                : String.Empty);

                    DynamicPowershellApiEvents.Raise.PowerShellScriptFinalised("The powershell has completed - anlaysing results now");

                    // check the other output streams (for example, the error stream)
                    if (powerShellInstance.HadErrors && powerShellInstance.Streams.Error.Count > 0)
                    {
                        var runtimeErrors = new List<PowerShellException>();

                        // Create a string builder for the errors
                        StringBuilder sb = new StringBuilder();

                        // error records were written to the error stream.
                        // do something with the items found.
                        sb.Append("PowerShell script raised errors:" + Environment.NewLine);
                        sb.Append(String.Format("{0}", sMessage));

                        var errors = powerShellInstance.Streams.Error.ReadAll();
                        if (errors != null)
                        {
                            foreach (var error in errors)
                            {
                                if (error.ErrorDetails == null)
                                    DynamicPowershellApiEvents.Raise.UnhandledException("error.ErrorDetails is null");

                                string errorDetails = error.ErrorDetails != null ? error.ErrorDetails.Message : error.Exception.Message ?? String.Empty;
                                string scriptStack = error.ScriptStackTrace ?? String.Empty;
                                string commandPath = error.InvocationInfo.PSCommandPath ?? String.Empty;
                                ErrorCategory category = error.CategoryInfo.Category;

                                
                                if (error.CategoryInfo.Category == ErrorCategory.PermissionDenied ||
                                    error.CategoryInfo.Category == ErrorCategory.ObjectNotFound   ||
                                    error.CategoryInfo.Category == ErrorCategory.InvalidArgument)
                                {
                                    throw new PowerShellClientException(errorDetails, error.CategoryInfo.Category);
                                }


                                if (error.ScriptStackTrace == null)
                                    DynamicPowershellApiEvents.Raise.UnhandledException("error.ScriptStackTrace is null");

                                if (error.InvocationInfo == null)
                                    DynamicPowershellApiEvents.Raise.UnhandledException("error.InvocationInfo is null");
                                else
                                {
                                    if (error.InvocationInfo.PSCommandPath == null)
                                        DynamicPowershellApiEvents.Raise.UnhandledException("error.InvocationInfo.PSCommandPath is null");
                                }

                                if (error.Exception == null)
                                    DynamicPowershellApiEvents.Raise.UnhandledException("error.Exception is null");

                                DynamicPowershellApiEvents.Raise.PowerShellError(
                                    category.ToString(),
                                    errorDetails,
                                    scriptStack,
                                    commandPath,
                                    error.InvocationInfo.ScriptLineNumber);

                                runtimeErrors.Add(new PowerShellException
                                {
                                    Category = category,
                                    StackTrace = scriptStack,
                                    ErrorMessage = errorDetails,
                                    LineNumber = error.InvocationInfo != null ? error.InvocationInfo.ScriptLineNumber : 0,
                                    ScriptName = filename
                                });

                                if (error.Exception != null)
                                {
                                    sb.Append(String.Format("PowerShell Exception {0} : {1}", error.Exception.Message, error.Exception.StackTrace));
                                }

                                sb.Append(String.Format("Error {0}", error.ScriptStackTrace));
                            }
                        }
                        else
                        {
                            sb.Append(sMessage);
                        }

                        DynamicPowershellApiEvents.Raise.PowerShellScriptFinalised(String.Format("An error was rasied {0}", sb));

                        throw new PowerShellExecutionException(sb.ToString())
                        {
                            Exceptions = runtimeErrors,
                            LogTime = DateTime.Now
                        };
                    }

                    var psGood = new PowershellReturn
                    {
                        PowerShellReturnedValidData = true,
                        ActualPowerShellData = sMessage
                    };

                    DynamicPowershellApiEvents.Raise.PowerShellScriptFinalised(String.Format("The powershell returned the following {0}", psGood.ActualPowerShellData));

                    return Task.FromResult(psGood);
                }
			}
			catch (Exception runnerException)
			{
				if (runnerException.GetType() == typeof(PowerShellExecutionException))
					throw;

                if (runnerException.GetType() == typeof(PowerShellClientException))
                    throw;

                DynamicPowershellApiEvents.Raise.UnhandledException(runnerException.Message, runnerException.StackTrace);
				throw new PowerShellExecutionException(runnerException.Message)
				{
					Exceptions = new List<PowerShellException>
					{
						new PowerShellException
						{
							ErrorMessage = runnerException.Message,
							LineNumber = 0,
							ScriptName = "PowerShellRunner.cs",
							StackTrace = runnerException.StackTrace
						}
					},
					LogTime = DateTime.Now
				};
			}
		}

		/// <summary>	Gets a job. </summary>
		/// <remarks>	Anthony, 5/29/2015. </remarks>
		/// <param name="jobId">	Identifier for the job. </param>
		/// <returns>	The job. </returns>
		public Task<PowershellReturn> GetJob(Guid jobId)
		{
			RunspaceConfiguration rsConfig = RunspaceConfiguration.Create();

			InitialSessionState initialSession = InitialSessionState.Create();

			using (PowerShell powerShellInstance = PowerShell.Create(initialSession))
			{
				powerShellInstance.RunspacePool = RunspacePoolWrapper.Pool;
				if (powerShellInstance.Runspace == null)
				{
					powerShellInstance.Runspace = RunspaceFactory.CreateRunspace(rsConfig);
					powerShellInstance.Runspace.Open();
				}

				ICollection<PSJobProxy> jobProxyCollection = PSJobProxy.Create(powerShellInstance.Runspace);

				var proxy = jobProxyCollection.First();

				return Task.FromResult(
					new PowershellReturn
					{
						PowerShellReturnedValidData = true,
						ActualPowerShellData = proxy.Output.LastOrDefault().ToString()
					}
					);
			}
		}
	}
}