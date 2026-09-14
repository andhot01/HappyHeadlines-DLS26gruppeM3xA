using ProfanityService.Data;
using ProfanityService.Models;
using Microsoft.EntityFrameworkCore;

namespace ProfanityService.Repositories;

public class ProfanityRepository : IProfanityRepository
{
    private readonly ProfanityDbContext _context;

    public ProfanityRepository(ProfanityDbContext context)
    {
        _context = context;
    }

    public IEnumerable<ProfanityWord> GetAll()
    {
        return _context.ProfanityWords
            .AsNoTracking()
            .ToList();
    }

    public ProfanityWord Create(ProfanityWord profanityWord)
    {
        profanityWord.Id = Guid.NewGuid();

        _context.ProfanityWords.Add(profanityWord);
        _context.SaveChanges();

        return profanityWord;
    }

    public bool ContainsProfanity(string text)
    {
        var words = _context.ProfanityWords
            .AsNoTracking()
            .Select(p => p.Word)
            .ToList();

        return words.Any(word =>
            text.Contains(word, StringComparison.OrdinalIgnoreCase));
    }
}