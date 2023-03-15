﻿using System.Threading.Tasks;
using OpenAI_API;

namespace HalGpt
{
    public class Gpt
    {
        private readonly OpenAIAPI _api;
        private readonly OpenAI_API.Chat.Conversation _chat;

        public Gpt(string apiKey, string systemMessage)
        {
            _api = new OpenAIAPI(apiKey);
            _chat = _api.Chat.CreateConversation();
            _chat.AppendSystemMessage(systemMessage);
        }
        
        public async Task<string> ReplyTo(string input)
        {
            _chat.AppendUserInput(input);
            var response = await _chat.GetResponseFromChatbot();
            return response;
        }
    }
}