using System.CodeDom;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
	/// <summary>
	/// Classes that implement this interface are able to generate and return a CodeDOM.
	/// </summary>
	internal interface ICodeGenerator
	{
		/// <summary>
		/// Generate the required code.
		/// </summary>
		/// <returns>A <see cref="CodeNamespace"/> containing the generated code.</returns>
		CodeNamespace GenerateCode();
	}
}