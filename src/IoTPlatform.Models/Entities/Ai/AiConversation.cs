using IoTPlatform.Models.Common;

namespace IoTPlatform.Models.Entities.Ai;

public class AiConversation : TenantEntity
{
    public Guid UserId { get; set; }
    public string? Title { get; set; }

    public ICollection<AiMessage> Messages { get; set; } = new List<AiMessage>();
}
