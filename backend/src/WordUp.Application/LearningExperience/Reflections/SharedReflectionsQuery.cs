using MediatR;

namespace WordUp.Application.LearningExperience.Reflections;

public sealed record SharedReflectionsQuery(Guid YouthId) : IRequest<IReadOnlyList<SharedReflectionView>>;

public sealed record SharedReflectionView(
    Guid Id,
    Guid YouthId,
    Guid LessonId,
    string LessonTitle,
    string Text,
    string Mood,
    IReadOnlyDictionary<string, string> SecondaryResponses,
    DateTime UpdatedAt);
