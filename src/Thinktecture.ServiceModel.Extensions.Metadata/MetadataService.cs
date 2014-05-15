// ---------------------------------------------------------------------------------
// File:            MetadataService.cs
// Description:     
//
// Author:          Buddhike de Silva
// Date Created:    27th April 2008
// ---------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Channels;
using System.ServiceModel;
using System.Xml;
using System.ServiceModel.Web;
using System.Diagnostics;
using System.Net;
using Thinktecture.ServiceModel.Extensions.Metadata.Properties;
using System.IO;
using System.Security;
using System.Globalization;

namespace Thinktecture.ServiceModel.Extensions.Metadata
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, InstanceContextMode = InstanceContextMode.PerCall, UseSynchronizationContext = false, Namespace = "http://www.thinktecture.com/servicemodel/extensions/metadataservice")]
    internal sealed class MetadataService : IMetadataService
    {
        #region Private Members

        private MetadataServiceRuntimeProperties metadataServiceRuntimeProperties;
        private string trimmedMetadataServiceUriLocalPath;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of MetadataService class.
        /// </summary>
        public MetadataService()
        {
            ResolveMetadataServiceRuntimeProperties(); 
            // Trim trailing \ and / characters from metadata service uri's local path and 
            // save a reference to that trimmed copy.
            trimmedMetadataServiceUriLocalPath = this.metadataServiceRuntimeProperties.MetadataServiceUri.LocalPath.TrimEnd('/');
            trimmedMetadataServiceUriLocalPath = trimmedMetadataServiceUriLocalPath.TrimEnd('\\');
        }

        #endregion

        #region Operation Behaviors

        /// <summary>
        /// This is the main entry point to MetadataService. All requests sent to the 
        /// configured metadata url are forwarded to this method.
        /// If the request is identified as legitimate, this method returns the 
        /// content of the metadata file that is matching the request.
        /// </summary>
        /// <returns>
        /// A Message with its body contaning an XmlReader to read the metadata file.
        /// </returns>
        [OperationBehavior]
        public Message GetMetadata()
        {
            Uri requestedResource = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.RequestUri;

            // Validate the request Uri. We currently do not support query strings.
            // Therefore set the HTTP status code to 404 if the query string is found
            // in the request.
            if (!string.IsNullOrEmpty(requestedResource.Query))
            {
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound(Resources.HttpErrorFileNotFound);
                WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
                return null;
            }

            string metadataFileLocation = ResolvePhysicalMetadataFile(requestedResource);
            try
            {
                XmlReader metadataFileReader = XmlReader.Create(metadataFileLocation);
                Message response = Message.CreateMessage(MessageVersion.None, string.Empty, metadataFileReader);
                return response;
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                DiagnosticUtility.LogMetadataServiceException(fileNotFoundException);
                WebOperationContext.Current.OutgoingResponse.SetStatusAsNotFound(Resources.HttpErrorFileNotFound);
                return null;
            }
            catch (SecurityException securityException)
            {
                DiagnosticUtility.LogMetadataServiceException(securityException);
                WebOperationContext.Current.OutgoingResponse.StatusCode = HttpStatusCode.Forbidden;
                return null;
            }           
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method resolves the instance of MetadataServiceRuntimeProperties class that is 
        /// attached to the current service host and makes it available for the other members in 
        /// this class during its lifetime.
        /// </summary>
        /// <remarks>
        /// This method is invoked by the constructor.
        /// </remarks>
        private void ResolveMetadataServiceRuntimeProperties()
        {
            Debug.Assert(OperationContext.Current != null, Resources.DebugOperationContextNotInitialized);

            ServiceHostBase thisHost = OperationContext.Current.Host;            
            this.metadataServiceRuntimeProperties = thisHost.Extensions.Find<MetadataServiceRuntimeProperties>();
            Debug.Assert(this.metadataServiceRuntimeProperties != null, "");
        }

        /// <summary>
        /// Resolves the physical metadata file path that corresponds to the request.
        /// </summary>
        /// <param name="requestedUri">
        /// Requested uri.
        /// </param>        
        /// <returns>        
        /// </returns>
        private string ResolvePhysicalMetadataFile(Uri requestedUri)
        {
            Debug.Assert(requestedUri != null, string.Format(CultureInfo.CurrentCulture, Resources.DebugParameterIsNull, "requestedUri"));           

            // Trim the trailing / and \ characters from the requested uri's local path.
            // This is done in order to support metadata requests containing a trailing / character.
            // e.g. 
            // both http://www.thinktecture.com/samples/fooservice/metadata
            // and  http://www.thinktecture.com/samples/fooservice/metadata/
            // will behave equally.
            string trimmedRequestedUriLocalPath = requestedUri.LocalPath.TrimEnd('/');
            trimmedRequestedUriLocalPath = trimmedRequestedUriLocalPath.TrimEnd('\\');

            // Compare the local paths of requested and metadata service uris.
            int localPathDifference = string.Compare(trimmedRequestedUriLocalPath, trimmedMetadataServiceUriLocalPath, true);
            
            // Do they match?
            if (localPathDifference == 0)
            {
                // Requested uri is as same as the endpoint uri. In this case we return the root metadata 
                // file.
                return this.metadataServiceRuntimeProperties.RootMetadataFileLocation;
            }

            if (localPathDifference > 0)
            {
                // Requested uri is greater than the endpoint uri. 
                // Build the relative uri by replacing the local path of the parent uri
                // by an empty string.
                string relativePath = requestedUri.LocalPath.Replace(this.trimmedMetadataServiceUriLocalPath, string.Empty);

                // Change the forward slashes to backward.
                relativePath = relativePath.Replace('/', '\\');
                // Get rid of the leading backslash.
                relativePath = relativePath.TrimStart('\\');
                // Resolve the directory of the configured root metadata file.
                string metadataDirectory = Path.GetDirectoryName(this.metadataServiceRuntimeProperties.RootMetadataFileLocation);
                // Finally construct the resolved path by combining the root directory and the 
                // computed relative path.
                return Path.Combine(metadataDirectory, relativePath);
            }

            // Local path comparison should always return either zero or greater than zero. It becomes less 
            // than zero if the requested uri's host, port or scheme is different to the metadata services's
            // endpoint address. 
            // Inside a legitimate execution context this condition should not be met as WCF will make sure 
            // not to forward the requests that don't match the registered endpoint uri.
            throw DiagnosticUtility.ThrowInvalidOperationExceptionFromService(Resources.AmbiguousRequest);
        }

        #endregion
    }
}
