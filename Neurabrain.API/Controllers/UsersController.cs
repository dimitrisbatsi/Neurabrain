using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Neurabrain.Domain.Entities;
using Neurabrain.Domain.Enums;
using Neurabrain.Infrastructure.Data;
using Neurabrain.Shared.DTOs;
using System.Security.Claims;

namespace Neurabrain.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var query = _context.Users.AsQueryable();

            if (currentUserRole == "Organization")
            {
                // Βρίσκουμε σε ποια φροντιστήρια ανήκει αυτός που κάνει το request
                var myOrgIds = await _context.UserOrganizations
                    .Where(uo => uo.UserId == currentUserId)
                    .Select(uo => uo.OrganizationId)
                    .ToListAsync();

                // Φέρνουμε μόνο τους χρήστες που ανήκουν σε αυτά τα φροντιστήρια
                query = query.Where(u => u.UserOrganizations.Any(uo => myOrgIds.Contains(uo.OrganizationId)));
            }
            else if (currentUserRole == "Teacher")
            {
                // Ο Καθηγητής βλέπει τον εαυτό του ΚΑΙ τους Μαθητές του φροντιστηρίου του
                var myOrgIds = await _context.UserOrganizations
                    .Where(uo => uo.UserId == currentUserId)
                    .Select(uo => uo.OrganizationId)
                    .ToListAsync();

                query = query.Where(u => u.Id == currentUserId ||
                                        (u.Role == UserRole.Student && u.UserOrganizations.Any(uo => myOrgIds.Contains(uo.OrganizationId))));
            }
            else if (currentUserRole != "SuperAdmin" && currentUserRole != "Admin")
            {
                // Οποιοσδήποτε άλλος (π.χ. απλός Μαθητής) βλέπει ΜΟΝΟ τον εαυτό του
                query = query.Where(u => u.Id == currentUserId);
            }

            var users = await query
                .Include(u => u.UserOrganizations)
                .OrderBy(u => u.FullName)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    Role = u.Role.ToString(),
                    OrganizationIds = u.UserOrganizations.Select(uo => uo.OrganizationId).ToList(),
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin, Admin, Organization")]
        public async Task<ActionResult<UserDto>> CreateUser(UserDto dto)
        {
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (currentUserRole == "Organization")
            {
                // 1. Δεν μπορεί να φτιάξει Admins!
                if (dto.Role == "SuperAdmin" || dto.Role == "Admin" || dto.Role == "Organization")
                {
                    return Forbid();
                }

                // 2. Πρέπει υποχρεωτικά να βάλει τον χρήστη στο ΔΙΚΟ ΤΟΥ φροντιστήριο
                var myOrgIds = await _context.UserOrganizations
                    .Where(uo => uo.UserId == currentUserId)
                    .Select(uo => uo.OrganizationId)
                    .ToListAsync();

                if (dto.OrganizationIds.Any(id => !myOrgIds.Contains(id)))
                {
                    return Forbid(); // Προσπαθεί να τον βάλει σε ξένο φροντιστήριο
                }
            }

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = Enum.Parse<UserRole>(dto.Role),
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);

            // Συνδέουμε τον χρήστη με τα επιλεγμένα φροντιστήρια
            foreach (var orgId in dto.OrganizationIds)
            {
                _context.UserOrganizations.Add(new UserOrganization
                {
                    UserId = user.Id,
                    OrganizationId = orgId
                });
            }

            await _context.SaveChangesAsync();

            dto.Id = user.Id;
            dto.Password = string.Empty; // Δεν επιστρέφουμε ποτέ τον κωδικό πίσω στο UI
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, dto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, UserDto dto)
        {
            if (id != dto.Id) return BadRequest();

            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var user = await _context.Users
                .Include(u => u.UserOrganizations)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            if (currentUserRole == "Organization")
            {
                var myOrgIds = await _context.UserOrganizations
                    .Where(uo => uo.UserId == currentUserId)
                    .Select(uo => uo.OrganizationId)
                    .ToListAsync();

                // 1. Ελέγχουμε αν ο χρήστης που πάει να κάνει update ανήκει στο φροντιστήριό του
                if (!user.UserOrganizations.Any(uo => myOrgIds.Contains(uo.OrganizationId))) return Forbid();

                // 2. Δεν μπορεί να τον κάνει Admin!
                if (dto.Role == "SuperAdmin" || dto.Role == "Admin") return Forbid();

                // 3. Δεν μπορεί να του δώσει πρόσβαση σε ξένο φροντιστήριο
                if (dto.OrganizationIds.Any(orgId => !myOrgIds.Contains(orgId))) return Forbid();
            }
            else if (currentUserRole != "SuperAdmin" && currentUserRole != "Admin")
            {
                // Αν είναι απλός χρήστης (π.χ. Καθηγητής), μπορεί να κάνει update ΜΟΝΟ τον εαυτό του!
                if (id != currentUserId) return Forbid();

                // Αγνοούμε αν προσπαθεί να "χακάρει" τον ρόλο του ή τα φροντιστήριά του μέσω JSON
                dto.Role = user.Role.ToString();
                dto.OrganizationIds = user.UserOrganizations.Select(uo => uo.OrganizationId).ToList();
            }

            user.FullName = dto.FullName;
            user.Email = dto.Email;
            user.Role = Enum.Parse<UserRole>(dto.Role);

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            // Ενημέρωση Φροντιστηρίων (Διαγράφουμε τα παλιά και βάζουμε τα νέα)
            _context.UserOrganizations.RemoveRange(user.UserOrganizations);
            foreach (var orgId in dto.OrganizationIds)
            {
                _context.UserOrganizations.Add(new UserOrganization
                {
                    UserId = user.Id,
                    OrganizationId = orgId
                });
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin, Admin, Organization")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var user = await _context.Users
                .Include(u => u.UserOrganizations)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            if (currentUserRole == "Organization")
            {
                var myOrgIds = await _context.UserOrganizations
                    .Where(uo => uo.UserId == currentUserId)
                    .Select(uo => uo.OrganizationId)
                    .ToListAsync();

                // Αν πάει να διαγράψει χρήστη άλλου φροντιστηρίου, ρίξε πόρτα!
                if (!user.UserOrganizations.Any(uo => myOrgIds.Contains(uo.OrganizationId))) return Forbid();

                // Προαιρετικά: Να μην μπορεί να διαγράψει τον εαυτό του
                if (id == currentUserId) return BadRequest("Δεν μπορείτε να διαγράψετε τον δικό σας λογαριασμό.");
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}