using System.Configuration;
using System.Diagnostics;
using System.ServiceModel.Configuration;

using Thinktecture.ServiceModel.Extensions.Metadata;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    internal sealed class ConfigurationDecorator : ICodeDecorator
    {
        #region Private Fields

        private ExtendedCodeDomTree code;
        private CustomCodeGenerationOptions options;
        private Configuration configuration;

        #endregion

        #region ICodeDecorator Members

        public void Decorate(ExtendedCodeDomTree code, CustomCodeGenerationOptions options)
        {
            this.code = code;
            this.options = options;
            this.configuration = code.Configuration;

            // Are we on the service side code gen?
            if (options.GenerateService)
            {
                // Then generate the service side configuration.
                GenerateServiceConfiguration();

                // Do we have to enable metadata endpoint?
                if (options.EnableWsdlEndpoint)
                {
                    AddMetadataServiceBehavior();
                }
            }            
        }

        #endregion

        #region Private Methods        

        private string GetServiceTypeName()
        {
            // Let us know if there is more than one service type.
            Debug.Assert(code.ServiceTypes.Count == 1, "There is not only one service type.");
            string serviceTypeName = code.ServiceTypes[0].ExtendedObject.Name;
            return serviceTypeName;
        }

        private string GetFullyQulifiedTypeName(string typeName)
        {
        	return (options.Language == CodeLanguage.VisualBasic) 
				? string.Format("{0}.{1}.{2}", options.ProjectName, options.ClrNamespace, typeName) 
				: string.Format("{0}.{1}", options.ClrNamespace, typeName);
        }

    	private void GenerateServiceConfiguration()
        {
            // Get a pointer to system.serviceModel section.
            ConfigurationSectionGroup csg = configuration.SectionGroups["system.serviceModel"];
            // Notify if we get a null reference.
            Debug.Assert(csg != null, "system.serviceModel section could not be found in the configuration.");

            if (csg != null)
            {                
                // Get a reference to the client section.
                ClientSection cs = csg.Sections["client"] as ClientSection;
                // Also get a reference to the services section.
                ServicesSection ss = csg.Sections["services"] as ServicesSection;

                // If there is no services section, we create a new one.
                if (ss == null)
                {
                    // Create a new services section.
                    ss = new ServicesSection();
                    // Add it to the sections collection.
                    csg.Sections.Add("services", ss);
                }

                string fqServiceTypeName = GetFullyQulifiedTypeName(GetServiceTypeName());
                ServiceElement se = new ServiceElement(fqServiceTypeName);
                ss.Services.Add(se);

                if (cs != null)
                {
                    foreach (ChannelEndpointElement cee in cs.Endpoints)
                    {
                        // TODO: May be we will want to give an option to use fully qulified interface names
                        // in the endpoints.
                        ServiceEndpointElement see = new ServiceEndpointElement(cee.Address, cee.Contract);
                        see.BehaviorConfiguration = cee.BehaviorConfiguration;
                        see.BindingConfiguration = cee.BindingConfiguration;
                        see.Binding = cee.Binding;
                        se.Endpoints.Add(see);
                    }
                    csg.Sections.Remove("client");
                }
            }
        }

        // We currently don't use this function. Because the automatically generated code uses the 
        // ConfigurationName attribute in the service contract to configure the contract name to 
        // be used in the configuration file.
        private void UseFullyQualifiedTypeNamesInClientConfiguration()
        {
            // Get a pointer to system.serviceModel section.
            ConfigurationSectionGroup csg = configuration.SectionGroups["system.serviceModel"];
            // Notify if we get a null reference.
            Debug.Assert(csg != null, "system.serviceModel section could not be found in the configuration.");

            if (csg != null)
            {
                // Get a reference to the client section.
                ClientSection cs = csg.Sections["client"] as ClientSection;                
                // Proceed if we have a valid reference to the client section.
                if (cs != null)
                {
                    foreach (ChannelEndpointElement cee in cs.Endpoints)
                    {
                        cee.Contract = GetFullyQulifiedTypeName(cee.Contract);                        
                    }                    
                }
            }
        }

        /// <summary>
        /// This method adds the neccessary configuration elements to hook up 
        /// Thinktecture.ServiceModel.Extensions.Metdata extension to service the 
        /// service code being generated.
        /// </summary>
        private void AddMetadataServiceBehavior()
        {
            // Get a pointer to system.serviceModel section.
            ConfigurationSectionGroup csg = configuration.SectionGroups["system.serviceModel"];
            // Notify if we get a null reference.
            Debug.Assert(csg != null, "system.serviceModel section could not be found in the configuration.");
            
            if (csg != null)
            {
                // Try to find the extensions element.
                ExtensionsSection extensionsSection = csg.Sections["extensions"] as ExtensionsSection;
                // Create it if it wasn't found.
                if (extensionsSection == null)
                {
                    extensionsSection = new ExtensionsSection();
                    csg.Sections.Add("extensions", extensionsSection);
                }

                // Now create the new behavior extension.
                ExtensionElement metadataServiceExtensionElement = new ExtensionElement();
                metadataServiceExtensionElement.Name = "metadataService";
                //TODO: Make this more dynamic so it can discover the assembly version etc otherwise this will always throw exceptions
                // that the behavior extension was not found in the collection.
                metadataServiceExtensionElement.Type = "Thinktecture.ServiceModel.Extensions.Metadata.StaticMetadataBehaviorElement, Thinktecture.ServiceModel.Extensions.Metadata, Version=1.0.13.0, Culture=neutral, PublicKeyToken=20fb7cabbfb92df4";                                                        
                
                // Add the newly created behavior extension to the extensions section.
                extensionsSection.BehaviorExtensions.Add(metadataServiceExtensionElement);                
                                
                // Try to find the behaviors element.
                BehaviorsSection behaviorsSection = csg.Sections["behaviors"] as BehaviorsSection;
                // Create it if it wasn't found.
                if (behaviorsSection == null)
                {
                    behaviorsSection = new BehaviorsSection();
                    csg.Sections.Add("behaviors", behaviorsSection);                    
                }

                // Add the new service behavior.
                ServiceBehaviorElement serviceBehavior = new ServiceBehaviorElement();
                serviceBehavior.Name = "metadataServiceExtension";

                behaviorsSection.ServiceBehaviors.Add(serviceBehavior);

                StaticMetadataBehaviorElement behaviorExtensionElement = new StaticMetadataBehaviorElement();                
                behaviorExtensionElement.RootMetadataFileLocation = options.MetadataLocation;
                behaviorExtensionElement.MetadataUrl = "wsdl";
                serviceBehavior.Add(behaviorExtensionElement);
                
                // Find the service section.
                ServicesSection servicesSection = csg.Sections["services"] as ServicesSection;
                if (servicesSection != null)
                {
                    string fqServiceTypeName = GetFullyQulifiedTypeName(GetServiceTypeName());
                    ServiceElement serviceElement = servicesSection.Services[fqServiceTypeName] as ServiceElement;
                    if (serviceElement != null)
                    {
                        serviceElement.BehaviorConfiguration = "metadataServiceExtension";
                    }
                }
            }
        }

        #endregion
    }
}