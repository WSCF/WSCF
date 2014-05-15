using System;
using Microsoft.Win32;

namespace Thinktecture.Tools.Web.Services.Wscf.Environment
{
	public class RegistryHelper
	{
		private bool showError = false;
		public bool ShowError
		{
			get { return showError; }
			set	{ showError = value; }
		}

		private string subKey = "";
		public string SubKey
		{
			get { return subKey; }
			set	{ subKey = value; }
		}

		private RegistryKey baseRegistryKey = Registry.LocalMachine;
		public RegistryKey BaseRegistryKey
		{
			get { return baseRegistryKey; }
			set	{ baseRegistryKey = value; }
		}

		public string ReadKey(string KeyName)
		{
			RegistryKey rk = baseRegistryKey ;
			RegistryKey sk1 = rk.OpenSubKey(subKey);

			if ( sk1 == null )
			{
				return null;
			}
			else
			{
				try 
				{
					return (string)sk1.GetValue(KeyName.ToUpper());
				}
				catch (Exception)
				{
					return null;
				}
			}
		}	

		public bool WriteKey(string KeyName, object Value)
		{
			try
			{
				RegistryKey rk = baseRegistryKey ;
				RegistryKey sk1 = rk.CreateSubKey(subKey);
				sk1.SetValue(KeyName.ToUpper(), Value);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool DeleteKey(string KeyName)
		{
			try
			{
				RegistryKey rk = baseRegistryKey ;
				RegistryKey sk1 = rk.CreateSubKey(subKey);

				if ( sk1 == null )
					return true;
				else
					sk1.DeleteValue(KeyName);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		public bool DeleteSubKeyTree()
		{
			try
			{
				RegistryKey rk = baseRegistryKey ;
				RegistryKey sk1 = rk.OpenSubKey(subKey);

				if ( sk1 != null )
					rk.DeleteSubKeyTree(subKey);

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}