namespace FaturamentoService.Application.DTOs.Responses;

public class PrintInvoiceResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public InvoiceResponseDto Invoice { get; set; } = new();
}
