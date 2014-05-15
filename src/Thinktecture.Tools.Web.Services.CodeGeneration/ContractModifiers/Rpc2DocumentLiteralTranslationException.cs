using System;
using System.Runtime.Serialization;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
	[Serializable]
	public class Rpc2DocumentLiteralTranslationException : Exception
	{
		public Rpc2DocumentLiteralTranslationException()
		{
		}

		public Rpc2DocumentLiteralTranslationException(string message)
			: base(message)
		{
		}

		public Rpc2DocumentLiteralTranslationException(string message, Exception inner)
			: base(message, inner)
		{
		}

		protected Rpc2DocumentLiteralTranslationException(SerializationInfo serializationInfo, StreamingContext serializationContext) : base(serializationInfo, serializationContext)
		{}
	}
}