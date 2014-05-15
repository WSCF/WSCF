using System;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class defines the data structure for holding the code generation options 
    /// selected by the client.
    /// </summary>
    /// <remarks>
    /// This data structure simply contains an assorted list of options available.
    /// Use CodeGenerationOptionsParser to parse these options and gain access to 
    /// much more factored code generation options collection.
    /// </remarks>
    [DebuggerStepThrough]
    public class CodeGenerationOptions
    {
        #region Public properties

    	/// <summary>
    	/// Gets or sets the metadata location.
    	/// e.g. 
    	/// c:\contracts\newsservice.wsdl
    	/// http://www.newsservice.com/contracts/newsservice.wsdl
    	/// http://www.newsservice.com/endpoints/newsservice/mex
    	/// </summary>
    	public string MetadataLocation { get; set; }

    	/// <summary>
    	/// Gets or sets the preferred code generation language.
    	/// </summary>
    	public CodeLanguage Language { get; set; }

    	/// <summary>
    	/// Gets or sets the output location for the generated code.
    	/// e.g. 
    	/// c:\mynewsproject\code
    	/// </summary>
    	public string OutputLocation { get; set; }

    	/// <summary>
		///	Gets or sets the project directory.
		/// </summary>
		public string ProjectDirectory { get; set; }

    	/// <summary>
    	/// Gets or sets the configuration file that should be altered by 
    	/// code generation process.
    	/// If this value is not available generated configuration is written
    	/// to output.config file in location specified by OutputLocation 
    	/// property.
    	/// </summary>
    	public string ConfigurationFile { get; set; }

    	/// <summary>
    	/// Gets or sets which type of code (client side or service side) has to be generated.
    	/// </summary>
    	public bool GenerateService { get; set; }

        /// <summary>
		/// Gets or sets a value indicating whether data contracts only are to be generated.
		/// </summary>
		public bool GenerateDataContracts { get; set; }

		/// <summary>
		/// Gets or sets the data contract files (XSD and WSDL).
		/// </summary>
		public string[] DataContractFiles { get; set; }

    	/// <summary>
    	/// Gets or sets a value indicating whether properties should be generated or not.
    	/// </summary>
    	public bool GenerateProperties { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether properties should be marked as virtual.
        /// </summary>
        public bool VirtualProperties { get; set; }

        /// <summary>
		/// Gets or sets a value indicating whether SOAP actions use the default WCF formatting.
		/// </summary>
		public bool FormatSoapActions { get; set; }

    	/// <summary>
    	/// Gets or sets a value indicating whether collections should be generated or not
    	/// instead of arrays.
    	/// </summary>
    	public bool GenerateCollections { get; set; }

    	/// <summary>
    	/// Gets or sets a value indicating whether typed lists should be generated or not
    	/// instead of arrays.
    	/// </summary>
    	public bool GenerateTypedLists { get; set; }

    	/// <summary>
    	/// Gets or sets a value indicating whether databinding code should be generated or not.
    	/// </summary>
    	public bool EnableDataBinding { get; set; }

    	/// <summary>
    	/// Gets or sets a value indicating whether order identifiers should be generated or not.
    	/// </summary>
    	public bool GenerateOrderIdentifiers { get; set; }

    	/// <summary>
    	/// Gets or sets a value indicating whether old async code should be generated or not.
    	/// </summary>
    	public bool GenerateAsyncCode { get; set; }

    	/// <summary>
    	/// Gets or sets a value indicating whether a physical file should be created per type 
    	/// basis.
    	/// </summary>
    	public bool GenerateSeparateFiles { get; set; }

    	/// <summary>
    	/// Gets or sets a value indicating whether the case adjustments should be performed or not.
    	/// </summary>
    	public bool AdjustCasing { get; set; }

		/// <summary>
		/// Gets or sets the username.
		/// </summary>
    	public string Username { get; set; }

		/// <summary>
		/// Gets or sets the password.
		/// </summary>
    	public string Password { get; set; }

		/// <summary>
		/// Gets or sets the CLR namespace.
		/// </summary>
    	public string ClrNamespace { get; set; }

		/// <summary>
		/// Gets or sets the name of the project.
		/// </summary>
		public string ProjectName { get; set; }

		/// <summary>
		/// Gets or sets the name of the output file.
		/// </summary>
    	public string OutputFileName { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to overwrite existing files.
		/// </summary>
    	public bool OverwriteExistingFiles { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to enable a WSDL endpoint.
		/// </summary>
    	public bool EnableWsdlEndpoint { get; set; }

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

    	#endregion
    }              
}
