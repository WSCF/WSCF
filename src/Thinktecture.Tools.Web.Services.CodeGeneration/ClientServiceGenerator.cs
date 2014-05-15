using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.ServiceModel.Channels;
using System.Data.Design;
using System.Data;
using System.Configuration;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ServiceModel.Configuration;

using Thinktecture.Tools.Web.Services.Wscf.Environment;

using Binding=System.ServiceModel.Channels.Binding;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
	/// <summary>
    /// Factory class for generating instances of GeneratedCode type.
    /// </summary>
    internal class ClientServiceGenerator : ICodeGenerator
	{
        #region Private members

        private readonly PrimaryCodeGenerationOptions options;
        private readonly WsdlImporter wsdlImporter;
        private readonly CodeCompileUnit compileUnit;
        private readonly CodeDomProvider codeProvider;

    	#endregion

        #region Contructors

        public ClientServiceGenerator(MetadataSet metadataSet, PrimaryCodeGenerationOptions options, CodeDomProvider codeProvider)
        {
        	Enforce.IsNotNull(metadataSet, "metadataSet");
			this.options = Enforce.IsNotNull(options, "options");
			this.codeProvider = Enforce.IsNotNull(codeProvider, "codeProvider");

            compileUnit = new CodeCompileUnit();
            wsdlImporter = new WsdlImporter(metadataSet);

			InitializeConfiguration();
        }

    	#endregion

    	#region Public properties

    	public Configuration Configuration { get; private set; }

    	#endregion

        #region Public methods

		public CodeNamespace GenerateCode()
		{
			CreateBasicCodeDomTree();

			Debug.Assert(compileUnit.Namespaces.Count == 1, "Attention: More code namespaces than expected.");

			CodeNamespace codeNamespace = compileUnit.Namespaces[0];
			codeNamespace.Name = options.ClrNamespace;
			return codeNamespace;
		}

    	#endregion

        #region Private methods

        /// <summary>
        /// Generates the basic CodeNamespace using .NET Fx code generation API.
        /// </summary>
        private void CreateBasicCodeDomTree()
        {
            TweakWsdlImporter();
            Collection<ContractDescription> contracts = wsdlImporter.ImportAllContracts();
            Collection<Binding> bindings = wsdlImporter.ImportAllBindings();
            ServiceEndpointCollection endpoints = wsdlImporter.ImportAllEndpoints();

			if (wsdlImporter.Errors.Any(e => !e.IsWarning))
			{
				throw new ClientServiceGenerationException(wsdlImporter.Errors);
			}

            ServiceContractGenerator scg = new ServiceContractGenerator(compileUnit, Configuration);
            TweakServiceContractGenerator(scg);

            foreach (ContractDescription contract in contracts)
            {
				contract.Name = "I" + contract.Name.Replace("Interface", string.Empty);
                scg.GenerateServiceContractType(contract);
            }

            foreach (Binding binding in bindings)
            {
                string bindingSectionName, configurationName;
                scg.GenerateBinding(binding, out bindingSectionName, out configurationName);
            }

            foreach (ServiceEndpoint endpoint in endpoints)
            {
                ChannelEndpointElement channelElement;
                scg.GenerateServiceEndpoint(endpoint, out channelElement);
            }
        }

    	/// <summary>
        /// Performs several actions to tweak WsdlImporter before using it.
        /// </summary>
        private void TweakWsdlImporter()
        {
            RemoveDataContractSerializerExtension();
            AddXmlSerializerImportOptions();
    		AddFaultImportOptions();
        }

        private void TweakServiceContractGenerator(ServiceContractGenerator scg)
        {
            // Do we have to genrate the async code?
            if (options.GenerateAsyncCode)
            {
                scg.Options |= ServiceContractGenerationOptions.AsynchronousMethods;
            }
            // Are we generating the service end code?
            if (options.GenerateService)
            {
                // Then turn off the channel interface and client class generation.
                scg.Options &= ~(ServiceContractGenerationOptions.ClientClass | ServiceContractGenerationOptions.ChannelInterface);
            }

            // Map all XML namespaces to default CLR namespace specified for the generated code.
            // This generates fully qualified type names in the method signatures. 
            // And that is not compatible with some of the tweaks we do to the code later 
            // with the decorators. Therefore switching back to namespace less code generation.
            // scg.NamespaceMappings.Add("*", options.ClrNamespace);
        }

        #region Helper methods for tweaking WsdlImporter

        /// <summary>
        /// Remove(s) DataContract serialization extension(s). Because we only generate XmlSerializer serializable types in 
        /// order to support broader set of XSD constructs.
        /// </summary>
        private void RemoveDataContractSerializerExtension()
        {
            for (int i = 0; i < wsdlImporter.WsdlImportExtensions.Count; i++)
            {
                IWsdlImportExtension iext = wsdlImporter.WsdlImportExtensions[i];
                DataContractSerializerMessageContractImporter dcsmci = iext as DataContractSerializerMessageContractImporter;
                if (dcsmci != null)
                {
                    wsdlImporter.WsdlImportExtensions.RemoveAt(i);
                    break;
                }
            }
        }

        /// <summary>
        /// Creates and adds an XmlSerializerImportOptions instance to be used with WsdlImporter.
        /// </summary>
        private void AddXmlSerializerImportOptions()
        {
            XmlSerializerImportOptions xsio = new XmlSerializerImportOptions(compileUnit);
            xsio.CodeProvider = codeProvider;
            // xsio.ClrNamespace = options.ClrNamespace;

            if (options.EnableDataBinding)
            {
                xsio.WebReferenceOptions.CodeGenerationOptions |= System.Xml.Serialization.CodeGenerationOptions.EnableDataBinding;
            }

            if (!options.GenerateProperties)
            {
                xsio.WebReferenceOptions.CodeGenerationOptions &= ~System.Xml.Serialization.CodeGenerationOptions.GenerateProperties;
            }

            if (!options.GenerateOrderIdentifiers)
            {
                xsio.WebReferenceOptions.CodeGenerationOptions &= ~System.Xml.Serialization.CodeGenerationOptions.GenerateOrder;
            }

            xsio.WebReferenceOptions.SchemaImporterExtensions.Add(typeof(TypedDataSetSchemaImporterExtensionFx35).AssemblyQualifiedName);
            xsio.WebReferenceOptions.SchemaImporterExtensions.Add(typeof(DataSetSchemaImporterExtension).AssemblyQualifiedName);

            wsdlImporter.State.Add(typeof(XmlSerializerImportOptions), xsio);
        }

		private void AddFaultImportOptions()
		{
			FaultImportOptions faultImportOptions = new FaultImportOptions {UseMessageFormat = true};

			wsdlImporter.State.Add(typeof(FaultImportOptions), faultImportOptions);
		}

		#endregion

        #region Configuration manipulation methods

        /// <summary>
        /// Initializes the Configuration object to use for the code generation.
        /// </summary>
        private void InitializeConfiguration()
        {
            Configuration mc = ConfigurationManager.OpenMachineConfiguration();

            ExeConfigurationFileMap map1 = new ExeConfigurationFileMap();
            map1.ExeConfigFilename = "EC0AF989-C6B4-43e7-BD11-25C9F48DF4BD.config";
            map1.MachineConfigFilename = mc.FilePath;            
            Configuration = ConfigurationManager.OpenMappedExeConfiguration(map1, ConfigurationUserLevel.None);
            Configuration.NamespaceDeclared = true;
        }

        #endregion

        #endregion
    }
}
