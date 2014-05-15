using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Globalization;
using FxMessage = System.Web.Services.Description.Message;
using FxOperation = System.Web.Services.Description.Operation;
using System.ServiceModel.Description;
using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Channels;

using Binding = System.Web.Services.Description.Binding;

namespace Thinktecture.Tools.Web.Services.ServiceDescription
{
    #region ServiceDescriptionEngine class

    /// <summary>
    /// Provides static methods for WSDL generation. 
    /// </summary>
    /// <remarks>This class could not be inherited.</remarks>
    public sealed class ServiceDescriptionEngine
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of ServiceDescriptionEngine class.
        /// </summary>
        private ServiceDescriptionEngine()
        { }

        #endregion

        #region Public static methods.

        /// <summary>
        /// Generates the WSDL file for specified <see cref="InterfaceContract"/>.
        /// </summary>
        /// <param name="serviceInterfaceContract">
        /// <see cref="InterfaceContract"/> to use for the WSDL generation.
        /// </param>
        /// <param name="wsdlSaveLocation">Location to save the generated WSDL file.</param>
        /// <param name="xmlComment">XML comment to add to the top of the WSDL file.</param>
        /// <returns>The path of the WSDL file generated.</returns>
        public static string GenerateWsdl(InterfaceContract serviceInterfaceContract,
            string wsdlSaveLocation, string xmlComment)
        {
            return GenerateWsdl(serviceInterfaceContract, wsdlSaveLocation, xmlComment, null);
        }

