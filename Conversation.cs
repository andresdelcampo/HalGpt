using System;
using System.Linq;
using System.Threading.Tasks;

namespace HalGpt
{
    public class Conversation
    {
        private readonly Gpt _brain;

        public string FullConversation { get; private set; }
        
        public Conversation(string apiKey, string systemMessage)
        {
            _brain = new Gpt(apiKey, systemMessage);
        }
        
        public async Task<string> ReplyTo(string input, bool logInput = true)
        {
            if (logInput) FullConversation += "<USER>" + input + "</USER>" + Environment.NewLine + Environment.NewLine;
            var response = await _brain.ReplyTo(input);
            FullConversation += "<AI>" + response + "</AI>" + Environment.NewLine + Environment.NewLine;
            return response;
        }
    }
}