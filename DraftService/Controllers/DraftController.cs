using DraftService.Data;
using DraftService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DraftService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DraftController : ControllerBase
{
    private readonly DraftDbContext _context;
    private readonly ILogger<DraftController> _logger;

    public DraftController(DraftDbContext context, ILogger<DraftController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Draft>>> GetAll()
    {
        return Ok(await _context.Drafts.ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Draft>> GetById(int id)
    {
        var draft = await _context.Drafts.FindAsync(id);

        if (draft == null)
        {
            _logger.LogWarning(
                "Draft {DraftId} was not found",
                id);

            return NotFound();
        }

        return Ok(draft);
    }

    [HttpPost]
    public async Task<ActionResult<Draft>> Create(Draft draft)
    {
        draft.Id = 0;
        draft.CreatedAt = DateTime.UtcNow;
        draft.UpdatedAt = DateTime.UtcNow;

        _context.Drafts.Add(draft);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Draft {DraftId} created",
            draft.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = draft.Id },
            draft);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Draft updatedDraft)
    {
        var draft = await _context.Drafts.FindAsync(id);

        if (draft == null)
        {
            _logger.LogWarning(
                "Cannot update draft {DraftId}: draft was not found",
                id);

            return NotFound();
        }

        draft.Title = updatedDraft.Title;
        draft.Content = updatedDraft.Content;
        draft.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Draft {DraftId} updated",
            id);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var draft = await _context.Drafts.FindAsync(id);

        if (draft == null)
        {
            _logger.LogWarning(
                "Cannot delete draft {DraftId}: draft was not found",
                id);

            return NotFound();
        }

        _context.Drafts.Remove(draft);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Draft {DraftId} deleted",
            id);

        return NoContent();
    }
}