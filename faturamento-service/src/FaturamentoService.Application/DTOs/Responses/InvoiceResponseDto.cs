namespace FaturamentoService.Application.DTOs.Responses;

public class InvoiceResponseDto
{
    public Guid Id { get; set; }
    public int SequentialNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int PrintAttempts { get; set; }
    public string? LastPrintError { get; set; }
    public List<InvoiceItemResponseDto> Items { get; set; } = [];
}
