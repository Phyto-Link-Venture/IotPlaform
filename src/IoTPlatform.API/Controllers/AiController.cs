using IoTPlatform.API.Contracts;
using IoTPlatform.Common.Results;
using IoTPlatform.Services.Ai;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoTPlatform.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class AiController(IAiAssistantService assistant) : ControllerBase
{
    /// <summary>Sends a message to the tenant's AI assistant and returns the reply.</summary>
    [HttpPost("chat")]
    public async Task<ActionResult<ChatResponseDto>> Chat(ChatRequest request, CancellationToken cancellationToken)
    {
        var result = await assistant.ChatAsync(request.ConversationId, request.Message, cancellationToken);
        if (result.IsFailure)
            return result.Error.Type switch
            {
                ErrorType.Unauthorized => Unauthorized(result.Error.Message),
                ErrorType.Forbidden => Forbid(),
                _ => BadRequest(result.Error.Message),
            };

        var reply = result.Value;
        return new ChatResponseDto(
            reply.ConversationId, reply.Content, reply.PromptTokens, reply.CompletionTokens, reply.Model);
    }
}
