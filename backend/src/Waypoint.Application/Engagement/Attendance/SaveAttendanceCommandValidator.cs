using FluentValidation;

namespace Waypoint.Application.Engagement.Attendance;

public class SaveAttendanceCommandValidator : AbstractValidator<SaveAttendanceCommand>
{
    private static readonly string[] AllowedStatuses = { "Present", "Late", "Excused", "Absent" };

    public SaveAttendanceCommandValidator()
    {
        RuleFor(x => x.SessionId).NotEmpty();
        RuleFor(x => x.Marks).NotEmpty();
        RuleFor(x => x.Marks)
            .Must(marks => marks.Select(mark => mark.YouthId).Distinct().Count() == marks.Count)
            .WithMessage("Each youth may appear only once in an attendance save.");
        RuleFor(x => x.IdempotencyKey).MaximumLength(100);
        RuleForEach(x => x.Marks).ChildRules(mark =>
        {
            mark.RuleFor(m => m.YouthId).NotEmpty();
            mark.RuleFor(m => m.Status)
                .NotEmpty()
                .Must(s => AllowedStatuses.Any(a => string.Equals(a, s, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Status must be one of: Present, Late, Excused, Absent.");
            mark.RuleFor(m => m.Note).MaximumLength(1000);
            mark.RuleFor(m => m.ExpectedVersion).MaximumLength(100);
        });
    }
}
