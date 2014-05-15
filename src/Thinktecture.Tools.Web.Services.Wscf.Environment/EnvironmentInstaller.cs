using System.Collections;
using System.ComponentModel;
using System.Configuration.Install;
using Microsoft.Win32;
using System.Security;
using System.Windows.Forms;
using System;
using System.IO;
using System.Xml;
using System.Resources;
using System.Reflection;
using System.Text;
using FxEnvironment = System.Environment;

namespace Thinktecture.Tools.Web.Services.Wscf.Environment
{
	/// <summary>
	/// Summary description for EnvironmentInstaller.
	/// </summary>
	[RunInstaller(true)]
	public class EnvironmentInstaller : Installer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private Container components = null;

        const string RegistrySubKey = @"Software\thinktecture\WSCF";
        const string Path = @"Path";
        
		public EnvironmentInstaller()
		{
			// This call is required by the Designer.
			InitializeComponent();
			// TODO: Add any initialization after the InitializeComponent call
		}

		public override void Install(IDictionary stateSaver)
		{
            base.Install(stateSaver);

			string regValue = "";

			RegistryHelper registry = new RegistryHelper();
			registry.BaseRegistryKey = Registry.LocalMachine;
			registry.SubKey += @"System\CurrentControlSet\Control\Session Manager\Environment";
			regValue = registry.ReadKey("PATH");

            string installDir = Context.Parameters["myDir"];            

			if(regValue != null)
			{
                // BDS: 13-11-2006
                // Saving the original value and restoring it during the uninstallation would
                // loose the PATH information set by other software installed after WSCF.
                // Therefore I'm saving the installation directory to our state saver and later 
                // using it to remove it from the PATH variable.
                // Consequently this line will soon become obsolete.
				stateSaver.Add("OriginalPathRegValue", regValue);                 
                stateSaver.Add("InstallDir", installDir);
				string newPathValue = regValue + ";" + installDir;
				registry.WriteKey("PATH", newPathValue);
			}
            // What happens if this value is null is not defined.

			EnvironmentHelper.BroadCast();

            // BDS: Write the installation directory into the registry. 
            // Write the .addin file to the selected folder.
            try
            {
                RegistryKey subkey = Registry.LocalMachine.CreateSubKey(
                    RegistrySubKey);

                subkey.SetValue(Path, installDir);

                // Write the .addin file.
                ResourceManager rm = new ResourceManager("Thinktecture.Tools.Web.Services.Wscf.Environment.Properties.Resources",
                Assembly.GetExecutingAssembly());
                string settings = rm.GetString("WSCF");

                string asmPath = installDir;
                
                if (!installDir.EndsWith(@"\"))
                {
                    asmPath += @"\";
                }
                
                asmPath += @"Thinktecture.Tools.Web.Services.ContractFirst.dll";                
                settings = settings.Replace(@"@@ASM@@", asmPath);

                string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                settings = settings.Replace(@"@@FNVER@@", version);

                string configDir = null;
                if (Context.Parameters[@"ALLUSERS"] == @"1")
                {
                    configDir = FxEnvironment.GetEnvironmentVariable(@"ALLUSERSPROFILE");

                    if (configDir == null)
                    {
                        throw new InstallException("All Users directory not found.");
                    }

                    configDir += @"\Application Data\Microsoft\MSEnvShared\Addins";
                    
                    if (!Directory.Exists(configDir))
                    {
                        Directory.CreateDirectory(configDir);
                    }
                }
                else
                {
                    configDir = FxEnvironment.GetFolderPath(FxEnvironment.SpecialFolder.ApplicationData);
                    configDir += @"\Microsoft\MSEnvShared\Addins";

                    if (!Directory.Exists(configDir))
                    {
                        Directory.CreateDirectory(configDir);
                    }
                }

                string addinFile = configDir + @"\WSCF.addin";                
                StreamWriter writer = new StreamWriter(addinFile, false, Encoding.Unicode);
                writer.Write(settings);
                writer.Flush();
                writer.Close();

                stateSaver.Add(@"AddinFilePath", addinFile);                
            }
            catch (SecurityException)
            {
                MessageBox.Show(@"You must have write permission to the HKEY_LOCAL_MACHINE registry key in order to install this software.");
                throw new InstallException(@"Access denied.");
            }
            catch(Exception ex)
            {
                throw new InstallException(@"Unknown error:" + ex.Message);
            }                        
		}

		public override void Commit(IDictionary savedState)
		{
            base.Commit(savedState);
			EnvironmentHelper.BroadCast();                      
		}

		public override void Rollback(IDictionary savedState)
		{
            base.Rollback(savedState);
			string origRegValue = (string)savedState["OriginalPathRegValue"];

			RegistryHelper registry = new RegistryHelper();
			registry.BaseRegistryKey = Registry.LocalMachine;
			registry.SubKey += @"System\CurrentControlSet\Control\Session Manager\Environment";

			if(origRegValue != null || origRegValue.Length > 0)
			{
				registry.WriteKey("PATH", origRegValue);
			}

			EnvironmentHelper.BroadCast();

            try
            {
                // Remove the registry entry.
                Registry.LocalMachine.DeleteSubKey(RegistrySubKey, false);

                // Remove the .addin file.
                string addinFile = (string)savedState[@"AddinFilePath"];

                if (addinFile != null && File.Exists(addinFile))
                {
                    File.Delete(addinFile);
                }
            }
            catch (SecurityException)
            {
                MessageBox.Show(@"You must have write permission to the HKEY_LOCAL_MACHINE registry key in order to uninstall this software.");
                throw new InstallException(@"Access denied.");
            }
            catch
            {
                throw new InstallException(@"Unknown error.");
            }           
		}

		public override void Uninstall(IDictionary savedState)
		{
            base.Uninstall(savedState);
			// BDS 13-11-2006
            // Instead of simply restoring the originalPathRegValue stored during the 
            // installation now we read the installDir from the state saver and 
            // replace it in the PATH stored in the registry. This will not damage 
            // the other entries in the PATH variable.
            string origRegValue = (string)savedState["OriginalPathRegValue"];
            string installDir = (string)savedState["InstallDir"];
            
			RegistryHelper registry = new RegistryHelper();
			registry.BaseRegistryKey = Registry.LocalMachine;
			registry.SubKey += @"System\CurrentControlSet\Control\Session Manager\Environment";
            string currentPath = registry.ReadKey("PATH");

			if(currentPath != null && currentPath.Length > 0)
			{
                if (installDir != null && installDir.Length > 0)
                {
                    registry.WriteKey("PATH", currentPath.Replace(";" + installDir, ""));
                }				
			}

			EnvironmentHelper.BroadCast();

            try
            {
                // Remove the registry entry.
                Registry.LocalMachine.DeleteSubKey(RegistrySubKey, false);

                // Remove the .addin file.
                string addinFile = (string)savedState[@"AddinFilePath"];

                if (addinFile != null && File.Exists(addinFile))
                {
                    File.Delete(addinFile);
                }
            }
            catch (SecurityException)
            {
                MessageBox.Show(@"You must have write permission to the HKEY_LOCAL_MACHINE registry key in order to uninstall this software.");
                throw new InstallException(@"Access denied.");
            }
            catch
            {
                throw new InstallException(@"Unknown error.");
            }            
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}        

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
		}
		#endregion
	}
}
