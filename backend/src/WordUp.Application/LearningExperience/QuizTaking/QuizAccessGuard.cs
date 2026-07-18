using Microsoft.EntityFrameworkCore;
using WordUp.Application.LearningExperience.YouthCurriculum;
using WordUp.Application.Shared.Abstractions;
using WordUp.Application.Shared.Common;

namespace WordUp.Application.LearningExperience.QuizTaking;

internal static class QuizAccessGuard
{
    public static async Task<AccessibleQuiz> EnsureAccessibleAsync(
        IAppDbContext db,
        Guid quizId,
        Guid youthId,
        CancellationToken cancellationToken)
    {
        var quiz = await db.Quizzes.FirstOrDefaultAsync(item => item.Id == quizId, cancellationToken)
            ?? throw new NotFoundException("Quiz not found.");

        if (quiz.ModuleId is { } moduleId)
        {
            var module = await db.Modules.FirstOrDefaultAsync(item => item.Id == moduleId, cancellationToken)
                ?? throw new NotFoundException("Module not found.");
            var enrolledTrackIds = await CurriculumProgressCalculator.ResolveYouthTrackIdsAsync(
                db, youthId, cancellationToken);
            if (module.Status != PublishStatus.Published || !enrolledTrackIds.Contains(module.TrackId))
            {
                throw new ForbiddenException("This quiz is locked.");
            }

            var moduleStatus = (await CurriculumProgressCalculator.BuildModuleStatusesAsync(
                    db, module.TrackId, youthId, cancellationToken))
                .FirstOrDefault(item => item.ModuleId == module.Id);
            var quizItem = moduleStatus?.Items.FirstOrDefault(item =>
                item.Type == "quiz" && item.ItemId == quiz.Id);
            if (moduleStatus is null || moduleStatus.Locked || quizItem is null || quizItem.Locked)
            {
                throw new ForbiddenException("This quiz is locked.");
            }

            return new AccessibleQuiz(quiz, $"{module.Name} quiz");
        }

        if (quiz.LessonId is { } lessonId)
        {
            var lesson = await CurriculumProgressCalculator.EnsureLessonAccessibleAsync(
                db, lessonId, youthId, cancellationToken);
            return new AccessibleQuiz(quiz, $"{lesson.Lesson.Title} quiz");
        }

        // Standalone quizzes are retained for existing explicitly assigned assessment workflows.
        return new AccessibleQuiz(quiz, "Quiz");
    }
}

internal sealed record AccessibleQuiz(Quiz Quiz, string Title);
