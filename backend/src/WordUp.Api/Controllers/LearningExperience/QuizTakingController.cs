using WordUp.Api.Contracts.LearningExperience;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordUp.Application.LearningExperience.QuizTaking;

namespace WordUp.Api.Controllers.LearningExperience;

[ApiController]
[Authorize(Roles = Roles.Youth)]
[Route("api/me")]
public class QuizTakingController : ControllerBase
{
    private readonly IMediator _mediator;

    public QuizTakingController(IMediator mediator) => _mediator = mediator;

    [HttpPost("quizzes/{quizId:guid}/attempts")]
    public async Task<ActionResult<QuizAttemptView>> Start(Guid quizId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new StartQuizAttemptQuery(quizId), cancellationToken));

    [HttpPut("attempts/{attemptId:guid}/answers")]
    public async Task<IActionResult> SaveAnswers(Guid attemptId, [FromBody] SaveAnswersRequest request, CancellationToken cancellationToken)
    {
        await _mediator.Send(new SaveAnswersCommand(attemptId, request.AnswersJson), cancellationToken);
        return NoContent();
    }

    [HttpPost("attempts/{attemptId:guid}/submit")]
    public async Task<ActionResult<QuizResultView>> Submit(Guid attemptId, [FromBody] SubmitAttemptRequest request, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new SubmitQuizAttemptCommand(attemptId, request.AnswersJson), cancellationToken));

    [HttpGet("attempts/{attemptId:guid}")]
    public async Task<ActionResult<QuizResultView>> Result(Guid attemptId, CancellationToken cancellationToken) =>
        Ok(await _mediator.Send(new QuizResultQuery(attemptId), cancellationToken));
}
