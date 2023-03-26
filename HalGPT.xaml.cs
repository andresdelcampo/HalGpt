using System;
using System.Configuration;
using System.Windows;
using System.Windows.Input;

namespace HalGpt
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class HalUi
    {
        private const string WaitingForApi = "Just a moment...";
        private const double HorizBorderMargin = 25;
        private const double VertBorderMargin = 50;
        private string _welcomeSpeech;

        private readonly Speech _speech = new();
        private readonly Conversation _conversation;
        private readonly ConversationOutput _output;
        bool _firstTimeOpened = true;

        //**********************************************************************************************
        // HAL_UI
        //**********************************************************************************************
        public HalUi()
        {
            InitializeComponent();
            // Set window location (default or saved)
            if(Math.Abs((double)Properties.Settings.Default.PropertyValues["WindowTop"].PropertyValue - (-1)) < 0.001)
                Top = SystemParameters.PrimaryScreenHeight - Height - VertBorderMargin;
            if (Math.Abs((double)Properties.Settings.Default.PropertyValues["WindowLeft"].PropertyValue - (-1)) < 0.001)
                Left = HorizBorderMargin;

            var appSettings = ConfigurationManager.AppSettings;
            string apiKeyOpenAi = appSettings["ApiKeyOpenAI"];
            string longDateTimeString = DateTime.Now.ToString("dddd, MMMM dd, yyyy h:mm tt");
            string systemMessage = appSettings["SystemMessage"] + $" Today is {longDateTimeString}";
            _conversation = new Conversation(apiKeyOpenAi, systemMessage);
            _output = new ConversationOutput();
                
            TxtChat.Text = WaitingForApi;
            TxtSay.Text = "";
            TxtSay.Focus();

            if (SpeechEnabled())
                _speech.InitSpeech();
        }

        //**********************************************************************************************
        // Window_Activated
        //**********************************************************************************************
        private async void Window_Activated_Async(object sender, EventArgs e)
        {
            if (_firstTimeOpened)
            {
                _firstTimeOpened = false;

                var appSettings = ConfigurationManager.AppSettings;
                string welcomeRequest = appSettings["WelcomeRequest"];
                if (string.IsNullOrEmpty(welcomeRequest))
                    _welcomeSpeech = "";
                else
                    _welcomeSpeech = await _conversation.ReplyToInput(welcomeRequest, false);

                TxtChat.Text = "";
                _speech.Speak(_welcomeSpeech);
                _ = _output.ReplySlowly(_welcomeSpeech, TxtChat, ScrollConversation);
            }
        }

        //**********************************************************************************************
        // TextBox_KeyUp
        //**********************************************************************************************
        private async void TextBox_KeyUp_Async(object sender, KeyEventArgs e)
        {
            _output.SpeedUpPendingConversation();

            // Process the sentence introduced by the user
            if (e.Key == Key.Enter)
            {
                // We will process meaningful sentences
                string inputText = TxtSay.Text.Trim();
                if (inputText != "")
                {
                    // Thinking
                    TxtChat.Text = WaitingForApi;

                    var answer = await _conversation.ReplyToInput(inputText);
                    _speech.Speak(answer);

                    TxtChat.Text = "> " + TxtSay.Text + Environment.NewLine + Environment.NewLine;
                    TxtSay.Text = "";

                    await _output.ReplySlowly(answer, TxtChat, ScrollConversation);
                }
            }
        }

        //**********************************************************************************************
        // Window_Closing
        //**********************************************************************************************
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
            
            var appSettings = ConfigurationManager.AppSettings;
            string savePath = appSettings["SaveConversationsPath"];
            _conversation.SaveAsHtml(savePath);
        }

        //**********************************************************************************************
        // imageHal_MouseLeftButtonDown 
        //**********************************************************************************************
        private void imageHal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();         // Allows moving the window
        }
        
        //**********************************************************************************************
        // TxtChat_OnPreviewMouseDoubleClick
        //**********************************************************************************************
        private void TxtChat_OnPreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenConversationWindow();
            e.Handled = true;
        }

        //**********************************************************************************************
        // OpenConversationWindow
        //**********************************************************************************************
        private void OpenConversationWindow()
        {
            _output.SpeedUpPendingConversation();
            
            var conversation = new ConversationHistory(_conversation, _speech);
            conversation.ShowDialog();
            
            TxtChat.Text = "";
            TxtSay.Text = "";
            TxtSay.Focus();
        }

        #region Context menu

        //**********************************************************************************************
        // SpeechEnabled
        //**********************************************************************************************
        private static bool SpeechEnabled()
        {
            return (bool)Properties.Settings.Default.PropertyValues["SpeechEnabled"].PropertyValue;
        }
        
        //**********************************************************************************************
        // Speech_Checked
        //**********************************************************************************************
        private void Speech_Checked(object sender, RoutedEventArgs e)
        {
            _speech.InitSpeech();
        }

        //**********************************************************************************************
        // Speech_Unchecked
        //**********************************************************************************************
        private void Speech_Unchecked(object sender, RoutedEventArgs e)
        {
            _speech.Dispose();
        }
        
        //**********************************************************************************************
        // Conversation_Click
        //**********************************************************************************************
        private void Conversation_Click(object sender, RoutedEventArgs e)
        {
            OpenConversationWindow();
        }

        //**********************************************************************************************
        // CloseAnimationAndExit
        //**********************************************************************************************
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        #endregion
    }
}
