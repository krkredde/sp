using System;
using System.Diagnostics;
using System.IO;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ik.PowerShell
{
	internal class PS2EXE : PS2EXEApp
	{
		private bool shouldExit;

		private int exitCode;

		public int ExitCode
		{
			get
			{
				return this.exitCode;
			}
			set
			{
				this.exitCode = value;
			}
		}

		public bool ShouldExit
		{
			get
			{
				return this.shouldExit;
			}
			set
			{
				this.shouldExit = value;
			}
		}

		public PS2EXE()
		{
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			throw new Exception("Unhandled exception in PS2EXE");
		}

		[DllImport("Kernel32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		public static extern PS2EXE.FileType GetFileType(UIntPtr hFile);

		[DllImport("Kernel32.dll", CharSet=CharSet.None, ExactSpelling=false)]
		public static extern UIntPtr GetStdHandle(PS2EXE.STDHandle stdHandle);

		public static bool IsInputRedirected()
		{
			PS2EXE.FileType fileType = PS2EXE.GetFileType(PS2EXE.GetStdHandle(PS2EXE.STDHandle.STD_INPUT_HANDLE));
			if (fileType != PS2EXE.FileType.FILE_TYPE_CHAR && fileType != PS2EXE.FileType.FILE_TYPE_UNKNOWN)
			{
				return true;
			}
			return false;
		}

		[STAThread]
		private static int Main(string[] args)
		{
			PS2EXE.<>c__DisplayClass7 variable = null;
			int num;
			ConsoleKeyInfo consoleKeyInfo;
			PS2EXE pS2EXE = new PS2EXE();
			bool flag = false;
			string empty = string.Empty;
			PS2EXEHostUI pS2EXEHostUI = new PS2EXEHostUI();
			PS2EXEHost pS2EXEHost = new PS2EXEHost(pS2EXE, pS2EXEHostUI);
			ManualResetEvent manualResetEvent = new ManualResetEvent(false);
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(PS2EXE.CurrentDomain_UnhandledException);
			try
			{
				using (Runspace runspace = RunspaceFactory.CreateRunspace(pS2EXEHost))
				{
					runspace.ApartmentState = ApartmentState.STA;
					runspace.Open();
					using (System.Management.Automation.PowerShell powerShell = System.Management.Automation.PowerShell.Create())
					{
						Console.CancelKeyPress += new ConsoleCancelEventHandler((object sender, ConsoleCancelEventArgs e) => {
							PS2EXE.<>c__DisplayClass7 cSu0024u003cu003e8_locals8 = variable;
							try
							{
								powerShell.BeginStop((IAsyncResult r) => {
									cSu0024u003cu003e8_locals8.mre.Set();
									e.Cancel = true;
								}, null);
							}
							catch
							{
							}
						});
						powerShell.Runspace = runspace;
						powerShell.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>((object sender, DataAddedEventArgs e) => pS2EXEHostUI.WriteErrorLine(((PSDataCollection<ErrorRecord>)sender)[e.Index].ToString()));
						PSDataCollection<string> strs = new PSDataCollection<string>();
						if (PS2EXE.IsInputRedirected())
						{
							string str = "";
							while (true)
							{
								string str1 = Console.ReadLine();
								str = str1;
								if (str1 == null)
								{
									break;
								}
								strs.Add(str);
							}
						}
						strs.Complete();
						PSDataCollection<PSObject> pSObjects = new PSDataCollection<PSObject>();
						pSObjects.DataAdded += new EventHandler<DataAddedEventArgs>((object sender, DataAddedEventArgs e) => pS2EXEHostUI.WriteLine(pSObjects[e.Index].ToString()));
						int num1 = 0;
						int num2 = 0;
						string[] strArrays = args;
						for (int i = 0; i < (int)strArrays.Length; i++)
						{
							string str2 = strArrays[i];
							if (string.Compare(str2, "-wait", true) == 0)
							{
								flag = true;
							}
							else if (str2.StartsWith("-extract", StringComparison.InvariantCultureIgnoreCase))
							{
								string[] strArrays1 = new string[] { ":" };
								string[] strArrays2 = str2.Split(strArrays1, 2, StringSplitOptions.RemoveEmptyEntries);
								if ((int)strArrays2.Length == 2)
								{
									empty = strArrays2[1].Trim(new char[] { '\"' });
								}
								else
								{
									Console.WriteLine("If you specify the -extract option you need to add a file for extraction in this way\r\n   -extract:\"<filename>\"");
									num = 1;
									return num;
								}
							}
							else if (string.Compare(str2, "-end", true) == 0)
							{
								num1 = num2 + 1;
								break;
							}
							else if (string.Compare(str2, "-debug", true) == 0)
							{
								System.Diagnostics.Debugger.Launch();
								break;
							}
							num2++;
						}
						string str3 = Encoding.UTF8.GetString(Convert.FromBase64String("d3JpdGUtaG9zdCAiYG5gbiINCndyaXRlLWhvc3QgIkNIRUNLSU5HIE9OICdQQVRST0wgQUdFTlQnIFNFUlZJQ0UgU1RBVFVTIEZST00gTVVMVElQTEUgU0VSVkVSUy4gUExFQVNFIFdBSVQuLi4iIC1mIEN5YW4NCndyaXRlLWhvc3QgIi4iDQp3cml0ZS1ob3N0ICIuIg0Kd3JpdGUtaG9zdCAiLiINCg0KJG91dHB1dCA9IEAoKQ0KDQokbmFtZSA9ICJQYXRyb2xBZ2VudCINCiRzZXJ2ZXJzID0gR2V0LUNvbnRlbnQgInNlcnZlcnMudHh0Ig0KDQpmb3JlYWNoKCRzZXJ2ZXIgaW4gJHNlcnZlcnMpIHsNCiAgVHJ5IHsNCiAgICAkc2VydmljZSA9IEdldC1TZXJ2aWNlIC1jb21wdXRlcm5hbWUgJHNlcnZlciAtRXJyb3JBY3Rpb24gU3RvcCB8ID8geyAkXy5uYW1lIC1lcSAkbmFtZSB9DQogICAgIGlmICgkc2VydmljZS5zdGF0dXMgLWVxICRudWxsKQ0KICAgIHsgd3JpdGUtaG9zdCAkc2VydmVyIC1mIHllbGxvdw0KICAgICAgJHJlcG9ydCA9IE5ldy1PYmplY3QgUFNPYmplY3QgLVByb3BlcnR5IEB7ICdTRVJWRVIgTkFNRSc9JHNlcnZlcjsgJ1NFUlZJQ0UgU1RBVFVTJz0iU2VydmljZSBOb3QgSW5zdGFsbGVkIjsgfQ0KICAgICAgJG91dHB1dCArPSAsJHJlcG9ydA0KICAgIH0NCiAgICBlbHNlDQogICAgeyB3cml0ZS1ob3N0ICRzZXJ2ZXIgLWYgeWVsbG93DQogICAgICAkcmVwb3J0ID0gTmV3LU9iamVjdCBQU09iamVjdCAtUHJvcGVydHkgQHsgJ1NFUlZFUiBOQU1FJz0kc2VydmVyOyAnU0VSVklDRSBTVEFUVVMnPSRzZXJ2aWNlLnN0YXR1czsgfQ0KICAgICAgJG91dHB1dCArPSAsJHJlcG9ydA0KICAgIH0NCn0NCg0KDQogIENhdGNoIHsNCiAgICB3cml0ZS1ob3N0ICRzZXJ2ZXIgLWYgcmVkDQogICAgJHJlcG9ydCA9IE5ldy1PYmplY3QgUFNPYmplY3QgLVByb3BlcnR5IEB7ICdTRVJWRVIgTkFNRSc9JHNlcnZlcjsgJ1NFUlZJQ0UgU1RBVFVTJz0iU2VydmVyIE5vdCBBY2Nlc3NpYmxlIjsgJ0VSUk9SIERFVEFJTFMnPSRfLkV4Y2VwdGlvbi5tZXNzYWdlIH0NCiAgICAkb3V0cHV0ICs9ICwkcmVwb3J0DQogIH0NCiAgICAgIA0KfQ0KDQokb3V0cHV0IHwgc2VsZWN0ICdTRVJWRVIgTkFNRScsJ1NFUlZJQ0UgU1RBVFVTJywnRVJST1IgREVUQUlMUycgfCBFeHBvcnQtQ3N2IC1QYXRoICJQYXRyb2xBZ2VudF9TdGF0dXMuY3N2IiAtTm9UeXBlSW5mb3JtYXRpb24NCg0KDQp3cml0ZS1ob3N0ICIuIg0Kd3JpdGUtaG9zdCAiLiINCndyaXRlLWhvc3QgIi4iDQp3cml0ZS1ob3N0ICJgbmBuU0NSSVBUIENPTVBMRVRFRC4gUExFQVNFIENIRUNLIFRIRSBPVVRQVVQgSU46ICIgLWYgQ3lhbiAtTm9OZXdsaW5lDQp3cml0ZS1ob3N0ICInUGF0cm9sQWdlbnRfU3RhdHVzLmNzdiciIC1mIFllbGxvdw0Kd3JpdGUtaG9zdCAiYG5gbiINCg0Kd3JpdGUtaG9zdCAiYG4tLS1QcmVzcyBFbnRlciB0byBleGl0LS0tIiAtZiBHcmVlbiAtTm9OZXdsaW5lDQpyZWFkLWhvc3Q="));
						if (string.IsNullOrEmpty(empty))
						{
							powerShell.AddScript(str3);
							string value = null;
							Regex regex = new Regex("^-([^: ]+)[ :]?([^:]*)$");
							for (int j = num1; j < (int)args.Length; j++)
							{
								Match match = regex.Match(args[j]);
								if (match.Success && match.Groups.Count == 3)
								{
									if (value != null)
									{
										powerShell.AddParameter(value);
									}
									if (match.Groups[2].Value.Trim() == "")
									{
										value = match.Groups[1].Value;
									}
									else if (match.Groups[2].Value == "True" || match.Groups[2].Value.ToUpper() == "$TRUE")
									{
										powerShell.AddParameter(match.Groups[1].Value, true);
										value = null;
									}
									else if (match.Groups[2].Value == "False" || match.Groups[2].Value.ToUpper() == "$FALSE")
									{
										powerShell.AddParameter(match.Groups[1].Value, false);
										value = null;
									}
									else
									{
										powerShell.AddParameter(match.Groups[1].Value, match.Groups[2].Value);
										value = null;
									}
								}
								else if (value == null)
								{
									powerShell.AddArgument(args[j]);
								}
								else
								{
									powerShell.AddParameter(value, args[j]);
									value = null;
								}
							}
							if (value != null)
							{
								powerShell.AddParameter(value);
							}
							powerShell.AddCommand("out-string");
							powerShell.AddParameter("stream");
							powerShell.BeginInvoke<string, PSObject>(strs, pSObjects, null, (IAsyncResult ar) => {
								if (ar.IsCompleted)
								{
									manualResetEvent.Set();
								}
							}, null);
							while (!pS2EXE.ShouldExit && !manualResetEvent.WaitOne(100))
							{
							}
							powerShell.Stop();
							if (powerShell.InvocationStateInfo.State == PSInvocationState.Failed)
							{
								pS2EXEHostUI.WriteErrorLine(powerShell.InvocationStateInfo.Reason.Message);
							}
						}
						else
						{
							File.WriteAllText(empty, str3);
							num = 0;
							return num;
						}
					}
					runspace.Close();
				}
				if (flag)
				{
					Console.WriteLine("Hit any key to exit...");
					consoleKeyInfo = Console.ReadKey();
				}
				return pS2EXE.ExitCode;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Console.Write("An exception occured: ");
				Console.WriteLine(exception.Message);
				if (flag)
				{
					Console.WriteLine("Hit any key to exit...");
					consoleKeyInfo = Console.ReadKey();
				}
				return pS2EXE.ExitCode;
			}
			return num;
		}

		public enum FileType : uint
		{
			FILE_TYPE_UNKNOWN = 0,
			FILE_TYPE_DISK = 1,
			FILE_TYPE_CHAR = 2,
			FILE_TYPE_PIPE = 3,
			FILE_TYPE_REMOTE = 32768
		}

		public enum STDHandle : uint
		{
			STD_ERROR_HANDLE = 4294967284,
			STD_OUTPUT_HANDLE = 4294967285,
			STD_INPUT_HANDLE = 4294967286
		}
	}
}