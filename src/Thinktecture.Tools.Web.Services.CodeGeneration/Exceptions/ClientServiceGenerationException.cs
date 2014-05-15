using System;
using System.Collections.Generic;
using System.ServiceModel.Description;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
	/// <summary>
	/// Exception thrown if critical errors occur during Client/Service code generation.
	/// </summary>
	public class ClientServiceGenerationException : Exception
	{
		private readonly List<string> messages = new List<string>();

		/// <summary>
		/// Initializes a new instance of the <see cref="ClientServiceGenerationException"/> class.
		/// </summary>
		/// <param name="errors">The metadata conversion errors.</param>
		public ClientServiceGenerationException(IEnumerable<MetadataConversionError> errors)
		{
			foreach (MetadataConversionError error in errors)
			{
				messages.Add(error.Message);
			}
		}

		/// <summary>
		/// Gets the error messages.
		/// </summary>
		public IEnumerable<string> Messages
		{
			get { return messages; }
		}
	}
}
