using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace DownloadWorker.Services.Prompt
{
    /// <summary>
    /// Runs the prompt and executes the specified commands.
    /// </summary>
    public sealed class Prompt : IPrompt
    {
        #region Events and Delegates

        /// <summary>
        /// Fires after Run process.
        /// </summary>
        public event EventHandler<PromptOutputMessageEventArgs> WrittenOutput;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public Prompt()
        {
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Runs command line.
        /// </summary>
        /// <param name="commandLine">Command line.</param>
        /// <returns>Output from prompt.</returns>
        public IEnumerable<string> Run(string commandLine)
        {
            string[] commandLines = new string[] { commandLine };
            return Run(string.Empty, commandLines);
        }

        /// <summary>
        /// Runs command lines.
        /// </summary>
        /// <param name="commandLines">Command line collection.</param>
        /// <returns>Output from prompt.</returns>
        public IEnumerable<string> Run(string[] commandLines)
        {
            return Run(string.Empty, commandLines);
        }

        /// <summary>
        /// Runs command line with specified starting arguments.
        /// </summary>
        /// <param name="startingArguments">Starting arguments.</param>
        /// <param name="commandLine">Command line.</param>
        /// <returns>Output from prompt.</returns>
        public IEnumerable<string> Run(string startingArguments, string commandLine)
        {
            string[] commandLines = new string[] { commandLine };
            return Run(startingArguments, commandLines);
        }

        /// <summary>
        /// Runs command line with specified starting arguments.
        /// </summary>
        /// <param name="startingArguments">Starting arguments.</param>
        /// <param name="commandLines">Command line collection.</param>
        /// <returns>Output from prompt.</returns>
        public IEnumerable<string> Run(string startingArguments, string[] commandLines)
        {
            var outputMessages = new List<string>();

            try
            {
                Process process = new Process();
                process.StartInfo = GetStartInfo(startingArguments);

                process.Start();

                foreach (string commandLine in commandLines)
                {
                    process.StandardInput.WriteLine(commandLine);
                    process.StandardInput.Flush();
                    process.StandardInput.Close();

                    while (!process.StandardOutput.EndOfStream)
                    {
                        var outputMessage = process.StandardOutput.ReadLine();
 
                        outputMessages.Add(outputMessage);
                        WrittenOutput?.Invoke(this, new PromptOutputMessageEventArgs(outputMessage));
                    }
                }

                process.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return outputMessages;
        }

        #endregion

        #region Private Functions

        /// <summary>
        /// Creates ProcessStartInfo based on starting arguments.
        /// </summary>
        /// <param name="startingArguments">Starting arguments.</param>
        /// <returns>ProcessStartInfo.</returns>
        ProcessStartInfo GetStartInfo(string startingArguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();

            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;

            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardOutput = true;

            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = startingArguments;

            return startInfo;
        }

        #endregion
    }
}
