using CommentService.Data;
using CommentService.Models;
using Microsoft.EntityFrameworkCore;

namespace CommentService.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly CommentDbContext _context;

    public CommentRepository(CommentDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Comment> GetByArticleId(Guid articleId)
    {
        return _context.Comments
            .AsNoTracking()
            .Where(c => c.ArticleId == articleId)
            .OrderBy(c => c.CreatedAt)
            .ToList();
    }

    public Comment? GetById(Guid id)
    {
        return _context.Comments
            .AsNoTracking()
            .FirstOrDefault(c => c.Id == id);
    }

    public Comment Create(Comment comment)
    {
        comment.Id = Guid.NewGuid();
        comment.CreatedAt = DateTime.UtcNow;

        _context.Comments.Add(comment);
        _context.SaveChanges();

        return comment;
    }

    public bool Delete(Guid id)
    {
        var comment = _context.Comments.FirstOrDefault(c => c.Id == id);

        if (comment == null)
            return false;

        _context.Comments.Remove(comment);
        _context.SaveChanges();

        return true;
    }
}