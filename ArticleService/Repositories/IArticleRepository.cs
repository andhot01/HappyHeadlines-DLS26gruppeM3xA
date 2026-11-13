using ArticleService.Models;

namespace ArticleService.Repositories;

public interface IArticleRepository
{
    IEnumerable<Article> GetAll(
        Region region,
        int page = 1,
        int pageSize = 10);

    Article? GetById(Guid id, Region region);

    Article Create(Article article);

    bool Update(Guid id, Region region, Article article);

    bool Delete(Guid id, Region region);
}