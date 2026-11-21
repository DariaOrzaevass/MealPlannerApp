using System;
using System.Collections.Generic;
using System.Text;
using FluentValidation;

namespace MealPlanner.Application.MealPlans.CompareSolvers;

// Validates planning constraints before the comparison of solvers. Planning constraints
// are the same as for the meal plan generation, so they were extracted to the
// separate input type, the validation of which is reused here
internal sealed class CompareSolversQueryValidator : AbstractValidator<CompareSolversQuery>
{
    public CompareSolversQueryValidator() =>
        RuleFor(q => q.Constraints)
        .NotNull().WithErrorCode("MealPlan.ConstraintsRequired")
        .WithMessage("Planning constraints are required.")
        .SetValidator(new PlanningConstraintsInputValidator()!);
}
