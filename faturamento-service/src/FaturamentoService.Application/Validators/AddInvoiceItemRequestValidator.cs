using FaturamentoService.Application.DTOs.Requests;
using FluentValidation;

namespace FaturamentoService.Application.Validators;

public class AddInvoiceItemRequestValidator : AbstractValidator<AddInvoiceItemRequestDto>
{
    public AddInvoiceItemRequestValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.ProductDescription)
            .NotEmpty()
            .MaximumLength(255);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
