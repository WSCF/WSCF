// ---------------------------------------------------------------------------------
// File:            StaticMetadataBehaviorElement.cs
// Description:     
//
// Author:          Buddhike de Silva
// Date Created:    27th April 2008
// ---------------------------------------------------------------------------------

using System;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.Diagnostics;

namespace Thinktecture.ServiceModel.Extensions.Metadata
{
    /// <summary>
    /// Contains the implementation of BehaviorExtensionElement that is used to hook up 
    /// StaticMetadataBehavior via configuration system.
    /// </summary>
    public sealed class StaticMetadataBehaviorElement : BehaviorExtensionElement
    {        
        #region Constructors

        /// <summary>
        /// Creates an instance of StaticMetdataBehaviorElement class.
        /// </summary>
        [DebuggerStepThrough]
        public StaticMetadataBehaviorElement()
        {
        }
        
        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the URL where static metadata is going to be available.
        /// </summary>
        [ConfigurationProperty("metadataUrl", IsRequired=true)]        
        public string MetadataUrl
        {
            [DebuggerStepThrough]
            get { return (string)base["metadataUrl"]; }
            [DebuggerStepThrough]
            set { base["metadataUrl"] = value; }
        }

        [ConfigurationProperty("rootMetadataFileLocation", IsRequired=true)]
        public string RootMetadataFileLocation
        {
            [DebuggerStepThrough]
            get { return (string)base["rootMetadataFileLocation"]; }
            [DebuggerStepThrough]
            set { base["rootMetadataFileLocation"] = value; }
        }

        #endregion

        #region BehaviorExtensionElemenet Overrides

        /// <summary>
        /// Gets the type of behavior. In this case it is StaticMetadataBehavior type.
        /// </summary>
        public override Type BehaviorType
        {
            get { return typeof(StaticMetadataBehavior); }
        }

        /// <summary>
        /// Creates a behavior extension based on the current configuration settings.
        /// In this case we create a new instance of StaticMetadataBehavior by passing
        /// the configured MetadataUrl.
        /// </summary>
        protected override object CreateBehavior()
        {
            return new StaticMetadataBehavior(MetadataUrl, RootMetadataFileLocation);
        }        
        
        #endregion
    }
}
