using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    // May be we should convert this to static.
    internal class CodeRefactoringAgent
    {
        #region Private Fields

        // This dictionary is used to keep track of variable declaration statements by veriable names.
        // Make sure that this is cleared before adding method variables.
        private Dictionary<string, CodeVariableDeclarationStatement> stackVariables = new Dictionary<string,CodeVariableDeclarationStatement>();

        #endregion

        #region Constructors

        public CodeRefactoringAgent()
        {
        }

        #endregion

        #region Public Methods

        public void Refactor(CodeTypeExtension typeExtension, string oldName, string newName)
        {
            RefactorType(typeExtension, oldName, newName);
            RefactorFields(typeExtension.Fields, oldName, newName);
            RefactorProperties(typeExtension.Properties, oldName, newName);
            
            // We refactor methods only for client types, service contracts and
            // service types.
            if (typeExtension.Kind == CodeTypeKind.ClientType ||
                typeExtension.Kind == CodeTypeKind.ServiceContract ||
                typeExtension.Kind == CodeTypeKind.ServiceType)
            {
                RefactorMethods(typeExtension.Methods, oldName, newName);
            }
        }

        #endregion

        #region Private Methods

        #region Refactor Type

        private void RefactorType(CodeTypeExtension typeExtension, string oldName, string newName)
        {
            CodeTypeDeclaration type = (CodeTypeDeclaration)typeExtension.ExtendedObject;
            RefactorCodeTypeReferenceCollection(type.BaseTypes, oldName, newName);
            RefactorCodeTypeReferencesInAttributes(type.CustomAttributes, oldName, newName);
        }

        #endregion

        #region Refactor Fields

        private void RefactorFields(FilteredTypeMembers fields, string oldName, string newName)
        {
            foreach (CodeTypeMemberExtension memberExtension in fields)
            {
                CodeMemberField field = (CodeMemberField)memberExtension.ExtendedObject;
                RefactorCodeTypeReference(field.Type, oldName, newName);
                RefactorCodeTypeReferencesInAttributes(field.CustomAttributes, oldName, newName);
            }
        }

        #endregion

        #region Refactor Properties

        private void RefactorProperties(FilteredTypeMembers properties, string oldName, string newName)
        {
            foreach (CodeTypeMemberExtension memberExtension in properties)
            {
                CodeMemberProperty property = (CodeMemberProperty)memberExtension.ExtendedObject;
                // Update the property type.
                RefactorCodeTypeReference(property.Type, oldName, newName);
                // Update the type references in attributes.
                RefactorCodeTypeReferencesInAttributes(property.CustomAttributes, oldName, newName);
            }
        }

        #endregion 

        #region Refactor Methods

        /// <summary>
        /// This method updates the references in methods.
        /// </summary>
        private void RefactorMethods(FilteredTypeMembers methods, string oldName, string newName)
        {
            // Do this for all member extensions wrapping methods.
            foreach (CodeTypeMemberExtension memberExtension in methods)
            {
                // Get a reference to the underlying CodeMemberMethod object.
                CodeMemberMethod method = (CodeMemberMethod)memberExtension.ExtendedObject;
                // Update the parameters types.
                RefactorMethodParameterReferences(method.Parameters, oldName, newName);       
                // Do we have a return type?
                if (method.ReturnType != null)
                {
                    // Update the return type if necessary.
                    RefactorCodeTypeReference(method.ReturnType, oldName, newName);
                }
                // Update method attributes.
                RefactorCodeTypeReferencesInAttributes(method.CustomAttributes, oldName, newName);
                // Update the statements.
                RefactorCodeStatements(method.Statements, oldName, newName);
            }
        }

        private void RefactorMethodParameterReferences(CodeParameterDeclarationExpressionCollection parameters, string oldName, string newName)
        {
            foreach (CodeParameterDeclarationExpression parameter in parameters)
            {
                RefactorCodeTypeReference(parameter.Type, oldName, newName);
                RefactorCodeTypeReferencesInAttributes(parameter.CustomAttributes, oldName, newName);
            }
        }

        /// <summary>
        /// This method updates the references in code statements.
        /// </summary>
        private void RefactorCodeStatements(CodeStatementCollection statements, string oldName, string newName)
        {
            // First clear the stackVariables.
            stackVariables.Clear();

            // Do this for all statements.
            foreach (CodeStatement statement in statements)
            {
                // Update the CodeTypeReferences in this statement.
                CodeTypeReferenceCollection ctrs = GetCodeTypeReferenceInCodeStatement(statement);
                RefactorCodeTypeReferenceCollection(ctrs, oldName, newName);
                RefactorMethodNamesInStatement(statement, newName);

                // From this point we can do the additional things we need to do to 
                // tweak the statement.

                // Is this a variable declaration statement?
                if (typeof(CodeVariableDeclarationStatement) == statement.GetType())
                {
                    CodeVariableDeclarationStatement vdeclStatement = (CodeVariableDeclarationStatement)statement;
                    stackVariables.Add(vdeclStatement.Name, vdeclStatement);                    
                    continue;
                }
                
                // Is this an assignment statement?
                if (typeof(CodeAssignStatement) == statement.GetType())
                {
                    CodeAssignStatement asnStatement = (CodeAssignStatement)statement;
                    // Update the field reference on the left expression.
                    RefactorFieldNamesInFieldReferences(asnStatement.Left, newName);
                    // Update the field reference on the right expression.
                    RefactorFieldNamesInFieldReferences(asnStatement.Right, newName);
                    continue;
                }

                // Is this a return statement?
                if (typeof(CodeMethodReturnStatement) == statement.GetType())
                {
                    CodeMethodReturnStatement retStatement = (CodeMethodReturnStatement)statement;
                    // Update the field reference in the return expression if available.
                    RefactorFieldNamesInFieldReferences(retStatement.Expression, newName);
                }
            }
        }

        private void RefactorFieldNamesInFieldReferences(CodeExpression expression, string newName)
        {
            // Do we have a field reference in the expression we are given?
            if(typeof(CodeFieldReferenceExpression) != expression.GetType())
            {
                return;
            }

            CodeFieldReferenceExpression frefStatement = (CodeFieldReferenceExpression)expression;
            
            // Is target object of field reference referring to a variable declaration?
            if (typeof(CodeVariableReferenceExpression) == frefStatement.TargetObject.GetType())
            {
                CodeVariableReferenceExpression vrefStatement = (CodeVariableReferenceExpression)frefStatement.TargetObject;
                // Get a reference to the variable declaration statement.
                CodeVariableDeclarationStatement vdeclStatement = stackVariables[vrefStatement.VariableName];

                // Is this a variable of type we are modifying now?
                if (vdeclStatement.Type.BaseType == newName)
                {
                    // Then we can convert the field names to PascalCase.
                    frefStatement.FieldName = PascalCaseConverterHelper.GetPascalCaseName(frefStatement.FieldName);
                }
            }
        }

        private void RefactorMethodNamesInStatement(CodeStatement statement, string newName)
        {
            if (typeof(CodeVariableDeclarationStatement) == statement.GetType())
            {
                CodeVariableDeclarationStatement vdeclStatement = (CodeVariableDeclarationStatement)statement;
                if (vdeclStatement.InitExpression != null)
                {
                    if (typeof(CodeCastExpression) == vdeclStatement.InitExpression.GetType())
                    {
                        CodeCastExpression castExp = (CodeCastExpression)vdeclStatement.InitExpression;
                        if (typeof(CodeMethodInvokeExpression) == castExp.Expression.GetType())
                        {
                            CodeMethodInvokeExpression miExp = (CodeMethodInvokeExpression)castExp.Expression;
                            miExp.Method.MethodName = PascalCaseConverterHelper.GetPascalCaseMethodName(miExp.Method.MethodName);
                        }
                    }
                    else if (typeof(CodeMethodInvokeExpression) == vdeclStatement.InitExpression.GetType())
                    {
                        CodeMethodInvokeExpression miExp = (CodeMethodInvokeExpression)vdeclStatement.InitExpression;
                        miExp.Method.MethodName = PascalCaseConverterHelper.GetPascalCaseMethodName(miExp.Method.MethodName);
                    }
                }                
            }
            else if (typeof(CodeExpressionStatement) == statement.GetType())
            {
                CodeExpressionStatement ceStatement = (CodeExpressionStatement)statement;
                if (typeof(CodeMethodInvokeExpression) == ceStatement.Expression.GetType())
                {
                    CodeMethodInvokeExpression miExp = (CodeMethodInvokeExpression)ceStatement.Expression;
                    miExp.Method.MethodName = PascalCaseConverterHelper.GetPascalCaseMethodName(miExp.Method.MethodName);
                }
            }
            else if (typeof(CodeAssignStatement) == statement.GetType())
            {
                CodeAssignStatement asnStatement = (CodeAssignStatement)statement;                
                if (typeof(CodeCastExpression) == asnStatement.Right.GetType())
                {
                    CodeCastExpression castExp = (CodeCastExpression)asnStatement.Right;
                    if (typeof(CodeMethodInvokeExpression) == castExp.Expression.GetType())
                    {
                        CodeMethodInvokeExpression miExp = (CodeMethodInvokeExpression)castExp.Expression;
                        miExp.Method.MethodName = PascalCaseConverterHelper.GetPascalCaseMethodName(miExp.Method.MethodName);
                    }
                }
                else if (typeof(CodeMethodInvokeExpression) == asnStatement.Right.GetType())
                {
                    CodeMethodInvokeExpression miExp = (CodeMethodInvokeExpression)asnStatement.Right;
                    miExp.Method.MethodName = PascalCaseConverterHelper.GetPascalCaseMethodName(miExp.Method.MethodName);
                }                
            }
            else if (typeof(CodeMethodReturnStatement) == statement.GetType())
            {
                CodeMethodReturnStatement retStatement = (CodeMethodReturnStatement)statement;
                if (typeof(CodeMethodInvokeExpression) == retStatement.Expression.GetType())
                {
                    CodeMethodInvokeExpression miExp = (CodeMethodInvokeExpression)retStatement.Expression;
                    miExp.Method.MethodName = PascalCaseConverterHelper.GetPascalCaseMethodName(miExp.Method.MethodName);
                }
            }            
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// This method contains the core logic for changing a type reference to use 
        /// a new type.
        /// </summary>
        private void RefactorCodeTypeReference(CodeTypeReference codeTypeReference, string oldName, string newName)
        {
            // Is this an array type field? Then check the array type.
            if (codeTypeReference.ArrayElementType != null)
            {
                // Is the array type is as same as the type we modified.
                if (codeTypeReference.ArrayElementType.BaseType == oldName)
                {
                    // Change the array type name.
                    codeTypeReference.ArrayElementType.BaseType = newName;
                }
            }
            else
            {
                // We arrive here if the field type is not an array type.
                // Check if the type is a generic type or not.
                if (codeTypeReference.TypeArguments.Count == 0)
                {
                    // Is the field type as same as the type we modified.
                    if (codeTypeReference.BaseType == oldName)
                    {
                        // Change the field type name.
                        codeTypeReference.BaseType = newName;
                    }
                }
                else
                {
                    // We arrive here if the field type is a genric type.
                    // Check for the code type arguments for generic type fields.
                    foreach (CodeTypeReference gtype in codeTypeReference.TypeArguments)
                    {
                        // Do we have a generic argument of type we modified?
                        if (gtype.BaseType == oldName)
                        {
                            // Set the generic argument type name.s
                            gtype.BaseType = newName;
                        }
                    }
                }
            }
        }

        private void RefactorCodeTypeReferenceCollection(CodeTypeReferenceCollection codeTypeReferences, string oldName, string newName)
        {
            foreach (CodeTypeReference codeTypeReference in codeTypeReferences)
            {
                RefactorCodeTypeReference(codeTypeReference, oldName, newName);
            }
        }

        private void RefactorCodeTypeReferencesInAttributes(CodeAttributeDeclarationCollection attribs, string oldName, string newName)
        {
            foreach (CodeAttributeDeclaration attrib in attribs)
            {
                RefactorCodeTypeReferencesInAttributeArguments(attrib.Arguments, oldName, newName);
            }
        }

        private void RefactorCodeTypeReferencesInAttributeArguments(CodeAttributeArgumentCollection arguments, string oldName, string newName)
        {
            foreach (CodeAttributeArgument argument in arguments)
            {
                CodeTypeReference typeRef = GetCodeTypeReferenceInCodeExpression(argument.Value);
                if (typeRef != null)
                {
                    RefactorCodeTypeReference(typeRef, oldName, newName);
                }
            }
        }

        private CodeTypeReference GetCodeTypeReferenceInCodeExpression(CodeExpression expression)
        {
            if (typeof(CodeTypeOfExpression) == expression.GetType())
            {
                return ((CodeTypeOfExpression)expression).Type;
            }

            if (typeof(CodeTypeReferenceExpression) == expression.GetType())
            {
                return ((CodeTypeReferenceExpression)expression).Type;
            }

            if (typeof(CodeFieldReferenceExpression) == expression.GetType())
            {
                return GetCodeTypeReferenceInCodeExpression(((CodeFieldReferenceExpression)expression).TargetObject);
            }

            if (typeof(CodeObjectCreateExpression) == expression.GetType())
            {
                return ((CodeObjectCreateExpression)expression).CreateType;
            }

            if (typeof(CodeCastExpression) == expression.GetType())
            {
                return ((CodeCastExpression)expression).TargetType;
            }

            if (typeof(CodeMethodInvokeExpression) == expression.GetType())
            {
                return GetCodeTypeReferenceInCodeExpression(((CodeMethodInvokeExpression)expression).Method);
            }

            if (typeof(CodeMethodReferenceExpression) == expression.GetType())
            {
                return GetCodeTypeReferenceInCodeExpression(((CodeMethodReferenceExpression)expression).TargetObject);
            }
            
            return null;
        }

        private CodeTypeReferenceCollection GetCodeTypeReferenceInCodeStatement(CodeStatement statement)
        {
            CodeTypeReferenceCollection references = new CodeTypeReferenceCollection();

            // Is this a variable declaration statement?
            if (typeof(CodeVariableDeclarationStatement) == statement.GetType())
            {
                // Add CodeTypeReference used to define the type of the variable to output.
                CodeVariableDeclarationStatement vdeclStatement = (CodeVariableDeclarationStatement)statement;
                references.Add(vdeclStatement.Type);
                
                // Do we have an initialization expression?
                if (vdeclStatement.InitExpression != null)
                {
                    // Add CodeTypeReference in the initialization statement if available.
                    CodeTypeReference r = GetCodeTypeReferenceInCodeExpression(vdeclStatement.InitExpression);
                    Debug.Assert(r != null, "Could not obtain a proper CodeTypeReference from the variable initialization expression.");
                    if (r == null)
                    {
                        Debugger.Break();
                    }
                    references.Add(r);
                }                
            }
            //// Is this a return statement?
            //else if (typeof(CodeMethodReturnStatement) == statement.GetType())
            //{
            //    CodeMethodReturnStatement retStatement = (CodeMethodReturnStatement)statement;
            //    // Add CodeTypeReference in the return statement if available.
            //    CodeTypeReference r = GetCodeTypeReferenceInCodeExpression(retStatement.Expression);
            //    Debug.Assert(r != null, "Could not obtain a proper CodeTypeReference from the variable initialization expression.");
            //    references.Add(r);
            //}

            // Finally return the references.
            return references;
        }

        #endregion

        #endregion
    }
}
