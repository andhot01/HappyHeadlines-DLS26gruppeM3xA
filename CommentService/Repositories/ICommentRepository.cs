using CommentService.Models;

namespace CommentService.Repositories;

public interface ICommentRepository
{
    IEnumerable<Comment> GetByArticleId(Guid articleId);
    Comment? GetById(Guid id);
    Comment Create(Comment comment);
    bool Delete(Guid id);
}