using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Infrastructure.Persistence;

namespace SIAE_LA.Web.Controllers;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class UsersAdminController(ApplicationDbContext db, UserManager<ApplicationUser> um) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        var list = await db.Users
            .Where(u => !u.IsApproved)
            .Select(u => new { u.Id, u.Email, u.FullName })
            .ToListAsync();
        return Ok(list);
    }

    [HttpPost("{userId}/approve")]
    public async Task<IActionResult> Approve(string userId)
    {
        var approver = await um.GetUserAsync(User) ?? throw new InvalidOperationException();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound();

        var roles = await um.GetRolesAsync(user);
        bool requiereDocente = roles.Contains("Docente") ||
                               roles.Contains("JefeArea");

        if (requiereDocente)
        {
            if (user.PersonaId is null)
                return BadRequest(new { message = "Usuario con rol Docente/JefeArea debe tener Persona vinculada." });

            bool existeDocente = await db.Docentes.AnyAsync(d => d.PersonaId == user.PersonaId);
            if (!existeDocente)
                return BadRequest(new { message = "Usuario con rol Docente/JefeArea debe tener ficha de Docente asociada." });
        }

        user.IsApproved = true;
        user.ApprovedByUserId = approver.Id;
        user.ApprovedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        return Ok(new { message = "Usuario aprobado." });
    }

}
