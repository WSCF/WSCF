using System.Collections.Generic;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace Thinktecture.Tools.Web.Services.CodeGeneration
{
    /// <summary>
    /// This class implements the methods that are required to produce the final code
    /// out of the CodeDom object graph and write the generated code in to the desired 
    /// location.
    /// </summary>
    internal class CodeWriter
    {
        #region Private Fields

        // Reference to the CodeNamespace instance that we generating code from.
        private CodeNamespace codeNamespace;
        // Reference to the configuration object to be written to the disk.
        private Configuration configuration;
        // Reference to the CodeWriterOptions instance containing the code 
        // writer options.
        private CodeWriterOptions options;
        // Reference to the CodeProvider instance that we use to generate code.
        private CodeDomProvider provider;
        // Reference to the array of strings holding the generated file names.
        private string[] generatedCodeFileNames;
        // Reference to the generated configuration file path.
        private string configurationFile;
        // Reference to the CodeGeneratorOptions instance we use for writing the 
        // code to files.
        private CodeGeneratorOptions codeGenerationOptions;
        private List<TextFile> textFiles;
        private int codeFilesCount;

        #endregion

        #region Constructors

        /// <summary>
        /// Private constructor.
        /// </summary>
        CodeWriter(CodeNamespace codeNamespace, Configuration configuration, CodeWriterOptions options, List<TextFile> textFiles, CodeDomProvider provider)
        {
            // Initialize the state with supplied parameters.
            this.codeNamespace = codeNamespace;
            this.configuration = configuration;
            this.options = options;
            this.provider = provider;
            this.textFiles = textFiles;
        }

        #endregion

        /// <summary>
        /// Writes the code to the disk according to the given options.
        /// </summary>
        private void WriteCodeFiles()
        {
            // Ensure the output directory exist in the file system.
            EnsureDirectoryExists(options.OutputLocation);

            // Create the CodeGenerationOptions instance.
            codeGenerationOptions = CodeWriter.CreateCodeGeneratorOptions();

            // Do we have to generate separate files?
            if (options.GenerateSeparateFiles)
            {
                // Write the code into separate files.
                WriteSeparateCodeFiles();
            }
            else
            {
                // Write the code into a singl file.
                WriteSingleCodeFile();
            }

            // Finally write the configuration file.
			if (configuration != null)
			{
				WriteConfigurationFile();				
			}
			WriteTextFiles();	
        }

        private void WriteTextFiles()
        {
            for (int i = 0; i < textFiles.Count; i++)
            {
                TextFile textFile = textFiles[0];
            	string directory = options.GetOutputDirectoryForFileType(textFile.Filename);
				string fileName = Path.Combine(directory, textFile.Filename);
                FileStream fs = null;
                try
                {
                    fs = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
                    StreamWriter writer = new StreamWriter(fs);
                    writer.Write(textFile.Content);
                    writer.Flush();
                }
                catch (IOException e)
                {
                    throw new CodeWriterException(string.Format(
                        "An error occurred while trying write to file {0}: {1}", fileName, e.Message), e);
                }
                finally
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }
                }

                generatedCodeFileNames[codeFilesCount + i] = fileName;
            }
        }

        private void WriteConfigurationFile()
        {
            // Some assertions to make debugging easier.
            Debug.Assert(!string.IsNullOrEmpty(options.OutputLocation), "This action cannot be performed when output location is null or an empty string.");
            Debug.Assert(!string.IsNullOrEmpty(options.ConfigurationFile), "This action cannot be performed when configuration file name is null or an empty string");
            // Hold the absolute path of the configuration file.
            string fileName = null;
            try
            {
                // Extract only the file name.
                fileName = Path.GetFileName(options.ConfigurationFile);
                
				// Get the directory that the config file should be written ot.
            	string directory = options.GetOutputDirectoryForFileType(fileName);

                // Combine the file name with output location to construct the absolute path for the 
                // configuration file.
				fileName = Path.Combine(directory, options.ConfigurationFile);

                // Write the configuration to the disk.
                configuration.SaveAs(fileName);

                // Finally make the generated file name accessiable via ConfigurationFile property.
                configurationFile = fileName;
            }
            catch (IOException e)
            {
                // Catch the IO exception and wrap it around a CodeWriterException with a little bit more 
                // information.
                throw new CodeWriterException(string.Format(
                    "An error occurred while trying write to file {0}: {1}", fileName, e.Message), e);
            }
        }

        /// <summary>
        /// This method writes the generated code into a single file.
        /// </summary>
        private void WriteSingleCodeFile()
        {
            // Some assertions to make debugging easier.
            Debug.Assert(!string.IsNullOrEmpty(options.OutputLocation), "This action cannot be performed when output location is null or an empty string.");
            Debug.Assert(!string.IsNullOrEmpty(options.OutputFileName), "This action cannot be performed when output file name is null or an empty string");

            // Get the destination file name.
            string fileName = CodeWriter.GetUniqueFileName(options.OutputLocation, options.OutputFileName, options.Language, options.OverwriteExistingFiles);
            // Create a StreamWriter for writing to the destination file.
            StreamWriter writer = new StreamWriter(fileName);
            try
            {
                // Write out the code to the destination file.
                provider.GenerateCodeFromNamespace(codeNamespace, writer, codeGenerationOptions);
                // Flush all buffers in the writer.
                writer.Flush();
                codeFilesCount = 1;
                // Initialize generatedFileNames array to hold the one and only one 
                // file we just generated.
                generatedCodeFileNames = new string[codeFilesCount + textFiles.Count];
                // Finally add the file name to the generatedFileNames array.
                generatedCodeFileNames[0] = fileName;
            }
            catch (IOException e)
            {
                // Wrap the IOException in a CodeWriterException with little bit 
                // more information.
                throw new CodeWriterException(
                    string.Format("An error occurred while trying write to file {0}: {1}", fileName, e.Message), e);
            }
            finally
            {
                // No matter what happens, dispose the stream writer and release the unmanaged 
                // resources.
                writer.Dispose();
            }
        }

        /// <summary>
        /// This method writes each type generated into a separate file. 
        /// The type name is used as the file name.
        /// </summary>
        private void WriteSeparateCodeFiles()
        {
            // Some assertions to make debugging easier.
            Debug.Assert(!string.IsNullOrEmpty(options.OutputLocation), "This action cannot be performed when output location is null or an empty string.");

            codeFilesCount = codeNamespace.Types.Count;
            // Initialize the file names array to hold enough all our type names.
            generatedCodeFileNames = new string[codeFilesCount + textFiles.Count];

            // Itterate the the CodeTypeDeclaration types in the codeNamespace and 
            // generate a file for each of them.
            for (int i = 0; i < codeNamespace.Types.Count; i++)
            {
                // Take a reference to the CodeTypeDeclaration at the current index.
                CodeTypeDeclaration ctd = codeNamespace.Types[i];
                // Create a temporary CodeNamespace.
                CodeNamespace tempns = new CodeNamespace(codeNamespace.Name);
                // Add the type to the temporary namespace.
                tempns.Types.Add(ctd);
                // Get the destination file name.
                string fileName = CodeWriter.GetUniqueFileName(options.OutputLocation, ctd.Name, options.Language, options.OverwriteExistingFiles);
                // Create a StreamWriter for writing to the destination file.
                StreamWriter writer = new StreamWriter(fileName);
                try
                {
                    // Write out the code to the destination file.
                    provider.GenerateCodeFromNamespace(tempns, writer, codeGenerationOptions);
                    // Flush all buffers in the writer.
                    writer.Flush();
                    // Finally add the file name to the generated file names array.
                    generatedCodeFileNames[i] = fileName;
                }
                catch (IOException e)
                {
                    // Wrap the IOException in a CodeWriterException with little bit 
                    // more information.
                    throw new CodeWriterException(
                        string.Format("An error occurred while trying write to file {0}: {1}", fileName, e.Message), e);
                }
                finally
                {
                    // No matter what happens, dispose the stream writer and release the unmanaged 
                    // resources.
                    writer.Dispose();
                }
            }
        }

        /// <summary>
        /// This is a helper method for acquiring a unique file name. 
        /// </summary>
        /// <returns>
        /// A string representing the absolute path of a given file. If overwrite flag is turned off and 
        /// if the evaluvated file name already exists in the file system, this function creates a new 
        /// file name by appending a numeric constant at the end of the file name.
        /// </returns>
        private static string GetUniqueFileName(string directory, string fileName, CodeLanguage language, bool overwrite)
        {
            // Get the appropriate file extension for the selected programming language.
            string ext = CodeWriter.GetExtension(language);
            // Read the file name without the extension.
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
            // Construct the absolute path of the file by concatanating directory name, file name and the 
            // extension.
            string absPath = Path.Combine(directory, string.Format("{0}.{1}", fileNameWithoutExt, ext));

            // Can't we overwrite files?
            if (!overwrite)
            {
                // We arrive here if we cannot overwrite existing files.

                // Counter used for generating the numeric constant for unique file name generation.
                int counter = 1;

                // Create a new file name until the created file name does not exist in the file system.
                while (File.Exists(absPath))
                {
                    // Create the new file name.
                    absPath = Path.Combine(directory, string.Format("{0}{1}.{2}", fileNameWithoutExt, counter.ToString(), ext));
                    // Increment the counter.
                    counter++;
                }
            }

            // Finally return the generated absolute path of the file.
            return absPath;
        }

        /// <summary>
        /// This is a helper method to ensure that a given directory
        /// exists in the file system.
        /// </summary>
        /// <remarks>
        /// If the specified directory does not exist in the file system
        /// this method creates it before returning.
        /// </remarks>
        private static void EnsureDirectoryExists(string directory)
        {
            // Some assertions to make debugging easier.
            Debug.Assert(!string.IsNullOrEmpty(directory), "directory parameter could not be null or an empty string.");

            try
            {
                // Can't we see the directory in the file system?
                if (!Directory.Exists(directory))
                {
                    // Create it.
                    Directory.CreateDirectory(directory);
                }
            }
            catch (IOException e)
            {
                throw new CodeWriterException(
                    string.Format("An error occurred while trying verify the output directory: {0}", e.Message), e);
            }
        }

        /// <summary>
        /// This is a helper method to create an instance of 
        /// CodeGeneratorOptions class with desired code generation options.
        /// </summary>        
        private static CodeGeneratorOptions CreateCodeGeneratorOptions()
        {
            // Create and instance of CodeGeneratorOptions class.
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            // Set the bracing style to "C". This will make sure that braces start on the line following 
            // the statement or declaration that they are associated with.
            options.BracingStyle = "C";
            // Finally return the CodeGeneratorOptions class instance.
            return options;
        }

        #region Public Static Methods

        /// <summary>
        /// Generates the code using the appropriate code provider and writes it to the 
        /// desired location.
        /// </summary>        
        public static CodeWriterOutput Write(CodeNamespace codeNamespace, Configuration configuration, CodeWriterOptions options, List<TextFile> textFiles, CodeDomProvider provider)
        {
            // Create a new instance of CodeWriter class with given options.
            CodeWriter writer = new CodeWriter(codeNamespace, configuration, options, textFiles, provider);
            // Execute the code writing procedure.
            writer.WriteCodeFiles();
            // Crate an instance of CodeWriterOutput class with the code writer's output.
            CodeWriterOutput output = new CodeWriterOutput(writer.generatedCodeFileNames, writer.configurationFile);
            // Finally return the CodeWriterOutput.
            return output;
        }

        #endregion

        #region Private Static Methods

        /// <summary>
        /// Helper method to get the code file extension for a given programming 
        /// language.
        /// </summary>
        private static string GetExtension(CodeLanguage language)
        {
            // Switch the language.
            switch (language)
            {
                case CodeLanguage.CSharp:   // C#
                    return "cs";
                case CodeLanguage.VisualBasic: // Visual Basic
                    return "vb";
                default:
                    // If it's anything else we simply return an empty string
                    // representing an unknown language.
                    return string.Empty;
            }
        }

        #endregion
    }
}
