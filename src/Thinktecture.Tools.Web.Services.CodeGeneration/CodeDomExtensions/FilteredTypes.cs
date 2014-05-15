using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.Diagnostics;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This type abstracts a collection of CodeTypeMemberExtension(s).
    /// </summary>    
    public class FilteredTypes : IList<CodeTypeExtension>
    {
        // Reference to the CodeNamespace belonging the CodeTypeMember(s) 
        // in the collection.
        CodeNamespace codeNamespace;
        // Reference to the list of CodeTypeMemberExtension(s) we use internally.
        List<CodeTypeExtension> internalList;

        public FilteredTypes(CodeNamespace codeNamespace)
        {
            this.codeNamespace = codeNamespace;
            this.internalList = new List<CodeTypeExtension>();
        }

        #region IList<CodeTypeMemberExtension> Members

        public int IndexOf(CodeTypeExtension item)
        {
            return internalList.IndexOf(item);
        }

        public void Insert(int index, CodeTypeExtension item)
        {
            internalList.Insert(index, item);
            codeNamespace.Types.Add((CodeTypeDeclaration)item.ExtendedObject);
        }

        public void RemoveAt(int index)
        {
            // TODO: Validations
            if (internalList.Count > index)
            {
                // Take a reference to the extended object.
                CodeTypeExtension ext = internalList[index];
                // Remove the type from the CodeNamespace.
                codeNamespace.Types.Remove((CodeTypeDeclaration)ext.ExtendedObject);
                // Finally remove the extended object.
                internalList.RemoveAt(index);
            }
        }

        public CodeTypeExtension this[int index]
        {
            get
            {
                // Gets the CodeTypeMemberExtension at the given index.
                return internalList[index];
            }
            set
            {
                // Sets the CodeTypeMemberExtension at the given index.
                internalList[index] = value;
            }
        }

        #endregion

        #region ICollection<CodeTypeMemberExtension> Members

        public void Add(CodeTypeExtension item)
        {
            if (!codeNamespace.Types.Contains((CodeTypeDeclaration)item.ExtendedObject))
            {
                // First add the CodeTypeDeclaration to the CodeNamespace.
                codeNamespace.Types.Add((CodeTypeDeclaration)item.ExtendedObject);
            }
            // Now add the item to the inner list.
            internalList.Add(item);
        }

        public void Clear()
        {
            // Remove all types from the CodeNamespace.
            foreach (CodeTypeExtension ext in internalList)
            {
                codeNamespace.Types.Remove((CodeTypeDeclaration)ext.ExtendedObject);
            }
            // Finally clear the internal list.
            internalList.Clear();
        }

        public bool Contains(CodeTypeExtension item)
        {
            return internalList.Contains(item);
        }

        public void CopyTo(CodeTypeExtension[] array, int arrayIndex)
        {
            internalList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return internalList.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(CodeTypeExtension item)
        {
            codeNamespace.Types.Remove((CodeTypeDeclaration)item.ExtendedObject);
            return internalList.Remove(item);
        }

        #endregion

        #region IEnumerable<CodeTypeMemberExtension> Members

        public IEnumerator<CodeTypeExtension> GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return internalList.GetEnumerator();
        }

        #endregion
    }
}
