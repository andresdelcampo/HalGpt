using System.Configuration;
using System.Speech.Synthesis;

namespace HalGpt;

public class Speech
{
    private SpeechSynthesizer _synth;
    private ElevenLabsApi _elevenLabsApi;

    public void InitSpeech()
    {
        var appSettings = ConfigurationManager.AppSettings;
        string apiKeyElevenLabs = appSettings["ApiKeyElevenLabs"];

        if (string.IsNullOrEmpty(apiKeyElevenLabs))
        {
            _synth = new SpeechSynthesizer();
            _synth.Rate = 2;
        }
        else                       
            _elevenLabsApi = new ElevenLabsApi(apiKeyElevenLabs);
    }

    public void Speak(string textToSpeak)
    {
        if (string.IsNullOrEmpty(textToSpeak)) return;
        
        _ = _elevenLabsApi?.Speak(textToSpeak);
        _synth?.SpeakAsync(textToSpeak);
    }

    public void Dispose()
    {
        _elevenLabsApi = null;
        _synth = null;
    }

}