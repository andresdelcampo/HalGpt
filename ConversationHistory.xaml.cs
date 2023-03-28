using System.Windows;
using System.Windows.Input;

namespace HalGpt
{
    public partial class ConversationHistory : Window
    {
        private readonly ConversationOutput _output = new ();
        private readonly Conversation _conversation;
        private readonly Speech _speech;
        
        public ConversationHistory(Conversation conversation, Speech speech)
        {
            InitializeComponent();
            _conversation = conversation;
            _speech = speech;
            LoadHtmlContent(_conversation);
        }

        //**********************************************************************************************
        // LoadHtmlContent
        //**********************************************************************************************
        private void LoadHtmlContent(Conversation conversation)
        {
            string html = conversation.AsHtmlContent();
            HtmlContentWebBrowser.NavigateToString(html);
            
            // Set window title to conversation summary
            if (_conversation.Title != null)
                Title = _conversation.Title.Replace("_", " ");
        }

        //**********************************************************************************************
        // TxtSay_OnKeyUp
        //**********************************************************************************************
        private async void TxtSay_OnKeyUp(object sender, KeyEventArgs e)
        {
            _output.SpeedUpPendingConversation();

            // Process the sentence introduced by the user
            if (e.Key == Key.Enter)
            {
                // We will process meaningful sentences
                string inputText = TxtSay.Text.Trim();
                if (inputText != "")
                {
                    var answer = await _conversation.ReplyToInput(inputText);
                    _speech.Speak(answer);

                    LoadHtmlContent(_conversation);
                    TxtSay.Text = "";

                    //await _output.ReplySlowly(answer, TxtChat, ScrollConversation);
                }
            }
        }
    }
}