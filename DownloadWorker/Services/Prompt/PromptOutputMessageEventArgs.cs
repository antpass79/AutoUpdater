using System;

namespace DownloadWorker.Services.Prompt
{
    public class PromptOutputMessageEventArgs : EventArgs
    {
        public PromptOutputMessageEventArgs(string outputMessage)
        {
            OutputMessage = outputMessage;
        }

        public string OutputMessage { get; }
    }
}