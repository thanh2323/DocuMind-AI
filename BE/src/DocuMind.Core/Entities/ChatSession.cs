namespace DocuMind.Core.Entities
{
    public class ChatSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? Title { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public User User { get; set; } = null!;
        public ICollection<SessionDocument> SessionDocuments { get; set; } = new List<SessionDocument>();
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}