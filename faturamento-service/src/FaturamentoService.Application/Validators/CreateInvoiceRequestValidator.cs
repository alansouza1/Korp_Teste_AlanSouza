using FaturamentoService.Application.DTOs.Requests;
using FluentValidation;

namespace FaturamentoService.Application.Validators;

public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequestDto>
{
    public CreateInvoiceRequestValidator()
    {
    }
}
