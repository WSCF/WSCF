using System;
using System.Collections.Generic;
using System.Text;
using System.ServiceModel.Description;
using System.Xml;
using System.IO;

namespace Thinktecture.Tools.Web.Services.ServiceDescription
{
    internal sealed class WsdlWorkshop
    {
        List<ServiceEndpoint> endpoints;
        string wsdlFile;
        string interfaceName;

        public WsdlWorkshop(List<ServiceEndpoint> endpoints, string wsdlFile, string interfaceName)
        {
            this.endpoints = endpoints;
            this.wsdlFile = wsdlFile;
            this.interfaceName = interfaceName;
        }

        private XmlDocument ExportEndpoints()
        {
            WsdlExporter exporter = new WsdlExporter();
            foreach (ServiceEndpoint ep in endpoints)
            {
                exporter.ExportEndpoint(ep);
            }

            MetadataSet metadataSet = exporter.GetGeneratedMetadata();
            StringBuilder b = new StringBuilder();
            StringWriter sw = new StringWriter(b);
            XmlTextWriter tw = new XmlTextWriter(sw);
            foreach (MetadataSection section in metadataSet.MetadataSections)
            {
                if (section.Metadata is System.Web.Services.Description.ServiceDescription)
                {
                    System.Web.Services.Description.ServiceDescription sd = (System.Web.Services.Description.ServiceDescription)section.Metadata;
                    sd.Write(tw);
                }
            }

            string wcfWsdl = b.ToString();
            // Read it in to an XmlDocument.
            XmlDocument wcfWsdlDoc = new XmlDocument();
            wcfWsdlDoc.LoadXml(wcfWsdl);
            return wcfWsdlDoc;
        }

        private XmlDocument GetWsdlDocument()
        {
            XmlDocument wscfWsdlDoc = new XmlDocument();
            wscfWsdlDoc.Load(wsdlFile);
            return wscfWsdlDoc;
        }

        public void BuildWsdl()
        {
            XmlDocument wcfWsdlDoc = ExportEndpoints();
            XmlDocument wscfWsdlDoc = GetWsdlDocument();

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(wcfWsdlDoc.NameTable);
            nsMgr.AddNamespace("wsp", Constants.NsWsp);
            nsMgr.AddNamespace("wsdl", Constants.NsWsdl);
            nsMgr.AddNamespace("wsu", Constants.NsWsu);

            XmlNodeList policyNodes = wcfWsdlDoc.DocumentElement.SelectNodes("wsp:Policy", nsMgr);

            // Process bottom-up to preserve the original order.
            for (int i = policyNodes.Count - 1; i >= 0; i--)
            {
                XmlNode policyNode = policyNodes[i];
                XmlAttribute IdAttribute = policyNode.Attributes["Id", Constants.NsWsu];
                IdAttribute.Value = IdAttribute.Value.Replace(Constants.InternalContractName, interfaceName);
                XmlNode importeredNode = wscfWsdlDoc.ImportNode(policyNode, true);
                wscfWsdlDoc.DocumentElement.PrependChild(importeredNode);
            }            

            XmlNodeList bindingNodes = wcfWsdlDoc.DocumentElement.SelectNodes("wsdl:binding", nsMgr);
            for (int i = bindingNodes.Count - 1; i >= 0; i--)
            {
                XmlNode bindingNode = bindingNodes[i];
                XmlNode policyRef = bindingNode.SelectSingleNode("wsp:PolicyReference", nsMgr);
                if (policyRef != null)
                {
                    policyRef.Attributes["URI"].Value = policyRef.Attributes["URI"].Value.Replace(Constants.InternalContractName, interfaceName);
                    string xPath = string.Format("wsdl:binding[@name=\"{0}\"]",
                        bindingNode.Attributes["name"].Value.Replace(Constants.InternalContractName, interfaceName));

                    XmlNode ourBindingNode = wscfWsdlDoc.DocumentElement.SelectSingleNode(xPath, nsMgr);
                    XmlNode importedNode = wscfWsdlDoc.ImportNode(policyRef, true);
                    ourBindingNode.PrependChild(importedNode);
                }
            }

            // Finally save the modifications.
            wscfWsdlDoc.Save(wsdlFile);
        }
    }
}
