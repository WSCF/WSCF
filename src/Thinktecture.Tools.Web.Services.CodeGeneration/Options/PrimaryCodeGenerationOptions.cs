using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class defines the data structure used for holding the primary code generation
    /// options. Primary code generation options are passed in to the .NET Fx code generation
    /// API.
    /// </summary>
    [DebuggerStepThrough]
    internal class PrimaryCodeGenerationOptions
    {
        private bool generateService;
        private bool generateProperties;
        private bool generateSerializableClasses;
        private bool enableDataBinding;
        private bool generateOrderIdentifiers;
        private bool generateAsyncCode;

        private string clrNamespace;

        public bool GenerateService
        {
            get { return generateService; }
            set { generateService = value; }
        }

		public bool GenerateDataContracts { get; set; }

        public bool GenerateProperties
        {
            get { return generateProperties; }
            set { generateProperties = value; }
        }

        public bool GenerateSerializableClasses
        {
            get { return generateSerializableClasses; }
            set { generateSerializableClasses = value; }
        }

        public bool EnableDataBinding
        {
            get { return enableDataBinding; }
            set { enableDataBinding = value; }
        }

        public bool GenerateOrderIdentifiers
        {
            get { return generateOrderIdentifiers; }
            set { generateOrderIdentifiers = value; }
        }

        public bool GenerateAsyncCode
        {
            get { return generateAsyncCode; }
            set { generateAsyncCode = value; }
        }

        public string ClrNamespace
        {
            get { return clrNamespace; }
            set { clrNamespace = value; }
        }
    }
}
