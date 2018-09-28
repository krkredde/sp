using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Reflection;
using System.Security;

namespace ik.PowerShell
{
	internal class PS2EXEHostUI : PSHostUserInterface
	{
		private PS2EXEHostRawUI rawUI;

		public override PSHostRawUserInterface RawUI
		{
			get
			{
				return this.rawUI;
			}
		}

		public PS2EXEHostUI()
		{
			this.rawUI = new PS2EXEHostRawUI()
			{
				ForegroundColor = Console.ForegroundColor,
				BackgroundColor = Console.BackgroundColor
			};
		}

		private SecureString getPassword()
		{
			SecureString secureString = new SecureString();
			while (true)
			{
				ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
				if (consoleKeyInfo.Key == ConsoleKey.Enter)
				{
					break;
				}
				if (consoleKeyInfo.Key != ConsoleKey.Backspace)
				{
					secureString.AppendChar(consoleKeyInfo.KeyChar);
					Console.Write("*");
				}
				else if (secureString.Length > 0)
				{
					secureString.RemoveAt(secureString.Length - 1);
					Console.Write("\b \b");
				}
			}
			Console.WriteLine();
			return secureString;
		}

		public override Dictionary<string, PSObject> Prompt(string caption, string message, Collection<FieldDescription> descriptions)
		{
			if (!string.IsNullOrEmpty(caption))
			{
				this.WriteLine(caption);
			}
			if (!string.IsNullOrEmpty(message))
			{
				this.WriteLine(message);
			}
			Dictionary<string, PSObject> strs = new Dictionary<string, PSObject>();
			foreach (FieldDescription description in descriptions)
			{
				Type type = null;
				type = (!string.IsNullOrEmpty(description.ParameterAssemblyFullName) ? Type.GetType(description.ParameterAssemblyFullName) : typeof(string));
				if (!type.IsArray)
				{
					object defaultValue = null;
					string str = null;
					try
					{
						if (type == typeof(SecureString))
						{
							if (!string.IsNullOrEmpty(description.Name))
							{
								this.Write(string.Format("{0}: ", description.Name));
							}
							defaultValue = this.ReadLineAsSecureString();
						}
						else if (type == typeof(PSCredential))
						{
							defaultValue = this.PromptForCredential("", "", "", "");
						}
						else
						{
							if (!string.IsNullOrEmpty(description.Name))
							{
								this.Write(description.Name);
							}
							if (!string.IsNullOrEmpty(description.HelpMessage))
							{
								this.Write(" (Type !? for help.)");
							}
							if (!string.IsNullOrEmpty(description.Name) || !string.IsNullOrEmpty(description.HelpMessage))
							{
								this.Write(": ");
							}
							do
							{
								str = this.ReadLine();
								if (str != "!?")
								{
									if (string.IsNullOrEmpty(str))
									{
										defaultValue = description.DefaultValue;
									}
									if (defaultValue != null)
									{
										continue;
									}
									try
									{
										defaultValue = Convert.ChangeType(str, type);
									}
									catch
									{
										this.Write("Wrong format, please repeat input: ");
										str = "!?";
									}
								}
								else
								{
									this.WriteLine(description.HelpMessage);
								}
							}
							while (str == "!?");
						}
						strs.Add(description.Name, new PSObject(defaultValue));
					}
					catch (Exception exception)
					{
						throw exception;
					}
				}
				else
				{
					Type elementType = type.GetElementType();
					char chr = '\u0060';
					Type type1 = Type.GetType(string.Concat("System.Collections.Generic.List", chr.ToString(), "1"));
					type1 = type1.MakeGenericType(new Type[] { elementType });
					ConstructorInfo constructor = type1.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.CreateInstance, null, Type.EmptyTypes, null);
					object obj1 = constructor.Invoke(null);
					int num = 0;
					string str1 = "";
					while (true)
					{
						try
						{
							if (!string.IsNullOrEmpty(description.Name))
							{
								this.Write(string.Format("{0}[{1}]: ", description.Name, num));
							}
							str1 = this.ReadLine();
							if (!string.IsNullOrEmpty(str1))
							{
								object obj2 = Convert.ChangeType(str1, elementType);
								object[] objArray = new object[] { obj2 };
								type1.InvokeMember("Add", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, obj1, objArray);
							}
							else
							{
								break;
							}
						}
						catch (Exception exception1)
						{
							throw exception1;
						}
						num++;
					}
					Array arrays = (Array)type1.InvokeMember("ToArray", BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, null, obj1, null);
					strs.Add(description.Name, new PSObject(arrays));
				}
			}
			return strs;
		}

