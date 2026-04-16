using EstoqueService.Application.DTOs.Requests;
using FluentValidation;

namespace EstoqueService.Application.Validators;

public class DebitStockRequestValidator : AbstractValidator<DebitStockRequestDto>
{
    public DebitStockRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new StockItemRequestValidator());
    }
}
