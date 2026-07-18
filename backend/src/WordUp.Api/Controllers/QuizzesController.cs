using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.Assessments;
using WordUp.Domain;

namespace WordUp.Api.Controllers;

[ApiController]
[Route("api/quizzes")]
public class QuizzesController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuizzesController(IMediator mediator) => _mediator = mediator;

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost]
    public async Task<ActionResult<QuizResult>> Create([FromBody] CreateQuizRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new CreateQuizCommand(
                request.ModuleId, request.LessonId, request.PassMark, request.AttemptsAllowed,
                request.FeedbackMode, request.Shuffle, request.RequiredToCompleteModule),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<QuizDetail>> Get(Guid id, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetQuizQuery(id), cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost("{id:guid}/questions")]
    public async Task<ActionResult<QuestionResult>> AddQuestion(Guid id, [FromBody] AddQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new AddQuizQuestionCommand(id, request.Text, request.Type, request.Options, request.CorrectIndexes),
            cancellationToken);
        return Ok(result);
    }
}

public record CreateQuizRequest(
    Guid? ModuleId,
    Guid? LessonId,
    int PassMark,
    int AttemptsAllowed,
    string FeedbackMode,
    bool Shuffle,
    bool RequiredToCompleteModule);

public record AddQuestionRequest(
    string Text,
    QuestionType Type,
    IReadOnlyList<string> Options,
    IReadOnlyList<int> CorrectIndexes);
