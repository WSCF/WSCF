using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
	public sealed class ConfigurationManager 
	{
		private XmlDocument xml; 
		private XmlDocument xmlOriginal;
		private string fileName;
		private IsolatedStorageFile isoStore;
		private static ConfigurationManager singleton; 

		public static ConfigurationManager GetConfigurationManager(string applicationName)
		{
			if (singleton == null)
			{
				singleton = new ConfigurationManager(applicationName);
			}

			return singleton;
		}

		private ConfigurationManager(string applicationName)
		{
			this.InitializeConfiguration(applicationName);
		}

		public string Read(string section)
		{
			return Read(section, String.Empty);
		}

		public string Read(string section, string defaultValue)
		{
			try
			{
				if (this.xml == null)
				{
					return String.Empty;
				}

				XmlNode sectionNode = this.xml.SelectSingleNode((@"/configuration/" + section));
				if (sectionNode == null)
				{
					return defaultValue;
				}
				return sectionNode.FirstChild.Value;
			}
			catch
			{
				return defaultValue;
			}
		}

		public int ReadInteger(string section)
		{
			return ReadInteger(section, 0);
		}

		public int ReadInteger(string section, int defaultValue) 
		{
			string valuestring = Read(section);
			if (valuestring.Length <= 0)
			{
				return defaultValue;
			}
			try
			{
				int value = Convert.ToInt32(valuestring);
				return value;
			}
			catch
			{
				return defaultValue;
			}
		}

		public bool ReadBoolean(string section)
		{
			return ReadBoolean(section, false);
		}

		public bool ReadBoolean(string section, bool defaultValue)
		{
			string value = this.Read(section);
			if (value.Length <= 0) 
			{
				return defaultValue;
			}
			try
			{
				return Boolean.Parse(value);
			}
			catch
			{
				return defaultValue;
			}
		}

		public void Write(string section, string value)
		{
			try
			{
				if (this.xml == null)
				{
					this.xml = new XmlDocument();
					XmlNode configurationRootNode = this.xml.CreateElement(@"configuration");
					this.xml.AppendChild(configurationRootNode);
				}

				XmlNode sectionNode = this.xml.SelectSingleNode((@"/configuration/" + section));
				if (sectionNode == null)
				{
					sectionNode = this.xml.CreateElement(section);
					XmlNode configurationRootNode = this.xml.SelectSingleNode(@"/configuration");
					configurationRootNode.AppendChild(sectionNode);
				}
				sectionNode.InnerText = value;
			}
			catch{}
		}

		public void ReadFormSettings(System.Windows.Forms.Form form)
		{
			string windowStateString = this.Read(form.Name + "WindowState");
			System.Windows.Forms.FormWindowState windowState = System.Windows.Forms.FormWindowState.Normal;
			if (windowStateString.Length > 0)
			{
				windowState = (System.Windows.Forms.FormWindowState)Convert.ToInt32(windowStateString);
			}

			if (windowState == System.Windows.Forms.FormWindowState.Maximized)
			{
				form.WindowState = windowState;
			}
			else
			{
				string valuesString = this.Read(form.Name);
				if (valuesString.Length > 0)
				{
					string[] values = valuesString.Split(Convert.ToChar(","));
					form.Top = Convert.ToInt16(values[0]);
					form.Left = Convert.ToInt16(values[1]);
					int width = Convert.ToInt16(values[2]);
					if (width > 0) form.Width = width;
					int height = Convert.ToInt16(values[3]);
					if (height > 0) form.Height = height;
				}
			}
		}

		public void WriteFormSettings(System.Windows.Forms.Form form)
		{
			this.Write(form.Name + "WindowState", ((int)form.WindowState).ToString());

			if (form.WindowState == System.Windows.Forms.FormWindowState.Normal)
			{
				string valuesstring = form.Top.ToString() + "," + form.Left.ToString() + "," + form.Width.ToString() + "," + form.Height.ToString();
				this.Write(form.Name, valuesstring);
			}
		}

		public void Persist()
		{
			try
			{
				this.WriteBackConfiguration();
			}	
			catch
			{
			}
			finally
			{
				singleton = null;
			}
		}

		private void InitializeConfiguration(string applicationName)
		{
			this.fileName = applicationName + ".config";
			this.isoStore = IsolatedStorageFile.GetStore(IsolatedStorageScope.User | IsolatedStorageScope.Assembly, null, null);

			string[] storeFileNames;
			storeFileNames = this.isoStore.GetFileNames(this.fileName);

			foreach(string storeFile in storeFileNames)
			{
				if (storeFile == this.fileName)
				{
					//Create isoStorage StreamReader
					StreamReader streamReader = new StreamReader(new IsolatedStorageFileStream(this.fileName, FileMode.Open, this.isoStore));
					this.xml = new XmlDocument();
					this.xml.Load(streamReader);
					this.xmlOriginal = new XmlDocument();
					this.xmlOriginal.LoadXml(this.xml.OuterXml);
					streamReader.Close();
				}
			}
		}

		private void WriteBackConfiguration()
		{
			if (this.xml == null) return;

			if (this.xmlOriginal != null)
			{
				if (this.xml.OuterXml == this.xmlOriginal.OuterXml) return;
			}

			StreamWriter streamWriter = null;
			try
			{
				streamWriter = new StreamWriter(new IsolatedStorageFileStream(this.fileName, FileMode.Create, this.isoStore));
				this.xml.Save(streamWriter);
				streamWriter.Flush();
				streamWriter.Close();

				if (this.xmlOriginal == null) this.xmlOriginal = new XmlDocument();
				this.xmlOriginal.LoadXml(this.xml.OuterXml);
			}
			catch
			{
				//throw;
			}
			finally
			{
				if (streamWriter != null) 
				{
					streamWriter.Flush();
					streamWriter.Close();
				}
			}
		}
	}
}
