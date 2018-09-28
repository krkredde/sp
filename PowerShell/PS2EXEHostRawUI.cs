using System;
using System.Management.Automation.Host;
using System.Runtime.InteropServices;

namespace ik.PowerShell
{
	internal class PS2EXEHostRawUI : PSHostRawUserInterface
	{
		private const int STD_OUTPUT_HANDLE = -11;

		public override ConsoleColor BackgroundColor
		{
			get
			{
				return Console.BackgroundColor;
			}
			set
			{
				Console.BackgroundColor = value;
			}
		}

		public override Size BufferSize
		{
			get
			{
				return new Size(Console.BufferWidth, Console.BufferHeight);
			}
			set
			{
				Console.BufferWidth = value.Width;
				Console.BufferHeight = value.Height;
			}
		}

		public override Coordinates CursorPosition
		{
			get
			{
				return new Coordinates(Console.CursorLeft, Console.CursorTop);
			}
			set
			{
				Console.CursorTop = value.Y;
				Console.CursorLeft = value.X;
			}
		}

		public override int CursorSize
		{
			get
			{
				return Console.CursorSize;
			}
			set
			{
				Console.CursorSize = value;
			}
		}

		public override ConsoleColor ForegroundColor
		{
			get
			{
				return Console.ForegroundColor;
			}
			set
			{
				Console.ForegroundColor = value;
			}
		}

		public override bool KeyAvailable
		{
			get
			{
				return Console.KeyAvailable;
			}
		}

		public override Size MaxPhysicalWindowSize
		{
			get
			{
				return new Size(Console.LargestWindowWidth, Console.LargestWindowHeight);
			}
		}

		public override Size MaxWindowSize
		{
			get
			{
				return new Size(Console.BufferWidth, Console.BufferWidth);
			}
		}

		public override Coordinates WindowPosition
		{
			get
			{
				Coordinates coordinate = new Coordinates()
				{
					X = Console.WindowLeft,
					Y = Console.WindowTop
				};
				return coordinate;
			}
			set
			{
				Console.WindowLeft = value.X;
				Console.WindowTop = value.Y;
			}
		}

		public override Size WindowSize
		{
			get
			{
				Size size = new Size()
				{
					Height = Console.WindowHeight,
					Width = Console.WindowWidth
				};
				return size;
			}
			set
			{
				Console.WindowWidth = value.Width;
				Console.WindowHeight = value.Height;
			}
		}

		public override string WindowTitle
		{
			get
			{
				return Console.Title;
			}
			set
			{
				Console.Title = value;
			}
		}

		public PS2EXEHostRawUI()
		{
		}

		public override void FlushInputBuffer()
		{
		}

		public override BufferCell[,] GetBufferContents(Rectangle rectangle)
		{
			IntPtr stdHandle = PS2EXEHostRawUI.GetStdHandle(-11);
			PS2EXEHostRawUI.CHAR_INFO[,] cHARINFOArray = new PS2EXEHostRawUI.CHAR_INFO[rectangle.Bottom - rectangle.Top + 1, rectangle.Right - rectangle.Left + 1];
			PS2EXEHostRawUI.COORD cOORD = new PS2EXEHostRawUI.COORD()
			{
				X = (short)(rectangle.Right - rectangle.Left + 1),
				Y = (short)(rectangle.Bottom - rectangle.Top + 1)
			};
			PS2EXEHostRawUI.COORD cOORD1 = cOORD;
			PS2EXEHostRawUI.COORD cOORD2 = new PS2EXEHostRawUI.COORD()
			{
				X = 0,
				Y = 0
			};
			PS2EXEHostRawUI.COORD cOORD3 = cOORD2;
			PS2EXEHostRawUI.SMALL_RECT sMALLRECT = new PS2EXEHostRawUI.SMALL_RECT()
			{
				Left = (short)rectangle.Left,
				Top = (short)rectangle.Top,
				Right = (short)rectangle.Right,
				Bottom = (short)rectangle.Bottom
			};
			PS2EXEHostRawUI.SMALL_RECT sMALLRECT1 = sMALLRECT;
			PS2EXEHostRawUI.ReadConsoleOutput(stdHandle, cHARINFOArray, cOORD1, cOORD3, ref sMALLRECT1);
			BufferCell[,] bufferCell = new BufferCell[rectangle.Bottom - rectangle.Top + 1, rectangle.Right - rectangle.Left + 1];
			for (int i = 0; i <= rectangle.Bottom - rectangle.Top; i++)
			{
				for (int j = 0; j <= rectangle.Right - rectangle.Left; j++)
				{
					bufferCell[i, j] = new BufferCell(cHARINFOArray[i, j].AsciiChar, (ConsoleColor)(cHARINFOArray[i, j].Attributes & 15), (ConsoleColor)((cHARINFOArray[i, j].Attributes & 240) / 16), BufferCellType.Complete);
				}
			}
			return bufferCell;
		}

