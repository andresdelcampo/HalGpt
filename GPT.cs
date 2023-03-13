using System.Threading.Tasks;
using OpenAI_API;
using OpenAI_API.Chat;

namespace HalGpt
{
    public class Gpt
    {
        private readonly OpenAIAPI _api;
        private readonly Conversation _chat;

        public Gpt(string apiKey, string systemMessage)
        {
            _api = new OpenAIAPI(apiKey);
            _chat = _api.Chat.CreateConversation();
            _chat.AppendSystemMessage(systemMessage);
        }
        
        public string ReplyTo(string input)
        {
            _chat.AppendUserInput(input);
            string response = Task.Run(async () => await _chat.GetResponseFromChatbot()).Result;
            return response;
        }

    }
}