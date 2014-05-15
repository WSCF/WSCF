// ---------------------------------------------------------------------------------
// File:            MetadataServiceExtension.cs
// Description:     
//
// Author:          Buddhike de Silva
// Date Created:    26th April 2008
// ---------------------------------------------------------------------------------

using System;
using System.ServiceModel;
using System.Diagnostics;
using System.ServiceModel.Description;
using System.ServiceModel.Web;

namespace Thinktecture.ServiceModel.Extensions.Metadata
{
    /// <summary>
    /// Contains the implementation of extension that is responsible for setting up
    /// the auxilary service for exposing metadata.
    /// It implements IExtension interface and hooked up to WCF runtime as an extension
    /// to the ServiceHost.
    /// </summary>
    /// <remarks>
    /// This class is an internal class.
    /// This class is not inheritable and therefore marked as sealed.
    /// </remarks>
    internal sealed class MetadataServiceExtension : IExtension<ServiceHostBase>
    {
        #region Private Members

        private Uri metadataServiceUri;
        private ServiceHost metadataServiceHost;
        private string rootMetadataFileLocation;        

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of MetadataService class.
        /// </summary>
        /// <param name="metadataServiceUri">
        /// Endpoint address uri for the metadata service.
        /// </param>
        public MetadataServiceExtension(Uri metadataServiceUri, string rootMetadataFileLocation)
        {
            this.metadataServiceUri = metadataServiceUri;
            this.rootMetadataFileLocation = rootMetadataFileLocation;
        }

        #endregion        

        #region IExtension<ServiceHostBase> Members

        /// <summary>
        /// Enables an extension object to find out when it has been aggregated. 
        /// Called when the extension is added to the IExtensibleObject.Extensions property. 
        /// </summary>
        /// <param name="owner">
        /// The extensible object that aggregates this extension.
        /// </param>
        public void Attach(ServiceHostBase owner)
        {
            try
            {
                // Setup the ServiceHost for hosting the MetadataService.
                this.metadataServiceHost = new ServiceHost(typeof(MetadataService));                
                WebHttpBinding webHttpBinding = new WebHttpBinding();

                WebHttpBehavior webHttpBehavior = new WebHttpBehavior();
                webHttpBehavior.DefaultBodyStyle = WebMessageBodyStyle.Bare;
                webHttpBehavior.DefaultOutgoingResponseFormat = WebMessageFormat.Xml;

                ServiceEndpoint metadataEndpoint = this.metadataServiceHost.AddServiceEndpoint(typeof(IMetadataService), webHttpBinding, this.metadataServiceUri);
                metadataEndpoint.Behaviors.Add(webHttpBehavior);

                // Hookup the runtime properties.
                MetadataServiceRuntimeProperties metadataServiceRuntimeProperties = new MetadataServiceRuntimeProperties(this.metadataServiceUri, this.rootMetadataFileLocation);
                metadataServiceHost.Extensions.Add(metadataServiceRuntimeProperties);
                this.metadataServiceHost.Open();
            }
            catch (CommunicationException)
            {
                this.metadataServiceHost.Abort();
            }
            catch (TimeoutException)
            {
                this.metadataServiceHost.Abort();
            }
            catch (Exception)
            {
                this.metadataServiceHost.Abort();
                throw;
            }
        }

        /// <summary>
        /// Enables an object to find out when it is no longer aggregated. 
        /// Called when an extension is removed from the IExtensibleObject.Extensions property. 
        /// </summary>
        /// <param name="owner">
        /// The extensible object that aggregates this extension.
        /// </param>
        public void Detach(ServiceHostBase owner)
        {
            try
            {
                // Try to tear down the metadata service host.
                metadataServiceHost.Close();
                // Gracefully shutdown the trace source.
                DiagnosticUtility.ShutDownTraceSourceGracefully();
            }
            catch (CommunicationException)
            {
                metadataServiceHost.Abort();
            }
            catch (TimeoutException)
            {
                metadataServiceHost.Abort();
            }
            catch (Exception)
            {
                metadataServiceHost.Abort();
                throw;
            }
        }

        #endregion
    }
}