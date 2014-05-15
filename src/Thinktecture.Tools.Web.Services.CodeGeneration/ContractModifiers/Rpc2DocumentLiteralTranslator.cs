using System;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
	class R2DBinding
	{
		private XmlElement elem;
		public XmlElement Elem
		{
			get
			{
				return elem;
			}

			set
			{
				elem = value;
			}
		}

		private XmlNamespaceManager nsmgr;
		private R2DPortType portType;
		private string name;

		public R2DBinding(XmlElement elem, XmlNamespaceManager nsmgr)
		{
			this.name = elem.Attributes["name"].Value;
			this.elem = elem;
			this.nsmgr = nsmgr;
		}

		public void SetupRelations(ElementStore store, Rpc2DocumentLiteralTranslator trans)
		{
			portType = (R2DPortType) store.PortTypes[trans.Normalize(elem.Attributes["type"].Value)];
		}

		public R2DPortType GetPortType()
		{
			return portType;
		}

		public void Translate()
		{
			elem.ParentNode.InsertBefore(elem.OwnerDocument.CreateComment("Changed style from rpc to document"), elem);
			elem["binding", "http://schemas.xmlsoap.org/wsdl/soap/"].Attributes["style"].Value = "document";
			portType.Translate();
			XmlNodeList soapBodies = elem.SelectNodes("wsdl:operation/wsdl:*/soap:body", nsmgr);
			foreach (XmlElement body in soapBodies)
			{
				XmlAttribute parts = body.GetAttributeNode("parts");
				if (parts != null)
				{
					string comment = string.Format(CultureInfo.CurrentCulture, "Changed <soap:body parts=\"{0}\" ... /> to <soap:body parts=\"parameter\" ... />", parts.Value );
					body.ParentNode.InsertBefore(body.OwnerDocument.CreateComment(comment), body);
					parts.Value = "parameter";
				}
				string attrComment="";
				XmlAttribute ns = body.GetAttributeNode("namespace");
				if (ns != null)
				{
					attrComment = "Attribute namespace=\""+ns.Value+"\" has been removed";
					body.ParentNode.InsertBefore(body.OwnerDocument.CreateComment(attrComment), body);
					body.RemoveAttribute(ns.Name);
				}
				XmlAttribute es = body.GetAttributeNode("encodingStyle");
				if (es != null)
				{
					attrComment = "Attribute encodingStyle=\""+es.Value+"\" has been removed";
					body.ParentNode.InsertBefore(body.OwnerDocument.CreateComment(attrComment), body);
					body.RemoveAttribute(es.Name);				}
			}
		}

		public string Name {get{ return name;}}
	}

	class R2DPortType
	{
		private XmlElement elem;
		public XmlElement Elem
		{
			get
			{
				return elem;
			}

			set
			{
				elem = value;
			}
		}
		private XmlNamespaceManager nsmgr;
		private ArrayList operations = new ArrayList();
		private string name;

		public R2DPortType(XmlElement elem, XmlNamespaceManager nsmgr)
		{
			this.name = elem.Attributes["name"].Value;
			this.elem = elem;
			this.nsmgr = nsmgr;
		}

		public void SetupRelations(ElementStore store, Rpc2DocumentLiteralTranslator trans)
		{
			foreach (XmlElement op in elem)
			{
				if (op.LocalName == "operation")
				{
					operations.Add((R2DOperation) store.Operations[trans.Normalize(op.Attributes["name"].Value)]);
				}
			}
		}

//		public ArrayList GetOperations()
//		{
//			return operations;
//		}

		public void Translate()
		{
			foreach (R2DOperation op in operations)
			{
				op.Translate();
			}
		}

		public string Name {get{ return name;}}
	}

	class R2DOperation
	{
		private XmlElement elem;
		public XmlElement Elem
		{
			get
			{
				return elem;
			}

			set
			{
				elem = value;
			}
		}
		private XmlNamespaceManager nsmgr;
		private string name;
		private string outNS;
		private string inNS;
		private R2DMessage inputMsg;
		private R2DMessage outputMsg;
		private bool translated = false;

		public R2DOperation(XmlElement elem, XmlNamespaceManager nsmgr)
		{
			this.name = elem.Attributes["name"].Value;
			this.elem = elem;
			this.nsmgr = nsmgr;		
		}

		public void SetupRelations(ElementStore store, Rpc2DocumentLiteralTranslator trans)
		{
			foreach (XmlElement op in elem)
			{
				if (op.LocalName == "input")
				{
					XmlNodeList inBodies = elem.OwnerDocument.SelectNodes("wsdl:definitions/wsdl:binding/wsdl:operation[@name='" + name +"']/wsdl:input/soap:body", nsmgr);
					inputMsg = ((R2DMessage) store.InputMessages[trans.Normalize(op.Attributes["message"].Value)]);
					inNS = GetNamespace(inBodies);
				}
				if (op.LocalName == "output")
				{
					XmlNodeList outBodies = elem.OwnerDocument.SelectNodes("wsdl:definitions/wsdl:binding/wsdl:operation[@name='" + name +"']/wsdl:output/soap:body", nsmgr);
					outputMsg = ((R2DMessage) store.OutputMessages[trans.Normalize(op.Attributes["message"].Value)]);
					outNS = GetNamespace(outBodies);
				}
			}
		}

		string GetNamespace(XmlNodeList bodyList)
		{
			XmlAttribute attr = bodyList[0].Attributes["namespace"];
			string ns="";
			if (attr != null)
			{
				ns = attr.Value;
				foreach (XmlElement ie in bodyList)
				{
					if (ns != ie.Attributes["namespace"].Value)
					{
						throw new Rpc2DocumentLiteralTranslationException("Can't deal with operations sharing names but not namespaces.");
					}
				}
			}
			else
			{
				throw new Rpc2DocumentLiteralTranslationException("RPC/literal body requires a namespace (WS-I).");
			}
			return ns;
		}

		public void Translate()
		{
			if (translated == false)
			{
				inputMsg.Translate(name, inNS, true);
				if (outputMsg != null)
				{
					outputMsg.Translate(name, outNS, false);
				}
			}
			translated = true;
		}

		public string Name {get{ return name;}}
		public R2DMessage InputMessage {get{ return inputMsg;}}
		public R2DMessage OutputMessage {get{ return outputMsg;}}

	}

	class R2DMessage
	{
		public string name;
		private XmlElement elem;
		public XmlElement Elem
		{
			get
			{
				return elem;
			}

			set
			{
				elem = value;
			}
		}
		private XmlNamespaceManager nsmgr;
		public ArrayList parameters = new ArrayList();

		public R2DMessage(XmlElement elem, XmlNamespaceManager nsmgr)
		{
			this.name = elem.Attributes["name"].Value;
			this.elem = elem;
			this.nsmgr = nsmgr;
		}

		public void SetupRelations(ElementStore store, Rpc2DocumentLiteralTranslator trans)
		{
			foreach (XmlElement op in elem)
			{
				if (op.LocalName == "part")
				{
					string typeNameAttributeName = "type";
					if (op.Attributes["element"] != null)
					{
						typeNameAttributeName = "element";
					}
					parameters.Add(store.Types[op.ParentNode.Attributes["name"].Value + "/" + op.Attributes[typeNameAttributeName].Value]);
				}
			}
		}

		public void Translate(string operationName, string operationNS, bool isInput)
		{
			elem.ParentNode.InsertBefore(elem.OwnerDocument.CreateComment(elem.OuterXml), elem);
			XmlElement schemaElem = (XmlElement) elem.OwnerDocument.SelectSingleNode("wsdl:definitions/wsdl:types/xsd:schema[@targetNamespace='"+operationNS+"']", nsmgr);
			
			if (schemaElem == null )
			{
				XmlElement typesElem = (XmlElement) elem.OwnerDocument.SelectSingleNode("wsdl:definitions/wsdl:types", nsmgr);
				if (typesElem == null)
				{
					throw new Rpc2DocumentLiteralTranslationException("<wsdl:types> not found.");
				}
				schemaElem = typesElem.OwnerDocument.CreateElement("xsd", "schema", "http://www.w3.org/2001/XMLSchema");
				schemaElem.Attributes.Append(typesElem.OwnerDocument.CreateAttribute("targetNamespace"));
				schemaElem.Attributes["targetNamespace"].Value = operationNS;
				typesElem.AppendChild(schemaElem);
			}
			
			string targetNamespace = schemaElem.Attributes["targetNamespace"].Value;
			string targetNamespacePrefix = "r2dtns";
			StringBuilder targetNamespacePrefixSb = new StringBuilder(targetNamespacePrefix);
			XmlAttribute tnsAttr = schemaElem.Attributes[targetNamespacePrefix];
			
			while (tnsAttr != null)
			{
				if (tnsAttr.Value != targetNamespace)
				{
					//targetNamespacePrefix += "x";
					targetNamespacePrefixSb.Append("x");
					tnsAttr = schemaElem.Attributes[targetNamespacePrefixSb.ToString()];
				}
			}
			
			targetNamespacePrefix = targetNamespacePrefixSb.ToString();
			
			if (tnsAttr == null)
			{
				tnsAttr = schemaElem.OwnerDocument.CreateAttribute("xmlns:"+ targetNamespacePrefix);
				tnsAttr.Value = targetNamespace;
				schemaElem.Attributes.Append(tnsAttr);
			}
			
			XmlElement elemElem = elem.OwnerDocument.CreateElement("xsd", "element", "http://www.w3.org/2001/XMLSchema");
			XmlElement typeElem = elem.OwnerDocument.CreateElement("xsd", "complexType", "http://www.w3.org/2001/XMLSchema");
			XmlAttribute nameAttribute = typeElem.OwnerDocument.CreateAttribute("name");
			XmlAttribute typeAttribute = typeElem.OwnerDocument.CreateAttribute("type");
			nameAttribute.Value = GetSchemaElementName(operationName, isInput);

			typeAttribute.Value = targetNamespacePrefix + ":" + nameAttribute.Value;
				
			typeElem.Attributes.Append(nameAttribute);
			elemElem.Attributes.Append((XmlAttribute)nameAttribute.CloneNode(true));
			elemElem.Attributes.Append(typeAttribute);
			schemaElem.InsertBefore(elemElem, schemaElem.FirstChild);
			schemaElem.InsertAfter(typeElem, elemElem);
			XmlElement sequenceElem = elem.OwnerDocument.CreateElement("xsd", "sequence", "http://www.w3.org/2001/XMLSchema");
			typeElem.InsertBefore(sequenceElem, typeElem.FirstChild);
			schemaElem.InsertBefore(elem.OwnerDocument.CreateComment("Begin of generated message type"), elemElem);
			schemaElem.InsertAfter(elem.OwnerDocument.CreateComment("End of generated message type"), typeElem);

			XmlElement part = (XmlElement) elem.FirstChild;
			
			foreach (R2DType p in parameters)
			{
				p.Translate(sequenceElem, part.Attributes["name"].Value, targetNamespacePrefix);
				part = (XmlElement) part.NextSibling;
			}
			
			XmlNode n = elem.LastChild;
			
			while (n!= null)
			{
				XmlNode toDelete = n;
				n = n.PreviousSibling;
				elem.RemoveChild(toDelete);
			}

			XmlElement paramElem = elem.OwnerDocument.CreateElement("wsdl", "part", "http://schemas.xmlsoap.org/wsdl/");
			elem.AppendChild(paramElem);
			XmlAttribute typeNsAttr = elem.OwnerDocument.CreateAttribute("xmlns:typens");
			typeNsAttr.Value = targetNamespace;
			elem.Attributes.Append(typeNsAttr);
			paramElem.Attributes.Append(paramElem.OwnerDocument.CreateAttribute("", "name", ""));
			paramElem.Attributes["name"].Value="parameter";
			paramElem.Attributes.Append(paramElem.OwnerDocument.CreateAttribute("", "element", "" ));
			paramElem.Attributes["element"].Value="typens:" + GetSchemaElementName(operationName, isInput);
		}

		private static string GetSchemaElementName(string operationName, bool isInput)
		{
			return operationName + (isInput==true?"":"Response");
		}

		public string Name {get{ return name;}}
	
	}

	class R2DType
	{
		public string name;
		private XmlElement elem;
		public XmlElement Elem
		{
			get
			{
				return elem;
			}

			set
			{
				elem = value;
			}
		} 
		private XmlNamespaceManager nsmgr;

		public R2DType(XmlElement elem, string name, XmlNamespaceManager nsmgr)
		{
			this.name = name;
			this.elem = elem;
			this.nsmgr = nsmgr;
		}

		public void Translate(XmlElement sequenceElem, string partName, string targetNamespacePrefix)
		{
			XmlElement paramElem = sequenceElem.OwnerDocument.CreateElement("xsd", "element", "http://www.w3.org/2001/XMLSchema");
			XmlAttribute nameAttribute = sequenceElem.OwnerDocument.CreateAttribute("name");
			string nsPrefix = targetNamespacePrefix;
			string typeName = name;
			string[] parts= name.Split(new char[] {':'});
			if (parts.Length == 2)
			{
				nsPrefix = parts[0];
				typeName = parts[1];
			}
			nameAttribute.Value = partName;
		
			paramElem.Attributes.Append(nameAttribute);
			XmlAttribute typeAttribute = sequenceElem.OwnerDocument.CreateAttribute("type");
			typeAttribute.Value = nsPrefix + ":" + typeName;
			paramElem.Attributes.Append(typeAttribute);
			sequenceElem.AppendChild(paramElem);
		}

		public string Name {get{ return name;}}

	}

	public class ElementStore
	{
		private Hashtable bindings = new Hashtable();
		public Hashtable Bindings
		{
			get
			{
				return bindings;
			}

//			set
//			{
//				bindings = value;
//			}
		}
		private Hashtable portTypes = new Hashtable();
		public Hashtable PortTypes
		{
			get
			{
				return portTypes;
			}

//			set
//			{
//				portTypes = value;
//			}
		}
		private Hashtable operations = new Hashtable();
		public Hashtable Operations
		{
			get
			{
				return operations;
			}

//			set
//			{
//				operations = value;
//			}
		}
		private Hashtable inputMessages = new Hashtable();
		public Hashtable InputMessages
		{
			get
			{
				return inputMessages;
			}

//			set
//			{
//				inputMessages = value;
//			}
		}
		private Hashtable outputMessages = new Hashtable();
		public Hashtable OutputMessages
		{
			get
			{
				return outputMessages;
			}

//			set
//			{
//				outputMessages = value;
//			}
		}
		private Hashtable types = new Hashtable();
		public Hashtable Types
		{
			get
			{
				return types;
			}

//			set
//			{
//				types = value;
//			}
		} 

		public void SetupRelations(Rpc2DocumentLiteralTranslator trans)
		{
			foreach(R2DBinding s in bindings.Values)
			{
				s.SetupRelations(this, trans);
			}
			foreach(R2DPortType s in portTypes.Values)
			{
				s.SetupRelations(this, trans);
			}
			foreach(R2DOperation s in operations.Values)
			{
				s.SetupRelations(this, trans);
			}
			foreach(R2DMessage s in inputMessages.Values)
			{
				s.SetupRelations(this, trans);
			}
			foreach(R2DMessage s in outputMessages.Values)
			{
				s.SetupRelations(this, trans);
			}
		}

		public void Translate()
		{
			foreach(R2DBinding b in bindings.Values)
			{
				b.Translate();
			}
		}
	}

	public class Rpc2DocumentLiteralTranslator
	{
		private XmlDocument xdoc;
		private XmlNamespaceManager nsmgr;
		private string targetNamespace;

		private ElementStore store = new ElementStore();
		public ElementStore ElementStore {get{ return store;}}

		public static Rpc2DocumentLiteralTranslator Translate(string wsdlFilename, string newWsdlFilename)
		{
			Rpc2DocumentLiteralTranslator ret;
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(wsdlFilename);
			ret = Translate(xdoc);
			StreamWriter sw = new StreamWriter(newWsdlFilename);
			xdoc.Save(sw);
			sw.Close();
			return ret;
		}

		public static Rpc2DocumentLiteralTranslator Translate(XmlDocument wsdl)
		{
			Rpc2DocumentLiteralTranslator translator = new Rpc2DocumentLiteralTranslator(wsdl);
			translator.TranslateStore();
			return translator;
		}

		private Rpc2DocumentLiteralTranslator(XmlDocument wsdl)
		{
			xdoc = wsdl;
			nsmgr = new XmlNamespaceManager(xdoc.NameTable);

			XmlElement root = (XmlElement) xdoc.DocumentElement;
			foreach (XmlAttribute a in root.Attributes)
			{
				if (a.Prefix == "xmlns")
				{
					Trace.WriteLine(a.LocalName + " = " + a.Value);
					nsmgr.AddNamespace(a.LocalName, a.Value);
				}
				if (a.Name == "targetNamespace")
				{
					targetNamespace = a.Value;
				}
			}
			if (nsmgr.HasNamespace("wsdl") == false)
			{
				nsmgr.AddNamespace("wsdl", "http://schemas.xmlsoap.org/wsdl/");
			}
			if (nsmgr.HasNamespace("xsd") == false)
			{
				nsmgr.AddNamespace("xsd", "http://www.w3.org/2001/XMLSchema");
			}
			if (nsmgr.HasNamespace("soap") == false)
			{
				nsmgr.AddNamespace("soap", "http://schemas.xmlsoap.org/wsdl/soap/");
			}
			BuildItems(nsmgr);
		}

		public bool ContainsRpcLiteralBindings()
		{
			return store.Bindings.Count > 0;
		}

		public static bool ContainsRpcLiteralBindings(XmlDocument wsdl)
		{
			Rpc2DocumentLiteralTranslator translator = new Rpc2DocumentLiteralTranslator(wsdl);
			return translator.ContainsRpcLiteralBindings();
		}

		public static bool ContainsRpcLiteralBindings(string wsdlFilename)
		{
			XmlDocument xdoc = new XmlDocument();
			xdoc.Load(wsdlFilename);
			return ContainsRpcLiteralBindings(xdoc);
		}

		private void TranslateStore()
		{
			store.Translate();
		}
	
		private bool BuildItems(XmlNamespaceManager nsmgr)
		{
			XmlNodeList list = GetAffectedBindings();
			if (list == null)
			{
				return false;
			}
			else
			{
				foreach(XmlElement n in list)
				{
					store.Bindings.Add(n.Attributes["name"].Value, new R2DBinding(n, nsmgr));
				}
			}
			list = GetAffectedPortTypes();
			if (list != null)
			{
				foreach(XmlElement n in  list)
				{
					store.PortTypes.Add(n.Attributes["name"].Value, new R2DPortType(n, nsmgr));
				}
			}
			list = GetAffectedOperations();
			if (list != null)
			{
				foreach(XmlElement n in  list)
				{
					store.Operations.Add(n.Attributes["name"].Value, new R2DOperation(n, nsmgr));
				}
			}
			list = GetAffectedInputMessages();
			if (list != null)
			{
				foreach(XmlElement n in  list)
				{
					store.InputMessages.Add(n.Attributes["name"].Value, new R2DMessage(n, nsmgr));
				}
			}
			list = GetAffectedOutputMessages();
			if (list != null)
			{
				foreach(XmlElement n in  list)
				{
					store.OutputMessages.Add(n.Attributes["name"].Value, new R2DMessage(n, nsmgr));
				}
			}
			list = GetAffectedInputTypes();
			if (list != null)
			{
				foreach(XmlElement n in  list)
				{
					string typeNameAttributeName = "type";
					if (n.Attributes["element"] != null)
					{
						typeNameAttributeName = "element";
					}
					try
					{
						store.Types.Add(n.ParentNode.Attributes["name"].Value + "/" + n.Attributes[typeNameAttributeName].Value, new R2DType(n, n.Attributes[typeNameAttributeName].Value, nsmgr));
					}
					catch (ArgumentException)
					{
						// that's fine
					}
				}
			}
			list = GetAffectedOutputTypes();
			if (list != null)
			{
				foreach(XmlElement n in  list)
				{
					string typeNameAttributeName = "type";
					if (n.Attributes["element"] != null)
					{
						typeNameAttributeName = "element";
					}
					try
					{
						store.Types.Add(n.ParentNode.Attributes["name"].Value + "/" + n.Attributes[typeNameAttributeName].Value, new R2DType(n, n.Attributes[typeNameAttributeName].Value, nsmgr));
					}
					catch (ArgumentException)
					{
						// that's fine
					}
				}
			}
			store.SetupRelations(this);
			return true;
		}

		private XmlNodeList GetAffectedBindings()
		{
			XmlNodeList list = xdoc.SelectNodes("/wsdl:definitions/wsdl:binding[(soap:binding/@style='rpc') and (./wsdl:operation/*/soap:body/@use='literal')]", nsmgr); 

			foreach (XmlNode n in list)
			{
				Trace.WriteLine(" name: " + n.Attributes["name"].Value);
			}
			if (list.Count == 0)
			{
				list = null;
			}
			return list;
		}

		private XmlNodeList GetAffectedInputTypes()
		{
			return GetAffectedTypes(GetAffectedInputMessages());
		}

		private XmlNodeList GetAffectedOutputTypes()
		{
			return GetAffectedTypes(GetAffectedOutputMessages());
		}

		private XmlNodeList GetAffectedTypes(XmlNodeList im)
		{
			XmlNodeList types = null;
			if (im != null)
			{
				StringBuilder sb = new StringBuilder(1000);
				foreach (XmlNode n in im)
				{
					if (sb.Length != 0)
					{
						sb.Append(" or ");
					}
					sb.AppendFormat("../@name='{0}'", Normalize(n.Attributes["name"].Value));
				}

				string selectStr = String.Format(CultureInfo.CurrentCulture, "/wsdl:definitions/wsdl:message/wsdl:part[{0}]", sb.ToString());
				types = xdoc.SelectNodes(selectStr, nsmgr);

				foreach (XmlNode p in types)
				{
					if (p.Attributes["type"] != null)
					{
						Trace.WriteLine("affected types: " + p.Attributes["type"].Value);
					}
					if (p.Attributes["element"] != null)
					{
						Trace.WriteLine("affected types: " + p.Attributes["element"].Value);
					}
				}
			}
			return types;
		}

		private XmlNodeList GetAffectedOutputMessages()
		{
			return GetAffectedMessages(GetAffectedOutputMessageRefs());
		}

		private XmlNodeList GetAffectedInputMessages()
		{
			return GetAffectedMessages(GetAffectedInputMessageRefs());
		}

		private XmlNodeList GetAffectedMessages(XmlNodeList m)
		{
			XmlNodeList msgs = null;
			if (m != null)
			{
				StringBuilder sb = new StringBuilder(1000);
				foreach (XmlNode n in m)
				{
					if (sb.Length != 0)
					{
						sb.Append(" or ");
					}
					sb.AppendFormat("(@name='{0}')", Normalize(n.Attributes["message"].Value));
				}

				string selectStr = String.Format(CultureInfo.CurrentCulture, "/wsdl:definitions/wsdl:message[{0}]", sb.ToString());
				msgs = xdoc.SelectNodes(selectStr, nsmgr);

				foreach (XmlNode p in msgs)
				{
					Trace.WriteLine("message: " + p.InnerXml);
				}
				
				if (msgs.Count == 0)
				{
					msgs = null;
				}
			}
			return msgs;
		}

		private XmlNodeList GetAffectedInputMessageRefs()
		{
			return GetAffectedMessageRefs("input");
		}

		private XmlNodeList GetAffectedOutputMessageRefs()
		{
			return GetAffectedMessageRefs("output");
		}

		private XmlNodeList GetAffectedMessageRefs(string direction)
		{
			XmlNodeList msgs = null;
			XmlNodeList ports = GetAffectedPortTypes();

			if (ports != null)
			{
				StringBuilder sb = new StringBuilder(1000);
				foreach (XmlNode n in ports)
				{
					if (sb.Length != 0)
					{
						sb.Append(" or ");
					}
					sb.AppendFormat("(../../@name='{0}')", n.Attributes["name"].Value);
				}

				string selectStr = String.Format(CultureInfo.CurrentCulture, "/wsdl:definitions/wsdl:portType/wsdl:operation/wsdl:{0}[{1}]", direction, sb.ToString());
				msgs = xdoc.SelectNodes(selectStr, nsmgr);
				foreach (XmlNode p in msgs)
				{
					Trace.WriteLine(direction + " msg: " + p.Attributes["message"].Value);
				}
				if (msgs.Count == 0)
				{
					msgs = null;
				}
			}
			return msgs;
		}

		private XmlNodeList GetAffectedOperations()
		{
			XmlNodeList ops = null;
			XmlNodeList ports = GetAffectedPortTypes();

			if (ports != null)
			{
				StringBuilder sb = new StringBuilder(1000);
				foreach (XmlNode n in ports)
				{
					if (sb.Length != 0)
					{
						sb.Append(" or ");
					}
					sb.AppendFormat("(../@name='{0}')", n.Attributes["name"].Value);
				}

				string selectStr = String.Format(CultureInfo.CurrentCulture, "/wsdl:definitions/wsdl:portType/wsdl:operation[{0}]", sb.ToString());
				ops = xdoc.SelectNodes(selectStr, nsmgr);
				foreach (XmlNode p in ops)
				{
					Trace.WriteLine("operation: " + p.Attributes["name"].Value);
				}
				if (ops.Count == 0)
				{
					ops = null;
				}
			}
			return ops;
		}

		private XmlNodeList GetAffectedPortTypes()
		{
			XmlNodeList ports = null;
			XmlNodeList bindings = GetAffectedBindings();

			if (bindings != null)
			{
				StringBuilder sb = new StringBuilder(1000);
				foreach (XmlNode n in bindings)
				{
					if (sb.Length != 0)
					{
						sb.Append(" or ");
					}
					sb.AppendFormat("(@name='{0}')", Normalize(n.Attributes["type"].Value));
				}

				string selectStr = String.Format(CultureInfo.CurrentCulture, "/wsdl:definitions/wsdl:portType[{0}]", sb.ToString());
				ports = xdoc.SelectNodes(selectStr, nsmgr);
				foreach (XmlNode p in ports)
				{
					Trace.WriteLine("port name: " + p.Attributes["name"].Value);
				}
			}
			return ports;
		}

		public string Normalize(string normalizeValue)
		{
			return RemoveTargetNamespace(MakeFullQualified(normalizeValue));
		}

		private string RemoveTargetNamespace(string val)
		{
			string ts = targetNamespace;
			if (	ts.EndsWith("/")==false)
			{
				ts += "/";
			}
			val = val.Replace(ts, "");
			return val;
		}

		private string MakeFullQualified(string val)
		{
			foreach (string p in nsmgr)
			{
				if (p.Length > 0)
				{
					string ns = nsmgr.LookupNamespace(p);
					StringBuilder nsSb = new StringBuilder(ns);
					string pref = p+":";

					if (nsSb.ToString().EndsWith("/") == false)
					{
						//ns += "/";
						nsSb.Append("/");
					}
					if (val.IndexOf(":") == (pref.Length -1))
					{
						val = val.Replace(pref, nsSb.ToString());
					}
				}
			}
			return val;
		}
	}
}
