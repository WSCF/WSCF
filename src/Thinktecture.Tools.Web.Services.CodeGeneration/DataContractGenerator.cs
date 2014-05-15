using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Xml.Schema;
using System.Xml.Serialization;

using Thinktecture.Tools.Web.Services.Wscf.Environment;

using GenerationOptions = System.Xml.Serialization.CodeGenerationOptions;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
	/// <summary>
	/// Generates the CodeDOM for data contracts.
	/// </summary>
	internal class DataContractGenerator : ICodeGenerator
	{
		#region Private members

		private readonly XmlSchemas schemas;
		private readonly CodeDomProvider codeProvider;
		private readonly PrimaryCodeGenerationOptions options;

		#endregion

		#region Contructors

		/// <summary>
		/// Initializes a new instance of the <see cref="DataContractGenerator"/> class.
		/// </summary>
		/// <param name="schemas">The XML schemas.</param>
		/// <param name="options">The code generation options.</param>
		/// <param name="codeProvider">The code provider.</param>
		public DataContractGenerator(XmlSchemas schemas, PrimaryCodeGenerationOptions options, CodeDomProvider codeProvider)
		{
			this.schemas = Enforce.IsNotNull(schemas, "schemas");
			this.options = Enforce.IsNotNull(options, "options");
			this.codeProvider = Enforce.IsNotNull(codeProvider, "codeProvider");
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Generates the data contracts for given xsd file(s).
		/// </summary>        
		public CodeNamespace GenerateCode()
		{
			CodeCompileUnit codeCompileUnit = new CodeCompileUnit();
			CodeNamespace codeNamespace = new CodeNamespace(options.ClrNamespace);
			codeCompileUnit.Namespaces.Add(codeNamespace);

			// Build the code generation options.
			GenerationOptions generationOptions = GenerationOptions.None;

			if (options.GenerateProperties)
			{
				generationOptions |= GenerationOptions.GenerateProperties;
			}

			if (options.EnableDataBinding)
			{
				generationOptions |= GenerationOptions.EnableDataBinding;
			}
			if (options.GenerateOrderIdentifiers)
			{
				generationOptions |= GenerationOptions.GenerateOrder;
			}

			// Build the CodeDom object graph.
			XmlCodeExporter codeExporter = new XmlCodeExporter(codeNamespace, codeCompileUnit, generationOptions, null);
			CodeIdentifiers codeIdentifiers = new CodeIdentifiers();
			ImportContext importContext = new ImportContext(codeIdentifiers, false);
			XmlSchemaImporter schemaimporter = new XmlSchemaImporter(schemas, generationOptions, codeProvider, importContext);
			for (int si = 0; si < schemas.Count; si++)
			{
				XmlSchema schema = schemas[si];
				IEnumerator enumerator = schema.Elements.Values.GetEnumerator();
				IEnumerator enumerator2 = schema.SchemaTypes.Values.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						XmlSchemaElement element = (XmlSchemaElement)enumerator.Current;
						if (element.IsAbstract) continue;

						XmlTypeMapping typemapping = schemaimporter.ImportTypeMapping(element.QualifiedName);
						codeExporter.ExportTypeMapping(typemapping);
					}
					while (enumerator2.MoveNext())
					{
						XmlSchemaType type = (XmlSchemaType)enumerator2.Current;
						if (CouldBeAnArray(type)) continue;

						XmlTypeMapping typemapping = schemaimporter.ImportSchemaType(type.QualifiedName);
						codeExporter.ExportTypeMapping(typemapping);
					}
				}
				finally
				{
					IDisposable disposableobject = enumerator as IDisposable;
					if (disposableobject != null)
					{
						disposableobject.Dispose();
					}
					IDisposable disposableobject2 = enumerator2 as IDisposable;
					if (disposableobject2 != null)
					{
						disposableobject2.Dispose();
					}
				}
			}
			if (codeNamespace.Types.Count == 0)
			{
				throw new Exception("No types were generated.");
			}

			return codeNamespace;
		}

		#endregion

		#region Private methods

		/// <summary>
		/// Checks whether a given XmlSchemaType could be represented as an array. That is the XmlSchemaType
		/// has to be:
		///     1. Complex type
		///     2. ...with no base type
		///     3. ...has no attributes
		///     4. ...has only one element
		///     5. ...whose maxOccurs is > 1
		/// </summary>
		/// <returns></returns>
		private static bool CouldBeAnArray(XmlSchemaType schematype)
		{
			XmlSchemaComplexType complextype = schematype as XmlSchemaComplexType;
			if (complextype != null)
			{
				if (complextype.Attributes.Count == 0)
				{
					XmlSchemaSequence sequence = complextype.Particle as XmlSchemaSequence;
					if (sequence != null)
					{
						if (sequence.Items.Count == 1)
						{
							XmlSchemaElement element = sequence.Items[0] as XmlSchemaElement;
							if (element != null)
							{
								if (element.MaxOccurs > 1 || (element.MaxOccursString != null && element.MaxOccursString.ToLower() == "unbounded"))
								{
									return true;
								}
							}
						}
					}
				}
			}
			return false;
		}

		#endregion
	}
}
