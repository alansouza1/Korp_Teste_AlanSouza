using FaturamentoService.Domain.Exceptions;

namespace FaturamentoService.Domain.Entities;

public class InvoicePrintIdempotencyRecord
{
    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public int ResponseStatusCode { get; private set; }
    public string ResponseJson { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private InvoicePrintIdempotencyRecord()
    {
    }

    public static InvoicePrintIdempotencyRecord Create(Guid invoiceId, string idempotencyKey)
    {
        if (invoiceId == Guid.Empty)
        {
            throw new DomainException("Invoice id is required.");
        }

        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new DomainException("Idempotency key is required.");
        }

        return new InvoicePrintIdempotencyRecord
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            IdempotencyKey = idempotencyKey.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public bool IsCompleted => ResponseStatusCode > 0 && !string.IsNullOrWhiteSpace(ResponseJson);

    public void Complete(int statusCode, string responseJson)
    {
        if (statusCode <= 0)
        {
            throw new DomainException("Response status code is required.");
        }

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            throw new DomainException("Response payload is required.");
        }

        ResponseStatusCode = statusCode;
        ResponseJson = responseJson;
        UpdatedAt = DateTime.UtcNow;
    }
}
