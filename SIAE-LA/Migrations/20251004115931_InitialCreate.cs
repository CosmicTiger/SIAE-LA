using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SIAE_LA.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CURSO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURSO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GRADO_SECCION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescripcionGrado = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    DescripcionSeccion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRADO_SECCION", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NIVEL",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DescripcionNivel = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    DescripcionTurno = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Horario = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NIVEL", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PERIODO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Descripcion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERIODO", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PERSONA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValorCodigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DocumentoIdentidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    FechaNacimiento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Sexo = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: false),
                    Ciudad = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(140)", maxLength: 140, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    NumeroTelefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PERSONA", x => x.Id);
                    table.CheckConstraint("CK_PERSONA_DOCID_FORMAT", "(DocumentoIdentidad LIKE '[0-9][0-9][0-9]-[0-3][0-9][0-1][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][A-Z]'  OR DocumentoIdentidad LIKE 'TUTOR-[0-9][0-9][0-9]-[0-3][0-9][0-1][0-9][0-9][0-9]-[0-9][0-9][0-9][0-9][A-Z]')");
                    table.CheckConstraint("CK_PERSONA_SEXO", "Sexo IN ('M','F')");
                    table.CheckConstraint("CK_PERSONA_TEL_NI", "NumeroTelefono IS NULL OR NumeroTelefono LIKE '________'");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NIVEL_DETALLE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NivelId = table.Column<int>(type: "int", nullable: false),
                    GradoSeccionId = table.Column<int>(type: "int", nullable: false),
                    TotalVacantes = table.Column<int>(type: "int", nullable: true),
                    VacantesOcupadas = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NIVEL_DETALLE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NIVEL_DETALLE_GRADO_SECCION_GradoSeccionId",
                        column: x => x.GradoSeccionId,
                        principalTable: "GRADO_SECCION",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NIVEL_DETALLE_NIVEL_NivelId",
                        column: x => x.NivelId,
                        principalTable: "NIVEL",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ALUMNO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ALUMNO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ALUMNO_PERSONA_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "PERSONA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "APODERADO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    TipoParentesco = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EstadoCivil = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_APODERADO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_APODERADO_PERSONA_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "PERSONA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PersonaId = table.Column<int>(type: "int", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_PERSONA_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "PERSONA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DOCENTE",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonaId = table.Column<int>(type: "int", nullable: false),
                    GradoEstudio = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOCENTE", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DOCENTE_PERSONA_PersonaId",
                        column: x => x.PersonaId,
                        principalTable: "PERSONA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NIVEL_DETALLE_CURSO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NivelDetalleId = table.Column<int>(type: "int", nullable: false),
                    CursoId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NIVEL_DETALLE_CURSO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NIVEL_DETALLE_CURSO_CURSO_CursoId",
                        column: x => x.CursoId,
                        principalTable: "CURSO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NIVEL_DETALLE_CURSO_NIVEL_DETALLE_NivelDetalleId",
                        column: x => x.NivelDetalleId,
                        principalTable: "NIVEL_DETALLE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MATRICULA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ValorCodigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Situacion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: true),
                    AlumnoId = table.Column<int>(type: "int", nullable: false),
                    NivelDetalleId = table.Column<int>(type: "int", nullable: false),
                    ApoderadoId = table.Column<int>(type: "int", nullable: true),
                    InstitucionProcedencia = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    EsRepetente = table.Column<bool>(type: "bit", nullable: true),
                    PeriodoId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MATRICULA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MATRICULA_ALUMNO_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "ALUMNO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MATRICULA_APODERADO_ApoderadoId",
                        column: x => x.ApoderadoId,
                        principalTable: "APODERADO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MATRICULA_NIVEL_DETALLE_NivelDetalleId",
                        column: x => x.NivelDetalleId,
                        principalTable: "NIVEL_DETALLE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MATRICULA_PERIODO_PeriodoId",
                        column: x => x.PeriodoId,
                        principalTable: "PERIODO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DOCENTE_NIVELDETALLE_CURSO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NivelDetalleCursoId = table.Column<int>(type: "int", nullable: false),
                    DocenteId = table.Column<int>(type: "int", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOCENTE_NIVELDETALLE_CURSO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DOCENTE_NIVELDETALLE_CURSO_DOCENTE_DocenteId",
                        column: x => x.DocenteId,
                        principalTable: "DOCENTE",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DOCENTE_NIVELDETALLE_CURSO_NIVEL_DETALLE_CURSO_NivelDetalleCursoId",
                        column: x => x.NivelDetalleCursoId,
                        principalTable: "NIVEL_DETALLE_CURSO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HORARIO",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NivelDetalleCursoId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraFin = table.Column<TimeSpan>(type: "time", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HORARIO", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HORARIO_NIVEL_DETALLE_CURSO_NivelDetalleCursoId",
                        column: x => x.NivelDetalleCursoId,
                        principalTable: "NIVEL_DETALLE_CURSO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CURRICULA",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocenteNivelDetalleCursoId = table.Column<int>(type: "int", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NivelDetalleCursoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CURRICULA", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CURRICULA_DOCENTE_NIVELDETALLE_CURSO_DocenteNivelDetalleCursoId",
                        column: x => x.DocenteNivelDetalleCursoId,
                        principalTable: "DOCENTE_NIVELDETALLE_CURSO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CURRICULA_NIVEL_DETALLE_CURSO_NivelDetalleCursoId",
                        column: x => x.NivelDetalleCursoId,
                        principalTable: "NIVEL_DETALLE_CURSO",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CALIFICACION",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurriculaId = table.Column<int>(type: "int", nullable: false),
                    AlumnoId = table.Column<int>(type: "int", nullable: false),
                    Nota = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CALIFICACION", x => x.Id);
                    table.CheckConstraint("CK_Calificacion_Nota", "[Nota]>=0 AND [Nota]<=100");
                    table.ForeignKey(
                        name: "FK_CALIFICACION_ALUMNO_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "ALUMNO",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CALIFICACION_CURRICULA_CurriculaId",
                        column: x => x.CurriculaId,
                        principalTable: "CURRICULA",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ALUMNO_PersonaId",
                table: "ALUMNO",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_APODERADO_PersonaId",
                table: "APODERADO",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PersonaId",
                table: "AspNetUsers",
                column: "PersonaId",
                unique: true,
                filter: "[PersonaId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CALIFICACION_AlumnoId",
                table: "CALIFICACION",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_CALIFICACION_CurriculaId_AlumnoId",
                table: "CALIFICACION",
                columns: new[] { "CurriculaId", "AlumnoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CURRICULA_DocenteNivelDetalleCursoId",
                table: "CURRICULA",
                column: "DocenteNivelDetalleCursoId");

            migrationBuilder.CreateIndex(
                name: "IX_CURRICULA_NivelDetalleCursoId",
                table: "CURRICULA",
                column: "NivelDetalleCursoId");

            migrationBuilder.CreateIndex(
                name: "IX_CURSO_Codigo",
                table: "CURSO",
                column: "Codigo");

            migrationBuilder.CreateIndex(
                name: "IX_DOCENTE_PersonaId",
                table: "DOCENTE",
                column: "PersonaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DOCENTE_NIVELDETALLE_CURSO_DocenteId",
                table: "DOCENTE_NIVELDETALLE_CURSO",
                column: "DocenteId");

            migrationBuilder.CreateIndex(
                name: "IX_DOCENTE_NIVELDETALLE_CURSO_NivelDetalleCursoId_DocenteId",
                table: "DOCENTE_NIVELDETALLE_CURSO",
                columns: new[] { "NivelDetalleCursoId", "DocenteId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HORARIO_NivelDetalleCursoId",
                table: "HORARIO",
                column: "NivelDetalleCursoId");

            migrationBuilder.CreateIndex(
                name: "IX_MATRICULA_AlumnoId_NivelDetalleId_PeriodoId",
                table: "MATRICULA",
                columns: new[] { "AlumnoId", "NivelDetalleId", "PeriodoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MATRICULA_ApoderadoId",
                table: "MATRICULA",
                column: "ApoderadoId");

            migrationBuilder.CreateIndex(
                name: "IX_MATRICULA_NivelDetalleId",
                table: "MATRICULA",
                column: "NivelDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_MATRICULA_PeriodoId",
                table: "MATRICULA",
                column: "PeriodoId");

            migrationBuilder.CreateIndex(
                name: "IX_NIVEL_DETALLE_GradoSeccionId",
                table: "NIVEL_DETALLE",
                column: "GradoSeccionId");

            migrationBuilder.CreateIndex(
                name: "IX_NIVEL_DETALLE_NivelId_GradoSeccionId",
                table: "NIVEL_DETALLE",
                columns: new[] { "NivelId", "GradoSeccionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NIVEL_DETALLE_CURSO_CursoId",
                table: "NIVEL_DETALLE_CURSO",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_NIVEL_DETALLE_CURSO_NivelDetalleId_CursoId",
                table: "NIVEL_DETALLE_CURSO",
                columns: new[] { "NivelDetalleId", "CursoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PERSONA_DocumentoIdentidad",
                table: "PERSONA",
                column: "DocumentoIdentidad",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "CALIFICACION");

            migrationBuilder.DropTable(
                name: "HORARIO");

            migrationBuilder.DropTable(
                name: "MATRICULA");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CURRICULA");

            migrationBuilder.DropTable(
                name: "ALUMNO");

            migrationBuilder.DropTable(
                name: "APODERADO");

            migrationBuilder.DropTable(
                name: "PERIODO");

            migrationBuilder.DropTable(
                name: "DOCENTE_NIVELDETALLE_CURSO");

            migrationBuilder.DropTable(
                name: "DOCENTE");

            migrationBuilder.DropTable(
                name: "NIVEL_DETALLE_CURSO");

            migrationBuilder.DropTable(
                name: "PERSONA");

            migrationBuilder.DropTable(
                name: "CURSO");

            migrationBuilder.DropTable(
                name: "NIVEL_DETALLE");

            migrationBuilder.DropTable(
                name: "GRADO_SECCION");

            migrationBuilder.DropTable(
                name: "NIVEL");
        }
    }
}
