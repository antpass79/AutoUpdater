using System;
using System.Collections.Generic;

namespace DownloadWorker.Services.Prompt
{
    /// <summary>
    /// Runs the prompt and executes the specified commands.
    /// </summary>
    public interface IPrompt
    {
        event EventHandler<PromptOutputMessageEventArgs> WrittenOutput;

        IEnumerable<string> Run(string commandLine);
        IEnumerable<string> Run(string[] commandLines);
        IEnumerable<string> Run(string startingArguments, string commandLine);
        IEnumerable<string> Run(string startingArguments, string[] commandLines);
    }
}
