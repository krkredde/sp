using System;

namespace ik.PowerShell
{
	internal interface PS2EXEApp
	{
		int ExitCode
		{
			get;
			set;
		}

		bool ShouldExit
		{
			get;
			set;
		}
	}
}