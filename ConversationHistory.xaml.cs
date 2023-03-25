using System.Windows;

namespace HalGpt
{
    public partial class ConversationHistory : Window
    {
        public ConversationHistory(Conversation conversation)
        {
            InitializeComponent();
            LoadHtmlContent(conversation);
        }

        private void LoadHtmlContent(Conversation conversation)
        {
            string html = conversation.AsHtmlContent();
            HtmlContentWebBrowser.NavigateToString(html);
        }
    }
}