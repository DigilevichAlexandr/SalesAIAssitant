using Microsoft.AspNetCore.Mvc;
using SalesAssistant.Application.Abstractions;
using SalesAssistant.Application.Models;

namespace SalesAssistant.Api.Controllers;

[ApiController]
[Route("api/webhooks/telegram")]
public class TelegramWebhookController : ControllerBase
{
    [HttpPost("{storeId:guid}")]
    public async Task<IActionResult> Handle(
        Guid storeId,
        [FromBody] TelegramUpdate update,
        [FromServices] ITelegramService telegramService,
        CancellationToken cancellationToken)
    {
        await telegramService.ProcessIncomingUpdateAsync(storeId, update, cancellationToken);
        return Ok();
    }
}
