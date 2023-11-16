namespace TranslationManagement.Common.Model
{
    public class TranslationJob
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public TranslationJobStatus Status { get; set; }
        public string OriginalContent { get; set; }
        public string TranslatedContent { get; set; }
        public int TranslatorId { get; set; }
        public double Price { get; set; }
    }
}
