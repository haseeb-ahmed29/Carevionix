using System.ComponentModel.DataAnnotations;

namespace Carevionix.DTOs;

public record SendChatMessageDto([Required] string ReceiverId, [Required, MinLength(1), MaxLength(4000)] string Message);
