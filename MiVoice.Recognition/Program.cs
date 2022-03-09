using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System.Text.Json;
using Tomlyn;
using MiVoice.Recognition;

ConfigModel config;
using (var reader = new StreamReader("config.toml"))
{
    config = Toml.ToModel<ConfigModel>(await reader.ReadToEndAsync());
}

var speechConfig = SpeechConfig.FromSubscription(config.Azure.SubscriptionKey, config.Azure.ServiceRegion);
speechConfig.SpeechRecognitionLanguage = "zh-CN";
speechConfig.OutputFormat = OutputFormat.Detailed;
// speechConfig.EnableDictation();

using var audioConfig = AudioConfig.FromWavFileInput("audio.wav");
using var speechRecognizer = new SpeechRecognizer(speechConfig, audioConfig);

var phraseList = PhraseListGrammar.FromRecognizer(speechRecognizer);
phraseList.AddPhrase("弥希Miki");

bool processing = true;
int id = 0;
List<SpeechRecognitionEventArgs> recognizedEvents = new();
speechRecognizer.Recognized += (e, args) =>
{
    using var stream = new FileStream($"result-{id++}.json", FileMode.Create);
    var jso = new JsonSerializerOptions
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    System.Text.Json.JsonSerializer.SerializeAsync(stream, args, jso);
    Console.WriteLine($"RECOGNIZED: {args}");
    foreach (var ev in args.Result.Best())
    {
        Console.WriteLine($"{ev.Confidence} {ev.LexicalForm} {ev.MaskedNormalizedForm} {ev.NormalizedForm} {ev.Text} {ev.Words}");
    }
    recognizedEvents.Add(args);
};
speechRecognizer.Recognizing += (e, args) =>
{
    Console.WriteLine($"RECOGNIZING: {args}");
};
speechRecognizer.SessionStopped += (e, args) =>
{
    processing = false;
};
speechRecognizer.Canceled += (e, args) =>
{
    processing = false;
};

await speechRecognizer.StartContinuousRecognitionAsync();

while (processing)
{
    Thread.Sleep(500);
}
await speechRecognizer.StopContinuousRecognitionAsync();

int idx = 0;
foreach (var e in recognizedEvents)
{
    var ss = new TimeSpan(Convert.ToInt64(e.Offset));
    var to = new TimeSpan(Convert.ToInt64(e.Offset)) + e.Result.Duration;
    Console.WriteLine(@$"{++idx}
{ss.Hours}:{ss.Minutes}:{ss.Seconds},{ss.Milliseconds} --> {to.Hours}:{to.Minutes}:{to.Seconds},{to.Milliseconds}
{e.Result.Text}
");
}
