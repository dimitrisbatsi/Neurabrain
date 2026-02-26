using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Neurabrain.Domain.Entities;
using Neurabrain.Infrastructure.Data;
using Neurabrain.Shared.DTOs;
using System.Security.Claims;

namespace Neurabrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SubscriptionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin, Admin, Organization")]
        public async Task<ActionResult<IEnumerable<SubscriptionDto>>> GetSubscriptions()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var query = _context.Subscriptions.AsQueryable();

            if (userRole != "SuperAdmin" && userRole != "Admin")
            {
                if (Guid.TryParse(userIdString, out Guid userId))
                {
                    // Φέρε μόνο τις συνδρομές του χρήστη μέσω των οργανισμών που ανήκει.
                    query = query.Where(o => o.Organization.UserOrganizations.Any(uo => uo.UserId == userId));
                }
                else
                {
                    return Unauthorized(); // Αν για κάποιο λόγο δεν έχει ID, ρίξε πόρτα.
                }
            }

            var subs = await query
                .Include(s => s.Organization) // Φέρνουμε και τα στοιχεία του Οργανισμού
                .OrderBy(s => s.Organization.Name)
                .Select(s => new SubscriptionDto
                {
                    Id = s.Id,
                    OrganizationId = s.OrganizationId,
                    OrganizationName = s.Organization.Name,
                    PlanName = s.PlanName,
                    Status = s.Status,
                    StripeSubscriptionId = s.StripeSubscriptionId,
                    CurrentPeriodEnd = s.CurrentPeriodEnd
                })
                .ToListAsync();

            return Ok(subs);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<ActionResult<SubscriptionDto>> CreateSubscription(SubscriptionDto dto)
        {
            var entity = new Subscription
            {
                OrganizationId = dto.OrganizationId,
                PlanName = dto.PlanName,
                Status = dto.Status,
                StripeSubscriptionId = dto.StripeSubscriptionId,
                CurrentPeriodEnd = DateTime.SpecifyKind(dto.CurrentPeriodEnd, DateTimeKind.Utc),
                CreatedAt = DateTime.UtcNow
            };

            _context.Subscriptions.Add(entity);
            await _context.SaveChangesAsync();

            dto.Id = entity.Id;
            return CreatedAtAction(nameof(GetSubscriptions), new { id = entity.Id }, dto);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> UpdateSubscription(Guid id, SubscriptionDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var entity = await _context.Subscriptions.FindAsync(id);
            if (entity == null) return NotFound();

            entity.OrganizationId = dto.OrganizationId;
            entity.PlanName = dto.PlanName;
            entity.Status = dto.Status;
            entity.StripeSubscriptionId = dto.StripeSubscriptionId;
            entity.CurrentPeriodEnd = DateTime.SpecifyKind(dto.CurrentPeriodEnd, DateTimeKind.Utc);

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> DeleteSubscription(Guid id)
        {
            var entity = await _context.Subscriptions.FindAsync(id);
            if (entity == null) return NotFound();

            _context.Subscriptions.Remove(entity);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}