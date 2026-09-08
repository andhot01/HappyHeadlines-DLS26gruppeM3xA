using ArticleService.Models;

namespace ArticleService.Repositories;

public class ArticleRepository : IArticleRepository
{
    private readonly List<Article> _articles = new();

    public IEnumerable<Article> GetAll(Region region)
    {
        return _articles.Where(a => a.Region == region);
    }

    public Article? GetById(Guid id, Region region)
    {
        return _articles.FirstOrDefault(
            a => a.Id == id && a.Region == region);
    }

    public Article Create(Article article)
    {
        article.Id = Guid.NewGuid();
        article.PublishedAt = DateTime.UtcNow;

        _articles.Add(article);

        return article;
    }

    public bool Update(Guid id, Region region, Article updatedArticle)
    {
        var article = GetById(id, region);

        if (article == null)
        {
            return false;
        }

        article.Title = updatedArticle.Title;
        article.Content = updatedArticle.Content;

        return true;
    }

    public bool Delete(Guid id, Region region)
    {
        var article = GetById(id, region);

        if (article == null)
        {
            return false;
        }

        _articles.Remove(article);

        return true;
    }
}