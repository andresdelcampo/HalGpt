using System;
using System.Configuration;
using System.Speech.Synthesis;
using System.Threading.Tasks;
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
        private const int TextDelay = 50;
        private const double HorizBorderMargin = 25;
        private const double VertBorderMargin = 50;
        private string _welcomeSpeech;

        private readonly Conversation _conversation;
        private SpeechSynthesizer _synth;
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
            string apiKey = appSettings["ApiKey"];
            string longDateTimeString = DateTime.Now.ToString("dddd, MMMM dd, yyyy h:mm tt");
            string systemMessage = appSettings["SystemMessage"] + $" Today is {longDateTimeString}";
            _conversation = new Conversation(apiKey, systemMessage);
            
            TxtChat.Text = WaitingForApi;
            TxtSay.Text = "";
            TxtSay.Focus();

            if (SpeechEnabled())
            {
                _synth = new SpeechSynthesizer();
                _synth.Rate = 2;
            }
        }

        //**********************************************************************************************
        // SpeechEnabled
        //**********************************************************************************************
        private static bool SpeechEnabled()
        {
            return (bool)Properties.Settings.Default.PropertyValues["SpeechEnabled"].PropertyValue;
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
                {
                    try
                    {
                        string answer = await _conversation.ReplyTo(welcomeRequest, false);
                        _welcomeSpeech = answer.TrimStart('\n', '\r');
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error using OpenAI API key. Remember to set the API key in the config file.");
                        throw;
                    }
                }

                TxtChat.Text = _welcomeSpeech;
            
                if (SpeechEnabled() && !string.IsNullOrEmpty(_welcomeSpeech))
                    _synth.SpeakAsync(_welcomeSpeech);
            }
        }

        //**********************************************************************************************
        // TextBox_KeyUp
        //**********************************************************************************************
        private async void TextBox_KeyUp_Async(object sender, KeyEventArgs e)
        {
            // Process the sentence introduced by the user
            if (e.Key == Key.Enter)
            {
                // We will process meaningful sentences
                string inputText = TxtSay.Text.Trim();
                if (inputText != "")
                {
                    // Thinking
                    TxtChat.Text = WaitingForApi;

                    // Getting an answer
                    string answer;
                    try
                    {
                        answer = await _conversation.ReplyTo(inputText);
                        answer = answer.TrimStart('\n', '\r');
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Error using OpenAI API key. Remember to set the API key in the config file.");
                        throw;
                    }

                    if (SpeechEnabled()) _synth.SpeakAsync(answer);

                    // Answering one character at a time
                    TxtChat.Text = "> " + TxtSay.Text + Environment.NewLine + Environment.NewLine; 
                    TxtSay.Text = "";
                    for (int i = 0; i < answer.Length; i++)
                    {
                        // Check if we have scrolled all the way to the bottom -to keep scrolling while answering
                        bool scrollToBottom = (ScrollConversation.VerticalOffset + ScrollConversation.ViewportHeight) >= ScrollConversation.ExtentHeight;
                        TxtChat.Text += answer.Substring(i, 1);
                        if (scrollToBottom) ScrollConversation.ScrollToBottom();
                        await Task.Delay(TextDelay);
                    }
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

        private void OpenConversationWindow()
        {
            var conversation = new ConversationHistory(_conversation);
            conversation.ShowDialog();
        }

        #region Context menu

        //**********************************************************************************************
        // Speech_Checked
        //**********************************************************************************************
        private void Speech_Checked(object sender, RoutedEventArgs e)
        {
            _synth = new SpeechSynthesizer();
        }

        //**********************************************************************************************
        // Speech_Unchecked
        //**********************************************************************************************
        private void Speech_Unchecked(object sender, RoutedEventArgs e)
        {
            _synth = null;
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
