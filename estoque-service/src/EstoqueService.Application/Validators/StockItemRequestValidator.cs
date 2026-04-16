using EstoqueService.Application.DTOs.Requests;
using FluentValidation;

namespace EstoqueService.Application.Validators;

public class StockItemRequestValidator : AbstractValidator<StockItemRequestDto>
{
    public StockItemRequestValidator()
    {
        RuleFor(x => x.ProductCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
