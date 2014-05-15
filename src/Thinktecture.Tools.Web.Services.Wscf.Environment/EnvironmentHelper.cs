using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Thinktecture.Tools.Web.Services.Wscf.Environment
{
	/// <summary>
	/// Summary description for EnvironmentHelper.
	/// </summary>
	public class EnvironmentHelper
	{
		public const int HWND_BROADCAST = 0xffff;
		public const int WM_SETTINGCHANGE = 0x001A;
		public const int SMTO_NORMAL = 0x0000;
		public const int SMTO_BLOCK = 0x0001;
		public const int SMTO_ABORTIFHUNG = 0x0002;
		public const int SMTO_NOTIMEOUTIFNOTHUNG = 0x0008;

		[DllImport("user32.dll", CharSet=CharSet.Auto, SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool
			SendMessageTimeout(
			IntPtr hWnd,
			int Msg,
			int wParam,
			string lParam,
			int fuFlags,
			int uTimeout,
			out int lpdwResult
			);

		public static void BroadCast()
		{
			try
			{
				const int SomeTimeoutValue = 1000;
				int result;
				SendMessageTimeout( (IntPtr)HWND_BROADCAST,
					WM_SETTINGCHANGE,0,"Environment",SMTO_BLOCK | SMTO_ABORTIFHUNG |
					SMTO_NOTIMEOUTIFNOTHUNG, SomeTimeoutValue, out result);
			}
			catch (Exception ex)
			{
				Trace.WriteLine ("Could not broadcast.");
				throw new Exception("Problem brodcasting message for change of system environment settings", ex);
			}
		}
	}
}
