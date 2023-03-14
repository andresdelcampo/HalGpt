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

        private readonly Gpt _brain;
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
            string systemMessage = appSettings["SystemMessage"] + $" Today is {DateTime.Now.ToLongDateString()}";
            _brain = new Gpt(apiKey, systemMessage);

            txtChat.Text = WaitingForApi;
            txtSay.Text = "";
            txtSay.Focus();

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
            var appSettings = ConfigurationManager.AppSettings;

            string welcomeRequest = appSettings["WelcomeRequest"];
            if (string.IsNullOrEmpty(welcomeRequest))
                _welcomeSpeech = "";
            else
            {
                string answer = await _brain.ReplyTo(welcomeRequest);
                _welcomeSpeech = answer.TrimStart('\n', '\r');
            }

            txtChat.Text = _welcomeSpeech;
            
            if (SpeechEnabled() &&  _firstTimeOpened && !string.IsNullOrEmpty(_welcomeSpeech))
            {
                _synth.SpeakAsync(_welcomeSpeech);
                _firstTimeOpened = false;
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
                string inputText = txtSay.Text.Trim();
                if (inputText != "")
                {
                    // Thinking
                    txtChat.Text = WaitingForApi;

                    // Getting an answer
                    string answer = await _brain.ReplyTo(inputText);
                    answer = answer.TrimStart('\n', '\r');

                    if (SpeechEnabled()) _synth.SpeakAsync(answer);

                    // Answering one character at a time
                    txtChat.Text = ""; 
                    for (int i = 0; i < answer.Length; i++)
                    {
                        // Check if we have scrolled all the way to the bottom -to keep scrolling while answering
                        bool scrollToBottom = (scrollConversation.VerticalOffset + scrollConversation.ViewportHeight) >= scrollConversation.ExtentHeight;
                        txtChat.Text += answer.Substring(i, 1);
                        if (scrollToBottom) scrollConversation.ScrollToBottom();
                        await Task.Delay(TextDelay);
                    }

                    txtSay.Text = "";
                }
            }
        }

        //**********************************************************************************************
        // Window_Closing
        //**********************************************************************************************
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        //**********************************************************************************************
        // imageHal_MouseLeftButtonDown 
        //**********************************************************************************************
        private void imageHal_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();         // Allows moving the window
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
        // CloseAnimationAndExit
        //**********************************************************************************************
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        #endregion

    }
}
