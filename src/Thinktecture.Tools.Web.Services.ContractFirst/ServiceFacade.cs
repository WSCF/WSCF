using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using EnvDTE80;
using Thinktecture.Tools.Web.Services.CodeGeneration;
using Thinktecture.Tools.Web.Services.ContractFirst.VsObjectWrappers;
using Thinktecture.Tools.Web.Services.WsdlWizard;

namespace Thinktecture.Tools.Web.Services.ContractFirst
{
    public enum WscfCommand
    {
        Undefined = 0,
        CreateWsdl = 1,
        EditWsdl = 2,
        GenerateDataContractCode = 3,
        GenerateWebServiceCode = 4,
        PasteXmlAsSchema = 5
    }

    /// <summary>
    /// </summary>
    // NOTE: Enable comments starting with *** to enable the simple logging mechanism we use.
    internal class ServiceFacade
    {
        private OutputWindowWriter OutputWindowWriter { get; }

        private VisualStudio VisualStudio { get; }

        public ServiceFacade(DTE2 application)
        {
            VisualStudio = new VisualStudio(application);

            OutputWindowWriter = new OutputWindowWriter(application);

            AppLog.LogMessage($"Created new instance of {nameof(ServiceFacade)}.");
        }

        public bool ExecuteCommand(WscfCommand command)
        {
            try
            {
                AppLog.LogMessage($"Entering {nameof(this.ExecuteCommand)} method.");

                switch (command)
                {
                    case WscfCommand.CreateWsdl:
                        return ProcessWsdlWizardRequest(false);
                    case WscfCommand.EditWsdl:
                        return ProcessWsdlWizardRequest(true);
                    case WscfCommand.GenerateDataContractCode:
                        return ProcessXsdCodeGenerationRequest();
                    case WscfCommand.GenerateWebServiceCode:
                        return ProcessCodeGenerationRequest();
                    case WscfCommand.PasteXmlAsSchema:
                        return ProcessPasteSchemaRequest();
                    default:
                        return false;
                }

            }
            finally
            {
                AppLog.LogMessage($"Leaving {nameof(this.ExecuteCommand)} method.");
            }

        }

