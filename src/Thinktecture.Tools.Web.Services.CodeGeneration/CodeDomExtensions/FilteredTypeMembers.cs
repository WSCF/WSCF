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
    [DebuggerStepThrough]
    public class FilteredTypeMembers : IList<CodeTypeMemberExtension>
    {
        // Reference to the CodeTypeMemberCollection belonging the CodeTypeMember(s) 
        // in the collection.
        CodeTypeMemberCollection internalMembersCollection;
        // Reference to the list of CodeTypeMemberExtension(s) we use internally.
        List<CodeTypeMemberExtension> internalList;

        public FilteredTypeMembers(CodeTypeMemberCollection internalMembersCollection)
        {
            this.internalMembersCollection = internalMembersCollection;
            this.internalList = new List<CodeTypeMemberExtension>();
        }

        #region IList<CodeTypeMemberExtension> Members

        public int IndexOf(CodeTypeMemberExtension item)
        {
            return internalList.IndexOf(item);
        }

        public void Insert(int index, CodeTypeMemberExtension item)
        {
            internalList.Insert(index, item);
            internalMembersCollection.Add(item.ExtendedObject);
        }

        public void RemoveAt(int index)
        {
            // TODO: Validations
            if (internalMembersCollection.Count > index)
            {
                // Take a reference to the extended object.
                CodeTypeMemberExtension memberExtension = internalList[index];
                // Remove the type from the CodeNamespace.
                internalMembersCollection.Remove(memberExtension.ExtendedObject);
                // Finally remove the extended object.
                internalList.RemoveAt(index);
            }
        }

        public CodeTypeMemberExtension this[int index]
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

        public void Add(CodeTypeMemberExtension item)
        {
            if (!internalMembersCollection.Contains(item.ExtendedObject))
            {
                // First add the CodeTypeDeclaration to the CodeNamespace.
                internalMembersCollection.Add(item.ExtendedObject);
            }
            // Now add the item to the inner list.
            internalList.Add(item);
        }

        public void Clear()
        {
            // Remove all types from the CodeNamespace.
            foreach (CodeTypeMemberExtension memberExtension in internalList)
            {
                internalMembersCollection.Remove(memberExtension.ExtendedObject);
            }
            // Finally clear the internal list.
            internalList.Clear();
        }

        public bool Contains(CodeTypeMemberExtension item)
        {
            return internalList.Contains(item);
        }

        public void CopyTo(CodeTypeMemberExtension[] array, int arrayIndex)
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

        public bool Remove(CodeTypeMemberExtension item)
        {
            internalMembersCollection.Remove(item.ExtendedObject);
            return internalList.Remove(item);
        }

        #endregion

        #region IEnumerable<CodeTypeMemberExtension> Members

        public IEnumerator<CodeTypeMemberExtension> GetEnumerator()
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
