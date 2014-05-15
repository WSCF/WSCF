namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
	/// <summary>
	/// The type of method implementation for operations on the service class.
	/// </summary>
	public enum MethodImplementation
	{
		/// <summary>
		/// The method will throw a NotImplementedException.
		/// </summary>
		NotImplementedException,

		/// <summary>
		/// The method will call an implementation method in a partial class.
		/// </summary>
		PartialClassMethodCalls,

		/// <summary>
		/// The method will be defined as an abstract method.
		/// </summary>
		AbstractMethods
	}
}