		[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
		private static extern IntPtr GetStdHandle(int nStdHandle);

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, EntryPoint="ReadConsoleOutputW", ExactSpelling=false, SetLastError=true)]
		internal static extern bool ReadConsoleOutput(IntPtr hConsoleOutput, [Out] PS2EXEHostRawUI.CHAR_INFO[,] lpBuffer, PS2EXEHostRawUI.COORD dwBufferSize, PS2EXEHostRawUI.COORD dwBufferCoord, ref PS2EXEHostRawUI.SMALL_RECT lpReadRegion);

		public override KeyInfo ReadKey(ReadKeyOptions options)
		{
			ConsoleKeyInfo consoleKeyInfo = Console.ReadKey((int)(options & ReadKeyOptions.NoEcho) != 0);
			ControlKeyStates controlKeyState = (ControlKeyStates)0;
			if ((int)(consoleKeyInfo.Modifiers & ConsoleModifiers.Alt) != 0)
			{
				controlKeyState = controlKeyState | ControlKeyStates.RightAltPressed | ControlKeyStates.LeftAltPressed;
			}
			if ((int)(consoleKeyInfo.Modifiers & ConsoleModifiers.Control) != 0)
			{
				controlKeyState = controlKeyState | ControlKeyStates.RightCtrlPressed | ControlKeyStates.LeftCtrlPressed;
			}
			if ((int)(consoleKeyInfo.Modifiers & ConsoleModifiers.Shift) != 0)
			{
				controlKeyState |= ControlKeyStates.ShiftPressed;
			}
			if (Console.CapsLock)
			{
				controlKeyState |= ControlKeyStates.CapsLockOn;
			}
			if (Console.NumberLock)
			{
				controlKeyState |= ControlKeyStates.NumLockOn;
			}
			return new KeyInfo((int)consoleKeyInfo.Key, consoleKeyInfo.KeyChar, controlKeyState, (int)(options & ReadKeyOptions.IncludeKeyDown) != 0);
		}

		public override void ScrollBufferContents(Rectangle source, Coordinates destination, Rectangle clip, BufferCell fill)
		{
			if (source.Left > clip.Right || source.Right < clip.Left || source.Top > clip.Bottom || source.Bottom < clip.Top)
			{
				return;
			}
			IntPtr stdHandle = PS2EXEHostRawUI.GetStdHandle(-11);
			PS2EXEHostRawUI.SMALL_RECT sMALLRECT = new PS2EXEHostRawUI.SMALL_RECT()
			{
				Left = (short)source.Left,
				Top = (short)source.Top,
				Right = (short)source.Right,
				Bottom = (short)source.Bottom
			};
			PS2EXEHostRawUI.SMALL_RECT sMALLRECT1 = sMALLRECT;
			PS2EXEHostRawUI.SMALL_RECT sMALLRECT2 = new PS2EXEHostRawUI.SMALL_RECT()
			{
				Left = (short)clip.Left,
				Top = (short)clip.Top,
				Right = (short)clip.Right,
				Bottom = (short)clip.Bottom
			};
			PS2EXEHostRawUI.SMALL_RECT sMALLRECT3 = sMALLRECT2;
			PS2EXEHostRawUI.COORD cOORD = new PS2EXEHostRawUI.COORD()
			{
				X = (short)destination.X,
				Y = (short)destination.Y
			};
			PS2EXEHostRawUI.COORD cOORD1 = cOORD;
			PS2EXEHostRawUI.CHAR_INFO cHARINFO = new PS2EXEHostRawUI.CHAR_INFO()
			{
				AsciiChar = fill.Character,
				Attributes = (ushort)((int)fill.ForegroundColor + (int)fill.BackgroundColor * 16)
			};
			PS2EXEHostRawUI.CHAR_INFO cHARINFO1 = cHARINFO;
			PS2EXEHostRawUI.ScrollConsoleScreenBuffer(stdHandle, ref sMALLRECT1, ref sMALLRECT3, cOORD1, ref cHARINFO1);
		}

