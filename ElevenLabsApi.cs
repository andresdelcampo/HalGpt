using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NAudio.Wave;

namespace HalGpt
{
    public class ElevenLabsApi
    {
        private string _apiKey;

        public ElevenLabsApi(string apiKey)
        {
            _apiKey = apiKey;
        }
        
        public async Task Speak(string textToSpeak, bool male = false)
        {
            using HttpClient httpClient = new HttpClient();
    
            // Set the API key
            httpClient.DefaultRequestHeaders.Add("xi-api-key", _apiKey);

            // Create JSON payload
            var payload = new
            {
                text = textToSpeak,
                voice_settings = new
                {
                    stability = 0.75,
                    similarity_boost = 0.75
                }
            };

            // Serialize the payload to JSON
            string jsonPayload = JsonConvert.SerializeObject(payload);

            // Prepare the request
            const string voiceJosh = "TxGEqnHWrfWFTfGW9XjX";
            const string voiceBella = "EXAVITQu4vr4xnSDxMaL";
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.elevenlabs.io/v1/text-to-speech/" + (male? voiceJosh : voiceBella));
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("audio/mpeg"));
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();
        
                // Read the response content as byte array
                byte[] responseBody = await response.Content.ReadAsByteArrayAsync();

                // Play!
                _ = PlayMp3FromMemory(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("An error occurred while making the request:");
                Console.WriteLine(e.Message);
            }
        }

        private static async Task PlayMp3FromMemory(byte[] mp3Data)
        {
            using (var mp3MemoryStream = new MemoryStream(mp3Data))
            using (var mp3FileReader = new Mp3FileReader(mp3MemoryStream))
            using (var waveOutEvent = new WaveOutEvent())
            {
                var playbackCompletion = new TaskCompletionSource<bool>();

                waveOutEvent.PlaybackStopped += (sender, e) => playbackCompletion.SetResult(true);
                waveOutEvent.Init(mp3FileReader);
                waveOutEvent.Play();

                await playbackCompletion.Task;
            }
        }

    }
}