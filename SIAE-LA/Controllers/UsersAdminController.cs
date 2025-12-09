#nullable enable
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.Domain.Entities;
using SIAE_LA.Infrastructure.Persistence;

namespace SIAE_LA.Web.Controllers;

/// <summary>
/// Controlador para la gestión de usuarios del sistema.
/// - Acceso: Admin y JefeArea (pueden gestionar usuarios según reglas de negocio).
/// - Conserva los endpoints existentes para revisión/aprobación de usuarios (pending/approve)
///   y agrega funcionalidades de listado, detalle, edición de roles y desactivación.
/// </summary>
[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin,JefeArea")]
public class UsersAdminController(ApplicationDbContext db, UserManager<ApplicationUser> um, RoleManager<IdentityRole> rm) : ControllerBase
{
    // DTOs internos
    public sealed record PendingUserDto(string Id, string? Email, string? FullName, DateTime? FechaRegistro);
    public sealed record UserReadDto(string Id, string? UserName, string? Email, string FullName, bool IsApproved, IEnumerable<string> Roles, int? PersonaId);
    public sealed record UserUpdateDto(string? FullName, string? Email, bool? IsApproved, IEnumerable<string>? Roles);
    public sealed record PasswordUpdateDto(string NewPassword);
    public sealed record EmailUpdateDto(string NewEmail);
    public sealed record UsersPageResult(int Page, int PageSize, int TotalItems, IEnumerable<UserReadDto> Items);

    private static readonly string[] ElevatedRoles = new[] { "Admin", "Direccion", "Subdireccion" };

    /// <summary>
    /// Lista de usuarios pendientes de aprobación.
    /// (Mejorado: devuelve ApiResponse con metadatos mínimos)
    /// </summary>
    [HttpGet("pending")]
    public async Task<IActionResult> Pending()
    {
        var list = await db.Users
            .Where(u => !u.IsApproved)
            .OrderBy(u => u.Email)
            .Select(u => new PendingUserDto(u.Id, u.Email, u.FullName ?? string.Empty, u.RegisteredAt ?? (DateTime?)null))
            .ToListAsync();

        return Ok(ApiResponse<IEnumerable<PendingUserDto>>.Success(list));
    }

    /// <summary>
    /// Convierte o revoca a un Docente en JefeArea.
    /// Roles permitidos: Admin.
    /// - Si promote=true: asigna rol JefeArea al usuario y al docente correspondiente.
    /// - Si promote=false: revoca el rol JefeArea.
    /// </summary>
    [HttpPost("docente/{docenteId}/jefearea")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetJefeArea(int docenteId, [FromQuery] bool promote = true)
    {
        var docente = await db.Docentes.Include(d => d.Persona).FirstOrDefaultAsync(d => d.Id == docenteId);
        if (docente is null) return NotFound(ApiResponse<string>.Fail("Docente no encontrado"));

        var user = await um.Users.FirstOrDefaultAsync(u => u.PersonaId == docente.PersonaId);
        if (user is null) return NotFound(ApiResponse<string>.Fail("Usuario vinculado al docente no encontrado"));

        if (promote)
        {
            var r = await um.AddToRoleAsync(user, "JefeArea");
            if (!r.Succeeded) return BadRequest(ApiResponse<string>.Fail(string.Join("; ", r.Errors.Select(e => e.Description))));
            return Ok(ApiResponse<string>.Success("OK", "Docente promovido a JefeArea"));
        }
        else
        {
            var r = await um.RemoveFromRoleAsync(user, "JefeArea");
            if (!r.Succeeded) return BadRequest(ApiResponse<string>.Fail(string.Join("; ", r.Errors.Select(e => e.Description))));
            return Ok(ApiResponse<string>.Success("OK", "JefeArea revocado"));
        }
    }

    /// <summary>
    /// Actualiza la contraseña de un usuario.
    /// Roles permitidos: Admin, JefeArea.
    /// - Admin puede actualizar la contraseña de cualquier usuario.
    /// - JefeArea puede actualizar contraseñas sólo de usuarios con rol Docente.
    /// </summary>
    [HttpPost("{id}/password")]
    public async Task<IActionResult> UpdatePassword(string id, [FromBody] PasswordUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.NewPassword)) return BadRequest(ApiResponse<string>.Fail("NewPassword es requerido"));
        var target = await um.FindByIdAsync(id);
        if (target is null) return NotFound(ApiResponse<string>.Fail("Usuario no encontrado"));

        var callerIsAdmin = User.IsInRole("Admin");
        if (!callerIsAdmin && !User.IsInRole("JefeArea")) return Forbid();

        if (User.IsInRole("JefeArea") && !callerIsAdmin)
        {
            // JefeArea puede cambiar contraseñas solo de Docentes
            if (!await um.IsInRoleAsync(target, "Docente")) return Forbid();
        }

        var token = await um.GeneratePasswordResetTokenAsync(target);
        var res = await um.ResetPasswordAsync(target, token, dto.NewPassword);
        if (!res.Succeeded) return BadRequest(ApiResponse<string>.Fail(string.Join("; ", res.Errors.Select(e => e.Description))));
        return Ok(ApiResponse<string>.Success("OK", "Contraseña actualizada"));
    }

    /// <summary>
    /// Actualiza el email de un usuario.
    /// Roles permitidos: Admin, JefeArea.
    /// - Admin puede actualizar el email de cualquier usuario.
    /// - JefeArea puede actualizar email sólo de Docentes.
    /// </summary>
    [HttpPost("{id}/email")]
    public async Task<IActionResult> UpdateEmail(string id, [FromBody] EmailUpdateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto?.NewEmail)) return BadRequest(ApiResponse<string>.Fail("NewEmail es requerido"));
        var target = await um.FindByIdAsync(id);
        if (target is null) return NotFound(ApiResponse<string>.Fail("Usuario no encontrado"));

        var callerIsAdmin = User.IsInRole("Admin");
        if (!callerIsAdmin && !User.IsInRole("JefeArea")) return Forbid();

        if (User.IsInRole("JefeArea") && !callerIsAdmin)
        {
            if (!await um.IsInRoleAsync(target, "Docente")) return Forbid();
        }

        var existing = await um.FindByEmailAsync(dto.NewEmail);
        if (existing is not null && existing.Id != target.Id)
            return Conflict(ApiResponse<string>.Fail("El email ya está en uso por otro usuario."));

        var setRes = await um.SetEmailAsync(target, dto.NewEmail);
        if (!setRes.Succeeded) return BadRequest(ApiResponse<string>.Fail(string.Join("; ", setRes.Errors.Select(e => e.Description))));
        target.UserName = dto.NewEmail;
        await um.UpdateAsync(target);
        return Ok(ApiResponse<string>.Success("OK", "Email actualizado"));
    }

    /// <summary>
    /// Aprueba un usuario: valida requisitos por rol (Docente/JefeArea requieren Persona y ficha Docente).
    /// Mejora: usa transacción y devuelve errores claros.
    /// </summary>
    [HttpPost("{userId}/approve")]
    public async Task<IActionResult> Approve(string userId)
    {
        var approver = await um.GetUserAsync(User) ?? throw new InvalidOperationException("No se pudo resolver el usuario aprobador.");
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return NotFound(ApiResponse<string>.Fail("Usuario no encontrado."));

        if (user.IsApproved) return BadRequest(ApiResponse<string>.Fail("Usuario ya se encuentra aprobado."));

        var roles = (await um.GetRolesAsync(user)).ToArray();
        if (roles.Length == 0) return BadRequest(ApiResponse<string>.Fail("Usuario no tiene roles asignados. Asigna roles antes de aprobar."));

        bool requiereDocente = roles.Contains("Docente", StringComparer.OrdinalIgnoreCase) ||
                               roles.Contains("JefeArea", StringComparer.OrdinalIgnoreCase);

        if (requiereDocente)
        {
            if (user.PersonaId is null)
                return BadRequest(ApiResponse<string>.Fail("Usuario con rol Docente/JefeArea debe tener Persona vinculada."));

            bool existeDocente = await db.Docentes.AnyAsync(d => d.PersonaId == user.PersonaId);
            if (!existeDocente)
                return BadRequest(ApiResponse<string>.Fail("Usuario con rol Docente/JefeArea debe tener ficha de Docente asociada."));
        }

        using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            user.IsApproved = true;
            user.ApprovedByUserId = approver.Id;
            user.ApprovedAt = DateTime.UtcNow;

            db.Update(user);
            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return Ok(ApiResponse<string>.Success("OK", "Usuario aprobado."));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return StatusCode(500, ApiResponse<string>.Fail($"Error al aprobar usuario: {ex.Message}"));
        }
    }

    // -------------------------------------------------------------------------
    // Nuevos endpoints de gestión
    // -------------------------------------------------------------------------

    /// <summary>
    /// Listado paginado de usuarios con búsqueda y filtro por rol.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] QueryParams q, [FromQuery] string? role)
    {
        var usersQ = um.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var s = q.Search.Trim();
            usersQ = usersQ.Where(u =>
                (u.UserName ?? "").Contains(s) ||
                (u.Email ?? "").Contains(s) ||
                (u.FullName ?? "").Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(role))
        {
            var roleEntity = await rm.Roles.FirstOrDefaultAsync(r => r.Name == role);
            if (roleEntity is null) return BadRequest(ApiResponse<UsersPageResult>.Fail($"Rol '{role}' no existe."));
            var userIds = await db.Set<IdentityUserRole<string>>()
                .Where(ur => ur.RoleId == roleEntity.Id)
                .Select(ur => ur.UserId)
                .ToListAsync();

            usersQ = usersQ.Where(u => userIds.Contains(u.Id));
        }

        var total = await usersQ.CountAsync();

        var page = Math.Max(1, q.Page);
        var pageSize = Math.Max(1, q.PageSize);
        var skip = (page - 1) * pageSize;

        var usersList = await usersQ
            .OrderBy(u => u.Email)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        var items = new List<UserReadDto>(usersList.Count);
        foreach (var u in usersList)
        {
            var roles = await um.GetRolesAsync(u);
            items.Add(new UserReadDto(u.Id, u.UserName, u.Email, u.FullName ?? string.Empty, u.IsApproved, roles, u.PersonaId));
        }

        var result = new UsersPageResult(page, pageSize, total, items);
        return Ok(ApiResponse<UsersPageResult>.Success(result));
    }

    /// <summary>
    /// Detalle de usuario por id.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetOne(string id)
    {
        var u = await um.FindByIdAsync(id);
        if (u is null) return NotFound(ApiResponse<UserReadDto>.Fail("Usuario no encontrado"));

        var roles = await um.GetRolesAsync(u);
        var dto = new UserReadDto(u.Id, u.UserName, u.Email, u.FullName ?? string.Empty, u.IsApproved, roles, u.PersonaId);
        return Ok(ApiResponse<UserReadDto>.Success(dto));
    }

    /// <summary>
    /// Actualiza datos básicos y roles de un usuario.
    /// - Reglas:
    ///   * Solo Admin puede asignar/quitar roles elevados (Admin/Direccion/Subdireccion).
    ///   * No se puede quitar Admin del último Admin del sistema.
    ///   * JefeArea puede editar usuarios pero no roles elevados.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UserUpdateDto dto)
    {
        var target = await um.FindByIdAsync(id);
        if (target is null) return NotFound(ApiResponse<UserReadDto>.Fail("Usuario no encontrado"));

        var caller = await um.GetUserAsync(User) ?? throw new InvalidOperationException();
        var callerIsAdmin = User.IsInRole("Admin");

        // Validaciones básicas
        if (!string.IsNullOrWhiteSpace(dto.Email) && !string.Equals(dto.Email, target.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existing = await um.FindByEmailAsync(dto.Email);
            if (existing is not null && existing.Id != target.Id)
                return Conflict(ApiResponse<UserReadDto>.Fail("El email ya está en uso por otro usuario."));
        }

        // Roles management
        if (dto.Roles is not null)
        {
            var requestedRoles = dto.Roles.Select(r => r.Trim()).Where(r => !string.IsNullOrEmpty(r)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

            var existingRoles = await rm.Roles.Select(r => r.Name!).ToListAsync();
            var invalid = requestedRoles.Except(existingRoles, StringComparer.OrdinalIgnoreCase).ToArray();
            if (invalid.Length > 0)
                return BadRequest(ApiResponse<UserReadDto>.Fail($"Roles inválidos: {string.Join(',', invalid)}"));

            if (!callerIsAdmin && requestedRoles.Any(r => ElevatedRoles.Contains(r, StringComparer.OrdinalIgnoreCase)))
                return Forbid();

            var currentRoles = (await um.GetRolesAsync(target)).ToArray();
            var toAdd = requestedRoles.Except(currentRoles, StringComparer.OrdinalIgnoreCase).ToArray();
            var toRemove = currentRoles.Except(requestedRoles, StringComparer.OrdinalIgnoreCase).ToArray();

            // Protege: no remover Admin del último Admin
            if (toRemove.Any(r => r.Equals("Admin", StringComparison.OrdinalIgnoreCase)))
            {
                var admins = await um.GetUsersInRoleAsync("Admin");
                if (admins.Count <= 1 && admins.Any(a => a.Id == target.Id))
                    return BadRequest(ApiResponse<UserReadDto>.Fail("No se puede quitar rol Admin del último usuario Admin."));
            }

            if (toAdd.Length > 0)
            {
                var addRes = await um.AddToRolesAsync(target, toAdd);
                if (!addRes.Succeeded) return BadRequest(ApiResponse<UserReadDto>.Fail(string.Join("; ", addRes.Errors.Select(e => e.Description))));
            }

            if (toRemove.Length > 0)
            {
                var remRes = await um.RemoveFromRolesAsync(target, toRemove);
                if (!remRes.Succeeded) return BadRequest(ApiResponse<UserReadDto>.Fail(string.Join("; ", remRes.Errors.Select(e => e.Description))));
            }
        }

        // Actualizar FullName y Email
        if (!string.IsNullOrWhiteSpace(dto.FullName) && dto.FullName != target.FullName)
            target.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != target.Email)
        {
            var setEmailRes = await um.SetEmailAsync(target, dto.Email);
            if (!setEmailRes.Succeeded)
                return BadRequest(ApiResponse<UserReadDto>.Fail(string.Join("; ", setEmailRes.Errors.Select(e => e.Description))));
            target.UserName = dto.Email; // opcional: mantener userName consistente
        }

        if (dto.IsApproved.HasValue)
        {
            if (dto.IsApproved.Value && !target.IsApproved)
            {
                // Si se está aprobando aquí, respetar las mismas reglas que Approve
                var roles = (await um.GetRolesAsync(target)).ToArray();
                bool requiereDocente = roles.Contains("Docente", StringComparer.OrdinalIgnoreCase) ||
                                       roles.Contains("JefeArea", StringComparer.OrdinalIgnoreCase);
                if (requiereDocente)
                {
                    if (target.PersonaId is null)
                        return BadRequest(ApiResponse<UserReadDto>.Fail("Usuario con rol Docente/JefeArea debe tener Persona vinculada para aprobar."));
                    if (!await db.Docentes.AnyAsync(d => d.PersonaId == target.PersonaId))
                        return BadRequest(ApiResponse<UserReadDto>.Fail("Usuario con rol Docente/JefeArea debe tener ficha de Docente asociada."));
                }

                target.IsApproved = true;
                target.ApprovedAt = DateTime.UtcNow;
                target.ApprovedByUserId = caller.Id;
            }
            else if (!dto.IsApproved.Value && target.IsApproved)
            {
                // desaprobar: sólo Admin puede desaprobar a Admins
                if (!callerIsAdmin && await um.IsInRoleAsync(target, "Admin"))
                    return Forbid();

                target.IsApproved = false;
            }
        }

        var upd = await um.UpdateAsync(target);
        if (!upd.Succeeded) return BadRequest(ApiResponse<UserReadDto>.Fail(string.Join("; ", upd.Errors.Select(e => e.Description))));

        var finalRoles = await um.GetRolesAsync(target);
        var dtoOut = new UserReadDto(target.Id, target.UserName, target.Email, target.FullName ?? string.Empty, target.IsApproved, finalRoles, target.PersonaId);
        return Ok(ApiResponse<UserReadDto>.Success(dtoOut, "Usuario actualizado"));
    }

    /// <summary>
    /// Desactiva (soft) un usuario: marca IsApproved=false y aplica bloqueo de acceso.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Deactivate(string id)
    {
        var target = await um.FindByIdAsync(id);
        if (target is null) return NotFound(ApiResponse<string>.Fail("Usuario no encontrado"));

        var callerIsAdmin = User.IsInRole("Admin");

        if (!callerIsAdmin && await um.IsInRoleAsync(target, "Admin"))
            return Forbid();

        target.IsApproved = false;
        target.LockoutEnabled = true;
        await um.SetLockoutEndDateAsync(target, DateTimeOffset.UtcNow.AddYears(100));
        var res = await um.UpdateAsync(target);
        if (!res.Succeeded) return BadRequest(ApiResponse<string>.Fail(string.Join("; ", res.Errors.Select(e => e.Description))));

        return Ok(ApiResponse<string>.Success("OK", "Usuario desactivado"));
    }

    /// <summary>
    /// Devuelve roles existentes en el sistema.
    /// </summary>
    [HttpGet("roles")]
    public async Task<IActionResult> GetRoles()
    {
        var roles = await rm.Roles.Select(r => r.Name!).ToListAsync();
        return Ok(ApiResponse<IEnumerable<string>>.Success(roles));
    }

    /// <summary>
    /// Endpoint exclusivo para Jefes de Área: obtiene reportes y herramientas de gestión propias del JefeArea.
    /// Roles: JefeArea
    /// </summary>
    [HttpGet("jefearea/tools")]
    [Authorize(Roles = "JefeArea")]
    public async Task<IActionResult> JefeAreaTools()
    {
        // Placeholder: devolver estadísticas rápidas
        var docentesCount = await db.Docentes.CountAsync();
        var jefesCount = await db.Set<IdentityUserRole<string>>().Where(ur => rm.Roles.Any(r => r.Id == ur.RoleId && r.Name == "JefeArea")).CountAsync();
        return Ok(ApiResponse<object>.Success(new { Docentes = docentesCount, Jefes = jefesCount }));
    }

    /// <summary>
    /// Endpoint exclusivo para Docentes (no JefeArea): herramientas propias del Docente.
    /// Roles: Docente
    /// </summary>
    [HttpGet("docente/tools")]
    [Authorize(Roles = "Docente")]
    public async Task<IActionResult> DocenteTools()
    {
        // Placeholder: devolver lista de asignaciones activas para el docente que consulta
        var user = await um.GetUserAsync(User);
        if (user?.PersonaId is null) return Forbid();
        var docente = await db.Docentes.FirstOrDefaultAsync(d => d.PersonaId == user.PersonaId);
        if (docente is null) return Forbid();
        var asigns = await db.DocentesNivelDetalleCurso.AsNoTracking().Where(a => a.DocenteId == docente.Id && a.Activo)
            .Select(a => new { a.Id, a.NivelDetalleCursoId, a.FechaRegistro })
            .ToListAsync();
        return Ok(ApiResponse<object>.Success(new { Asignaciones = asigns }));
    }
}
