using System;
using System.ServiceModel;
using System.Diagnostics;

namespace Thinktecture.ServiceModel.Extensions.Metadata
{
    [DebuggerStepThrough]
    internal class MetadataServiceRuntimeProperties : IExtension<ServiceHostBase>
    {
        #region Private Fields

        private Uri metadataServiceUri;
        private string rootMetadataFileLocation;

        #endregion

        #region Public Properties

        public Uri MetadataServiceUri
        {            
            get { return metadataServiceUri; }            
            set { metadataServiceUri = value; }
        }

        public string RootMetadataFileLocation
        {            
            get { return rootMetadataFileLocation; }         
            set { rootMetadataFileLocation = value; }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of MetadataServiceRuntimeProperties class.
        /// </summary>
        public MetadataServiceRuntimeProperties(Uri metadataServiceUri, string rootMetadataFileLocation)
        {
            this.metadataServiceUri = metadataServiceUri;
            this.rootMetadataFileLocation = rootMetadataFileLocation;
        }

        #endregion

        #region IExtension<ServiceHostBase> Members

        public void Attach(ServiceHostBase owner)
        {            
            // NOP
        }

        public void Detach(ServiceHostBase owner)
        {         
            // NOP
        }

        #endregion
    }
}
