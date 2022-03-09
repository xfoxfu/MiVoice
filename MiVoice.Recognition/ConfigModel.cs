namespace MiVoice.Recognition;

public class ConfigModel
{
    public AzureModel Azure { get; set; } = default!;

    public class AzureModel
    {
        public string SubscriptionKey { get; set; } = default!;
        public string ServiceRegion { get; set; } = default!;
    }
}