        /// <summary>
        /// Generates the WSDL file for a specified <see cref="InterfaceContract"/>.
        /// </summary>
        /// <param name="serviceInterfaceContract">
        /// <see cref="InterfaceContract"/> to use for the WSDL generation.
        /// </param>
        /// <param name="wsdlSaveLocation">Location to save the generated WSDL file.</param>
        /// <param name="xmlComment">XML comment to add to the top of the WSDL file.</param>
        /// <param name="wsdlLocation">Path of an existing WSDL file to overwrite with the generated 
        /// WSDL file.</param>
        /// <returns>The path of the WSDL file generated.</returns>
        /// <remarks>
        /// This methods loads the information, it receive in a <see cref="InterfaceContract"/> to
        /// a <see cref="System.Web.Services.Description.ServiceDescription"/> class, which is later 
        /// used to generate the WSDL file. The loading process takes place in several steps. <br></br>
        /// 1. Load the basic meta data from <see cref="InterfaceContract"/>.<br></br>
        /// 2. Load the schema imports in the <see cref="SchemaImports"/> collection.<br></br>
        /// 3. Load the messages in <see cref="OperationsCollection"/>.<br></br>
        /// 4. Create the WSDL Port Type.<br></br>
        /// 5. Add each operation and it's corresponding in/out messages to the Port Type.<br></br>
        /// 6. Create a WSDL Binding section and add OperationBinding for each operation.<br></br>
        /// 7. Generate the WSDL 'service' tags if required.<br></br>
        /// 8. Finally write the file to the output stream.<br></br>
        /// 
        /// This method generates <see cref="WsdlGenerationException"/> exception, if it fails to create the WSDL file.
        /// If a file is specified to overwrite with the new file, the original file is restored in case of
        /// a failure.
        /// </remarks>
        public static string GenerateWsdl(InterfaceContract serviceInterfaceContract,
            string wsdlSaveLocation, string xmlComment, string wsdlLocation)
        {
            System.Web.Services.Description.ServiceDescription desc = null;

            string serviceAttributeName = "";
            string bindingName = "";
            string serviceName = "";
            string portTypeName = "";

            // Load the existing WSDL if one specified.
            if (wsdlLocation != null)
            {
                #region Round-tripping

                desc = System.Web.Services.Description.ServiceDescription.Read(wsdlLocation);

                // Read the existing name values.
                serviceAttributeName = desc.Name;
                bindingName = desc.Bindings[0].Name;
                portTypeName = desc.PortTypes[0].Name;

                // Check whether we have a service element and save it's name for the 
                // future use.
                if (desc.Services.Count > 0)
                {
                    serviceName = desc.Services[0].Name;
                }
                else
                {
                    serviceName = serviceInterfaceContract.ServiceName + "Port"; ;
                }

                // Check for the place which has the Service name and assign the new value 
                // appropriatly.			
                if (serviceAttributeName != null && serviceAttributeName != "")
                {
                    serviceAttributeName = serviceInterfaceContract.ServiceName;
                }
                else if (serviceName != null && serviceName != "")
                {
                    // If the user has selected to remove the service element, 
                    // use the service name in the attribute by default.
                    if (serviceInterfaceContract.NeedsServiceElement)
                    {
                        serviceName = serviceInterfaceContract.ServiceName;
                    }
                    else
                    {
                        serviceAttributeName = serviceInterfaceContract.ServiceName;
                    }
                }
                else if (bindingName != null && bindingName != "")
                {
                    bindingName = serviceInterfaceContract.ServiceName;
                }

                // Clear the service description. But do not clear the types definitions.
                desc.Extensions.Clear();
                desc.Bindings.Clear();
                desc.Documentation = "";
                desc.Imports.Clear();
                desc.Messages.Clear();
                desc.PortTypes.Clear();
                desc.RetrievalUrl = "";

                if (desc.ServiceDescriptions != null)
                {
                    desc.ServiceDescriptions.Clear();
                }

                if (!serviceInterfaceContract.NeedsServiceElement)
                {
                    desc.Services.Clear();
                }
                #endregion
            }
            else
            {
                #region New WSDL

                desc = new System.Web.Services.Description.ServiceDescription();

                // Create the default names.
                serviceAttributeName = serviceInterfaceContract.ServiceName;
                bindingName = serviceInterfaceContract.ServiceName;
                portTypeName = serviceInterfaceContract.ServiceName + "Interface";
                serviceName = serviceInterfaceContract.ServiceName + "Port";

                #endregion
            }

            #region Load the basic meta data.
            if (serviceAttributeName != null && serviceAttributeName != "")
            {
                desc.Name = serviceAttributeName;
            }
            desc.TargetNamespace = serviceInterfaceContract.ServiceNamespace;
            desc.Documentation = serviceInterfaceContract.ServiceDocumentation;
            #endregion

            #region Load the schema imports.

            XmlSchema typesSchema = null;

            // Are we round-tripping? Then we have to access the existing types 
            // section.
            // Otherwise we just initialize a new XmlSchema for types.
            if (wsdlLocation != null)
            {
                typesSchema = desc.Types.Schemas[desc.TargetNamespace];
                // if we don't have a types section belonging to the same namespace as service description
                // we take the first types section available.                
                if (typesSchema == null)
                {
                    typesSchema = desc.Types.Schemas[0];
                }
                // Remove the includes. We gonna re-add them later in this operation.
                typesSchema.Includes.Clear();
            }
            else
            {
                typesSchema = new XmlSchema();
            }

            // Add imports to the types section resolved above.
            foreach (SchemaImport import in serviceInterfaceContract.Imports)
            {
                XmlSchemaExternal importedSchema = null;
                if (import.SchemaNamespace == null || import.SchemaNamespace == "")
                {
                    importedSchema = new XmlSchemaInclude();
                }
                else
                {
                    importedSchema = new XmlSchemaImport();
                    ((XmlSchemaImport)importedSchema).Namespace = import.SchemaNamespace;
                }
                if (serviceInterfaceContract.UseAlternateLocationForImports)
                {
                    importedSchema.SchemaLocation = import.AlternateLocation;
                }
                else
                {
                    importedSchema.SchemaLocation = import.SchemaLocation;
                }
                typesSchema.Includes.Add(importedSchema);
            }

            // If we are not round-tripping we have to link the types schema we just created to 
            // the service description.
            if (wsdlLocation == null)
            {
                // Finally add the type schema to the ServiceDescription.Types.Schemas collection.
                desc.Types.Schemas.Add(typesSchema);
            }

            #endregion

            #region Load the messages in all the operations

            MessageCollection msgs = desc.Messages;

            foreach (Operation op in serviceInterfaceContract.OperationsCollection)
            {
                foreach (Message msg in op.MessagesCollection)
                {
                    FxMessage tempMsg = new FxMessage();
                    tempMsg.Name = msg.Name;
                    tempMsg.Documentation = msg.Documentation;

                    MessagePart msgPart = new MessagePart();
                    msgPart.Name = Constants.DefaultMessagePartName;
                    msgPart.Element = new XmlQualifiedName(msg.Element.ElementName,
                            msg.Element.ElementNamespace);
                    tempMsg.Parts.Add(msgPart);

                    msgs.Add(tempMsg);
                }

				foreach (Message msg in op.Faults)
				{
					Message messageName = msg;
					if (msgs.OfType<FxMessage>().Any(m => m.Name == messageName.Name)) continue;

					FxMessage tempMsg = new FxMessage();
					tempMsg.Name = msg.Name;
					tempMsg.Documentation = msg.Documentation;

					MessagePart msgPart = new MessagePart();
					msgPart.Name = Constants.FaultMessagePartName;
					msgPart.Element = new XmlQualifiedName(msg.Element.ElementName, msg.Element.ElementNamespace);
					tempMsg.Parts.Add(msgPart);

					msgs.Add(tempMsg);
				}
            }

            #endregion

            #region Create the Port Type

            PortTypeCollection portTypes = desc.PortTypes;
            PortType portType = new PortType();
            portType.Name = portTypeName;
            portType.Documentation = serviceInterfaceContract.ServiceDocumentation;

            // Add each operation and it's corresponding in/out messages to the WSDL Port Type.
            foreach (Operation op in serviceInterfaceContract.OperationsCollection)
            {
                FxOperation tempOperation = new FxOperation();
                tempOperation.Name = op.Name;
                tempOperation.Documentation = op.Documentation;
                int i = 0;

                OperationInput operationInput = new OperationInput();
                operationInput.Message = new XmlQualifiedName(op.MessagesCollection[i].Name, desc.TargetNamespace);

                tempOperation.Messages.Add(operationInput);

                if (op.Mep == Mep.RequestResponse)
                {
                    OperationOutput operationOutput = new OperationOutput();
                    operationOutput.Message = new XmlQualifiedName(op.MessagesCollection[i + 1].Name, desc.TargetNamespace);

                    tempOperation.Messages.Add(operationOutput);
                }

				foreach (Message fault in op.Faults)
				{
					OperationFault operationFault = new OperationFault();
					operationFault.Name = fault.Name;
					operationFault.Message = new XmlQualifiedName(fault.Name, desc.TargetNamespace);
					tempOperation.Faults.Add(operationFault);
				}
				
                portType.Operations.Add(tempOperation);
                i++;
            }

            portTypes.Add(portType);

            #endregion

            // Here we have a list of WCF endpoints.
            // Currently we populate this list with only two endpoints that has default
            // BasicHttpBinding and default NetTcpBinding.
            List<ServiceEndpoint> endpoints = new List<ServiceEndpoint>();

            BasicHttpBinding basicHttpBinding = new BasicHttpBinding();
            endpoints.Add(ServiceEndpointFactory<IDummyContract>.CreateServiceEndpoint(basicHttpBinding));

            // BDS (10/22/2007): Commented out the TCP binding generation as we are not going to support this feature
            // in this version.
            //NetTcpBinding netTcpBinding = new NetTcpBinding();
            //endpoints.Add(ServiceEndpointFactory<IDummyContract>.CreateServiceEndpoint(netTcpBinding));

            // Now, for each endpoint we have to create a binding in our service description.
            foreach (ServiceEndpoint endpoint in endpoints)
            {
                // Create a WSDL BindingCollection.
                BindingCollection bindings = desc.Bindings;
                System.Web.Services.Description.Binding binding = new System.Web.Services.Description.Binding();
                binding.Name = endpoint.Name.Replace(Constants.InternalContractName, portTypeName);
                binding.Type = new XmlQualifiedName(portType.Name, desc.TargetNamespace);

                // Create Operation binding for each operation and add it the the BindingCollection.
                foreach (Operation op in serviceInterfaceContract.OperationsCollection)
                {
                    // SOAP 1.1 Operation bindings.
                    OperationBinding operationBinding1 = new OperationBinding();
                    operationBinding1.Name = op.Name;

                    InputBinding inputBinding1 = new InputBinding();
                    object bodyBindingExtension = GetSoapBodyBinding(endpoint.Binding);
                    if (bodyBindingExtension != null)
                    {
                        inputBinding1.Extensions.Add(bodyBindingExtension);
                    }
                    operationBinding1.Input = inputBinding1;

					// Faults.
                	foreach (Message fault in op.Faults)
                	{
                		FaultBinding faultBinding = new FaultBinding();
                		faultBinding.Name = fault.Name;

						SoapFaultBinding faultBindingExtension = GetFaultBodyBinding(endpoint.Binding);
						if (faultBindingExtension != null)
						{
							faultBindingExtension.Name = fault.Name;
							faultBinding.Extensions.Add(faultBindingExtension);
						}

                		operationBinding1.Faults.Add(faultBinding);
                	}

                    // Input message.
                    // Look up the message headers for each Message and add them to the current binding.
                    foreach (MessageHeader inHeader in op.Input.HeadersCollection)
                    {
                        object headerBindingExtension = GetSoapHeaderBinding(endpoint.Binding, inHeader.Message, desc.TargetNamespace);
                        if (headerBindingExtension != null)
                        {
                            inputBinding1.Extensions.Add(headerBindingExtension);
                        }
                    }

                    if (op.Mep == Mep.RequestResponse)
                    {
                        // Output message.
                        OutputBinding outputBinding1 = new OutputBinding();
                        object responseBodyBindingExtension = GetSoapBodyBinding(endpoint.Binding);
                        if (responseBodyBindingExtension != null)
                        {
                            outputBinding1.Extensions.Add(responseBodyBindingExtension);
                        }
                        operationBinding1.Output = outputBinding1;

                        // Look up the message headers for each Message and add them to the current binding. 
                        foreach (MessageHeader outHeader in op.Output.HeadersCollection)
                        {
                            object headerBindingExtension = GetSoapHeaderBinding(endpoint.Binding, outHeader.Message, desc.TargetNamespace);
                            if (headerBindingExtension != null)
                            {
                                outputBinding1.Extensions.Add(headerBindingExtension);
                            }
                        }
                    }

                    string action = desc.TargetNamespace + ":" + op.Input.Name;
                    object operationBindingExtension = GetSoapOperationBinding(endpoint.Binding, action);
                    if (operationBindingExtension != null)
                    {
                        operationBinding1.Extensions.Add(operationBindingExtension);
                    }

                    binding.Operations.Add(operationBinding1);
                    // End of SOAP 1.1 operation bindings.                    
                }

                object soapBindingExtension = GetSoapBinding(endpoint.Binding);
                if (soapBindingExtension != null)
                {
                    binding.Extensions.Add(soapBindingExtension);
                }
                bindings.Add(binding);
            }

            // Generate <service> element optionally - sometimes necessary for interop reasons
            if (serviceInterfaceContract.NeedsServiceElement)
            {
                Service defaultService = null;
                if (wsdlLocation == null || desc.Services.Count == 0)
                {
                    // Create a new service element.
                    defaultService = new Service();
                    defaultService.Name = serviceName;
                    foreach (ServiceEndpoint endpoint in endpoints)
                    {
                        if (endpoint.Binding.MessageVersion.Envelope == EnvelopeVersion.Soap11)
                        {
                            Port defaultPort = new Port();
                            defaultPort.Name = serviceInterfaceContract.ServiceName + "Port";
                            defaultPort.Binding = new XmlQualifiedName(endpoint.Name.Replace(Constants.InternalContractName, portTypeName), desc.TargetNamespace);
                            SoapAddressBinding defaultSoapAddressBinding = new SoapAddressBinding();
                            defaultSoapAddressBinding.Location = GetDefaultEndpoint(endpoint.Binding, serviceInterfaceContract.ServiceName);
                            defaultPort.Extensions.Add(defaultSoapAddressBinding);
                            defaultService.Ports.Add(defaultPort);
                        }
                        else if (endpoint.Binding.MessageVersion.Envelope == EnvelopeVersion.Soap12)
                        {
                            Port soap12Port = new Port();
                            soap12Port.Name = serviceInterfaceContract.ServiceName + "SOAP12Port";
                            soap12Port.Binding = new XmlQualifiedName(endpoint.Name.Replace(Constants.InternalContractName, portTypeName), desc.TargetNamespace);
                            Soap12AddressBinding soap12AddressBinding = new Soap12AddressBinding();
                            soap12AddressBinding.Location = GetDefaultEndpoint(endpoint.Binding, serviceInterfaceContract.ServiceName);
                            soap12Port.Extensions.Add(soap12AddressBinding);
                            defaultService.Ports.Add(soap12Port);
                        }                        
                    }
                    
                    desc.Services.Add(defaultService);                    
                }
                else
                {
                    defaultService = desc.Services[0];
                    defaultService.Name = serviceName;
                }
            }

            // Generate the WSDL file.
            string fileName = string.Empty;
            string bkFileName = string.Empty;

            // Overwrite the existing file if one specified.
            if (wsdlLocation == null)
            {
                fileName = wsdlSaveLocation + @"\" + serviceInterfaceContract.ServiceName + ".wsdl";
            }
            else
            {
                fileName = wsdlLocation;
            }

            // Backup existing file before proceeding.
            if (File.Exists(fileName))
            {
                int index = 1;
                // Create the backup file name. 
                // See whether the generated backup file name is already taken by an existing file and 
                // generate a new file name. 
                while (File.Exists(fileName + "." + index.ToString()))
                {
                    index++;
                }
                bkFileName = fileName + "." + index.ToString();

                // Backup the file.
                try
                {
                    File.Copy(fileName, bkFileName);
                }
                catch (Exception ex)
                {
                    throw new WsdlGenerationException("An error occured while trying to generate a WSDL. Failed to backup the existing WSDL file.", ex);
                }
            }

            StreamWriter writer1 = new StreamWriter(fileName);
            try
            {
                XmlTextWriter writer11 = new XmlTextWriter(writer1);

                writer11.Formatting = Formatting.Indented;
                writer11.Indentation = 2;
                writer11.WriteComment(xmlComment);
                // BDS: Added a new comment line with the date time of WSDL file.
                CultureInfo ci = new CultureInfo("en-US");
                writer11.WriteComment(DateTime.Now.ToString("dddd", ci) + ", " + DateTime.Now.ToString("dd-MM-yyyy - hh:mm tt", ci));

                XmlSerializer serializer1 = System.Web.Services.Description.ServiceDescription.Serializer;
                XmlSerializerNamespaces nsSer = new XmlSerializerNamespaces();
                nsSer.Add("soap", "http://schemas.xmlsoap.org/wsdl/soap/");
                nsSer.Add("soap12", "http://schemas.xmlsoap.org/wsdl/soap12/");
                nsSer.Add("xsd", "http://www.w3.org/2001/XMLSchema");
                nsSer.Add("tns", desc.TargetNamespace);

                // Add the imported namespaces to the WSDL <description> element.
                for (int importIndex = 0; importIndex < serviceInterfaceContract.Imports.Count;
                    importIndex++)
                {
                    if (serviceInterfaceContract.Imports[importIndex].SchemaNamespace != null &&
                        serviceInterfaceContract.Imports[importIndex].SchemaNamespace != "")
                    {
                        nsSer.Add("import" + importIndex.ToString(),
                                  serviceInterfaceContract.Imports[importIndex].SchemaNamespace);
                    }
                }
                // 

                // Finally write the file to the output stram.
                serializer1.Serialize(writer11, desc, nsSer);

                // Close the stream and delete the backupfile.
                writer1.Close();
                if (bkFileName != string.Empty)
                {
                    File.Delete(bkFileName);
                }

                WsdlWorkshop workshop = new WsdlWorkshop(endpoints, fileName, portTypeName);
                workshop.BuildWsdl();
                return fileName;
            }
            catch (Exception ex)
            {
                writer1.Close();
                string message = ex.Message;
                if (ex.InnerException != null)
                {
                    message += ex.InnerException.Message;
                }

                // Restore the original file.
                if (bkFileName != string.Empty)
                {
                    try
                    {
                        File.Copy(bkFileName, fileName, true);
                        File.Delete(bkFileName);
                    }
                    catch
                    {
                        throw new WsdlGenerationException(
                            message + "\nFailed to restore the original file.");
                    }
                }
                else if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                throw new WsdlGenerationException(
                    message, ex);
            }
        }

