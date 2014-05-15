using System;
using System.Collections;
using System.ServiceModel.Description;
using System.Xml;
using System.Web.Services.Discovery;
using System.Net;
using System.Xml.Schema;
using System.Net.Security;
using System.IO;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

using WebServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// Retrieves and imports meta data for WSDL documents and XSD files.
    /// </summary>
    internal sealed class MetadataFactory
    {
        #region Public methods

        /// <summary>
        /// Retrieves and imports meta data for a given WSDL document specified by 
        /// WsdlDocument property.
        /// </summary>
		/// <param name="options">The metadata resolving options.</param>
		/// <returns>A collection of service metadata in XML form.</returns>
        public static MetadataSet GetMetadataSet(MetadataResolverOptions options)
        {
            if (options == null)
            {
                throw new ArgumentException("options could not be null.");
            }

            if (string.IsNullOrEmpty(options.MetadataLocation))
            {
                throw new ArgumentException("MetadataLocation option could not be null or an empty string.");
            }

            try
            {
                // First download the contracts if they are accessed over the web.
                DownloadContract(options);

                // Try to run RPC2DocumentLiteral converter.
                TryTranslateRpc2DocumentLiteral(options);
                MetadataSet metadataSet = new MetadataSet();
                XmlDocument doc = new XmlDocument();
                doc.Load(options.MetadataLocation);
                MetadataSection ms = new MetadataSection(null, null, doc);
                metadataSet.MetadataSections.Add(ms);
                ResolveImports(options, metadataSet);

                return metadataSet;
            }
            catch (Exception ex)
            {
                // TODO: Log exception.
                throw new MetadataResolveException("Could not resolve metadata", ex);
            }
        }

		/// <summary>
		/// Gets the XML schemas for generating data contracts.
		/// </summary>
		/// <param name="options">The metadata resolving options.</param>
		/// <returns>A collection of the XML schemas.</returns>
		public static XmlSchemas GetXmlSchemas(MetadataResolverOptions options)
		{
			if (options.DataContractFiles == null)
				throw new ArgumentNullException("options");

			// Resolve the schemas.
			XmlSchemas schemas = new XmlSchemas();
			for (int fi = 0; fi < options.DataContractFiles.Length; fi++)
			{
				// Skip the non xsd/wsdl files.
				string lowext = Path.GetExtension(options.DataContractFiles[fi]).ToLower();
				if (lowext == ".xsd") // This is an XSD file.
				{
					XmlTextReader xmltextreader = null;

					try
					{
						xmltextreader = new XmlTextReader(options.DataContractFiles[fi]);
						XmlSchema schema = XmlSchema.Read(xmltextreader, null);
						XmlSchemaSet schemaset = new XmlSchemaSet();
						schemaset.Add(schema);
						RemoveDuplicates(ref schemaset);
						schemaset.Compile();
						schemas.Add(schema);
					}
					finally
					{
						if (xmltextreader != null)
						{
							xmltextreader.Close();
						}
					}
				}
				else if (lowext == ".wsdl") // This is a WSDL file.
				{
					XmlSchemaSet schemaset = new XmlSchemaSet();
					XmlSchemaSet embeddedschemaset = new XmlSchemaSet();
					DiscoveryClientProtocol dcp = new DiscoveryClientProtocol();
					dcp.AllowAutoRedirect = true;
					dcp.Credentials = CredentialCache.DefaultCredentials;
					dcp.DiscoverAny(options.DataContractFiles[fi]);
					dcp.ResolveAll();
					foreach (object document in dcp.Documents.Values)
					{
						if (document is XmlSchema)
						{
							schemaset.Add((XmlSchema)document);
							schemas.Add((XmlSchema)document);
						}
						if (document is WebServiceDescription)
						{
							schemas.Add(((WebServiceDescription)document).Types.Schemas);
							foreach (XmlSchema schema in ((WebServiceDescription)document).Types.Schemas)
							{
								embeddedschemaset.Add(schema);
								schemas.Add(schema);
							}
						}
					}
					RemoveDuplicates(ref schemaset);
					schemaset.Add(embeddedschemaset);
					schemaset.Compile();
				}
			}
			return schemas;
		}

        #endregion

        #region Private methods

        private static void DownloadContract(MetadataResolverOptions options)
        {
            // Return if we don't have an http endpoint.
            if (!options.MetadataLocation.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) &&
                !options.MetadataLocation.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            // Create a Uri for the specified metadata location.
            Uri uri = new Uri(options.MetadataLocation);
            string tempFilename = GetTempFilename(uri);

            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CertValidation);

            WebRequest req = WebRequest.Create(options.MetadataLocation);
            WebResponse result = req.GetResponse();
            Stream receiveStream = result.GetResponseStream();
            WriteStream(receiveStream, tempFilename);
            options.MetadataLocation = tempFilename;
        }
        
        private static string GetTempFilename(Uri metadataUri)
        {
            string tempDir = Path.GetTempPath();
            string filename = null;
            Debug.Assert(tempDir != null, "Could not determine the temp directory.");

            // Check if the contracts are published in the root.
            if (metadataUri.Segments.Length == 1)
            {
                if (metadataUri.Segments[0] == "/")
                {
                    filename = Guid.NewGuid().ToString();
                }
                else
                {
                    // I haven't yet thought about this case and AFAIK,
                    // this code should never arrive here.
                    Debug.Assert(false, "Special case.");
                }
            }
            else
            {
                filename = metadataUri.Segments[metadataUri.Segments.Length - 1];
                filename = Path.GetFileNameWithoutExtension(filename);
            }

            filename = filename + ".wsdl";
            return Path.Combine(tempDir, filename);
        }

        private static bool CertValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true;
        }        

        private static void WriteStream(Stream stream, string targetFile)
        {                        
            XmlWriter writer = null;
            StreamReader reader = null;
            FileStream fileStream = null;
            try
            {
                fileStream = File.Open(targetFile, FileMode.Create, FileAccess.Write, FileShare.None);                                
                reader = new StreamReader(stream);
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.OmitXmlDeclaration = true;
                xmlWriterSettings.Indent = true;                               
                writer = XmlWriter.Create(fileStream, xmlWriterSettings);
                string wsdl = reader.ReadToEnd();
                writer.WriteRaw(wsdl);
                writer.Flush();
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
                if (writer != null)
                {
                    writer.Close();                    
                }
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
                TrySetTempAttribute(targetFile);
            }
        }

        private static bool TrySetTempAttribute(string file)
        {
            try
            {
                File.SetAttributes(file, FileAttributes.Temporary);                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void TryTranslateRpc2DocumentLiteral(MetadataResolverOptions options)
        {
            // TODO: This will not work properly for file names like this my.wsdl.wsdl.
            string translatedWsdlFilename = options.MetadataLocation.Replace(".wsdl", "_transformed.wsdl");

            try
            {
                if (Rpc2DocumentLiteralTranslator.ContainsRpcLiteralBindings(options.MetadataLocation))
                {
                    // Execute the translation.
                    Rpc2DocumentLiteralTranslator r2d = Rpc2DocumentLiteralTranslator.Translate(options.MetadataLocation, translatedWsdlFilename);
                    options.MetadataLocation = translatedWsdlFilename;
                }
            }
            catch (Rpc2DocumentLiteralTranslationException)
            {
                // TODO: Log the exception details.c
            }            
        }

        private static void ResolveImports(MetadataResolverOptions options, MetadataSet metadataSet)
        {
            // Resolve metadata using a DiscoveryClientProtocol.
            DiscoveryClientProtocol dcp = new DiscoveryClientProtocol();
            dcp.Credentials = GetCredentials(options);
            dcp.AllowAutoRedirect = true;
            dcp.DiscoverAny(options.MetadataLocation);
            dcp.ResolveAll();

            foreach (object osd in dcp.Documents.Values)
            {
                if (osd is System.Web.Services.Description.ServiceDescription)
                {
                    MetadataSection mds = MetadataSection.CreateFromServiceDescription((System.Web.Services.Description.ServiceDescription)osd);
                    metadataSet.MetadataSections.Add(mds);
                    continue;
                }
                if (osd is XmlSchema)
                {
                    MetadataSection mds = MetadataSection.CreateFromSchema((XmlSchema)osd);
                    metadataSet.MetadataSections.Add(mds);
                    continue;
                }
            }
        }

        /// <summary>
        /// Returns an object of ICredentials type according to the options.
        /// </summary>        
        private static ICredentials GetCredentials(MetadataResolverOptions options)
        {
            if (string.IsNullOrEmpty(options.Username))
            {
                return CredentialCache.DefaultCredentials;
            }
            else
            {
                return new NetworkCredential(options.Username, options.Password);
            }
        }

		/// <summary>
		/// Removes the duplicate schemas in a given XmlSchemaSet instance.
		/// </summary>
		private static void RemoveDuplicates(ref XmlSchemaSet set)
		{
			ArrayList schemalist = new ArrayList(set.Schemas());
			ArrayList duplicatedschemalist = new ArrayList();

			for (int schemaindex = 0; schemaindex < schemalist.Count; schemaindex++)
			{
				if (((XmlSchema)schemalist[schemaindex]).SourceUri == string.Empty)
				{
					duplicatedschemalist.Add(schemalist[schemaindex]);
				}
				else
				{
					for (int lowerschemaindex = schemaindex + 1; lowerschemaindex < schemalist.Count; lowerschemaindex++)
					{
						if (((XmlSchema)schemalist[lowerschemaindex]).SourceUri == ((XmlSchema)schemalist[schemaindex]).SourceUri)
						{
							duplicatedschemalist.Add(schemalist[lowerschemaindex]);
						}
					}
				}
			}
			foreach (XmlSchema schema in duplicatedschemalist)
			{
				set.Remove(schema);
			}
		}
        
        #endregion
    }
}