using System.Collections;
using System;
using System.Collections.Generic;

namespace Thinktecture.Tools.Web.Services.ServiceDescription
{
	#region InterfaceContract class

	/// <summary>
	/// Represents the content of service description.
	/// </summary>
	/// <remarks>
	/// This type is used to explain the content of the WSDL file. WSDL generation engine use this simple 
	/// class rather than the .Net framework's ServiceDescription class to hold only the necessary details
	/// for the WSDL generation.
	/// </remarks>
	public class InterfaceContract
	{
        [Flags]
        public enum SoapBindings
        {
            Soap11 = 1,
            Soap12 = 2,
        }

		#region Private fields
		
		private string serviceName;
		private string serviceNamespace;
		private string serviceDocumentation;
		private string schemaNamespace;
		private bool needsServiceElement;
		private bool useAlternateLocationForImports;
		private bool isHttpBinding;
        private SoapBindings bindings;

		private SchemaElements types;
		private SchemaImports imports;
		private OperationsCollection operationsCollection;
		
		
		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the InterfaceContract class.
		/// </summary>
		public InterfaceContract()
		{
			types = new SchemaElements();
			isHttpBinding = false;
            bindings |= SoapBindings.Soap11;
		}
		
		#endregion

		#region Public properties.

		/// <summary>
		/// Gets or sets the value indicating whether the generated WSDL requires a "service" element or not.
		/// </summary>
		public bool NeedsServiceElement
		{
			get { return needsServiceElement; }
			set { needsServiceElement = value; }
		}

		/// <summary>
		/// Gets or sets the XML name attribute of the "descriptions" tag enclosing the Web Services 
		/// Description Language (WSDL) file.
		/// </summary>
		public string ServiceName
		{
			get { return serviceName; }
			set { serviceName = value; }
		}

		/// <summary>
		/// Gets or sets the XML targetNamespace attribute of the "descriptions" tag enclosing the 
		/// Web Services Description Language (WSDL) file.
		/// </summary>
		public string ServiceNamespace
		{
			get { return serviceNamespace; }
			set { serviceNamespace = value; }
		}

		/// <summary>
		/// Gets or sets the text documentation for the instance of the InterfaceContract.
		/// </summary>
		public string ServiceDocumentation
		{
			get { return serviceDocumentation; }
			set { serviceDocumentation = value; }
		}
		
