using System;
using System.Windows;

namespace HalGpt
{
    public partial class ConversationHistory : Window
    {
        public ConversationHistory(string conversation)
        {
            InitializeComponent();
            LoadHtmlContent(conversation);
        }

        private void LoadHtmlContent(string conversation)
        {
            string htmlString = @"
                <!DOCTYPE html>
                <html>
                <head>
                    <meta charset=""UTF-8"">
                    <style>
                		body { font-family: Segoe UI; font-size: 14px; color:white; background-color: #202020}
                    </style>
                </head>
                <body>
                    " + FormatHtmlConversation(conversation) + @"
                </body>
                </html>";

            HtmlContentWebBrowser.NavigateToString(htmlString);
        }

        private static string FormatHtmlConversation(string conversation)
        {
            return conversation?.Replace(Environment.NewLine, "<br>")
                .Replace("\n", "<br>")
                .Replace("<USER>", "<b>")
                .Replace("</USER>", "</b>")
                .Replace("<AI>", "")
                .Replace("</AI>", "");
        }
    }
}