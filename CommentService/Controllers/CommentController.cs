using CommentService.Models;
using CommentService.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace CommentService.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _repository;
    private readonly IHttpClientFactory _httpClientFactory;

    public CommentsController(ICommentRepository repository, IHttpClientFactory httpClientFactory)
    {
        _repository = repository;
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost]
    public async Task<ActionResult<Comment>> Create(Comment comment)
    {
        try
        {
            var client =
                _httpClientFactory.CreateClient("ProfanityService");

            var response = await client.PostAsJsonAsync(
                "/api/profanity/check",
                comment.Content);

            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(
                    StatusCodes.Status503ServiceUnavailable,
                    "Comment could not be checked for profanity.");
            }

            var result =
                await response.Content
                    .ReadFromJsonAsync<ProfanityCheckResponse>();

            if (result?.ContainsProfanity == true)
            {
                return BadRequest("Comment contains profanity.");
            }

            var createdComment = _repository.Create(comment);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdComment.Id },
                createdComment);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "ProfanityService is currently unavailable. Comment was not saved.");
        }
        catch (Exception)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "ProfanityService is currently unavailable. Comment was not saved.");
        }
    }

    [HttpGet("article/{articleId:guid}")]
    public ActionResult<IEnumerable<Comment>> GetByArticleId(Guid articleId)
    {
        return Ok(_repository.GetByArticleId(articleId));
    }

    [HttpGet("{id:guid}")]
    public ActionResult<Comment> GetById(Guid id)
    {
        var comment = _repository.GetById(id);

        if (comment == null)
            return NotFound();

        return Ok(comment);
    }

    [HttpDelete("{id:guid}")]
    public IActionResult Delete(Guid id)
    {
        var success = _repository.Delete(id);

        if (!success)
            return NotFound();

        return NoContent();
    }
}