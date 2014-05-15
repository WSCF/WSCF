// ---------------------------------------------------------------------------------
// File:            StaticMetadataBehavior.cs
// Description:     
//
// Author:          Buddhike de Silva
// Date Created:    26th April 2008
// ---------------------------------------------------------------------------------

using System;
using System.ServiceModel.Description;
using System.ServiceModel;
using System.Collections.ObjectModel;
using System.ServiceModel.Channels;
using Thinktecture.ServiceModel.Extensions.Metadata.Properties;
using System.Diagnostics;
using System.IO;
using System.Security;

namespace Thinktecture.ServiceModel.Extensions.Metadata
{
    /// <summary>
    /// Contains the IServiceBehavior implementation that is used to hookup MetadataService extension
    /// to the runtime.
    /// </summary>
    public sealed class StaticMetadataBehavior : IServiceBehavior
    {
        #region Private Memebers

        private string metadataUrl;
        private string rootMetadataFileLocation;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of StaticMetadataBehavior class.
        /// </summary>
        public StaticMetadataBehavior()
        {
        }
        
        /// <summary>
        /// Creates a new instance of StaticMetadtaBehavior class.
        /// </summary>
        /// <param name="endpointAddress">
        /// String indicating the Uri of the endpoint exposing metadata.
        /// </param>
        public StaticMetadataBehavior(string metadataUrl, string rootMetadataFileLocation)
        {
            this.metadataUrl = metadataUrl;
            this.rootMetadataFileLocation = rootMetadataFileLocation;
        }

        #endregion

        #region IServiceBehavior Members

        /// <summary>
        /// Provides the ability to pass custom data to binding elements to support the contract implementation. 
        /// </summary>
        /// <param name="serviceDescription">The service description of the service.</param>
        /// <param name="serviceHostBase">The host of the service.</param>
        /// <param name="endpoints">The service endpoints.</param>
        /// <param name="bindingParameters">Custom objects to which binding elements have access.</param>
        [DebuggerStepThrough]
        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
            // NOP
        }

        /// <summary>
        /// Provides the ability to change run-time property values or insert custom extension objects such as error handlers, 
        /// message or parameter interceptors, security extensions, and other custom extension objects.
        /// Here we use it to hook up our ServiceHostBase extension that is responsible for setting up the metadata
        /// service to the runtime.
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The host that is currently being built.</param>
        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            // Validate the current state.
            if (string.IsNullOrEmpty(this.metadataUrl))
            {
                throw new InvalidOperationException(Resources.EndpointAddressNullOrEmptyString);
            }
            if (string.IsNullOrEmpty(this.rootMetadataFileLocation))
            {
                throw new InvalidOperationException(Resources.RootMetadataLocationNullOrEmptyString);
            }

            Uri metadataServiceUri = null;
            Uri httpBaseAddress = null;

            // Traverse the base addresses collection to find the http base address.
            foreach (Uri baseAddress in serviceHostBase.BaseAddresses)
            {
                // Is this an http uri?
                if (baseAddress.Scheme == Uri.UriSchemeHttp)
                {
                    // Resolve the metadata service uri using this base address.
                    httpBaseAddress = baseAddress;
                    break;
                }
            }

            // Did we find the httpBaseAddress?
            if (httpBaseAddress != null)
            {
                metadataServiceUri = GetMetadataServiceUri(httpBaseAddress);
            }
            else
            {
                metadataServiceUri = GetAbsoluteMetadataServiceUri();
            }

            // Ensure that rootMetadataLocation is initialized to a full path.
            EnsureRootMetadataLocationsIsAnAbsolutePath();

            // Now that we have figured out the metadataServiceUri, let's go ahead 
            // and create MetadataService extension.
            MetadataServiceExtension metadataServiceExtension = new MetadataServiceExtension(metadataServiceUri, rootMetadataFileLocation);
            // And finally hook it up to service host.
            serviceHostBase.Extensions.Add(metadataServiceExtension);
        }

        /// <summary>
        /// Provides the ability to inspect the service host and the service description to confirm 
        /// that the service can run successfully. 
        /// </summary>
        /// <param name="serviceDescription">The service description.</param>
        /// <param name="serviceHostBase">The service host that is currently being constructed.</param>        
        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {            
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Constructs the absolute metadata service Uri from the configured 
        /// endpoint address.
        /// </summary>        
        private Uri GetAbsoluteMetadataServiceUri()
        {
            return GetMetadataServiceUri(null);
        }

        /// <summary>
        /// Creates the endpoint address Uri for metadata service based on host's http base address
        /// and what's configured as metadata service endpoint address.
        /// </summary>
        /// <param name="hostBaseAddress">Uri of host's http base address.</param>
        /// <returns>An instance of Uri indicating the Uri of medata service.</returns>
        private Uri GetMetadataServiceUri(Uri hostBaseAddress)
        {
            // Assert if metadataUrl is null or an empty string.
            if(string.IsNullOrEmpty(this.metadataUrl))
            {
                throw new InvalidOperationException(string.Format(Resources.DebugEndpointAddressNullInGetMetadataServiceUri, (this.metadataUrl == null ? "null" : this.metadataUrl)));
            }
            //Debug.Assert(!string.IsNullOrEmpty(this.metadataUrl), string.Format(Resources.DebugEndpointAddressNullInGetMetadataServiceUri, (this.metadataUrl == null ? "null" : this.metadataUrl)));
            
            Uri uri = null;
            // Do we have a well formed absolute Uri.
            if (Uri.IsWellFormedUriString(this.metadataUrl, UriKind.Absolute))
            {
                uri = new Uri(this.metadataUrl);
                // Do we have a vaid http endpoint uri?
                if (uri.Scheme != Uri.UriSchemeHttp)
                {
                    throw new InvalidOperationException(Resources.NonHttpAddress);
                }
                return uri;
            }
            else
            {
                // hostBaseAddress becomes null if this method is called by 
                // GetAbsoluteMetadataServiceUrl method.
                // Therefore throw if the there is no http base address.
                if (hostBaseAddress == null)
                {
                    throw new InvalidOperationException(Resources.AbsoluteUriRequired);
                }
                
                // Try to create the endpoint address relative to host's base address.
                if (Uri.TryCreate(hostBaseAddress, metadataUrl, out uri))
                {                    
                    return uri;
                }
                else
                {
                    throw new InvalidOperationException(string.Format(Resources.InvalidEndpointAddress, this.metadataUrl));
                }
            }                   
        }

        /// <summary>
        /// 
        /// </summary>
        private void EnsureRootMetadataLocationsIsAnAbsolutePath()
        {
            // Assert if this method is called without properly initializing 
            // rootMatadataLocation field.
            Debug.Assert(!string.IsNullOrEmpty(this.rootMetadataFileLocation), "");
            try
            {
                this.rootMetadataFileLocation = Path.GetFullPath(this.rootMetadataFileLocation);
            }
            catch (Exception exception)                
            {
                if (CanTranslateGetFullPathExceptionToInvalidOperation(exception))
                {
                    throw new InvalidOperationException(Resources.InvalidRootMetadataLocation, exception);
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        private static bool CanTranslateGetFullPathExceptionToInvalidOperation(Exception exception)
        {
            Type typeOfException = exception.GetType();

            if(typeOfException == typeof(ArgumentException))
            {
                return true;
            }
            else if(typeOfException == typeof(SecurityException))
            {
                return true;
            }
            else if(typeOfException == typeof(NotSupportedException))
            {
                return true;
            }
            else if(typeOfException == typeof(PathTooLongException))
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        #endregion
    }
}
