using System;
using System.Globalization;
using System.Management.Automation.Host;
using System.Threading;

namespace ik.PowerShell
{
	internal class PS2EXEHost : PSHost
	{
		private PS2EXEApp parent;

		private PS2EXEHostUI ui;

		private CultureInfo originalCultureInfo = Thread.CurrentThread.CurrentCulture;

		private CultureInfo originalUICultureInfo = Thread.CurrentThread.CurrentUICulture;

		private Guid myId = Guid.NewGuid();

		public override CultureInfo CurrentCulture
		{
			get
			{
				return this.originalCultureInfo;
			}
		}

		public override CultureInfo CurrentUICulture
		{
			get
			{
				return this.originalUICultureInfo;
			}
		}

		public override Guid InstanceId
		{
			get
			{
				return this.myId;
			}
		}

		public override string Name
		{
			get
			{
				return "PS2EXE_Host";
			}
		}

		public override PSHostUserInterface UI
		{
			get
			{
				return this.ui;
			}
		}

		public override System.Version Version
		{
			get
			{
				return new System.Version(0, 5, 0, 9);
			}
		}

		public PS2EXEHost(PS2EXEApp app, PS2EXEHostUI ui)
		{
			this.parent = app;
			this.ui = ui;
		}

		public override void EnterNestedPrompt()
		{
		}

		public override void ExitNestedPrompt()
		{
		}

		public override void NotifyBeginApplication()
		{
		}

		public override void NotifyEndApplication()
		{
		}

		public override void SetShouldExit(int exitCode)
		{
			this.parent.ShouldExit = true;
			this.parent.ExitCode = exitCode;
		}
	}
}