        private static object GetSoapBinding(System.ServiceModel.Channels.Binding binding)
        {
            if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                SoapBinding soapBinding = new SoapBinding();
                soapBinding.Transport = GetTransport(binding);
                soapBinding.Style = SoapBindingStyle.Document;
                return soapBinding;
            }
            else if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap12)
            {
                Soap12Binding soapBinding = new Soap12Binding();
                soapBinding.Transport = GetTransport(binding);
                soapBinding.Style = SoapBindingStyle.Document;
                return soapBinding;
            }
            return null;
        }

        private static object GetSoapOperationBinding(System.ServiceModel.Channels.Binding binding, string action)
        {
            if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                SoapOperationBinding soapOperationBinding = new SoapOperationBinding();
                soapOperationBinding.SoapAction = action;
                soapOperationBinding.Style = SoapBindingStyle.Document;
                return soapOperationBinding;
            }
            else if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap12)
            {
                Soap12OperationBinding soapOperationBinding = new Soap12OperationBinding();
                soapOperationBinding.SoapAction = action;
                soapOperationBinding.Style = SoapBindingStyle.Document;
                return soapOperationBinding;
            }
            return null;
        }

        private static object GetSoapHeaderBinding(System.ServiceModel.Channels.Binding binding, string message, string ns)
        {
            if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                SoapHeaderBinding headerBinding = new SoapHeaderBinding();
                headerBinding.Use = SoapBindingUse.Literal;
                headerBinding.Message = new XmlQualifiedName(message, ns);
                headerBinding.Part = Constants.DefaultMessagePartName;
                return headerBinding;
            }
            else if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap12)
            {
                Soap12HeaderBinding headerBinding = new Soap12HeaderBinding();
                headerBinding.Use = SoapBindingUse.Literal;
                headerBinding.Message = new XmlQualifiedName(message, ns);
                headerBinding.Part = Constants.DefaultMessagePartName;
                return headerBinding;
            }

            return null;
        }

        private static object GetSoapBodyBinding(System.ServiceModel.Channels.Binding binding)
        {
            if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap11)
            {
                SoapBodyBinding soapBinding = new SoapBodyBinding();
                soapBinding.Use = SoapBindingUse.Literal;
                return soapBinding;
            }
            else if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap12)
            {
                Soap12BodyBinding soap12Binding = new Soap12BodyBinding();
                soap12Binding.Use = SoapBindingUse.Literal;
                return soap12Binding;
            }

            return null;
        }

		private static SoapFaultBinding GetFaultBodyBinding(System.ServiceModel.Channels.Binding binding)
		{
			if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap11)
			{
				return new SoapFaultBinding {Use = SoapBindingUse.Literal};
			}
			if (binding.MessageVersion.Envelope == EnvelopeVersion.Soap12)
			{
				return new Soap12FaultBinding {Use = SoapBindingUse.Literal};
			}
			return null;
		}

    	private static string GetTransport(System.ServiceModel.Channels.Binding binding)
        {
            TransportBindingElement transport = binding.CreateBindingElements().Find<TransportBindingElement>();
            if (transport != null)
            {
                if (typeof(HttpTransportBindingElement) == transport.GetType())
                {
                    return "http://schemas.xmlsoap.org/soap/http";
                }
                if (typeof(HttpsTransportBindingElement) == transport.GetType())
                {
                    return "http://schemas.xmlsoap.org/soap/https";
                }
                if (typeof(TcpTransportBindingElement) == transport.GetType())
                {
                    return "http://schemas.microsoft.com/soap/tcp";
                }
                if (typeof(NamedPipeTransportBindingElement) == transport.GetType())
                {
                    return "http://schemas.microsoft.com/soap/named-pipe";
                }
                if (typeof(MsmqTransportBindingElement) == transport.GetType())
                {
                    return "http://schemas.microsoft.com/soap/msmq";
                }
            }
            return "";
        }

        private static string GetDefaultEndpoint(System.ServiceModel.Channels.Binding binding, string serviceName)
        {
            TransportBindingElement transport = binding.CreateBindingElements().Find<TransportBindingElement>();
            if (transport != null)
            {
                if (typeof(HttpTransportBindingElement) == transport.GetType())
                {
                    UriBuilder ub = new UriBuilder(transport.Scheme, "localhost");
                    ub.Path = serviceName;
                    return ub.Uri.ToString();
                }
                if (typeof(HttpsTransportBindingElement) == transport.GetType())
                {
                    UriBuilder ub = new UriBuilder(transport.Scheme, "localhost");
                    ub.Path = serviceName;
                    return ub.Uri.ToString();
                }
                if (typeof(TcpTransportBindingElement) == transport.GetType())
                {
                    UriBuilder ub = new UriBuilder(transport.Scheme, "localhost");
                    ub.Path = serviceName;
                    return ub.Uri.ToString();
                }
                if (typeof(NamedPipeTransportBindingElement) == transport.GetType())
                {
                    return "tbd";
                }
                if (typeof(MsmqTransportBindingElement) == transport.GetType())
                {
                    return "tbd";
                }
            }
            return "";
        }

        /// <summary>
        /// Reads a XML schema file and returns the information found in that.
        /// </summary>
        /// <param name="schemaFile">The XML schema file to read information from.</param>
        /// <param name="schemaNamespace">Ouput parameter which returns the namespace of the specified XML schema file.</param>
        /// <returns>
        /// An <see cref="ArrayList"/> with three items. 
        /// 1. Contains an <see cref="ArrayList"/> of <see cref="XmlSchemaElement"/> objects.
        /// 2. Contains an <see cref="ArrayList"/> of schema element names.
        /// 3. Contains a <see cref="SchemaElements"/> object. 
        /// </returns>
        public static ArrayList GetSchemasFromXsd(string schemaFile, out string schemaNamespace)
        {
            XmlTextReader reader = null;
            ArrayList schemas;
            ArrayList schemaNames;
            SchemaElements sElements;

            try
            {
                reader = new XmlTextReader(schemaFile);

                XmlSchema schema = XmlSchema.Read(reader, null);
                string schemaTargetNamesapce = schema.TargetNamespace;
                schemaNamespace = schemaTargetNamesapce;

                ArrayList xmlSchemaElements = new ArrayList();
                schemas = new ArrayList();
                schemaNames = new ArrayList();
                sElements = new SchemaElements();

                foreach (XmlSchemaObject xmlObj in schema.Items)
                {
                    if (xmlObj is XmlSchemaAnnotated) xmlSchemaElements.Add(xmlObj);
                }

                foreach (XmlSchemaAnnotated obj in xmlSchemaElements)
                {
                    if (obj is XmlSchemaElement)
                    {
                        XmlSchemaElement xse = (XmlSchemaElement)obj;

                        schemas.Add(xse);
                        schemaNames.Add(xse.Name);
                        sElements.Add(new SchemaElement(schemaTargetNamesapce, xse.Name));
                    }
                }

                reader.Close();

                ArrayList result = new ArrayList();
                result.Add(schemas);
                result.Add(sElements);
                result.Add(schemaNames);

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error occurred while reading the schema file.", ex);
            }
            finally
            {
                if (reader != null && reader.ReadState != ReadState.Closed)
                {
                    reader.Close();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static ArrayList GetIntrinsicSimpleTypesNames()
        {
            ArrayList primitiveNames = new ArrayList();
            Assembly ass = Assembly.GetAssembly(typeof(XmlSchema));

            Type type = ass.GetType("System.Xml.Schema.DatatypeImplementation");
            FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic);

            foreach (FieldInfo fi in fields)
            {
                int index = fi.Name.IndexOf("c_");
                if (index > -1)
                {
                    string fieldName = fi.Name.Substring(index + 2);
                    primitiveNames.Add("xsd:" + fieldName);
                }
            }

            return primitiveNames;
        }

        /// <summary>
        /// Creates an <see cref="InterfaceContract"/> object by loading the contents in a specified 
        /// WSDL file.
        /// </summary>
        /// <param name="wsdlFileName">Path of the WSDL file to load the information from.</param>
        /// <returns>An instance of <see cref="InterfaceContract"/> with the information loaded from the WSDL file.</returns>
        /// <remarks>
        /// This method first loads the content of the WSDL file to an instance of 
        /// <see cref="System.Web.Services.Description.ServiceDescription"/> class. Then it creates an 
        /// instance of <see cref="InterfaceContract"/> class by loading the data from that.
        /// This method throws <see cref="WsdlLoadException"/> in case of a failure to load the WSDL file.
        /// </remarks>
        public static InterfaceContract GetInterfaceContract(string wsdlFileName)
        {
            // Try to load the service description from the specified file.
            System.Web.Services.Description.ServiceDescription srvDesc = null;
            try
            {
                srvDesc = System.Web.Services.Description.ServiceDescription.Read(
                    wsdlFileName);
            }
            catch (Exception ex)
            {
                throw new WsdlLoadException(
                    "Could not load service description from the specified file.",
                    ex);
            }

            // Validate the WSDL before proceeding.
            bool isHttpBinding = false;
            if (!ValidateWsdl(srvDesc, ref isHttpBinding))
            {
                throw new WsdlNotCompatibleForRoundTrippingException("Not a valid file for round tripping");
            }

            // Start building the simplified InterfaceContract object from the 
            // .Net Fx ServiceDescription we created.
            InterfaceContract simpleContract = new InterfaceContract();

            // Initialize the basic meta data.
            simpleContract.ServiceNamespace = srvDesc.TargetNamespace;
            simpleContract.ServiceDocumentation = srvDesc.Documentation;
            // Try to get the service namespace from the service description.
            simpleContract.ServiceName = srvDesc.Name;

            // If it was not found in the service description. Then try to get it from the 
            // service. If it is not found their either, then try to get it from binding.	
            if (simpleContract.ServiceName == null || simpleContract.ServiceName == "")
            {
                if (srvDesc.Services.Count > 0 && srvDesc.Services[0].Name != null &&
                    srvDesc.Services[0].Name != "")
                {
                    simpleContract.ServiceName = srvDesc.Services[0].Name;
                }
                else
                {
                    simpleContract.ServiceName = srvDesc.Bindings[0].Name;
                }
            }

            // Set the http binding property.
            simpleContract.IsHttpBinding = isHttpBinding;

            // Initialize the imports.
            foreach (XmlSchema typeSchema in srvDesc.Types.Schemas)
            {
                foreach (XmlSchemaObject schemaObject in typeSchema.Includes)
                {
                    XmlSchemaImport import = schemaObject as XmlSchemaImport;

					if (import != null && import.SchemaLocation != null)
                    {
                        SchemaImport simpleImport = new SchemaImport();
                        simpleImport.SchemaNamespace = import.Namespace;
                        simpleImport.SchemaLocation = import.SchemaLocation;
                        simpleContract.Imports.Add(simpleImport);
                    }
                }
            }

            // Initialize the types embedded to the WSDL.
            simpleContract.SetTypes(GetSchemaElements(srvDesc.Types.Schemas, srvDesc.TargetNamespace));

            // Initialize the operations and in/out messages.
            PortType ptype = srvDesc.PortTypes[0];
            if (ptype != null)
            {
                foreach (FxOperation op in ptype.Operations)
                {
                    // Create the Operation.
                    Operation simpleOp = new Operation();
                    simpleOp.Name = op.Name;
                    simpleOp.Documentation = op.Documentation;

					if (op.Faults != null)
					{
						foreach (OperationFault fault in op.Faults)
						{
							FxMessage faultMessage = srvDesc.Messages[fault.Message.Name];
							if (faultMessage == null)
							{
								// WSDL modified.
								string message = string.Format("Could not find the fault message '{0}'", fault.Message.Name);
								throw new WsdlModifiedException(message);
							}

							MessagePart part = faultMessage.Parts[0];
							if (part != null)
							{
								Message message = new Message();
								message.Name = faultMessage.Name;
								message.Element.ElementName = part.Element.Name;
								message.Element.ElementNamespace = part.Element.Namespace;
								message.Documentation = faultMessage.Documentation;

								simpleOp.MessagesCollection.Add(message);
								
								simpleOp.Faults.Add(message);
							}
						}
					}

                    if (op.Messages.Input != null)
                    {
                        FxMessage inMessage = srvDesc.Messages[op.Messages.Input.Message.Name];

                        if (inMessage == null)
                        {
                            // WSDL modified.
                            throw new WsdlModifiedException("Could not find the message");
                        }

                        MessagePart part = inMessage.Parts[0];
                        if (part != null)
                        {
                            // Create the input message.
                            Message simpleInMessage = new Message();
                            simpleInMessage.Name = inMessage.Name;
                            simpleInMessage.Element.ElementName = part.Element.Name;
                            simpleInMessage.Element.ElementNamespace = part.Element.Namespace;
                            simpleInMessage.Documentation = inMessage.Documentation;

                            simpleOp.MessagesCollection.Add(simpleInMessage);

                            simpleOp.Input = simpleInMessage;
                        }
                        else
                        {
                            // WSDL is modified. 
                            throw new WsdlModifiedException("Could not find the message part");
                        }
                    }

                    if (op.Messages.Output != null)
                    {
                        FxMessage outMessage = srvDesc.Messages[op.Messages.Output.Message.Name];

                        if (outMessage == null)
                        {
                            // WSDL is modified.
                            throw new WsdlModifiedException("Could not find the message");
                        }

                        MessagePart part = outMessage.Parts[0];
                        if (part != null)
                        {
                            // Create the output message.
                            Message simpleOutMessage = new Message();
                            simpleOutMessage.Name = outMessage.Name;
                            simpleOutMessage.Element.ElementName = part.Element.Name;
                            simpleOutMessage.Element.ElementNamespace = part.Element.Namespace;
                            simpleOutMessage.Documentation = outMessage.Documentation;

                            simpleOp.MessagesCollection.Add(simpleOutMessage);

                            simpleOp.Output = simpleOutMessage;
                        }
                        else
                        {
                            // WSDL is modified. 
                            throw new WsdlModifiedException("Could not find the message part");
                        }

                        // Set the message direction.
                        simpleOp.Mep = Mep.RequestResponse;
                    }
                    else
                    {
                        simpleOp.Mep = Mep.OneWay;
                    }

                    // Finally add the Operation to Operations collection.
                    simpleContract.OperationsCollection.Add(simpleOp);
                }
            }
            else
            {
                // WSDL is modified.
                throw new WsdlModifiedException("Could not find the portType");
            }

            // Initialize the message headers and header messages.
            System.Web.Services.Description.Binding binding1 = srvDesc.Bindings[0];
            if (binding1 != null)
            {
                // Find the corresponding Operation in the InterfaceContract, for each OperationBinding 
                // in the binding1.Operations collection. 
                foreach (OperationBinding opBinding in binding1.Operations)
                {
                    foreach (Operation simpleOp in simpleContract.OperationsCollection)
                    {
                        if (simpleOp.Name == opBinding.Name)
                        {
                            if (opBinding.Input != null)
                            {
                                // Enumerate the message headers for the input message.
                                foreach (ServiceDescriptionFormatExtension extension in opBinding.Input.Extensions)
                                {
                                    SoapHeaderBinding inHeader = extension as SoapHeaderBinding;
                                    if (inHeader != null)
                                    {
                                        // Create the in header and add it to the headers collection.
                                        MessageHeader simpleInHeader = new MessageHeader();
                                        FxMessage inHeaderMessage = srvDesc.Messages[inHeader.Message.Name];

                                        if (inHeaderMessage == null)
                                        {
                                            // WSDL modified.
                                            throw new WsdlModifiedException("Could not find the message");
                                        }

                                        simpleInHeader.Name = inHeaderMessage.Name;
                                        simpleInHeader.Message = inHeaderMessage.Name;
                                        simpleOp.Input.HeadersCollection.Add(simpleInHeader);

                                        // Create the in header message and put it to the Operation's messeages collection.
                                        MessagePart part = inHeaderMessage.Parts[0];
                                        if (part != null)
                                        {
                                            Message simpleInHeaderMessage = new Message();
                                            simpleInHeaderMessage.Name = inHeaderMessage.Name;
                                            simpleInHeaderMessage.Element.ElementName = part.Element.Name;
                                            simpleInHeaderMessage.Element.ElementNamespace = part.Element.Namespace;

                                            simpleOp.MessagesCollection.Add(simpleInHeaderMessage);
                                        }
                                        else
                                        {
                                            // WSDL is modified.
                                            throw new WsdlModifiedException("Could not find the message part");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // WSDL modified.
                                throw new WsdlModifiedException("Could not find the operation binding");
                            }

                            if (simpleOp.Mep == Mep.RequestResponse && opBinding.Output != null)
                            {
                                // Enumerate the message headers for the output message.
                                foreach (ServiceDescriptionFormatExtension extension in opBinding.Output.Extensions)
                                {
                                    SoapHeaderBinding outHeader = extension as SoapHeaderBinding;
                                    if (outHeader != null)
                                    {
                                        // Create the in header and add it to the headers collection.
                                        MessageHeader simpleOutHeader = new MessageHeader();
                                        FxMessage outHeaderMessage = srvDesc.Messages[outHeader.Message.Name];

                                        if (outHeaderMessage == null)
                                        {
                                            // WSDL is modified.
                                            throw new WsdlModifiedException("Could not find the message");
                                        }

                                        simpleOutHeader.Name = outHeaderMessage.Name;
                                        simpleOutHeader.Message = outHeaderMessage.Name;
                                        simpleOp.Output.HeadersCollection.Add(simpleOutHeader);

                                        // Create the out header message and put it to the Operation's messeages collection.
                                        MessagePart part = outHeaderMessage.Parts[0];
                                        if (part != null)
                                        {
                                            Message simpleOutHeaderMessage = new Message();
                                            simpleOutHeaderMessage.Name = outHeaderMessage.Name;
                                            simpleOutHeaderMessage.Element.ElementName = part.Element.Name;
                                            simpleOutHeaderMessage.Element.ElementNamespace = part.Element.Namespace;

                                            simpleOp.MessagesCollection.Add(simpleOutHeaderMessage);
                                        }
                                        else
                                        {
                                            // WSDL is modified.
                                            throw new WsdlModifiedException("Could not find the message part");
                                        }
                                    }
                                }
                            }
                            else if (simpleOp.Mep == Mep.RequestResponse)
                            {
                                // WSDL modified.
                                throw new WsdlModifiedException("Could not find the operation binding");
                            }
                        }
                    }
                }
            }

            // Check for the "Generate service tags" option.
            if (srvDesc.Services.Count == 1)
            {
                simpleContract.NeedsServiceElement = true;
            }

            // Turn on the SOAP 1.2 binding if available.
            foreach (System.Web.Services.Description.Binding binding in srvDesc.Bindings)
            {
                if (binding.Extensions.Find(typeof(Soap12Binding)) != null)
                {
                    simpleContract.Bindings |= InterfaceContract.SoapBindings.Soap12;
                }
            }

            return simpleContract;
        }

        #endregion

        #region Private static methods.

        /// <summary>
        /// Validates a specified instance of <see cref="ServiceDescription"/> class for the round tripping feature.
        /// </summary>
        /// <param name="serviceDescription">
        /// An instance of <see cref="ServiceDescription"/> class to 
        /// validate.
        /// </param>
        /// <param name="isHttpBinding">A reference to a Boolean variable. Value is this variable is set to true if the service description has Http binding.</param>
        /// <returns>
        /// A value indicating whether the specified instance of <see cref="ServiceDescription"/> 
        /// class is valid for the round tripping feature.
        /// </returns>
        private static bool ValidateWsdl(
            System.Web.Services.Description.ServiceDescription serviceDescription,
            ref bool isHttpBinding)
        {
            // Rule No 1: Service description must have atleast one schema in the types definitions.
            if (serviceDescription.Types.Schemas.Count == 0)
            {
                return false;
            }

            // Rule No 2: Service description must have only one <porttype>.
            if (serviceDescription.PortTypes.Count != 1)
            {
                return false;
            }

            // Rule No 3: Service description must have only SOAP 1.1 and/or SOAP 1.2 binding(s).
            if (!((serviceDescription.Bindings.Count == 1 && serviceDescription.Bindings[0].Extensions.Find(typeof(SoapBinding)) != null) ||
             (serviceDescription.Bindings.Count == 2 && serviceDescription.Bindings[0].Extensions.Find(typeof(SoapBinding)) != null &&
               serviceDescription.Bindings[1].Extensions.Find(typeof(Soap12Binding)) != null)))
            {
                return false;
            }

            // Rule No 4: Service description can not have more than one <service>. But it is possible 
            // not to have a <service>.
            if (serviceDescription.Services.Count > 1)
            {
                return false;
            }

            // Rule No 5: Each message must have only one <part>.
            foreach (FxMessage message in serviceDescription.Messages)
            {
                if (message.Parts.Count > 1)
                {
                    return false;
                }
            }

            // Rule No 6: For soap bindings the binding style must be 'Document' and encoding must be 'Literal'.

            // Obtain a reference to the one and only binding we have.
            System.Web.Services.Description.Binding binding = serviceDescription.Bindings[0];

            // Search for the soap binding style and return false if it is not 'Document'
            foreach (ServiceDescriptionFormatExtension extension in binding.Extensions)
            {
                SoapBinding soapBinding = extension as SoapBinding;
                if (soapBinding != null)
                {
                    if (soapBinding.Style != SoapBindingStyle.Document)
                    {
                        return false;
                    }
                }
                else if (extension is HttpBinding)
                {
                    isHttpBinding = true;
                }
            }

            // Validate the operation bindings.
            foreach (OperationBinding operationBinding in binding.Operations)
            {
                // Validate the soap binding style in soap operation binding extension.
                foreach (ServiceDescriptionFormatExtension extension in operationBinding.Extensions)
                {
                    SoapOperationBinding soapOperationBinding = extension as SoapOperationBinding;

                    if (soapOperationBinding != null)
                    {
                        if (soapOperationBinding.Style != SoapBindingStyle.Document)
                        {
                            return false;
                        }
                    }
                }

                // Validate the 'use' element in input message body and the headers.
                foreach (ServiceDescriptionFormatExtension extension in operationBinding.Input.Extensions)
                {
                    // Check for a header.
                    SoapHeaderBinding headerBinding = extension as SoapHeaderBinding;
                    if (headerBinding != null)
                    {
                        if (headerBinding.Use != SoapBindingUse.Literal)
                        {
                            return false;
                        }
                        continue;
                    }

                    // Check for the body.
                    SoapBodyBinding bodyBinding = extension as SoapBodyBinding;
                    if (bodyBinding != null)
                    {
                        if (bodyBinding.Use != SoapBindingUse.Literal)
                        {
                            return false;
                        }

                        continue;
                    }
                }

                // Validate the 'use' element in output message body and the headers.
                if (operationBinding.Output != null)
                {
                    foreach (ServiceDescriptionFormatExtension extension in operationBinding.Output.Extensions)
                    {
                        // Check for the header.
                        SoapHeaderBinding headerBinding = extension as SoapHeaderBinding;
                        if (headerBinding != null)
                        {
                            if (headerBinding.Use != SoapBindingUse.Literal)
                            {
                                return false;
                            }
                            continue;
                        }

                        // Check for the body.
                        SoapBodyBinding bodyBinding = extension as SoapBodyBinding;
                        if (bodyBinding != null)
                        {
                            if (bodyBinding.Use != SoapBindingUse.Literal)
                            {
                                return false;
                            }

                            continue;
                        }
                    }
                }
            }

            return true;
        }


        /// <summary>
        /// Extracts the elements from a given schemas.
        /// </summary>
        /// <param name="schemas">Schemas to extract the elements from.</param>
        /// <param name="tns">String specifying the target namespace of the <see cref="schemas" />.</param>
        /// <returns>An instance of <see cref="SchemaElements" /> class which contains the extracted elements.</returns>
        private static SchemaElements GetSchemaElements(XmlSchemas schemas, string tns)
        {
            ArrayList xmlSchemaElements = new ArrayList();
            SchemaElements sElements = new SchemaElements();

            foreach (XmlSchema schema in schemas)
            {
                foreach (XmlSchemaObject xmlObj in schema.Items)
                {
                    if (xmlObj is XmlSchemaAnnotated)
                    {
                        if (xmlObj is XmlSchemaElement)
                        {
                            XmlSchemaElement xse = (XmlSchemaElement)xmlObj;
                            sElements.Add(new SchemaElement(tns, xse.Name));
                        }
                    }
                }
            }

            return sElements;
        }

        #endregion
    }

    #endregion
}