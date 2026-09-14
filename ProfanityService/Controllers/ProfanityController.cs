using Microsoft.AspNetCore.Mvc;
using ProfanityService.Models;
using ProfanityService.Repositories;

namespace ProfanityService.Controllers;

[ApiController]
[Route("api/profanity")]
public class ProfanityController : ControllerBase
{
    private readonly IProfanityRepository _repository;

    public ProfanityController(IProfanityRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProfanityWord>> GetAll()
    {
        return Ok(_repository.GetAll());
    }

    [HttpPost]
    public ActionResult<ProfanityWord> Create(ProfanityWord profanityWord)
    {
        var created = _repository.Create(profanityWord);
        return Ok(created);
    }

    [HttpPost("check")]
    public IActionResult Check([FromBody] string text)
    {
        var containsProfanity = _repository.ContainsProfanity(text);

        return Ok(new
        {
            containsProfanity
        });
    }
}