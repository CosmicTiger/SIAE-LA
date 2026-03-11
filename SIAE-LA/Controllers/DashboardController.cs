#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIAE_LA.Abstractions;
using SIAE_LA.Infrastructure.Persistence;
using SIAE_LA.Domain.Entities;

namespace SIAE_LA.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class DashboardController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DashboardController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _db = db;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public sealed record RoleCountDto(string Role, int Count);
    public sealed record MatriculaByNivelDto(string Nivel, int Count);
    public sealed record VacancyDto(int NivelDetalleId, string Nivel, string GradoSeccion, int? TotalVacantes, int? VacantesOcupadas, double OccupancyPct);

    public sealed record AdminDashboardDto(
        string View,
        int TotalUsers,
        int ActiveUsers,
        int PendingApprovals,
        int NewRegistrationsLast30Days,
        int TotalDocentes,
        int TotalAlumnos,
        int TotalApoderados,
        int TotalMatriculas,
        IEnumerable<RoleCountDto> UsersByRole,
        IEnumerable<MatriculaByNivelDto> MatriculasByNivel
    );

    public sealed record DireccionDashboardDto(
        string View,
        int TotalMatriculas,
        IEnumerable<MatriculaByNivelDto> MatriculasByNivel,
        IEnumerable<VacancyDto> NivelDetalleVacancias
    );

    // Additional DTOs for advanced metrics
    public sealed record TimePointDto(DateTime Date, int Count);
    public sealed record ActivitySummaryDto(string View, int DAU, int WAU, int MAU, IEnumerable<TimePointDto> LoginsByDay);
    public sealed record DataQualityDto(string View, int PersonasSinUsuario, int UsuariosSinPersona, int DocentesSinFicha, int UsuariosSinEmail, int UsuariosSinTelefono, IDictionary<string,int> PendingAgingBuckets);
    public sealed record ApprovalAgingDto(string View, double AverageDaysToApprove, IDictionary<string,int> Buckets);
    public sealed record CourseAcademicDto(int CursoId, string CursoDescripcion, double? NotaMedia, double PercentApproved);
    public sealed record AcademicSummaryDto(string View, IEnumerable<CourseAcademicDto> ByCourse);

    [HttpGet("admin")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<AdminDashboardDto>>> AdminSummary()
    {
        var totalUsers = await _userManager.Users.CountAsync();
        var activeUsers = await _userManager.Users.CountAsync(u => u.IsApproved);
        var pending = await _userManager.Users.CountAsync(u => !u.IsApproved);

        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        var newRegs = await _userManager.Users.CountAsync(u => EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= thirtyDaysAgo);

        var roles = await _roleManager.Roles.Select(r => new { r.Id, r.Name }).ToListAsync();
        var usersByRole = new List<RoleCountDto>();
        foreach (var r in roles)
        {
            var cnt = await _db.Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>().CountAsync(ur => ur.RoleId == r.Id);
            usersByRole.Add(new RoleCountDto(r.Name ?? string.Empty, cnt));
        }

        var totalDocentes = await _db.Docentes.CountAsync();
        var totalAlumnos = await _db.Alumnos.CountAsync();
        var totalApoderados = await _db.Apoderados.CountAsync();

        var totalMatriculas = await _db.Matriculas.CountAsync();

        var matriculasByNivel = await _db.Matriculas
            .AsNoTracking()
            .Include(m => m.NivelDetalle).ThenInclude(nd => nd.Nivel)
            .GroupBy(m => m.NivelDetalle.Nivel.DescripcionNivel)
            .Select(g => new MatriculaByNivelDto(g.Key ?? "(sin nivel)", g.Count()))
            .ToListAsync();

        var dto = new AdminDashboardDto(
            View: "DashboardAdmin",
            TotalUsers: totalUsers,
            ActiveUsers: activeUsers,
            PendingApprovals: pending,
            NewRegistrationsLast30Days: newRegs,
            TotalDocentes: totalDocentes,
            TotalAlumnos: totalAlumnos,
            TotalApoderados: totalApoderados,
            TotalMatriculas: totalMatriculas,
            UsersByRole: usersByRole,
            MatriculasByNivel: matriculasByNivel
        );

        // Also compute some advanced metrics for Admin view
        // DAU/WAU/MAU simple computation based on FechaRegistro shadow property as approximation for activity
        var now = DateTime.UtcNow;
        var dau = await _userManager.Users.CountAsync(u => EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-1));
        var wau = await _userManager.Users.CountAsync(u => EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-7));
        var mau = await _userManager.Users.CountAsync(u => EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-30));

        // Logins by day (last 14 days) — if no login table available we approximate with FechaRegistro distribution
        var loginsByDay = await _userManager.Users
            .Where(u => EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-14))
            .GroupBy(u => EF.Property<DateTime>(u, "FechaRegistro").Date)
            .Select(g => new TimePointDto(g.Key, g.Count()))
            .ToListAsync();

        var adminAct = new ActivitySummaryDto(View: "DashboardAdminActivity", DAU: dau, WAU: wau, MAU: mau, LoginsByDay: loginsByDay);

        // Data quality
        var personasSinUsuario = await _db.Personas.CountAsync(p => !_db.Users.Any(u => u.PersonaId == p.Id));
        var usuariosSinPersona = await _db.Users.CountAsync(u => u.PersonaId == null);
        var docentesSinFicha = await _db.Users.CountAsync(u => (u.PersonaId != null) && !_db.Docentes.Any(d => d.PersonaId == u.PersonaId) && ( _db.Users.Where(x => x.Id == u.Id).SelectMany(x => _roleManager.Roles.Where(r=>true)).Any() ));

        var usuariosSinEmail = await _db.Users.CountAsync(u => string.IsNullOrWhiteSpace(u.Email));
        var usuariosSinTelefono = await _db.Users.CountAsync(u => string.IsNullOrWhiteSpace(u.PhoneNumber));

        var pendingAging = new Dictionary<string,int>
        {
            ["0-3"] = await _userManager.Users.CountAsync(u => !u.IsApproved && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-3)),
            ["4-7"] = await _userManager.Users.CountAsync(u => !u.IsApproved && EF.Property<DateTime>(u, "FechaRegistro") < now.AddDays(-3) && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-7)),
            ["8+"] = await _userManager.Users.CountAsync(u => !u.IsApproved && EF.Property<DateTime>(u, "FechaRegistro") < now.AddDays(-7))
        };

        var dataQuality = new DataQualityDto(View: "DashboardDataQuality", PersonasSinUsuario: personasSinUsuario, UsuariosSinPersona: usuariosSinPersona, DocentesSinFicha: docentesSinFicha, UsuariosSinEmail: usuariosSinEmail, UsuariosSinTelefono: usuariosSinTelefono, PendingAgingBuckets: pendingAging);

        // Approval aging
        var approvedUsers = await _db.Users.Where(u => u.ApprovedAt != null).ToListAsync();
        double avgDays = 0;
        var buckets = new Dictionary<string,int> { ["0-3"] = 0, ["4-7"] = 0, ["8+"] = 0 };
        if (approvedUsers.Count > 0)
        {
            avgDays = approvedUsers.Average(u => (u.ApprovedAt - EF.Property<DateTime?>(u, "FechaRegistro"))?.TotalDays ?? 0);
            foreach (var u in approvedUsers)
            {
                var reg = EF.Property<DateTime?>(u, "FechaRegistro");
                if (reg is null || u.ApprovedAt is null) continue;
                var diff = (u.ApprovedAt.Value - reg.Value).TotalDays;
                if (diff <= 3) buckets["0-3"]++;
                else if (diff <= 7) buckets["4-7"]++;
                else buckets["8+"]++;
            }
        }
        var approvalAging = new ApprovalAgingDto(View: "DashboardApprovals", AverageDaysToApprove: Math.Round(avgDays,2), Buckets: buckets);

        // Academic summary sample (by course) — average note and percent approved (nota >=60)
        var academic = await _db.Calificaciones
            .AsNoTracking()
            .GroupBy(c => c.Curricula.DocenteNivelDetalleCurso.NivelDetalleCurso.CursoId)
            .Select(g => new CourseAcademicDto(
                g.Key,
                g.Select(c => c.Curricula.DocenteNivelDetalleCurso.NivelDetalleCurso.Curso.Descripcion).FirstOrDefault() ?? string.Empty,
                g.Average(c => (double?)c.Nota),
                g.Count(c => c.Nota >= 60) * 100.0 / Math.Max(1, g.Count())
            ))
            .ToListAsync();

        // Return main DTO bundled with extras in headers (so frontend can request extra endpoints separately if desired)
        Response.Headers.Add("X-Dashboard-Activity-View", "DashboardAdminActivity");
        Response.Headers.Add("X-Dashboard-DataQuality-View", "DashboardDataQuality");
        Response.Headers.Add("X-Dashboard-ApprovalAging-View", "DashboardApprovals");

        var wrapper = new
        {
            Summary = dto,
            Activity = adminAct,
            DataQuality = dataQuality,
            ApprovalAging = approvalAging,
            AcademicSummary = new AcademicSummaryDto(View: "DashboardAcademic", ByCourse: academic)
        };

        return Ok(ApiResponse<object>.Success(wrapper));
    }

    // ---------------------------------------------------------------------
    // Separated endpoints for frontend to request smaller payloads
    // ---------------------------------------------------------------------

    [HttpGet("admin/activity")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ActivitySummaryDto>>> AdminActivity()
    {
        var now = DateTime.UtcNow;
        var dau = await _userManager.Users.CountAsync(u => EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-1));
        var wau = await _userManager.Users.CountAsync(u => EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-7));
        var mau = await _userManager.Users.CountAsync(u => EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-30));

        var loginsByDay = await _userManager.Users
            .Where(u => EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-14))
            .GroupBy(u => EF.Property<DateTime>(u, "FechaRegistro").Date)
            .Select(g => new TimePointDto(g.Key, g.Count()))
            .OrderBy(tp => tp.Date)
            .ToListAsync();

        var dto = new ActivitySummaryDto(View: "DashboardAdminActivity", DAU: dau, WAU: wau, MAU: mau, LoginsByDay: loginsByDay);
        return Ok(ApiResponse<ActivitySummaryDto>.Success(dto));
    }

    [HttpGet("admin/data-quality")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<DataQualityDto>>> AdminDataQuality()
    {
        var personasSinUsuario = await _db.Personas.CountAsync(p => !_db.Users.Any(u => u.PersonaId == p.Id));
        var usuariosSinPersona = await _db.Users.CountAsync(u => u.PersonaId == null);

        // Users who have role Docente or JefeArea but no Docente row
        var targetRoleNames = new[] { "Docente", "JefeArea" };
        var roleIds = await _roleManager.Roles.Where(r => targetRoleNames.Contains(r.Name!)).Select(r => r.Id).ToListAsync();
        var userIdsWithRole = await _db.Set<Microsoft.AspNetCore.Identity.IdentityUserRole<string>>()
            .Where(ur => roleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync();

        var usuariosConRolesDocente = await _db.Users.Where(u => userIdsWithRole.Contains(u.Id) && u.PersonaId != null).ToListAsync();
        int docentesSinFicha = 0;
        foreach (var u in usuariosConRolesDocente)
        {
            if (!await _db.Docentes.AnyAsync(d => d.PersonaId == u.PersonaId)) docentesSinFicha++;
        }

        var usuariosSinEmail = await _db.Users.CountAsync(u => string.IsNullOrWhiteSpace(u.Email));
        var usuariosSinTelefono = await _db.Users.CountAsync(u => string.IsNullOrWhiteSpace(u.PhoneNumber));

        var now = DateTime.UtcNow;
        var pendingAging = new Dictionary<string,int>
        {
            ["0-3"] = await _userManager.Users.CountAsync(u => !u.IsApproved && EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-3)),
            ["4-7"] = await _userManager.Users.CountAsync(u => !u.IsApproved && EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") < now.AddDays(-3) && EF.Property<DateTime>(u, "FechaRegistro") >= now.AddDays(-7)),
            ["8+"] = await _userManager.Users.CountAsync(u => !u.IsApproved && EF.Property<DateTime?>(u, "FechaRegistro") != null && EF.Property<DateTime>(u, "FechaRegistro") < now.AddDays(-7))
        };

        var dto = new DataQualityDto(View: "DashboardDataQuality", PersonasSinUsuario: personasSinUsuario, UsuariosSinPersona: usuariosSinPersona, DocentesSinFicha: docentesSinFicha, UsuariosSinEmail: usuariosSinEmail, UsuariosSinTelefono: usuariosSinTelefono, PendingAgingBuckets: pendingAging);
        return Ok(ApiResponse<DataQualityDto>.Success(dto));
    }

    [HttpGet("admin/approval-aging")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ApiResponse<ApprovalAgingDto>>> AdminApprovalAging()
    {
        var approvedUsers = await _db.Users.Where(u => u.ApprovedAt != null).ToListAsync();
        double avgDays = 0;
        var buckets = new Dictionary<string,int> { ["0-3"] = 0, ["4-7"] = 0, ["8+"] = 0 };
        if (approvedUsers.Count > 0)
        {
            avgDays = approvedUsers.Average(u => (u.ApprovedAt - EF.Property<DateTime?>(u, "FechaRegistro"))?.TotalDays ?? 0);
            foreach (var u in approvedUsers)
            {
                var reg = EF.Property<DateTime?>(u, "FechaRegistro");
                if (reg is null || u.ApprovedAt is null) continue;
                var diff = (u.ApprovedAt.Value - reg.Value).TotalDays;
                if (diff <= 3) buckets["0-3"]++;
                else if (diff <= 7) buckets["4-7"]++;
                else buckets["8+"]++;
            }
        }

        var dto = new ApprovalAgingDto(View: "DashboardApprovals", AverageDaysToApprove: Math.Round(avgDays,2), Buckets: buckets);
        return Ok(ApiResponse<ApprovalAgingDto>.Success(dto));
    }

    [HttpGet("admin/academic")]
    [Authorize(Roles = "Admin,Direccion,Subdireccion,JefeArea")]
    public async Task<ActionResult<ApiResponse<AcademicSummaryDto>>> AdminAcademicSummary()
    {
        var academic = await _db.Calificaciones
            .AsNoTracking()
            .GroupBy(c => c.Curricula.DocenteNivelDetalleCurso.NivelDetalleCurso.CursoId)
            .Select(g => new CourseAcademicDto(
                g.Key,
                g.Select(c => c.Curricula.DocenteNivelDetalleCurso.NivelDetalleCurso.Curso.Descripcion).FirstOrDefault() ?? string.Empty,
                g.Average(c => (double?)c.Nota),
                g.Count(c => c.Nota >= 60) * 100.0 / Math.Max(1, g.Count())
            ))
            .ToListAsync();

        var dto = new AcademicSummaryDto(View: "DashboardAcademic", ByCourse: academic);
        return Ok(ApiResponse<AcademicSummaryDto>.Success(dto));
    }

    [HttpGet("direccion")]
    [Authorize(Roles = "Admin,Direccion,Subdireccion")]
    public async Task<ActionResult<ApiResponse<DireccionDashboardDto>>> DireccionSummary()
    {
        var totalMatriculas = await _db.Matriculas.CountAsync(m => m.Activo);

        var matriculasByNivel = await _db.Matriculas
            .AsNoTracking()
            .Include(m => m.NivelDetalle).ThenInclude(nd => nd.Nivel)
            .GroupBy(m => m.NivelDetalle.Nivel.DescripcionNivel)
            .Select(g => new MatriculaByNivelDto(g.Key ?? "(sin nivel)", g.Count()))
            .ToListAsync();

        var vacancias = await _db.NivelesDetalle
            .AsNoTracking()
            .Include(nd => nd.Nivel)
            .Include(nd => nd.GradoSeccion)
            .Where(nd => nd.Activo)
            .Select(nd => new VacancyDto(
                nd.Id,
                nd.Nivel.DescripcionNivel,
                nd.GradoSeccion.DescripcionGrado + " " + nd.GradoSeccion.DescripcionSeccion,
                nd.TotalVacantes,
                nd.VacantesOcupadas,
                nd.TotalVacantes.HasValue && nd.TotalVacantes.Value > 0 ? Math.Round((double)(nd.VacantesOcupadas ?? 0) * 100.0 / nd.TotalVacantes.Value, 2) : 0.0
            ))
            .ToListAsync();

        var dto = new DireccionDashboardDto(
            View: "DashboardDireccion",
            TotalMatriculas: totalMatriculas,
            MatriculasByNivel: matriculasByNivel,
            NivelDetalleVacancias: vacancias
        );

        return Ok(ApiResponse<DireccionDashboardDto>.Success(dto));
    }
}
