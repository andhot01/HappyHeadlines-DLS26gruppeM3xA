using ProfanityService.Models;

namespace ProfanityService.Repositories;

public interface IProfanityRepository
{
    IEnumerable<ProfanityWord> GetAll();
    ProfanityWord Create(ProfanityWord profanityWord);
    bool ContainsProfanity(string text);
}