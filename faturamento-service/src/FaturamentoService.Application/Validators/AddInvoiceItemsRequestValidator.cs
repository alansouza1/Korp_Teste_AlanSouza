using FaturamentoService.Application.DTOs.Requests;
using FluentValidation;

namespace FaturamentoService.Application.Validators;

public class AddInvoiceItemsRequestValidator : AbstractValidator<AddInvoiceItemsRequestDto>
{
    public AddInvoiceItemsRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new AddInvoiceItemRequestValidator());
    }
}