		public override int PromptForChoice(string caption, string message, Collection<ChoiceDescription> choices, int defaultChoice)
		{
			string lower;
			int item;
			if (!string.IsNullOrEmpty(caption))
			{
				this.WriteLine(caption);
			}
			this.WriteLine(message);
			int num = 0;
			SortedList<string, int> strs = new SortedList<string, int>();
			foreach (ChoiceDescription choice in choices)
			{
				string upper = choice.Label.Substring(0, 1);
				string label = choice.Label;
				int num1 = choice.Label.IndexOf('&');
				if (num1 > -1)
				{
					upper = choice.Label.Substring(num1 + 1, 1).ToUpper();
					label = (num1 <= 0 ? choice.Label.Substring(1) : string.Concat(choice.Label.Substring(0, num1), choice.Label.Substring(num1 + 1)));
				}
				strs.Add(upper.ToLower(), num);
				if (num > 0)
				{
					this.Write("  ");
				}
				if (num != defaultChoice)
				{
					this.Write(ConsoleColor.Gray, Console.BackgroundColor, string.Format("[{0}] {1}", upper, label));
					if (!string.IsNullOrEmpty(choice.HelpMessage))
					{
						this.Write(ConsoleColor.Gray, Console.BackgroundColor, string.Format(" ({0})", choice.HelpMessage));
					}
				}
				else
				{
					this.Write(ConsoleColor.Yellow, Console.BackgroundColor, string.Format("[{0}] {1}", upper, label));
					if (!string.IsNullOrEmpty(choice.HelpMessage))
					{
						this.Write(ConsoleColor.Gray, Console.BackgroundColor, string.Format(" ({0})", choice.HelpMessage));
					}
				}
				num++;
			}
			this.Write(": ");
			try
			{
				do
				{
					lower = Console.ReadLine().ToLower();
					if (!strs.ContainsKey(lower))
					{
						continue;
					}
					item = strs[lower];
					return item;
				}
				while (!string.IsNullOrEmpty(lower));
				item = defaultChoice;
			}
			catch
			{
				return defaultChoice;
			}
			return item;
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName, PSCredentialTypes allowedCredentialTypes, PSCredentialUIOptions options)
		{
			string str;
			if (!string.IsNullOrEmpty(caption))
			{
				this.WriteLine(caption);
			}
			this.WriteLine(message);
			if (string.IsNullOrEmpty(userName) || (options & PSCredentialUIOptions.ReadOnlyUserName) == PSCredentialUIOptions.None)
			{
				this.Write("User name: ");
				str = this.ReadLine();
			}
			else
			{
				this.Write("User name: ");
				if (!string.IsNullOrEmpty(targetName))
				{
					this.Write(string.Concat(targetName, "\\"));
				}
				this.WriteLine(userName);
				str = userName;
			}
			SecureString secureString = null;
			this.Write("Password: ");
			secureString = this.ReadLineAsSecureString();
			if (string.IsNullOrEmpty(str))
			{
				str = "<NOUSER>";
			}
			if (!string.IsNullOrEmpty(targetName) && str.IndexOf('\\') < 0)
			{
				str = string.Concat(targetName, "\\", str);
			}
			return new PSCredential(str, secureString);
		}

		public override PSCredential PromptForCredential(string caption, string message, string userName, string targetName)
		{
			string str;
			if (!string.IsNullOrEmpty(caption))
			{
				this.WriteLine(caption);
			}
			this.WriteLine(message);
			if (!string.IsNullOrEmpty(userName))
			{
				this.Write("User name: ");
				if (!string.IsNullOrEmpty(targetName))
				{
					this.Write(string.Concat(targetName, "\\"));
				}
				this.WriteLine(userName);
				str = userName;
			}
			else
			{
				this.Write("User name: ");
				str = this.ReadLine();
			}
			SecureString secureString = null;
			this.Write("Password: ");
			secureString = this.ReadLineAsSecureString();
			if (string.IsNullOrEmpty(str))
			{
				str = "<NOUSER>";
			}
			if (!string.IsNullOrEmpty(targetName) && str.IndexOf('\\') < 0)
			{
				str = string.Concat(targetName, "\\", str);
			}
			return new PSCredential(str, secureString);
		}

		public override string ReadLine()
		{
			return Console.ReadLine();
		}

		public override SecureString ReadLineAsSecureString()
		{
			SecureString secureString = new SecureString();
			return this.getPassword();
		}

		public override void Write(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			ConsoleColor consoleColor = Console.ForegroundColor;
			ConsoleColor consoleColor1 = Console.BackgroundColor;
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
			Console.Write(value);
			Console.ForegroundColor = consoleColor;
			Console.BackgroundColor = consoleColor1;
		}

		public override void Write(string value)
		{
			Console.Write(value);
		}

		public override void WriteDebugLine(string message)
		{
			ConsoleColor foregroundColor = Console.ForegroundColor;
			ConsoleColor backgroundColor = Console.BackgroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.WriteLine(string.Format("DEBUG: {0}", message));
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
		}

		public override void WriteErrorLine(string value)
		{
			ConsoleColor foregroundColor = Console.ForegroundColor;
			ConsoleColor backgroundColor = Console.BackgroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.WriteLine(string.Format("ERROR: {0}", value));
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
		}

		public override void WriteLine()
		{
			Console.WriteLine();
		}

		public override void WriteLine(ConsoleColor foregroundColor, ConsoleColor backgroundColor, string value)
		{
			ConsoleColor consoleColor = Console.ForegroundColor;
			ConsoleColor consoleColor1 = Console.BackgroundColor;
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
			Console.WriteLine(value);
			Console.ForegroundColor = consoleColor;
			Console.BackgroundColor = consoleColor1;
		}

		public override void WriteLine(string value)
		{
			Console.WriteLine(value);
		}

		public override void WriteProgress(long sourceId, ProgressRecord record)
		{
		}

		public override void WriteVerboseLine(string message)
		{
			ConsoleColor foregroundColor = Console.ForegroundColor;
			ConsoleColor backgroundColor = Console.BackgroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.WriteLine(string.Format("VERBOSE: {0}", message));
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
		}

		public override void WriteWarningLine(string message)
		{
			ConsoleColor foregroundColor = Console.ForegroundColor;
			ConsoleColor backgroundColor = Console.BackgroundColor;
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.WriteLine(string.Format("WARNING: {0}", message));
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
		}
	}
}