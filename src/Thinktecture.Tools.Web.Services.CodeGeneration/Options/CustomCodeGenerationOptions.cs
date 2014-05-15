using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class defines the data structure used for holding the custom 
    /// code generation options. Custom code generation options are used for 
    /// turning on/off the code decorators implementing ICodeDecorator interface.
    /// </summary>
    [DebuggerStepThrough]
    public class CustomCodeGenerationOptions
    {
		/// <summary>
		/// Gets or sets a value indicating whether to generate the service code.
		/// </summary>
    	public bool GenerateService { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether the System.Serializable attribute will be removed.
		/// </summary>
		public bool RemoveSerializableAttribute { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether SOAP actions use the default WCF formatting.
		/// </summary>
		public bool FormatSoapActions { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate collections.
		/// </summary>
    	public bool GenerateCollections { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to generate typed lists.
		/// </summary>
    	public bool GenerateTypedLists { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether casing should be adjusted.
		/// </summary>
    	public bool AdjustCasing { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to enable a WSDL endpoint.
		/// </summary>
    	public bool EnableWsdlEndpoint { get; set; }

		/// <summary>
		/// Gets or sets the CLR namespace.
		/// </summary>
    	public string ClrNamespace { get; set; }

		/// <summary>
		/// Gets or sets the name of the project.
		/// </summary>
		public string ProjectName { get; set; }

		/// <summary>
		/// Gets or sets the code language.
		/// </summary>
		public CodeLanguage Language { get; set; }

		/// <summary>
		/// Gets or sets the metadata location.
		/// </summary>
    	public string MetadataLocation { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether a .svc file should be generated.
		/// </summary>
    	public bool GenerateSvcFile { get; set; }

		/// <summary>
		/// Gets or sets the instance context mode service behavior.
		/// </summary>
		public string InstanceContextMode { get; set; }

		/// <summary>
		/// Gets or sets the concurrency mode service behavior.
		/// </summary>
		public string ConcurrencyMode { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to use the synchronization context.
		/// </summary>
		public bool UseSynchronizationContext { get; set; }

		/// <summary>
		/// Gets or sets the type of method implementation for operations on the service class.
		/// </summary>
		public MethodImplementation MethodImplementation { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether properties should be marked as virtual.
		/// </summary>
		public bool VirtualProperties { get; set; }
	}
}
