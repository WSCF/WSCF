using System;
using System.Collections.Generic;
using System.CodeDom;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceModel;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class contains the code decorator implementation for generating the 
    /// service type.
    /// </summary>
    internal class ServiceTypeGenerator : ICodeDecorator
    {
        ExtendedCodeDomTree code;
        CustomCodeGenerationOptions options;
        string serviceTypeName;

        #region ICodeDecorator Members

        public void Decorate(ExtendedCodeDomTree code, CustomCodeGenerationOptions options)
        {
            // Some validations to make debugging easier.
            Debug.Assert(code != null, "code parameter could not be null.");
            Debug.Assert(options != null, "options parameter could not be null.");

            // We execute this decorator only if we are generating the service side code.
            if (options.GenerateService)
            {
                // Initialize the state.
                this.code = code;
                this.options = options;
                CreateServiceType();                
            }
        }

        #endregion

        #region Private Methods

        private void CreateServiceType()
        {
            // We can create the service type(s) only if we have one or more service
            // contract.
            if (code.ServiceContracts.Count > 0)
            {
                // Take a reference to the first ServiceContract available.
                // IMPORTANT!:(Currently we only support single service type)
                // May be want to support multiple service contracts in the next version.
                CodeTypeExtension srvContract = code.ServiceContracts[0];
                // Notify if srvContract is null. This would mean that we have constructed a bad 
                // GeneratedCode instance from our CodeFactory.
                Debug.Assert(srvContract != null, "Generated service contract could not be null.");

                // Construct the service type name by removing the leading "I" character from 
				// the service contract name that was added for generation of the interface.
            	string srvTypeName = srvContract.ExtendedObject.Name.Substring(1);

                // Create a new instance of CodeTypeDeclaration type representing the service type.
                CodeTypeDeclaration srvType = new CodeTypeDeclaration(srvTypeName);

                // Also wrap the CodeTypeDeclaration in an extension.
                CodeTypeExtension typeExt = new CodeTypeExtension(srvType);

                // This class.
                srvType.IsClass = true;

				switch (options.MethodImplementation)
				{
					case MethodImplementation.PartialClassMethodCalls:
						// The service type is partial so that the implementation methods can be written in separate file.
						srvType.IsPartial = true;
						break;
					case MethodImplementation.AbstractMethods:
						// The service type is abstract so that the operation methods can be made abstract.
						srvType.TypeAttributes |= TypeAttributes.Abstract;
						break;
				}

                // And this implements the service contract interface.
				if (code.CodeLanguauge == CodeLanguage.VisualBasic)
				{
					srvType.Members.Add(new CodeSnippetTypeMember("Implements " + srvContract.ExtendedObject.Name));
				}
				else
				{
					srvType.BaseTypes.Add(new CodeTypeReference(srvContract.ExtendedObject.Name));					
				}

                // Now itterate the srvContractObject.Members and add each and every method in 
                // the service contract type to the new type being created.
                foreach (CodeTypeMemberExtension methodExtension in srvContract.Methods)
                {
                    // Get a referece to the actual CodeMemberMethod object extended
                    // by ext.
                    CodeMemberMethod method = methodExtension.ExtendedObject as CodeMemberMethod;

                    // Create a new CodeMemeberMethod and copy the attributes.
                    CodeMemberMethod newMethod = new CodeMemberMethod();
                    newMethod.Name = method.Name;

                    // Implemented method has to be public.
                    newMethod.Attributes = MemberAttributes.Public;

                    // Notify that this member is implementing a method in the service contract.
					if (code.CodeLanguauge == CodeLanguage.VisualBasic)
					{
						newMethod.ImplementationTypes.Add(new CodeTypeReference(srvContract.ExtendedObject.Name));
					}
					else
					{
						newMethod.ImplementationTypes.Add(srvType.BaseTypes[0]);						
					}

                    // Add all parametes to the newly created method.
                    foreach (CodeParameterDeclarationExpression cpde in method.Parameters)
                    {
                        newMethod.Parameters.Add(cpde);
                    }

                    // Set the return type.
                    newMethod.ReturnType = method.ReturnType;

					switch (options.MethodImplementation)
					{
						case MethodImplementation.PartialClassMethodCalls:
							{
								// Gather the parameters from the operation to pass into the implementation method.
								IEnumerable<CodeArgumentReferenceExpression> parameters = newMethod.Parameters
									.OfType<CodeParameterDeclarationExpression>()
									.Select(p => new CodeArgumentReferenceExpression(p.Name));

								// Create an expression to invoke the implementation method.
								CodeMethodInvokeExpression methodInvocation = new CodeMethodInvokeExpression(null, newMethod.Name + "Implementation", parameters.ToArray());

								// Check if the method has a return type.
								if (newMethod.ReturnType.BaseType != "System.Void")
								{
									// Make sure the call to the implementation method is returned.
									CodeMethodReturnStatement returnStatement = new CodeMethodReturnStatement(methodInvocation);
									newMethod.Statements.Add(returnStatement);
								}
								else
								{
									// Add the call to the implementation method without a return.
									newMethod.Statements.Add(methodInvocation);
								}
							}
							break;
						case MethodImplementation.NotImplementedException:
							{
								// Create a new code statement to throw NotImplementedExcption.
								CodeThrowExceptionStatement niex = new CodeThrowExceptionStatement(
									new CodeObjectCreateExpression(
										new CodeTypeReference(typeof(NotImplementedException)), new CodeExpression[] { })
									);

								// Add it to the statements collection in the new method.
								newMethod.Statements.Add(niex);
							}
							break;
						case MethodImplementation.AbstractMethods:
							{
								// No statement is required for the abstract methods.
								newMethod.Attributes |= MemberAttributes.Abstract;
								break;
							}
					}

					// Wrap the CodeMemberMethod in an extension. This could be useful for other extensions.
					CodeTypeMemberExtension newMethodExt = new CodeTypeMemberExtension(newMethod, typeExt);
					srvType.Members.Add(newMethodExt);
                }

                // Add the ServiceBehaviorAttribute attribute.
            	CodeAttributeDeclaration serviceBehaviorAttribute = new CodeAttributeDeclaration(
            		new CodeTypeReference(typeof(ServiceBehaviorAttribute)));

				if (!string.IsNullOrEmpty(options.InstanceContextMode))
				{
					CodeTypeReferenceExpression instanceContextModeEnum = new CodeTypeReferenceExpression(typeof(InstanceContextMode));
					CodeFieldReferenceExpression instanceContextModeValue = new CodeFieldReferenceExpression(instanceContextModeEnum, options.InstanceContextMode);
					CodeAttributeArgument instanceContextModeArgument = new CodeAttributeArgument("InstanceContextMode", instanceContextModeValue);
					serviceBehaviorAttribute.Arguments.Add(instanceContextModeArgument);					
				}

				if (!string.IsNullOrEmpty(options.ConcurrencyMode))
				{
					CodeTypeReferenceExpression concurrencyModeEnum = new CodeTypeReferenceExpression(typeof(ConcurrencyMode));
					CodeFieldReferenceExpression concurrencyModeValue = new CodeFieldReferenceExpression(concurrencyModeEnum, options.ConcurrencyMode);
					CodeAttributeArgument concurrencyModeArgument = new CodeAttributeArgument("ConcurrencyMode", concurrencyModeValue);
					serviceBehaviorAttribute.Arguments.Add(concurrencyModeArgument);	
				}

				if (!options.UseSynchronizationContext)
				{
					CodeAttributeArgument useSynchronizationContextAttribute = new CodeAttributeArgument("UseSynchronizationContext", new CodePrimitiveExpression(false));
					serviceBehaviorAttribute.Arguments.Add(useSynchronizationContextAttribute);					
				}

            	typeExt.AddAttribute(serviceBehaviorAttribute);

                this.serviceTypeName = srvType.Name;
                // Finally add the newly created type to the code being generated.
                code.ServiceTypes.Add(typeExt);
            }
        }        

        #endregion
    }
}
