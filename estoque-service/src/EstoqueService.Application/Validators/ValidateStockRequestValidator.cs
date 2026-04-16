using EstoqueService.Application.DTOs.Requests;
using FluentValidation;

namespace EstoqueService.Application.Validators;

public class ValidateStockRequestValidator : AbstractValidator<ValidateStockRequestDto>
{
    public ValidateStockRequestValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty();

        RuleForEach(x => x.Items)
            .SetValidator(new StockItemRequestValidator());
    }
}
