using System.Threading.Tasks;
using System.Windows.Controls;

namespace HalGpt;

public class ConversationOutput
{
    private const int TextDelay = 50;
    private int _currentTextDelay = 50;

    //**********************************************************************************************
    // ReplySlowly
    //**********************************************************************************************
    public async Task ReplySlowly(string answer, TextBox textBox, ScrollViewer scrollViewer)
    {
        // Answering one character at a time
        _currentTextDelay = TextDelay;
        for (int i = 0; i < answer.Length; i++)
        {
            // Check if we have scrolled all the way to the bottom -to keep scrolling while answering
            bool scrollToBottom = (scrollViewer.VerticalOffset + scrollViewer.ViewportHeight) >=
                                  scrollViewer.ExtentHeight;
            textBox.Text += answer.Substring(i, 1);
            if (scrollToBottom) scrollViewer.ScrollToBottom();
            await Task.Delay(_currentTextDelay);
        }
    }
    
    //**********************************************************************************************
    // SpeedUpPendingConversation
    //**********************************************************************************************
    public void SpeedUpPendingConversation()
    {
        // Make sure any previous answer goes instantly to prevent parallel answers 
        _currentTextDelay = 0;
    }
}