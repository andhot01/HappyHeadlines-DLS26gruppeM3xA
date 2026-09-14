using CommentService.Models;
using CommentService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _repository;

    public CommentsController(ICommentRepository repository)
    {
        _repository = repository;
    }

    [HttpPost]
    public ActionResult<Comment> Create(Comment comment)
    {
        var createdComment = _repository.Create(comment);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdComment.Id },
            createdComment);
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