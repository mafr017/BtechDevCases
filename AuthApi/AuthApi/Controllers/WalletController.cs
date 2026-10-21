using System.Security.Claims;
using AuthApi.Dtos;
using AuthApi.Repositories;
using AuthApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;

namespace AuthApi.Controllers;

[ApiController]
[Authorize]
[Route("api/wallet")]
public sealed class WalletController(IWalletService wallet) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var response = await GetWalletResponseAsync(userId, ct);
        return response is null ? Unauthorized() : Ok(response);
    }

    [HttpPost("transfers")]
    public async Task<IActionResult> Transfer(
        TransferRequest request, [FromHeader(Name = "Idempotency-Key")] Guid? idempotencyKey, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();
        if (idempotencyKey is null)
            return Problem(statusCode: StatusCodes.Status400BadRequest, title: "The Idempotency-Key header (a UUID) is required.");

        var result = await wallet.TransferAsync(userId, request.Recipient, request.Amount, request.Notes, idempotencyKey.Value, ct);
        var error = result.Status switch
        {
            TransferStatus.RecipientNotFound => "Recipient not found.",
            TransferStatus.SelfTransfer => "You cannot transfer to yourself.",
            TransferStatus.InsufficientFunds => "Insufficient balance.",
            _ => null,
        };
        if (error is not null)
            return Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: error);

        var response = await GetWalletResponseAsync(userId, ct);
        return result.Status == TransferStatus.Completed
            ? StatusCode(StatusCodes.Status201Created, response)
            : Ok(response);
    }

    private async Task<WalletResponse?> GetWalletResponseAsync(Guid userId, CancellationToken ct)
    {
        var summary = await wallet.GetWalletAsync(userId, ct);
        if (summary is null)
            return null;

        return new WalletResponse
        {
            Balance = summary.Balance,
            Transfers = summary.Transfers.Select(t => new TransferItem
            {
                Id = t.Id,
                Direction = t.SenderId == userId ? "sent" : "received",
                Counterparty = t.SenderId == userId ? t.Recipient.Email : t.Sender.Email,
                Amount = t.Amount,
                Notes = t.Notes,
                CreatedAt = t.CreatedAt,
            }).ToList(),
        };
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(JwtRegisteredClaimNames.Sub), out userId);
}