		/// <summary>
		/// Gets or sets the <see cref="SchemaImports"/> for the instance of the InterfaceContract.
		/// </summary>
		/// <remarks>This list is placed on the WSDL by enclosing them in the "import" tags.</remarks>
		public SchemaImports Imports
		{
			get 
			{
				if(this.imports == null)
					this.imports = new SchemaImports();
				return this.imports;
			}
			set 
			{
				this.imports = value;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string SchemaNamespace
		{
			get { return schemaNamespace; }
			set { schemaNamespace = value; }
		}

		/// <summary>
		/// Gets the  <see cref="OperationsCollection"/> defined by the InterfaceContract.
		/// </summary>
		public OperationsCollection OperationsCollection
		{
			get
			{
				if(operationsCollection == null) operationsCollection = new OperationsCollection();
				return operationsCollection;
			}
		}
		
		/// <summary>
		/// Gets the <see cref="SchemaElements"/> defined by the InterfaceContract.
		/// </summary>
		/// <remarks>
		/// This property could be used only when loading an existing WSDL using <see cref="GetInterfaceContract"/> 
		/// method. A call to <see cref="GetInterfaceContract"/> initializes this property with type definitions
		/// embedded to WSDL.
		/// </remarks>
		public SchemaElements Types
		{
			get { return  types; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether to use the alternative location for the schemaLocation attribute of 
		/// "imports" tags.
		/// </summary>
		public bool UseAlternateLocationForImports
		{
			get { return this.useAlternateLocationForImports; }
			set { this.useAlternateLocationForImports = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether the current instance of InterfaceContract
		/// class uses 'HttpBinding'.
		/// </summary>
		/// <remarks>
		/// This property is set only when the instance of InterfaceContract is loaded
		/// from an existing WSDL file.
		/// </remarks>
		public bool IsHttpBinding
		{
			get { return this.isHttpBinding; }	
			set { this.isHttpBinding = value; }
		}

        /// <summary>
        /// Gets or sets the SOAP bindings to be included in the WSDL.
        /// </summary>
        public SoapBindings Bindings
        {
            get { return this.bindings; }
            set { this.bindings = value; }
        }

		#endregion

		#region Internal methods
		
		/// <summary>
		/// Initializes the <see cref="Type"/> property.
		/// </summary>
		/// <param name="schemaElements">
		/// An instance of <see cref="SchemaElements"/> class.
		/// </param>
		/// <remarks>This method is accessible only within the assembly.</remarks>
		internal void SetTypes(SchemaElements schemaElements)
		{
			this.types.AddRange(schemaElements);
		}
		
		#endregion
	}
	
	#endregion
	
	#region Operation class

	/// <summary>
	/// Represents the content of an action supported by the XML web service.
	/// </summary>
	public class Operation
	{
		#region Private fields

		private string name;
		private string documentation;
		private Mep mep;
		private MessagesCollection messagesCollection;
		private Message input;
		private Message output;
		private List<Message> faults = new List<Message>();

		#endregion

		#region Public properties

		/// <summary>
		/// Gets or sets the name of the Operation.
		/// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets or sets the documentation text of the Operation.
		/// </summary>
		public string Documentation
		{
			get { return documentation; }
			set { documentation = value; }
		}

		/// <summary>
		/// Gets or sets the message exchange pattern ( <see cref="Mep"/>) of the operation.
		/// </summary>
		public Mep Mep
		{
			get { return mep; }
			set { mep = value; }
		}

		/// <summary>
		/// Gets or sets the  <see cref="MessagesCollection"/> exchanged by the operation.
		/// </summary>
		/// <remarks>
		/// Typically this messages collection contains an in bound message, optional out bound message 
		/// and header messages.
		/// </remarks>
		public MessagesCollection MessagesCollection
		{
			get
			{
				if (messagesCollection == null) messagesCollection = new MessagesCollection();
				return messagesCollection;
			}
		}

		/// <summary>
		/// Gets or sets the input  <see cref="Message"/> for the instance of Operation class.
		/// </summary>
		public Message Input
		{
			get { return this.input; }
			set { this.input = value; }
		}

		/// <summary>
		/// Gets or sets the output <see cref="Message"/> for the instance of Operation class.
		/// </summary>
		public Message Output
		{
			get { return this.output; }
			set { this.output = value; }
		}

		/// <summary>
		/// Gets or sets the fault messages for the instance of Operation class.
		/// </summary>
		public List<Message> Faults
		{
			get { return faults; }
		}

		#endregion
	}

	#endregion

	#region Messages class

	/// <summary>
	/// Represents the content of the Message exchanged by XML web service.
	/// </summary>
	public class Message
	{
		#region Private fields.

		private string name;
		private string documentation;
		private SchemaElement element;
		private MessageHeadersCollection headersCollection;
		
		#endregion

		#region Constructors
		
		/// <summary>
		/// Initializes a new instance of the Message class.
		/// </summary>
		public Message()
		{
			element = new SchemaElement();
		}

		#endregion
		
		#region Public properties
		/// <summary>
		/// Gets or sets the MessageHeader(s) for the current instance of the Message.
		/// </summary>
		public MessageHeadersCollection HeadersCollection
		{
			get
			{
				if (headersCollection == null) headersCollection = new MessageHeadersCollection();
				return headersCollection;
			}
		}

		/// <summary>
		/// Gets or sets the name of the Message.
		/// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets or sets the SchemaElement of the Message.
		/// </summary>
		public SchemaElement Element
		{
			get { return element; }
			set { element = value; }
		}

		/// <summary>
		/// Gets or sets the documentation text of the Message.
		/// </summary>
		public string Documentation
		{
			get { return documentation; }
			set { documentation = value; }
		}

		#endregion
	}
	
	#endregion

	#region MessageHeader class
	/// <summary>
	/// Represents the content of the Message headers exchanged by the XML web service.
	/// </summary>
	public class MessageHeader
	{
		#region Private fields

		private string name;
		private string message;
		private string part;

		#endregion

		#region Public properties

		/// <summary>
		/// Gets or sets the name of the MessageHeader.
		/// </summary>
		public string Name
		{
			get { return name; }
			set { name = value; }
		}

		/// <summary>
		/// Gets or sets the name of the Message for the instance of MessageHeader.
		/// </summary>
		public string Message
		{
			get { return message; }
			set { message = value; }
		}

		/// <summary>
		/// Gets or sets the message part of the instance of the MessageHeader.
		/// </summary>
		public string Part
		{
			get { return part; }
			set { part = value; }
		}

		#endregion
	}

	#endregion

	#region Mep enumeration

	/// <summary>
	/// Defines the available options for the Message Exchange Patterns used by the XML web service.
	/// </summary>
	public enum Mep
	{
		/// <summary>
		/// Indicates the message is a one-way message.
		/// </summary>
		OneWay,
		/// <summary>
		/// Indicates the message is a request/response (two-way) message.
		/// </summary>
		RequestResponse
	}

	#endregion

	#region SchemaImport class

	/// <summary>
	/// Represents the details of the imported schemas to the WSDL.
	/// </summary>
	public class SchemaImport
	{
		#region Private fields
		
		private string schemaName;
		private string schemaLocation;
		private string schemaNamespace;
		private string alternateLocation;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of SchemaImport class.
		/// </summary>
		public SchemaImport()
		{

		}

		/// <summary>
		/// Initializes a new instance of the SchemaImport class with the specified values.
		/// </summary>
		/// <param name="schemaLocation">File location of the imported schema.</param>
		/// <param name="schemaNamespace">Namespace of the imported schema.</param>
		/// <param name="schemaName">Name of the imported schema.</param>
		public SchemaImport(string schemaLocation, string schemaNamespace, string schemaName)
		{
			this.schemaLocation = schemaLocation;
			this.schemaNamespace = schemaNamespace;
			this.schemaName = schemaName;
		}

		#endregion
		
		#region Public properties
		
		/// <summary>
		/// Gets or sets the name of the imported schema.
		/// </summary>
		public string SchemaName
		{
			get { return this.schemaName; }
			set { this.schemaName = value; }
		}

		/// <summary>
		/// Gets or sets the file name of the imported schema.
		/// </summary>
		public string SchemaLocation
		{
			get { return this.schemaLocation; }
			set { this.schemaLocation = value; }
		}

		/// <summary>
		/// Gets or sets the namespace of the imported schema.
		/// </summary>
		public string SchemaNamespace
		{
			get { return this.schemaNamespace; }
			set { this.schemaNamespace = value; }
		}

		#endregion
		
		/// <summary>
		/// Gets or sets the alternate location for the imported schema.
		/// </summary>
		public string AlternateLocation
		{
			get { return this.alternateLocation; }
			set { this.alternateLocation = value; }
		}
	}
	
	#endregion

	#region SchemaImports class

	/// <summary>
	/// Represents a collection of <see cref="SchemaImport"/> objects.
	/// </summary>
	public class SchemaImports : CollectionBase
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of a SchemaImports class.
		/// </summary>
		public SchemaImports()
		{
		}

		#endregion
		
		#region Public methods

		/// <summary>
		/// Adds the specified <see cref="SchemaImport"/> to the end of the SchemaImports collection. 
		/// </summary>
		/// <param name="value">The <see cref="SchemaImport"/> to add to the collection.</param>
		/// <returns>The zero-based index where the value parameter has been added.</returns>
		public int Add(SchemaImport value)
		{
			return this.InnerList.Add(value);
		}
		
		#endregion

		#region Public properties

		/// <summary>
		/// Property indexer for the SchemaImports class. Gets or sets the value of a <see cref="SchemaImport"/>
		/// at the specified zero-based index.
		/// </summary>
		public SchemaImport this[int index]
		{
			get { return (SchemaImport)this.InnerList[index]; }
			set { this.InnerList[index] = value; }
		}
		
		#endregion
	}
	
	#endregion

	#region SchemaElement class

	/// <summary>
	/// Represents the contents of Schema Element in a XML document.
	/// </summary>
	public class SchemaElement
	{
		#region Private fields

		private string elementNamespace;
		private string elementName;

		#endregion

		#region Constructors

		/// <summary>
		/// Initializes a new instance of a SchemaElement class.
		/// </summary>
		public SchemaElement()
		{
		}
		
		/// <summary>
		/// Initializes a new instance of a SchemaElement class with the specified values.
		/// </summary>
		/// <param name="elementNamespace">Namespace name of the schema element.</param>
		/// <param name="elementName">Name of the schema element.</param>
		public SchemaElement(string elementNamespace, string elementName)
		{
			this.elementNamespace = elementNamespace;
			this.elementName = elementName;
		}

		#endregion


		#region Public properties

		/// <summary>
		/// Gets or sets the namespace name of the SchemaElement.
		/// </summary>
		public string ElementNamespace
		{
			get { return this.elementNamespace; }
			set { this.elementNamespace = value; }
		}

		/// <summary>
		/// Gets or sets the name of the SchemaElement.
		/// </summary>
		public string ElementName
		{
			get { return this.elementName; }
			set { this.elementName = value; }
		}

		#endregion

		#region Operator overloadings
		
		/// <summary>
		/// Compares two InterfaceContract objects for equality.
		/// </summary>
		/// <param name="left">L value of the comparison operator.</param>
		/// <param name="right">R value of the comparison operator.</param>
		/// <returns>A Boolean value indicating whether the two instances are equal or not.</returns>
		public static bool operator ==(SchemaElement left, SchemaElement right)
		{
			return(left.ElementName.ToLower() == right.ElementName.ToLower() && 
				((left.ElementNamespace == null && right.ElementNamespace == null) || 
                left.ElementNamespace.ToLower() == right.ElementNamespace.ToLower()));
		}
		
		/// <summary>
		/// Compares two InterfaceContract objects for inequality.
		/// </summary>
		/// <param name="left">L value of the comparison operator.</param>
		/// <param name="right">R value of the comparison operator.</param>
		/// <returns>A Boolean value indicating whether the two instances are unequal or not.</returns>
		public static bool operator !=(SchemaElement left, SchemaElement right)
		{
            return(!(left.ElementName.ToLower() == right.ElementName.ToLower() &&
                ((left.ElementNamespace == null && right.ElementNamespace == null) ||
                left.ElementNamespace.ToLower() == right.ElementNamespace.ToLower())));
		}
		
		#endregion

		#region Overloaded functions
		
		/// <summary>
		/// Overrides the default Equals function. This function checks whether a given object is 
		/// equal to the current instance.
		/// </summary>
		/// <param name="obj">Object to compare with current instance.</param>
		/// <returns>A Boolean value indicating whether the objects are equal or not.</returns>
		public override bool Equals(object obj)
		{			
			return (this == (SchemaElement)obj);
		}

		/// <summary>
		/// Overrides the default GetHashCode() function. This function builds the hash code for 
		/// this instance by combining all the hash codes of private fields of the current instance.
		/// </summary>
		/// <returns>An integer value indicating the Hash code for this instance.</returns>
		public override int GetHashCode()
		{
			return this.ElementName.GetHashCode() ^ this.ElementNamespace.GetHashCode();
		}

		#endregion

	}
	
	#endregion
	
	#region SchemaElements class

	/// <summary>
	/// Represents a collection of <see cref="SchemaElement"/> objects.
	/// </summary>
	public class SchemaElements : CollectionBase
	{
		#region Constructors

		/// <summary>
		/// Initializes a new instance of a SchemaElements class.
		/// </summary>
		public SchemaElements()
		{
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Adds the specified <see cref="SchemaElement"/> to the end of the SchemaElements collection. 
		/// </summary>
		/// <param name="value">The <see cref="SchemaElement"/> to add to the collection.</param>
		/// <returns>The zero-based index where the value parameter has been added.</returns>
		public int Add(SchemaElement value)
		{
			return this.InnerList.Add(value);
		}

		/// <summary>
		/// Adds the elements in a collection of SchemaElements to the end of the SchemaElements.
		/// </summary>
		/// <param name="values">The SchemaElements collection to add to this collection.</param>
		/// <returns>The number of items in the collection after adding the new SchemaElements.</returns>
		public int AddRange(SchemaElements values)
		{
			foreach(SchemaElement item in values)
			{
				this.InnerList.Add(item);
			}

			return this.InnerList.Count;
		}
		
		/// <summary>
		/// Checks whether a given schema element exists in the collection.
		/// </summary>
		/// <param name="value">Reference to an instance of SchemaElement class to find.</param>
		/// <returns>A value indicating whether the schema element is found in the collection or not.</returns>
		public bool Contains(SchemaElement value)
		{
			foreach(SchemaElement e in this.InnerList)
			{
				if(e.ElementName.ToLower() == value.ElementName.ToLower() && 
					e.ElementNamespace.ToLower() == value.ElementNamespace.ToLower())
				{
					return true;
				}
			}

			return false;
		}

		#endregion

		#region Public properties

		/// <summary>
		/// Property indexer for the SchemaElements class. Gets or sets the value of a <see cref="SchemaElement"/>
		/// object at the specified zero-based index.
		/// </summary>
		public SchemaElement this[int index]
		{
			get { return (SchemaElement)this.InnerList[index]; }
			set { this.InnerList[index] = value; }
		}

		#endregion
	}

	#endregion
}