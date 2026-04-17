using EstoqueService.Application.DTOs.Requests;
using FluentValidation;

namespace EstoqueService.Application.Validators;

public class SuggestProductDescriptionRequestValidator : AbstractValidator<SuggestProductDescriptionRequestDto>
{
    public SuggestProductDescriptionRequestValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.PartialDescription)
            .MaximumLength(255);
    }
}
