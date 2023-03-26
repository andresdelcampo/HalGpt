using System;
using System.Configuration;
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
        private const double HorizBorderMargin = 25;
        private const double VertBorderMargin = 50;
        private const int TextDelay = 50;
        private int _currentTextDelay = 50;
        private string _welcomeSpeech;

        private readonly Speech _speech = new();
        private readonly Conversation _conversation;
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

                TxtChat.Text = "";
                _speech.Speak(_welcomeSpeech);
                _ = ReplySlowly(_welcomeSpeech);
            }
        }

        //**********************************************************************************************
        // TextBox_KeyUp
        //**********************************************************************************************
        private async void TextBox_KeyUp_Async(object sender, KeyEventArgs e)
        {
            SpeedUpPendingConversation();

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

                    _speech.Speak(answer);

                    TxtChat.Text = "> " + TxtSay.Text + Environment.NewLine + Environment.NewLine;
                    TxtSay.Text = "";

                    await ReplySlowly(answer);
                }
            }
        }

        //**********************************************************************************************
        // SpeedUpPendingConversation
        //**********************************************************************************************
        private void SpeedUpPendingConversation()
        {
            // Make sure any previous answer goes instantly to prevent parallel answers 
            _currentTextDelay = 0;
        }

        //**********************************************************************************************
        // ReplySlowly
        //**********************************************************************************************
        private async Task ReplySlowly(string answer)
        {
            // Answering one character at a time
            _currentTextDelay = TextDelay;
            for (int i = 0; i < answer.Length; i++)
            {
                // Check if we have scrolled all the way to the bottom -to keep scrolling while answering
                bool scrollToBottom = (ScrollConversation.VerticalOffset + ScrollConversation.ViewportHeight) >=
                                      ScrollConversation.ExtentHeight;
                TxtChat.Text += answer.Substring(i, 1);
                if (scrollToBottom) ScrollConversation.ScrollToBottom();
                await Task.Delay(_currentTextDelay);
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
            SpeedUpPendingConversation();
            
            var conversation = new ConversationHistory(_conversation);
            conversation.ShowDialog();
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
