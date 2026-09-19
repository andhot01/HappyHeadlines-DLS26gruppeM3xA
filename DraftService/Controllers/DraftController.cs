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

    public DraftController(DraftDbContext context)
    {
        _context = context;
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
            return NotFound();
        }

        draft.Title = updatedDraft.Title;
        draft.Content = updatedDraft.Content;
        draft.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var draft = await _context.Drafts.FindAsync(id);

        if (draft == null)
        {
            return NotFound();
        }

        _context.Drafts.Remove(draft);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}