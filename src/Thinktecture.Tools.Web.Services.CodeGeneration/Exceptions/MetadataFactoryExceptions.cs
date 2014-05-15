using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This exceptions is thrown when there is a problem while resolving meta data
    /// for a given WSDL document.
    /// </summary>
    public class MetadataResolveException : Exception
    {
        #region Constructors

        public MetadataResolveException() 
        {
        }

        public MetadataResolveException(string message)
            : base(message)
        {
        }

        public MetadataResolveException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MetadataResolveException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}
