using Carevionix.Models;

namespace Carevionix.Interfaces;

public interface IChatService
{
    Task<ChatMessage> SendAsync(string senderId, string receiverId, string message);
    Task<IReadOnlyList<ChatMessage>> ConversationAsync(string currentUserId, string otherUserId);
}
