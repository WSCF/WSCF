using System;
using System.IO;

namespace Thinktecture.Tools.Web.Services.Wscf.Environment
{
	public class FileManipulationHelper
	{
		public static string GetRelativePath(string relativePathOf, string relativeTo)
		{
			// Remove the leading and trailing back slashs.
			relativePathOf = relativePathOf.Trim('\\');
			relativeTo = relativeTo.Trim('\\');
			
			// Break the directory paths to arrays.
			string[] ofDirectoryStack = relativePathOf.Split('\\');
			string[] toDirectoryStack = relativeTo.Split('\\');
			
			int lastMatch = -1;
			
			for(int index = 0; index < ofDirectoryStack.Length; index++)
			{
				if(index > toDirectoryStack.Length - 1)
				{
					break;
				}

				if(ofDirectoryStack[index] == toDirectoryStack[index])
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
			for(int index = 0; index < reverse; index++)
			{
				relativePath = "../" + relativePath;
			}
			
			int nextItem = lastMatch + 1;
			if(lastMatch < ofDirectoryStack.Length - 1)
			{
				relativePath = relativePath + string.Join("/", ofDirectoryStack, nextItem, 
					(ofDirectoryStack.Length - nextItem));
			}
			return relativePath;
		}
		
		public static string GetAbsolutePath(string relativePath, string currentFolder, string rootFolder)
		{
			// Trim the leading and trailing / charactors.
			relativePath = relativePath.Trim('/');
			
			// Break the path in to an array.
			string[] directories = relativePath.Split('/');
			string parents = "";
			int index = 0;

			string cf = currentFolder;
			DirectoryInfo pd = null;

			for(index = 0; index < directories.Length; index++)
			{
				if(directories[index] == "..")
				{
					pd = Directory.GetParent(cf);
					if(pd != null)
					{
						if(pd.FullName.ToLower() == rootFolder.ToLower())
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
			if(pd != null && pd.FullName.ToLower() != rootFolder.ToLower())
			{
				pd = Directory.GetParent(pd.FullName);
				while(pd.FullName.ToLower() != rootFolder.ToLower())
				{
					parents = pd.Name + "\\" + parents;
					pd = Directory.GetParent(pd.FullName);
				}
			}

			string children = string.Join("\\", directories, index, directories.Length - index);
			string absPath = ""; 
			if(parents != "")
			{
				absPath = rootFolder + "\\" + parents;
			}
			else if(pd != null)
			{
				absPath = rootFolder;
			}
			else
			{
				absPath = currentFolder;
			}

			if(children != "")
			{
				absPath = absPath + "\\" + children; 
			}
			return absPath;

		}
	}
}
