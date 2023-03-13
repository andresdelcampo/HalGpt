using System;
using System.Configuration;
using System.Linq;
using System.Speech.Synthesis;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace HalGpt
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class HAL_UI : Window
    {
        private const double HorizBorderMargin = 25;
        private const double VertBorderMargin = 50;
        private string WelcomeSpeech;

        private const bool allowCloseWithEscape = false;

        private Gpt brain;
        private SpeechSynthesizer synth;
        bool firstTimeOpened = true;

        //**********************************************************************************************
        // HAL_UI
        //**********************************************************************************************
        public HAL_UI()
        {
            InitializeComponent();
            // Set window location (default or saved)
            if((double)Properties.Settings.Default.PropertyValues["WindowTop"].PropertyValue == -1)
                Top = System.Windows.SystemParameters.PrimaryScreenHeight - Height - VertBorderMargin;
            if ((double)Properties.Settings.Default.PropertyValues["WindowLeft"].PropertyValue == -1)
                Left = HorizBorderMargin;

            var appSettings = ConfigurationManager.AppSettings;
            string apiKey = appSettings["ApiKey"];
            string systemMessage = appSettings["SystemMessage"] + $" Today is {DateTime.Now.ToLongDateString()}";
            brain = new Gpt(apiKey, systemMessage);

            string welcomeRequest = appSettings["WelcomeRequest"];
            if (string.IsNullOrEmpty(welcomeRequest))
                WelcomeSpeech = "";
            else
                WelcomeSpeech = brain.ReplyTo(welcomeRequest).TrimStart('\n', '\r');
            txtChat.Text = WelcomeSpeech;
            txtSay.Text = "";
            txtSay.Focus();

            if (SpeechEnabled())
            {
                synth = new SpeechSynthesizer();
                synth.Rate = 2;
            }
        }

        //**********************************************************************************************
        // SpeechEnabled
        //**********************************************************************************************
        private static bool SpeechEnabled()
        {
            return (bool)Properties.Settings.Default.PropertyValues["SpeechEnabled"].PropertyValue == true;
        }

        //**********************************************************************************************
        // Window_Activated
        //**********************************************************************************************
        private void Window_Activated(object sender, EventArgs e)
        {
            if (SpeechEnabled() &&  firstTimeOpened)
            {
                synth.SpeakAsync(WelcomeSpeech);
                firstTimeOpened = false;
            }
        }

        //**********************************************************************************************
        // TextBox_KeyUp
        //**********************************************************************************************
        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Close the application after running the close animation
            if (allowCloseWithEscape == true && e.Key == Key.Escape)
            {
                Storyboard board = (Storyboard)TryFindResource("CloseAnim");
                board.Completed += new EventHandler(CloseAnimation_Completed);
                if (board != null)
                    board.Begin(this);
            }
            // Process the sentence introduced by the user
            else if (e.Key == Key.Enter)
            {
                // We will process meaningful sentences
                string inputText = txtSay.Text.Trim();
                if (inputText != "")
                {
                    // Thinking
                    txtChat.Text = "Just a moment...";
                    App.DoEvents();

                    // Getting an answer
                    string answer = brain.ReplyTo(inputText).TrimStart('\n', '\r');
                    
                    // Answering one character at a time
                    if (SpeechEnabled())
                        synth.SpeakAsync(answer);

                    txtChat.Text = "";
                    for (int i = 0; i < answer.Length; i++)
                    {
                        // Check if we have scrolled all the way to the bottom -to keep scrolling while answering
                        bool scrollToBottom = (scrollConversation.VerticalOffset + scrollConversation.ViewportHeight) >= scrollConversation.ExtentHeight;
                        txtChat.Text += answer.Substring(i, 1);
                        if (scrollToBottom) scrollConversation.ScrollToBottom();
                        App.DoEvents();
                        System.Threading.Thread.Sleep(50);
                    }

                    txtSay.Text = "";
                }
            }
        }

        //**********************************************************************************************
        // CloseAnimation_Completed
        //**********************************************************************************************
        void CloseAnimation_Completed(object sender, EventArgs e)
        {
            // Animation ended. Close the application
            System.Threading.Thread.Sleep(100);
            Close();
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
            synth = new SpeechSynthesizer();
        }

        //**********************************************************************************************
        // Speech_Unchecked
        //**********************************************************************************************
        private void Speech_Unchecked(object sender, RoutedEventArgs e)
        {
            synth = null;
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
