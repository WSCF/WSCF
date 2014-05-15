using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Thinktecture.Tools.Web.Services.Wscf.Environment
{
    public static class IOPathHelper
    {
        /// <summary>
        /// Retrieves the relative path of a given file relative to a given directory.
        /// </summary>
        /// <param name="relativePathOf">Absolute path of the file to find the relative path of.</param>
        /// <param name="relativeTo">Path of the directory which the result must be relative to.</param>param>
        /// <returns>A string containing the relative path of the requested file.</returns>
        public static string GetRelativePath(string relativePathOf, string relativeTo)
        {
            // Remove the leading and trailing back slashes.
            relativePathOf = relativePathOf.Trim('\\');
            relativeTo = relativeTo.Trim('\\');

            // Break the directory paths to arrays.
            // 07-23-2005
            // BDS: Modified this section to initialize empty arrays when relativePathOf or relativeTo is 
            // empty.
            string[] ofDirectoryStack;
            string[] toDirectoryStack;

            if (relativePathOf != "")
            {
                ofDirectoryStack = relativePathOf.Split('\\');
            }
            else
            {
                ofDirectoryStack = new string[0];
            }

            if (relativeTo != "")
            {
                toDirectoryStack = relativeTo.Split('\\');
            }
            else
            {
                toDirectoryStack = new string[0];
            }

            int lastMatch = -1;

            for (int index = 0; index < ofDirectoryStack.Length; index++)
            {
                if (index > toDirectoryStack.Length - 1)
                {
                    break;
                }

                if (ofDirectoryStack[index] == toDirectoryStack[index])
                {
                    lastMatch++;
                }
                else
                {
                    break;
                }
            }

            string relativePath = "";
            // Add the reverse lookup.
            int reverse = toDirectoryStack.Length - (lastMatch + 1);
            for (int index = 0; index < reverse; index++)
            {
                relativePath = "../" + relativePath;
            }

            int nextItem = lastMatch + 1;
            if (lastMatch < ofDirectoryStack.Length - 1)
            {
                relativePath = relativePath + string.Join("/", ofDirectoryStack, nextItem,
                    (ofDirectoryStack.Length - nextItem));
            }
            return relativePath;
        }

        /// <summary>
        /// Gets the absolute path of a given file with relative path.
        /// </summary>
        /// <param name="relativePath">Relative path of the file to fine the absolute path.</param>
        /// <param name="currentFolder">Path of the current directory.</param>
        /// <param name="rootFolder">Path of the root directory.</param>
        /// <returns>A string containing the absolute path of the given file.</returns>
        public static string GetAbsolutePath(string relativePath, string currentFolder, string rootFolder)
        {
            // Trim the leading and trailing / and \ characters.
            relativePath = relativePath.Trim('/');
            currentFolder = currentFolder.Trim('\\');
            rootFolder = rootFolder.Trim('\\');

            // Break the path into an array.
            string[] directories = relativePath.Split('/');
            string parents = "";
            int index = 0;

            string cf = currentFolder;
            DirectoryInfo pd = null;

            for (index = 0; index < directories.Length; index++)
            {
                if (directories[index] == "..")
                {
                    pd = Directory.GetParent(cf);
                    if (pd != null)
                    {
                        if (pd.FullName.ToLower() == rootFolder.ToLower())
                        {
                            parents = "";
                            index++;
                            break;
                        }
                        else
                        {
                            parents = pd.Name;
                            cf = pd.FullName;
                        }
                    }
                    else
                    {
                        throw new DirectoryNotFoundException("Directory not found.");
                    }

                }
                else
                {
                    break;
                }
            }

            // Fill the blanks.
            if (pd != null && pd.FullName.ToLower() != rootFolder.ToLower())
            {
                pd = Directory.GetParent(pd.FullName);
                while (pd.FullName.ToLower() != rootFolder.ToLower())
                {
                    parents = pd.Name + "\\" + parents;
                    pd = Directory.GetParent(pd.FullName);
                }
            }

            string children = string.Join("\\", directories, index, directories.Length - index);
            string absPath = "";
            if (parents != "")
            {
                absPath = rootFolder + "\\" + parents;
            }
            else if (pd != null)
            {
                absPath = rootFolder;
            }
            else
            {
                absPath = currentFolder;
            }

            if (children != "")
            {
                absPath = absPath + "\\" + children;
            }
            return absPath;

        }
    }
}
