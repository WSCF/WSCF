using System;
using System.Collections.Generic;
using System.CodeDom;
using System.Configuration;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class represents the generated code tree.
    /// </summary>
    /// <remarks>
    /// This is a customized data structure that is optimized for WSCF 
    /// code generation purposes. It provides faster access to service contracts,
    /// service types, client types, message contracts and data contracts. Furthermore
    /// this class provides the common operations for adding new types and substituting
    /// one type with another.
    /// An instance of this class is handed over to each ICodeDecorator.
    /// </remarks>
    public sealed class ExtendedCodeDomTree
    {
    	private readonly CodeNamespace codeNamespace;

        #region Constructors

        /// <summary>
        /// Creates a new instance of GeneratedCode class.
        /// </summary>
        /// <param name="codeNamespace">The <see cref="codeNamespace"/> containing the code to be wrapped.</param>
        /// <param name="codeLanguage">The language the code was generated for.</param>
        /// <param name="configuration">The configuration associated with the generated code.</param>
        /// <remarks>
        /// This class can only be initialized by CodeFactory. 
        /// Therefore this constructor is marked as internal.
        /// </remarks>
        internal ExtendedCodeDomTree(CodeNamespace codeNamespace, CodeLanguage codeLanguage, Configuration configuration)
        {
			this.codeNamespace = codeNamespace;

        	CodeLanguauge = codeLanguage;
			Configuration = configuration;

        	ServiceContracts = new FilteredTypes(codeNamespace);
            ServiceTypes = new FilteredTypes(codeNamespace);
            ClientTypes = new FilteredTypes(codeNamespace);
            DataContracts = new FilteredTypes(codeNamespace);
            MessageContracts = new FilteredTypes(codeNamespace);
            UnfilteredTypes = new FilteredTypes(codeNamespace);
            TextFiles = new List<TextFile>();

        	ParseAndFilterCodeNamespace();
        }

        #endregion        

        #region Public properties

    	/// <summary>
    	/// Gets the service contracts.
    	/// </summary>
    	public FilteredTypes ServiceContracts { get; private set; }

    	/// <summary>
    	/// Gets the service types.
    	/// </summary>
    	public FilteredTypes ServiceTypes { get; private set; }

    	/// <summary>
    	/// Gets the client types.
    	/// </summary>
    	public FilteredTypes ClientTypes { get; private set; }

    	/// <summary>
    	/// Gets the data contracts.
    	/// </summary>
    	public FilteredTypes DataContracts { get; private set; }

    	/// <summary>
    	/// Gets the message contracts.
    	/// </summary>
    	public FilteredTypes MessageContracts { get; private set; }

    	/// <summary>
    	/// Gets the unfiltered types.
    	/// </summary>
    	public FilteredTypes UnfilteredTypes { get; private set; }

		/// <summary>
		/// Gets the language the code was generated for.
		/// </summary>
		public CodeLanguage CodeLanguauge { get; private set; }

    	/// <summary>
    	/// Gets the generated configuration object.
    	/// </summary>
    	public Configuration Configuration { get; private set; }

		/// <summary>
		/// Gets or sets the text files.
		/// </summary>
    	public List<TextFile> TextFiles { get; private set; }

    	#endregion

        #region Public methods

    	/// <summary>
    	/// Gets the modified CodeDom CodeNamespace instance without additional wrappers.
    	/// This instance can be used with standard code generation APIs to emit the final
    	/// code.
    	/// </summary>        
		public CodeNamespace UnwrapCodeDomTree()
    	{
    		foreach (CodeTypeDeclaration ctd in codeNamespace.Types)
    		{
    			// Unwrap the members.
    			for (int j = 0; j < ctd.Members.Count; j++)
    			{
    				CodeTypeMemberExtension memberExt = ctd.Members[j] as CodeTypeMemberExtension;
    				if (memberExt != null)
    				{
    					ctd.Members[j] = memberExt.ExtendedObject;
    				}
    			}
    		}

			return codeNamespace;
    	}

    	/// <summary>
        /// Substitutes one type by another.
        /// </summary>
        /// <param name="oldType">Name of the type to substitute.</param>
        /// <param name="newType">CodeTypeDeclaration of the new type.</param>
        public void SubstituteType(string oldType, CodeTypeDeclaration newType)
        {
            throw new NotImplementedException();
        }
        
        #endregion

		/// <summary>
		/// This method contains the core implementation for generating the GeneratedCode
		/// instance.
		/// </summary>
		/// <remarks>
		/// This method decorates every type found in codeNamespace with a CodeTypeMemberExtension.
		/// And then it sends each type through series of ITypeFilters to figure out whether the type 
		/// is a service contract, service type, client type, message contract or data contract.
		/// </remarks>
		private void ParseAndFilterCodeNamespace()
		{
			ITypeFilter dataContractTypeFilter = new DataContractTypeFilter();
			ITypeFilter messageContractTypeFilter = new MessageContractTypeFilter();
			ITypeFilter serviceContractTypeFilter = new ServiceContractTypeFilter();
			ITypeFilter clientTypeTypeFilter = new ClientTypeTypeFilter();
			ITypeFilter serviceTypeTypeFilter = new ServiceTypeTypeFilter();

			for (int i = 0; i < codeNamespace.Types.Count; i++)
			{
				// Take a reference to the current CodeTypeDeclaration.
				CodeTypeDeclaration ctd = codeNamespace.Types[i];
				// Create a new instance of CodeTypeMemberExtension to wrap
				// the current CodeTypeDeclaration.
				CodeTypeExtension typeExtension = new CodeTypeExtension(ctd);

				// Also wrap the inner CodeTypeMember(s)
				ExtendTypeMembers(typeExtension);

				// Here we execute the type filters in the highest to lowest probability order.
				if (dataContractTypeFilter.IsMatching(typeExtension))
				{
					typeExtension.Kind = CodeTypeKind.DataContract;
					DataContracts.Add(typeExtension);
					continue;
				}
				if (messageContractTypeFilter.IsMatching(typeExtension))
				{
					typeExtension.Kind = CodeTypeKind.MessageContract;
					MessageContracts.Add(typeExtension);
					continue;
				}
				if (serviceContractTypeFilter.IsMatching(typeExtension))
				{
					typeExtension.Kind = CodeTypeKind.ServiceContract;
					ServiceContracts.Add(typeExtension);
					continue;
				}
				if (clientTypeTypeFilter.IsMatching(typeExtension))
				{
					typeExtension.Kind = CodeTypeKind.ClientType;
					ClientTypes.Add(typeExtension);
					continue;
				}
				if (serviceTypeTypeFilter.IsMatching(typeExtension))
				{
					typeExtension.Kind = CodeTypeKind.ServiceType;
					ServiceTypes.Add(typeExtension);
					continue;
				}
				UnfilteredTypes.Add(typeExtension);
			}
		}

		/// <summary>
		/// This methods adds CodeTypeMemberExtension to all CodeTypeMembers in a 
		/// given type.
		/// </summary>        
		private static void ExtendTypeMembers(CodeTypeExtension typeExtension)
		{
			CodeTypeDeclaration type = (CodeTypeDeclaration)typeExtension.ExtendedObject;

			for (int i = 0; i < type.Members.Count; i++)
			{
				CodeTypeMember member = type.Members[i];
				CodeTypeMemberExtension memberExtension = new CodeTypeMemberExtension(member, typeExtension);

				// Add the member to the correct filtered collection.
				if (memberExtension.Kind == CodeTypeMemberKind.Field)
				{
					typeExtension.Fields.Add(memberExtension);
				}
				else if (memberExtension.Kind == CodeTypeMemberKind.Property)
				{
					typeExtension.Properties.Add(memberExtension);
				}
				else if (memberExtension.Kind == CodeTypeMemberKind.Method)
				{
					typeExtension.Methods.Add(memberExtension);
				}
				else if (memberExtension.Kind == CodeTypeMemberKind.Constructor ||
					memberExtension.Kind == CodeTypeMemberKind.StaticConstructor)
				{
					typeExtension.Constructors.Add(memberExtension);
				}
				else if (memberExtension.Kind == CodeTypeMemberKind.Event)
				{
					typeExtension.Events.Add(memberExtension);
				}
				else
				{
					typeExtension.Unknown.Add(memberExtension);
				}

				// Finally update the collection item reference.
				type.Members[i] = memberExtension;
			}
		}
    } 
}