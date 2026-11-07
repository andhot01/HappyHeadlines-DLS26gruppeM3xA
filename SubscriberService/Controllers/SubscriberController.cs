using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubscriberService.Data;
using SubscriberService.Models;
using System.ComponentModel.DataAnnotations;
using SubscriberService.Services;

namespace SubscriberService.Controllers;

[ApiController]
[Route("api/subscribers")]
public class SubscriberController : ControllerBase
{
    private readonly SubscriberDbContext _db;
    private readonly SubscriberQueuePublisher _queuePublisher;
    
    public SubscriberController(SubscriberDbContext db,  SubscriberQueuePublisher queuePublisher)
    {
        _db = db;
        _queuePublisher = queuePublisher;
    }

    [HttpPost]
    public async Task<ActionResult<Subscriber>> Subscribe(
        SubscribeRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        
        var alreadySubscribed = await _db.Subscribers
            .AnyAsync(s => s.Email  == email);

        if (alreadySubscribed)
        {
            return Conflict("Email is already subscribed.");
        }

        var subscriber = new Subscriber
        {
            Email = email,
            SubscribedAt = DateTime.UtcNow
        };

        _db.Subscribers.Add(subscriber);
        await _db.SaveChangesAsync();
        await _queuePublisher.PublishAsync(subscriber);

        return CreatedAtAction(
            nameof(GetById),
            new { id = subscriber.Id },
            subscriber);
    }

    [HttpGet]
    public async Task<ActionResult<List<Subscriber>>> GetAll()
    {
        var subscribers = await _db.Subscribers.ToListAsync();

        return Ok(subscribers);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Subscriber>> GetById(int id)
    {
        var subscriber = await _db.Subscribers.FindAsync(id);
        
        if (subscriber == null)
            return NotFound();

        return Ok(subscriber);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Unsubscribe(int id)
    {
        var subscriber = await _db.Subscribers.FindAsync(id);
        
        if (subscriber == null)
            return NotFound();
        
        _db.Subscribers.Remove(subscriber);
        await _db.SaveChangesAsync();
        
        return NoContent();
    }
    
    public class SubscribeRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
    
}