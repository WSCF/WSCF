using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Diagnostics;
using System.IO;
using Thinktecture.Tools.Web.Services.CodeGeneration;
using VSLangProj;
using VSLangProj80;
using VsWebSite;

namespace Thinktecture.Tools.Web.Services.ContractFirst.VsObjectWrappers
{
    internal class VisualStudioProject
    {
        #region Private Fields

        private readonly Project project;

        private const string prjKindVBProject = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";
        private const string prjKindCSharpProject = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

        #endregion

        #region Public Constructors

        public VisualStudioProject(Project project)
        {
            this.project = project;
        }

        #endregion

        #region Public Properties

        public string ProjectName
        {
            get { ThreadHelper.ThrowIfNotOnUIThread(); return project.Name; }
        }

        public string ProjectFileName
        {
            get { ThreadHelper.ThrowIfNotOnUIThread(); return project.FileName; }
        }

        public bool IsWebProject
        {
            get { ThreadHelper.ThrowIfNotOnUIThread(); return project.Object is VSWebSite; }
        }

        public CodeLanguage ProjectLanguage
        {
            get
            {
                return (IsWebProject) ? GetWebProjectLanguage() : GetProjectLanguage();
            }
        }

        public string AssemblyNamespace
        {
            get
            {
                return GetProjectProperty("DefaultNamespace");
            }
        }

        public string ProjectDirectory
        {
            get
            {
                return GetProjectProperty("FullPath");
            }
        }

        private string AssemblyName
        {
            get
            {
                return GetProjectProperty("AssemblyName");
            }
        }

        #endregion

        #region Public Methods

        public void AddFile(string file)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Debug.Assert(File.Exists(file), "Attemp to add a non-existing file.");
            project.ProjectItems.AddFromFile(file);
            Refresh();
        }

        public string AddCodeFolderToWebProject(string name)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Debug.Assert(IsWebProject, "Cannot add code folders to a web project.");
            VSWebSite website = (VSWebSite)project.Object;
            string relativePath = string.Format(@"App_Code/{0}", name);
            website.CodeFolders.Add(relativePath);

            CodeFolder appCodeFolder = website.CodeFolders.Item(1);
            return Path.Combine(appCodeFolder.ProjectItem.get_FileNames(0), name);
        }

        public void AddReference(string assembly)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                if (IsWebProject)
                {
                    VSWebSite website = this.project.Object as VSWebSite;
                    website.References.AddFromGAC(assembly);
                }
                else if (this.project.Object is VSProject2 prj2)
                {
                    prj2.References.Add(assembly);
                }
                else
                {
                    VSProject prj = this.project.Object as VSProject;
                    prj.References.Add(assembly);
                }
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"Failure adding reference to assembly {assembly}: {ex.Message}");
            }
        }

        public string GetDefaultDestinationFilename(string fileName)
        {
            string baseFileName = Path.GetFileNameWithoutExtension(fileName);
            string extension = (ProjectLanguage == CodeLanguage.VisualBasic) ? "vb" : "cs";
            return Path.ChangeExtension(baseFileName, extension);
        }

        #endregion

        #region Private Methods

        private string GetProjectProperty(string key)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            try
            {
                return (string)project.Properties.Item(key).Value;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private CodeLanguage GetWebProjectLanguage()
        {
            string language = GetProjectProperty("CurrentWebSiteLanguage");

            switch (language)
            {
                case "Visual Basic":
                    return CodeLanguage.VisualBasic;
                case "Visual C#":
                    return CodeLanguage.CSharp;
                default:
                    return CodeLanguage.CSharp;
            }
        }

        private CodeLanguage GetProjectLanguage()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            //public abstract class PrjKind
            //{
            //    [MarshalAs(UnmanagedType.LPStr)]
            //    public const string prjKindVBProject = "{F184B08F-C81C-45F6-A57F-5ABD9991F28F}";
            //    [MarshalAs(UnmanagedType.LPStr)]
            //    public const string prjKindCSharpProject = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";
            //    [MarshalAs(UnmanagedType.LPStr)]
            //    public const string prjKindVSAProject = "{13B7A3EE-4614-11D3-9BC7-00C04F79DE25}";
            //}

            switch (project.Kind)
            {
                case prjKindVBProject:
                    return CodeLanguage.VisualBasic;
                case prjKindCSharpProject:
                    return CodeLanguage.CSharp;
                default:
                    return CodeLanguage.CSharp;
            }
        }

        private bool Refresh()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (IsWebProject)
            {
                VSWebSite website = this.project.Object as VSWebSite;
                if (website != null)
                {
                    website.Refresh();
                    return true;
                }
            }
            else
            {
                VSProject2 project = this.project.Object as VSProject2;
                if (project != null)
                {
                    project.Refresh();
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
