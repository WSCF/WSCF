using System;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class contains the code implementation of the code generation options parser.
    /// </summary>
    [DebuggerStepThrough]
    internal class CodeGenerationOptionsParser
    {
        /// <summary>
        /// Parses the code generation options specified by options parameter and returns an instance of 
        /// InternalCodeGenearationOptions type.
        /// </summary>        
        public static InternalCodeGenerationOptions ParseCodeGenerationOptions(CodeGenerationOptions options)
        {
            MetadataResolverOptions resolverOptions = GetMetadataResolverOptions(options);
            PrimaryCodeGenerationOptions primaryOptions = GetPrimaryCodeGenerationOptions(options);
            CustomCodeGenerationOptions customOptions = GetCustomCodeGenerationOptions(options);
            CodeWriterOptions writerOptions = GetCodeWriterOptions(options);

            InternalCodeGenerationOptions icgo = new InternalCodeGenerationOptions(resolverOptions, primaryOptions, customOptions, writerOptions, options);
            return icgo;
        }

        // Filters the primary code generation options.
        private static PrimaryCodeGenerationOptions GetPrimaryCodeGenerationOptions(CodeGenerationOptions options)
        {
            PrimaryCodeGenerationOptions primaryOptions = new PrimaryCodeGenerationOptions();
            primaryOptions.EnableDataBinding = options.EnableDataBinding;
            primaryOptions.GenerateAsyncCode = options.GenerateAsyncCode;
            primaryOptions.GenerateOrderIdentifiers = options.GenerateOrderIdentifiers;
            primaryOptions.GenerateProperties = options.GenerateProperties;
            primaryOptions.GenerateService = options.GenerateService;
        	primaryOptions.GenerateDataContracts = options.GenerateDataContracts;
            primaryOptions.ClrNamespace = options.ClrNamespace;
            return primaryOptions;
        }

        // Filters the custom code generation options.
        private static CustomCodeGenerationOptions GetCustomCodeGenerationOptions(CodeGenerationOptions options)
        {
            CustomCodeGenerationOptions customOptions = new CustomCodeGenerationOptions();
        	customOptions.FormatSoapActions = options.FormatSoapActions;
            customOptions.AdjustCasing = options.AdjustCasing;
            customOptions.GenerateCollections = options.GenerateCollections;
            customOptions.GenerateService = options.GenerateService;
            customOptions.GenerateTypedLists = options.GenerateTypedLists;
            customOptions.ClrNamespace = options.ClrNamespace;
        	customOptions.ProjectName = options.ProjectName;
        	customOptions.Language = options.Language;
            customOptions.EnableWsdlEndpoint = options.EnableWsdlEndpoint;
        	customOptions.GenerateSvcFile = options.GenerateSvcFile;
            customOptions.MetadataLocation = options.MetadataLocation;
        	customOptions.ConcurrencyMode = options.ConcurrencyMode;
        	customOptions.InstanceContextMode = options.InstanceContextMode;
        	customOptions.UseSynchronizationContext = options.UseSynchronizationContext;
			customOptions.MethodImplementation = options.MethodImplementation;
            customOptions.VirtualProperties = options.VirtualProperties;

            return customOptions;
        }

        // Filters the code writer options.
        private static CodeWriterOptions GetCodeWriterOptions(CodeGenerationOptions options)
        {
            CodeWriterOptions writerOptions = new CodeWriterOptions();
            writerOptions.GenerateSeparateFiles = options.GenerateSeparateFiles;
            writerOptions.ConfigurationFile = options.ConfigurationFile;
            writerOptions.OutputLocation = options.OutputLocation;
        	writerOptions.ProjectDirectory = options.ProjectDirectory;
            writerOptions.OutputFileName = options.OutputFileName;
            writerOptions.OverwriteExistingFiles = options.OverwriteExistingFiles;
            writerOptions.Language = options.Language;
            return writerOptions;
        }

        // Filters the metadata resolver options.
        private static MetadataResolverOptions GetMetadataResolverOptions(CodeGenerationOptions options)
        {
            MetadataResolverOptions resolverOptions = new MetadataResolverOptions();
            resolverOptions.MetadataLocation = options.MetadataLocation;
        	resolverOptions.DataContractFiles = options.DataContractFiles;
            resolverOptions.Username = options.Username;
            resolverOptions.Password = options.Password;
            return resolverOptions;
        }
    }
}
