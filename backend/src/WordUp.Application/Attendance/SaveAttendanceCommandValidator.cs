using FluentValidation;

namespace WordUp.Application.Attendance;

public class SaveAttendanceCommandValidator : AbstractValidator<SaveAttendanceCommand>
{
    private static readonly string[] AllowedStatuses = { "Present", "Late", "Excused", "Absent" };

    public SaveAttendanceCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Marks).NotNull();
        RuleForEach(x => x.Marks).ChildRules(mark =>
        {
            mark.RuleFor(m => m.YouthId).NotEmpty();
            mark.RuleFor(m => m.Status)
                .NotEmpty()
                .Must(s => AllowedStatuses.Any(a => string.Equals(a, s, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Status must be one of: Present, Late, Excused, Absent.");
        });
    }
}
