using ArticleService.Models;
using ArticleService.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace ArticleService.Controllers;

[ApiController]
[Route("api/articles")]
public class ArticlesController : ControllerBase
{
    private readonly IArticleRepository _repository;

    public ArticlesController(IArticleRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("{region}")]
    public ActionResult<Article> Create(
        Region region,
        Article article)
    {
        article.Region = region;

        var createdArticle = _repository.Create(article);

        return CreatedAtAction(
            nameof(GetById),
            new
            {
                region = createdArticle.Region,
                id = createdArticle.Id
            },
            createdArticle);
    }

    [HttpGet("{region}")]
    public ActionResult<IEnumerable<Article>> GetAll(Region region)
    {
        var articles = _repository.GetAll(region);

        return Ok(articles);
    }

    [HttpGet("{region}/{id:guid}")]
    public ActionResult<Article> GetById(
        Region region,
        Guid id)
    {
        var article = _repository.GetById(id, region);

        if (article == null)
        {
            return NotFound();
        }

        return Ok(article);
    }

    [HttpPut("{region}/{id:guid}")]
    public IActionResult Update(
        Region region,
        Guid id,
        Article updatedArticle)
    {
        var success = _repository.Update(
            id,
            region,
            updatedArticle);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{region}/{id:guid}")]
    public IActionResult Delete(
        Region region,
        Guid id)
    {
        var success = _repository.Delete(id, region);

        if (!success)
        {
            return NotFound();
        }

        return NoContent();
    }
}