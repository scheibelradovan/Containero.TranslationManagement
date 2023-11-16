using System.Text.Json.Serialization;

namespace TranslationManagement.Common.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TranslationJobStatus
    {
        New = 1,
        InProggress,
        Completed
    }
}
