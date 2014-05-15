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
    public class CodeWriterException : Exception
    {
        #region Constructors

        public CodeWriterException()
        {
        }

        public CodeWriterException(string message)
            : base(message)
        {
        }

        public CodeWriterException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected CodeWriterException(SerializationInfo serializationInfo, StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        {
        }

        #endregion
    }
}