        private bool ProcessWsdlWizardRequest(bool roundtrip)
        {
            VisualStudioSelectedItem selectedItem = this.VisualStudio.SelectedItem;
            if (selectedItem == null)
            {
                MessageBox.Show("No selected item.",
                        "Web Services Contract-First WSDL Wizard", MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);
                return true;
            }

            if (!selectedItem.HasProject)
            {
                MessageBox.Show("Cannot create a WSDL contract for items outside of a project.",
                    "Web Services Contract-First WSDL Wizard", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);

                return true;
            }

            string currentDir = selectedItem.Directory;
            string projectRootDir = selectedItem.ParentProject.ProjectDirectory;
            string metdataFile = selectedItem.FileName;

            WsdlWizardForm wizard = null;

            try
            {
                wizard = roundtrip ? new WsdlWizardForm(metdataFile, true) : new WsdlWizardForm(metdataFile);
                wizard.WsdlLocation = currentDir;
                wizard.DefaultPathForImports = "";
                wizard.ProjectRootDirectory = projectRootDir;
                wizard.ShowDialog();

                if (wizard.DialogResult == DialogResult.OK)
                {
                    if (wizard.WsdlLocation.Length > 0)
                    {
                        var wsdlFile = wizard.WsdlLocation;

                        if (!roundtrip)
                        {
                            AddFileToCurrentProject(wsdlFile);
                        }

                        if (wizard.OpenCodeGenDialog)
                        {
                            ProcessCodeGenerationRequest(wsdlFile);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                    "WSDL Wizard", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                wizard?.Close();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Handles the code generation workflow.
        /// </summary>
        private bool ProcessCodeGenerationRequestEx(string wsdlFromWizard)
        {
            return false;
        } // End of ProcessCodeGenerationRequestFunction.

        // This method is not current being used.
        private bool ProcessXsdCodeGenerationRequest()
        {
            if (!CanGenerateCode())
            {
                return false;
            }

            try
            {
                VisualStudioProject project = VisualStudio.SelectedProject;
                IEnumerable<VisualStudioSelectedItem> selectedItems = VisualStudio.SelectedItems;

                if (selectedItems.Count() == 0)
                {
                    MessageBox.Show(
                        "Cannot generate code for items outside of a project.",
                        "Web Services Contract-First code generation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    return true;
                }

                foreach (VisualStudioSelectedItem selectedItem in selectedItems)
                {
                    string extension = Path.GetExtension(selectedItem.FileName).ToLower();
                    if (extension == ".xsd" || extension == ".wsdl") continue;

                    MessageBox.Show(
                        "Data Contracts can only be generated for .xsd or .wsdl files.",
                        "Web Services Contract-First code generation",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    return true;
                }

                string[] dataContractFiles = selectedItems.Select(i => i.FileName).ToArray();
                XsdCodeGenDialog dialogForm = new XsdCodeGenDialog(dataContractFiles);
                if (!project.IsWebProject)
                {
                    dialogForm.Namespace = project.AssemblyNamespace;
                }
                dialogForm.TargetFileName = project.GetDefaultDestinationFilename(dataContractFiles[0]);

                if (dialogForm.ShowDialog() == DialogResult.Cancel)
                {
                    return false;
                }

                CodeGenerationOptions options = new CodeGenerationOptions();
                options.GenerateDataContracts = true;
                options.DataContractFiles = dataContractFiles;
                options.GenerateProperties = dialogForm.PublicProperties;
                options.VirtualProperties = dialogForm.VirtualProperties;
                options.GenerateCollections = dialogForm.Collections;
                options.GenerateSeparateFiles = dialogForm.GenerateMultipleFiles;
                options.OverwriteExistingFiles = dialogForm.OverwriteFiles;
                options.AdjustCasing = dialogForm.AdjustCasing;
                options.EnableDataBinding = dialogForm.DataBinding;
                options.GenerateOrderIdentifiers = dialogForm.OrderIdentifiers;
                options.GenerateTypedLists = dialogForm.GenericLists;
                options.ClrNamespace = dialogForm.Namespace;
                options.OutputFileName = dialogForm.TargetFileName;
                options.OutputLocation = GetOutputDirectory();
                options.ProjectDirectory = project.ProjectDirectory;
                options.Language = project.ProjectLanguage;
                options.ProjectName = project.ProjectName;

                CodeGenerator codeGenerator = new CodeGenerator();
                CodeWriterOutput output = codeGenerator.GenerateCode(options);

                AddGeneratedFilesToProject(output);

                // Finally add the project references.
                AddAssemblyReferences();

                MessageBox.Show("Code generation successfully completed.", "WSCF.Blue", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                AppLog.LogMessage(ex.ToString());
                MessageBox.Show(ex.ToString(), "CodeGeneration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // TODO: Log the exception.
                //System.Diagnostics.Debugger.Break();
            }
            return true;
        } // End of ProcessXsdCodeGenerationRequestFunction.

        /// <summary>
        /// 
        /// </summary>
        /// <param name="wsdlFile"></param>
        /// <returns></returns>
        public bool ProcessCodeGenerationRequest(string wsdlFile)
        {
            if (!File.Exists(wsdlFile))
            {
                return false;
            }
            if (!CanGenerateCode())
            {
                return false;
            }
            return ProcessCodeGenerationRequestCore(wsdlFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private bool ProcessCodeGenerationRequest()
        {
            if (!CanGenerateCode())
            {
                return false;
            }

            VisualStudioSelectedItem selectedItem = this.VisualStudio.SelectedItem;

            if (selectedItem.IsProject)
            {
                return ProcessCodeGenerationRequestCore(string.Empty);
            }
            else
            {
                return ProcessCodeGenerationRequestCore(selectedItem.FileName);
            }
        }

        private bool ProcessCodeGenerationRequestCore(string wsdlFile)
        {
            try
            {
                VisualStudioProject project = this.VisualStudio.SelectedProject;
                VisualStudioSelectedItem selectedItem = this.VisualStudio.SelectedItem;

                // Fist display the UI and get the options.
                WebServiceCodeGenDialogNew dialog = new WebServiceCodeGenDialogNew();
                if (!project.IsWebProject)
                {
                    dialog.DestinationNamespace = project.AssemblyNamespace;
                }
                dialog.DestinationFilename = project.GetDefaultDestinationFilename(wsdlFile);

                if (!selectedItem.IsProject)
                {
                    //dialog.WsdlLocation = selectedItem.FileName;
                    dialog.WsdlLocation = wsdlFile;
                }
                if (dialog.ShowDialog() == DialogResult.Cancel)
                {
                    return false;
                }

                wsdlFile = dialog.WsdlPath;
                // Try the Rpc2DocumentLiteral translation first.
                // wsdlFile = TryTranslateRpc2DocumentLiteral(wsdlFile);

                CodeGenerationOptions options = new CodeGenerationOptions();
                options.MetadataLocation = wsdlFile;
                options.ClrNamespace = dialog.DestinationNamespace;
                options.OutputFileName = dialog.DestinationFilename;
                options.OutputLocation = GetOutputDirectory();
                options.ProjectDirectory = project.ProjectDirectory;
                options.Language = project.ProjectLanguage;
                options.ProjectName = project.ProjectName;
                // TODO: Infer the config file type according to the project type
                // and merge the generated config file with the existing one.
                options.ConfigurationFile = "output.config";
                options.GenerateService = dialog.ServiceCode;
                options.GenerateProperties = dialog.GenerateProperties;
                options.VirtualProperties = dialog.VirtualProperties;
                options.FormatSoapActions = dialog.FormatSoapActions;
                options.GenerateCollections = dialog.Collections;
                options.GenerateTypedLists = dialog.GenericList;
                options.EnableDataBinding = dialog.EnableDataBinding;
                options.GenerateOrderIdentifiers = dialog.OrderIdentifiers;
                options.GenerateAsyncCode = dialog.AsyncMethods;
                options.GenerateSeparateFiles = dialog.GenerateMultipleFiles;
                options.AdjustCasing = dialog.ChangeCasing;
                options.OverwriteExistingFiles = dialog.Overwrite;
                options.EnableWsdlEndpoint = dialog.EnabledWsdlEndpoint;
                options.GenerateSvcFile = dialog.GenerateSvcFile;
                options.ConcurrencyMode = dialog.ConcurrencyMode;
                options.InstanceContextMode = dialog.InstanceContextMode;
                options.UseSynchronizationContext = dialog.UseSynchronizationContext;
                options.MethodImplementation = dialog.MethodImplementation;

                OutputWindowWriter.Clear();

                CodeGenerator codeGenerator = new CodeGenerator();
                CodeWriterOutput output = codeGenerator.GenerateCode(options);

                AddGeneratedFilesToProject(output);

                // Finally add the project references.
                AddAssemblyReferences();

                // add custom assembly references if necessary
                if (options.EnableWsdlEndpoint)
                {
                    AddMetadataExtensionsReference();
                }

                MessageBox.Show("Code generation successfully completed.", "WSCF.Blue", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (ClientServiceGenerationException ex)
            {
                AppLog.LogMessage(ex.ToString());

                const string separator = "---------------------------------------------------------------------------------";
                foreach (string message in ex.Messages)
                {
                    OutputWindowWriter.WriteMessage(message + "\r\n" + separator + "\r\n");
                }

                MessageBox.Show("Errors were found while importing the contract. Please check the 'WSCF.blue' pane in the Output window for more information.",
                    "CodeGeneration", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                AppLog.LogMessage(ex.ToString());
                MessageBox.Show(ex.ToString(), "CodeGeneration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // TODO: Log the exception.
                //System.Diagnostics.Debugger.Break();
            }
            return true;
        }

        private bool ProcessPasteSchemaRequest()
        {
            try
            {
                if (VisualStudio.SelectedProject == null)
                {
                    MessageBox.Show(
                        "You cannot paste schema items outside of a project.",
                        "Web Services Contract-First Paste Schema",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    return true;
                }

                IDataObject dataObject = Clipboard.GetDataObject();
                if (dataObject == null)
                {
                    MessageBox.Show(
                        "There was no data found in the clipboard.",
                        "Web Services Contract-First Paste Schema",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    return true;
                }

                string xml = (string)dataObject.GetData(typeof(string));
                if (xml == null)
                {
                    MessageBox.Show(
                        "There was no string data found in the clipboard.",
                        "Web Services Contract-First Paste Schema",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    return true;
                }

                XElement schemaXml;
                try
                {
                    schemaXml = XElement.Parse(xml);
                }
                catch (Exception)
                {
                    MessageBox.Show(
                        "The data found in the clipboard is not valid XML.",
                        "Web Services Contract-First Paste Schema",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Exclamation);

                    return true;
                }

                XmlSchemaSet schemas = new XmlSchemaSet();
                XmlSchemaInference inference = new XmlSchemaInference();

                using (XmlReader reader = schemaXml.CreateReader())
                {
                    inference.InferSchema(reader, schemas);
                }

                XmlWriterSettings settings = new XmlWriterSettings { Indent = true };

                foreach (XmlSchema schema in schemas.Schemas())
                {
                    if (schema.Items.Count == 0) continue;

                    string schemaName = ((XmlSchemaElement)(schema.Items[0])).Name;
                    string fileName = Path.Combine(VisualStudio.SelectedItem.Directory, Path.ChangeExtension(schemaName, "xsd"));

                    if (File.Exists(fileName))
                    {
                        DialogResult dialogResult = MessageBox.Show(
                            "A file named '" + Path.GetFileName(fileName) + "' already exist in the project. Do you want to overwrite this file?",
                            "Web Services Contract-First Paste Schema",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question);

                        if (dialogResult == DialogResult.No) continue;
                    }

                    using (XmlWriter writer = XmlWriter.Create(fileName, settings))
                    {
                        if (writer != null)
                        {
                            schema.Write(writer);
                        }
                    }
                    if (File.Exists(fileName))
                    {
                        AddFileToCurrentProject(fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                AppLog.LogMessage(ex.ToString());
                MessageBox.Show(ex.ToString(), "Web Services Contract-First Paste Schema", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return true;
        }

        private void AddGeneratedFilesToProject(CodeWriterOutput output)
        {
            foreach (string file in output.CodeFileNames)
            {
                AddFileToCurrentProject(file);
            }
            if (!string.IsNullOrEmpty(output.ConfigurationFile) && File.Exists(output.ConfigurationFile))
            {
                AddFileToCurrentProject(output.ConfigurationFile);
            }
        }

        private void AddFileToCurrentProject(string file)
        {
            VisualStudioProject project = this.VisualStudio.SelectedProject;
            project.AddFile(file);
        }

        private bool CanGenerateCode()
        {
            // Do we have a project?
            if (VisualStudio.SelectedProject == null)
            {
                DisplayMessage("Cannot generate code for items outside of a project.");
                return false;
            }
            return true;
        }

        private string GetOutputDirectory()
        {
            VisualStudioSelectedItem selectedItem = this.VisualStudio.SelectedItem;
            VisualStudioProject project = selectedItem.ParentProject;
            string outputDirectory;

            if (project.IsWebProject)
            {
                string serviceName = Path.GetFileNameWithoutExtension(selectedItem.FileName);
                outputDirectory = project.AddCodeFolderToWebProject(serviceName);
            }
            else
            {
                outputDirectory = selectedItem.Directory;
            }

            return outputDirectory;
        }

        private static void DisplayMessage(string message)
        {
            MessageBox.Show(message, "Web Services Contract-First code generation", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        #region Private helper methods

        private void AddAssemblyReferences()
        {
            VisualStudioProject project = VisualStudio.SelectedProject;
            project.AddReference("System");
            project.AddReference("System.Xml");
            project.AddReference("System.Runtime.Serialization");
            project.AddReference("System.ServiceModel");
            project.AddReference("System.Configuration");
        }

        /// <summary>
        /// Add more assembly references beyond the basic Fx assemblies
        /// </summary>
        private void AddMetadataExtensionsReference()
        {
            string addinDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string extensionPath = Path.Combine(addinDirectory, "Thinktecture.ServiceModel.Extensions.Metadata.dll");

            VisualStudioProject project = this.VisualStudio.SelectedProject;
            project.AddReference(extensionPath);
        }

        #endregion
    }
}