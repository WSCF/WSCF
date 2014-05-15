using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Specialized;
using System.Reflection;
using System.Xml;
using ConfigurationManager = System.Configuration.ConfigurationManager;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
    internal class WscfConfiguration
    {
        private const string APPSETTINGS_SECTION_NAME = "appSettings";
        private const string CONFIG_SECTIONS_PATH = "/configuration/configSections";
        private const string CONFIG_SECTIONS_GROUP_PATH = "sectionGroup";
        private const string CONFIG_SECTION_PATH = "section";

        public WscfConfiguration()
        {
        }

        public static NameValueCollection AppSettings
        {
            get
            {
                string wscfDirectory = Assembly.GetExecutingAssembly().Location;                
                string appConfig = wscfDirectory + ".config";                               
                NameValueCollection appSettings = null;

                if (File.Exists(appConfig))
                {
                    appSettings = (NameValueCollection)GetConfig(APPSETTINGS_SECTION_NAME, appConfig);
                }

                if (appSettings == null)
                {
                    appSettings = new NameValueCollection();
                }

                return appSettings;
            }
        }

        private static object GetConfig(string sectionName, string configFileName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(configFileName);

            IConfigurationSectionHandler handler = GetHandler(sectionName, xmlDoc);
            object config = null;

            if (sectionName == APPSETTINGS_SECTION_NAME)
            {
                config = GetAppSettingsFileHandler(sectionName, handler, xmlDoc);
            }
            else
            {
                XmlNode node = xmlDoc.SelectSingleNode("//" + sectionName);
                config = handler.Create(null, null, node);
            }

            return config;
        }

        protected static IConfigurationSectionHandler GetHandler(string sectionName, XmlDocument xmlDoc)
        {
            IConfigurationSectionHandler handler = null;

            if (sectionName == APPSETTINGS_SECTION_NAME)
            {
                handler = new NameValueSectionHandler();
                return handler;
            }

            string[] sections = sectionName.Split('/');
            string sectionGroup = string.Empty;
            string section = string.Empty;
            string xPath = string.Empty;

            //see if we have a section group that we have to go through
            if (sections.Length > 1)
            {
                sectionGroup = sections[0];
                section = sections[1];

                xPath = string.Format(CONFIG_SECTIONS_PATH + "/" +
                    CONFIG_SECTIONS_GROUP_PATH + "[@name='" + sectionGroup + "']/" +
                    CONFIG_SECTION_PATH + "[@name='" + section + "']");
            }
            else
            {
                section = sections[0];

                xPath = string.Format(CONFIG_SECTIONS_PATH + "/" +
                    CONFIG_SECTION_PATH + "[@name='" + section + "']");
            }

            XmlNode node = xmlDoc.SelectSingleNode(xPath);

            string typeName = node.Attributes["type", ""].Value;

            if (typeName == null || typeName.Length == 0)
                return handler;

            Type handlerType = Type.GetType(typeName);
            handler = (IConfigurationSectionHandler)Activator.CreateInstance(handlerType);

            return handler;
        }

        protected static object GetAppSettingsFileHandler(string sectionName, IConfigurationSectionHandler parentHandler, XmlDocument xmlDoc)
        {
            object handler = null;
            XmlNode node = xmlDoc.SelectSingleNode("//" + sectionName);
            XmlAttribute att = (XmlAttribute)node.Attributes.RemoveNamedItem("file");

            if (att == null || att.Value == null || att.Value.Length == 0)
            {
                return parentHandler.Create(null, null, node);
            }
            else
            {
                string fileName = att.Value;
                string dir = Path.GetDirectoryName(fileName);
                string fullName = Path.Combine(dir, fileName);
                XmlDocument xmlDoc2 = new XmlDocument();
                xmlDoc2.Load(fullName);

                object parent = parentHandler.Create(null, null, node);
                IConfigurationSectionHandler h = new NameValueSectionHandler();
                handler = h.Create(parent, null, xmlDoc2.DocumentElement);
            }

            return handler;
        }
    }

    public static class AppLog
    {
        public static void LogMessage(string message)
        {
            string t = WscfConfiguration.AppSettings["trace"];

            if ((t != null) && (t == "1" || t.ToLower() == "true"))
            {
                Mutex flock = null;
                try
                {
                    flock = new Mutex(false, "WSCFLOG");

                    if (flock.WaitOne())
                    {

                        string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                        string logFile = directory + "\\" + "WSCF.log";
                        StreamWriter writer = new StreamWriter(logFile, true, Encoding.UTF8);
                        writer.WriteLine(DateTime.Now.ToString("M-dd-yyyy H:mm"));
                        writer.WriteLine(message);
                        writer.WriteLine();
                        writer.Close();
                    }
                }
                catch (Exception ex)
                {
                    EventLog.WriteEntry("WSCF Log",
                        string.Format("An error occrred while trying to write the message {0} to the log. Details: {1}",
                        message, ex.Message), EventLogEntryType.Error);
                }
                finally
                {
                    if (flock != null)
                    {
                        flock.ReleaseMutex();
                    }
                }
            }
         }       
    }
}