		[DllImport("kernel32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
		private static extern bool ScrollConsoleScreenBuffer(IntPtr hConsoleOutput, [In] ref PS2EXEHostRawUI.SMALL_RECT lpScrollRectangle, [In] ref PS2EXEHostRawUI.SMALL_RECT lpClipRectangle, PS2EXEHostRawUI.COORD dwDestinationOrigin, [In] ref PS2EXEHostRawUI.CHAR_INFO lpFill);

		public override void SetBufferContents(Rectangle rectangle, BufferCell fill)
		{
			if (rectangle.Left < 0)
			{
				int width = this.BufferSize.Width;
				int height = this.BufferSize.Height;
				int num = this.BufferSize.Width;
				Size bufferSize = this.BufferSize;
				Console.MoveBufferArea(0, 0, width, height, num, bufferSize.Height, fill.Character, fill.ForegroundColor, fill.BackgroundColor);
				return;
			}
			int left = rectangle.Left;
			int top = rectangle.Top;
			int width1 = this.BufferSize.Width;
			Size size = this.BufferSize;
			Console.MoveBufferArea(left, top, rectangle.Right - rectangle.Left + 1, rectangle.Bottom - rectangle.Top + 1, width1, size.Height, fill.Character, fill.ForegroundColor, fill.BackgroundColor);
		}

		public override void SetBufferContents(Coordinates origin, BufferCell[,] contents)
		{
			IntPtr stdHandle = PS2EXEHostRawUI.GetStdHandle(-11);
			PS2EXEHostRawUI.CHAR_INFO[,] cHARINFOArray = new PS2EXEHostRawUI.CHAR_INFO[contents.GetLength(0), contents.GetLength(1)];
			PS2EXEHostRawUI.COORD cOORD = new PS2EXEHostRawUI.COORD()
			{
				X = (short)contents.GetLength(1),
				Y = (short)contents.GetLength(0)
			};
			PS2EXEHostRawUI.COORD cOORD1 = cOORD;
			PS2EXEHostRawUI.COORD cOORD2 = new PS2EXEHostRawUI.COORD()
			{
				X = 0,
				Y = 0
			};
			PS2EXEHostRawUI.COORD cOORD3 = cOORD2;
			PS2EXEHostRawUI.SMALL_RECT sMALLRECT = new PS2EXEHostRawUI.SMALL_RECT()
			{
				Left = (short)origin.X,
				Top = (short)origin.Y,
				Right = (short)(origin.X + contents.GetLength(1) - 1),
				Bottom = (short)(origin.Y + contents.GetLength(0) - 1)
			};
			PS2EXEHostRawUI.SMALL_RECT sMALLRECT1 = sMALLRECT;
			for (int i = 0; i < contents.GetLength(0); i++)
			{
				for (int j = 0; j < contents.GetLength(1); j++)
				{
					PS2EXEHostRawUI.CHAR_INFO cHARINFO = new PS2EXEHostRawUI.CHAR_INFO()
					{
						AsciiChar = contents[i, j].Character,
						Attributes = (ushort)((int)contents[i, j].ForegroundColor + (int)contents[i, j].BackgroundColor * 16)
					};
					cHARINFOArray[i, j] = cHARINFO;
				}
			}
			PS2EXEHostRawUI.WriteConsoleOutput(stdHandle, cHARINFOArray, cOORD1, cOORD3, ref sMALLRECT1);
		}

		[DllImport("kernel32.dll", CharSet=CharSet.Unicode, EntryPoint="WriteConsoleOutputW", ExactSpelling=false, SetLastError=true)]
		internal static extern bool WriteConsoleOutput(IntPtr hConsoleOutput, [In] PS2EXEHostRawUI.CHAR_INFO[,] lpBuffer, PS2EXEHostRawUI.COORD dwBufferSize, PS2EXEHostRawUI.COORD dwBufferCoord, ref PS2EXEHostRawUI.SMALL_RECT lpWriteRegion);

		[StructLayout(LayoutKind.Explicit)]
		public struct CHAR_INFO
		{
			[FieldOffset(0)]
			internal char UnicodeChar;

			[FieldOffset(0)]
			internal char AsciiChar;

			[FieldOffset(2)]
			internal ushort Attributes;
		}

		public struct COORD
		{
			public short X;

			public short Y;
		}

		public struct SMALL_RECT
		{
			public short Left;

			public short Top;

			public short Right;

			public short Bottom;
		}
	}
}