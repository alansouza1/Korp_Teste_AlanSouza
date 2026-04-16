using FaturamentoService.Application.DTOs.Responses;
using FaturamentoService.Domain.Entities;

namespace FaturamentoService.Application.Mapping;

public static class InvoiceMappingExtensions
{
    public static InvoiceResponseDto ToResponse(this Invoice invoice)
    {
        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            SequentialNumber = invoice.SequentialNumber,
            Status = invoice.Status.ToString().ToUpperInvariant(),
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt,
            ClosedAt = invoice.ClosedAt,
            PrintAttempts = invoice.PrintAttempts,
            LastPrintError = invoice.LastPrintError,
            Items = invoice.Items
                .OrderBy(x => x.ProductCode)
                .Select(x => x.ToResponse())
                .ToList()
        };
    }

    public static InvoiceItemResponseDto ToResponse(this InvoiceItem item)
    {
        return new InvoiceItemResponseDto
        {
            Id = item.Id,
            InvoiceId = item.InvoiceId,
            ProductCode = item.ProductCode,
            ProductDescription = item.ProductDescription,
            Quantity = item.Quantity
        };
    }
}
