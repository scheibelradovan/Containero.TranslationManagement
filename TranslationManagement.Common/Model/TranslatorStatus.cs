using System.Text.Json.Serialization;

namespace TranslationManagement.Common.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TranslatorStatus
    {
        Applicant = 1, 
        Certified,
        Deleted
    }
}
