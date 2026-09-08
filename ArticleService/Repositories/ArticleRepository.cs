using ArticleService.Data;
using ArticleService.Models;
using Microsoft.EntityFrameworkCore;

namespace ArticleService.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly ArticleDbContextFactory _contextFactory;

    public ArticleRepository(ArticleDbContextFactory contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public IEnumerable<Article> GetAll(Region region)
    {
        using var context = _contextFactory.Create(region);

        return context.Articles
            .AsNoTracking()
            .ToList();
    }

    public Article? GetById(Guid id, Region region)
    {
        using var context = _contextFactory.Create(region);

        return context.Articles
            .AsNoTracking()
            .FirstOrDefault(a => a.Id == id);
    }

    public Article Create(Article article)
    {
        using var context = _contextFactory.Create(article.Region);

        article.Id = Guid.NewGuid();
        article.PublishedAt = DateTime.UtcNow;

        context.Articles.Add(article);
        context.SaveChanges();

        return article;
    }

    public bool Update(Guid id, Region region, Article updatedArticle)
    {
        using var context = _contextFactory.Create(region);

        var article = context.Articles
            .FirstOrDefault(a => a.Id == id);

        if (article == null)
        {
            return false;
        }

        article.Title = updatedArticle.Title;
        article.Content = updatedArticle.Content;

        context.SaveChanges();

        return true;
    }

    public bool Delete(Guid id, Region region)
    {
        using var context = _contextFactory.Create(region);

        var article = context.Articles
            .FirstOrDefault(a => a.Id == id);

        if (article == null)
        {
            return false;
        }

        context.Articles.Remove(article);
        context.SaveChanges();

        return true;
    }
}