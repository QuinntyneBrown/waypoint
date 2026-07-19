using WordUp.Api.Contracts.CurriculumAuthoring;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Api.Authorization;
using WordUp.Application.CurriculumAuthoring.Assessments;

namespace WordUp.Api.Controllers.CurriculumAuthoring;

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
            new AddQuizQuestionCommand(
                id,
                request.Text,
                request.Type,
                request.Options,
                request.CorrectIndexes,
                request.CorrectAnswers ?? Array.Empty<string>()),
            cancellationToken);
        return Ok(result);
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] CreateQuizRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new UpdateQuizCommand(
                id,
                request.ModuleId,
                request.LessonId,
                request.PassMark,
                request.AttemptsAllowed,
                request.FeedbackMode,
                request.Shuffle,
                request.RequiredToCompleteModule),
            cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = Policies.EditCurriculum)]
    [HttpPost("{id:guid}/questions/reorder")]
    public async Task<IActionResult> ReorderQuestions(
        Guid id,
        [FromBody] ReorderQuestionsRequest request,
        CancellationToken cancellationToken)
    {
        await _mediator.Send(
            new ReorderQuizQuestionsCommand(id, request.OrderedQuestionIds),
            cancellationToken);
        return NoContent();
    }
}
