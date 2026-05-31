namespace Carevionix.Models;

public class ChatMessage
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public ApplicationUser Sender { get; set; } = null!;
    public string ReceiverId { get; set; } = string.Empty;
    public ApplicationUser Receiver { get; set; } = null!;
    public string Message { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; }
}
