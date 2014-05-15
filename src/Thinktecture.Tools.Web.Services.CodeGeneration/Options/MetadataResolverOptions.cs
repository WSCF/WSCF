using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class defines the data structure used for holding metadata resolver options.
    /// </summary>
    [DebuggerStepThrough]
    internal sealed class MetadataResolverOptions
    {
        private string metadataLocation;
        private string userName;
        private string password;
        private bool metadataLocationChanged;

        public MetadataResolverOptions()
        {
        }

        public string MetadataLocation
        {
            get { return metadataLocation; }
            set 
            {
                if (metadataLocation != null && metadataLocation != value)
                {
                    metadataLocationChanged = true;
                }
                metadataLocation = value;                 
            }
        }

		/// <summary>
		/// Gets or sets the data contract files (XSD and WSDL).
		/// </summary>
		public string[] DataContractFiles { get; set; }

        public string Username
        {
            get { return userName; }
            set { userName = value; }
        }

        public string Password
        {
            get { return password; }
            set { password = value; }
        }

        public bool MetadataLocationChanged
        {
            get { return metadataLocationChanged; }
        }
    }
}
