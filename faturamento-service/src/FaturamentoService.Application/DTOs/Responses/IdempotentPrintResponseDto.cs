namespace FaturamentoService.Application.DTOs.Responses;

public class IdempotentPrintResponseDto
{
    public int StatusCode { get; set; }
    public string ResponseJson { get; set; } = string.Empty;
}
