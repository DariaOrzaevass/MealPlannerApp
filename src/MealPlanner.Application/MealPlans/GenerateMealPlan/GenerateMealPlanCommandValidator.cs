using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace MealPlanner.Application.MealPlans.GenerateMealPlan;

// Validation of the input before generating a meal plan.
// Returns all detected problems to the user at once.
internal sealed class GenerateMealPlanCommandValidator : AbstractValidator<GenerateMealPlanCommand>
{
    public GenerateMealPlanCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithErrorCode("MealPlan.NameRequired")
            .WithMessage("Plan name is required.")
            .MaximumLength(200).WithErrorCode("MealPlan.NameTooLong")
            .WithMessage("Plan name cannot be longer than 200 characters.");

        RuleFor(c => c.Algorithm)
            .IsInEnum().WithErrorCode("MealPlan.UnknownAlgorithm")
            .WithMessage("An unknown algorithm was requested.");

        RuleFor(c => c.Constraints)
            .NotNull().WithErrorCode("MealPlan.ConstraintsRequired")
            .WithMessage("Planning constraints are required.")
            .SetValidator(new PlanningConstraintsInputValidator()!);
    }
}
