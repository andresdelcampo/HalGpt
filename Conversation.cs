using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace HalGpt
{
    public class Conversation
    {
        private readonly Gpt _brain;

        private string Title { get; set; }
        private string FullConversation { get; set; }

        public Conversation(string apiKey, string systemMessage)
        {
            _brain = new Gpt(apiKey, systemMessage);
        }
        
        public async Task<string> ReplyTo(string input, bool logInput = true)
        {
            if (logInput) FullConversation += "<USER>" + input + "</USER>" + Environment.NewLine;
            var response = await _brain.ReplyTo(input);
            FullConversation += "<AI>" + response + "</AI>" + Environment.NewLine;
            if (Title == null && logInput) SetTitle();
            return response;
        }
        
        public async void SetTitle()
        {
            var response = await _brain.IsolatedCompletion("Summarize the following conversation into a short filename of max 64 chars and without extension or dot: " + FullConversation);
            Title = response;
        }
        
        private int ConversationFontSize(bool forExport)
        {
            return (SystemParameters.PrimaryScreenWidth > 1920 && !forExport) ? 24 : 14;
        }

        public string AsHtmlContent(bool export=false)
        {
            string htmlString = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                		body { font-family: Segoe UI; font-size: " + ConversationFontSize(export) + @"px; color:white; background-color: #343541; }
		                .user { background-color: #343541; padding-left: 20px; }
		                .ai { background-color: #444654; padding-left: 20px; }
                    </style>
                </head>
                <body>
                    " + FormatHtmlConversation() + @"
                </body>
                </html>";

            return htmlString;
        }

        private string FormatHtmlConversation()
        {
            return FullConversation?.Replace(Environment.NewLine, "<br>")
                .Replace("\n", "<br>")
                .Replace("<USER>", "<div class=\"user\"><br><b>&gt; ")
                .Replace("</USER>", "</b><br><br></div>")
                .Replace("<AI>", "<div class=\"ai\"><br>")
                .Replace("</AI>", "<br><br></div>");
        }

        public void SaveAsHtml(string path)
        {
            if (Title == null) return;      // We did not even have a conversation yet
            if (path == null) path = ".";
            if (!path.EndsWith("\\")) path += "\\";
            
            File.WriteAllText(path + Title + ".html", AsHtmlContent(true));
        }
    }
}