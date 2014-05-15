using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.CodeDom;
using System.Diagnostics;

using Thinktecture.Tools.Web.Services.Wscf.Environment;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    [DebuggerDisplay("Name = {extendedObject.Name}")]
    public abstract class AttributableCodeDomObject : CodeTypeMember
    {
        #region Private Fields

        // Holds a reference to the actual object we are decorating.
        private CodeTypeMember extendedObject;
        // Dictionary we use for caching the CodeAttributeDeclaration(s).
        private Dictionary<string, CodeAttributeDeclaration> attributesCache;

        #endregion

        #region Constructors

        public AttributableCodeDomObject(CodeTypeMember extendedObject)
        {
            this.extendedObject = extendedObject;
            attributesCache = new Dictionary<string, CodeAttributeDeclaration>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This method finds the attribute as specified by attribute parameter.
        /// </summary>
        /// <param name="attribute">string containing the name of the attribute.</param>
        /// <returns>
        /// If the requested attribute is found, this method returns a reference to it. 
        /// Otherwise returns null.
        /// </returns>
        /// <remarks>
        /// This method initially looks up an internal attribute cache. If the attribute
        /// is not found in the cache, it searches for it in the actual object extended
        /// by this instance. If the attribute is found, this method adds it to internal 
        /// cache causing the faster access time in the subsequent requests.
        /// </remarks>
        public CodeAttributeDeclaration FindAttribute(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName))
            {
                throw new ArgumentException("attributeName could not be null or an empty string.");
            }

            if (attributesCache.ContainsKey(attributeName))
            {
                return attributesCache[attributeName];
            }

            foreach (CodeAttributeDeclaration attribDecl in extendedObject.CustomAttributes)
            {
                if (attribDecl.Name == attributeName)
                {
                    attributesCache.Add(attributeName, attribDecl);
                    return attribDecl;
                }
            }
            return null;
        }

		/// <summary>
		/// Finds all instances of the attribute with the specified name.
		/// </summary>
		/// <param name="attributeName">Name of the attribute.</param>
		/// <returns>A list of the matching attributes.</returns>
		public IEnumerable<CodeAttributeDeclaration> FindAttributes(string attributeName)
		{
			foreach (CodeAttributeDeclaration attribute in extendedObject.CustomAttributes
				.Cast<CodeAttributeDeclaration>()
				.Where(attribute => attribute.Name == attributeName))
			{
				yield return attribute;
			}
			yield break;
		}

    	/// <summary>
		/// Removes the attribute from the custom attributes collection of the type.
		/// </summary>
		/// <param name="attribute">The attribute to remove.</param>
		public void RemoveAttribute(CodeAttributeDeclaration attribute)
		{
			Enforce.IsNotNull(attribute, "attribute");

			if (attributesCache.ContainsKey(attribute.Name))
			{
				attributesCache.Remove(attribute.Name);
			}

			extendedObject.CustomAttributes.Remove(attribute);
		}

        /// <summary>
        /// Adds a new attribute to a attributes collection or modify an existing attribute.
        /// It checks whether a given attribute exists and adds it if its not there. If it is there, then it will
        /// add the arguments available in the new attribute but not available in the existing attribute.
        /// </summary>
        /// <param name="attribute">Attribute to add.</param>
        /// <returns>
        /// Returns true if the attribute is actually added to the object. Otherwise returns false. 
        /// The latter happens when the attribute already exists in the object.
        /// </returns>
        public bool AddAttribute(CodeAttributeDeclaration attribDecl)
        {
            if (attribDecl == null)
            {
                throw new ArgumentException("attribDecl could not be null.");
            }

            CodeAttributeDeclaration existingAttrib = FindAttribute(attribDecl.Name);
            // Can we see a matching, existing attribute? Then try to sync the attributes.
            if (existingAttrib != null)
            {
                CodeTypeMemberExtension.SyncSourceAttributes(existingAttrib, attribDecl);
                return false;
            }
            else
            {
                // Add this to the CustomAttributes collection.
                extendedObject.CustomAttributes.Add(attribDecl);
                attributesCache.Add(attribDecl.Name, attribDecl);
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private static void SyncSourceAttributes(CodeAttributeDeclaration source, CodeAttributeDeclaration destination)
        {
            Debug.Assert(source != null, "source parameter could not be null.");
            Debug.Assert(destination != null, "destination parameter could not be null.");

            // Process all arguments in the destination attribute.
            foreach (CodeAttributeArgument dstArg in destination.Arguments)
            {
                bool argumentFound = false;
                CodeAttributeArgumentExtended extDstArg = dstArg as CodeAttributeArgumentExtended;

                // Lookup all attribute arguments in the source to see if we can find a matching 
                // argument.
                foreach (CodeAttributeArgument srcArg in source.Arguments)
                {
                    // Can we access the argument with our extended type pointer?
                    // Then use it to match empty argument names generated for 
                    // default arguments.
                    if (extDstArg != null)
                    {
                        if ((string.Compare(srcArg.Name, extDstArg.Name, true) == 0) ||
                            (extDstArg.Default && srcArg.Name == ""))
                        {
                            // We've found the argument.                            
                            argumentFound = true;
                            // Sync value.
                            srcArg.Value = extDstArg.Value;
                            break;
                        }
                    }
                    else
                    {
                        if (string.Compare(srcArg.Name, dstArg.Name, true) == 0)
                        {
                            // We've found the argument.                            
                            argumentFound = true;
                            // Sync value.
                            srcArg.Value = dstArg.Value;
                            break;
                        }
                    }
                }

                // Add the argument if we haven't found it yet.
                if (!argumentFound)
                {
                    source.Arguments.Add(dstArg);
                }
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Gets the original instance extended by this decorator instance.        
        /// </summary>
        /// <remarks>
        /// If you want to use the internal instance use this property to access it.
        /// However, refrain from searching and adding attributes directly using this 
        /// reference if you are concerned about caching.
        /// </remarks>
        public CodeTypeMember ExtendedObject
        {
            get { return extendedObject; }
        }
        
        #endregion
    }
}
