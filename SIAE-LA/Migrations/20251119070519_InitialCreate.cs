using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

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
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "curso",
                columns: table => new
                {
                    curso_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descripcion = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curso", x => x.curso_id);
                });

            migrationBuilder.CreateTable(
                name: "grado_seccion",
                columns: table => new
                {
                    grado_seccion_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descripcion_grado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    descripcion_seccion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_grado_seccion", x => x.grado_seccion_id);
                });

            migrationBuilder.CreateTable(
                name: "nivel",
                columns: table => new
                {
                    nivel_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descripcion_nivel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    descripcion_turno = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    horario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nivel", x => x.nivel_id);
                });

            migrationBuilder.CreateTable(
                name: "periodo",
                columns: table => new
                {
                    periodo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_periodo", x => x.periodo_id);
                });

            migrationBuilder.CreateTable(
                name: "persona",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    valor_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    documento_identidad = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    fecha_nacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    sexo = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    ciudad = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    direccion = table.Column<string>(type: "character varying(140)", maxLength: 140, nullable: true),
                    email = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    numero_telefono = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persona", x => x.Id);
                    table.CheckConstraint("CK_PERSONA_DOCID_FORMAT", "(\r\n                        documento_identidad ~ '^\\d{3}-\\d{6}-\\d{3,4}[A-Z]?$'\r\n                        OR documento_identidad ~ '^TUTOR-\\d{3}-\\d{6}-\\d{3,4}[A-Z]?$'\r\n                      )");
                    table.CheckConstraint("CK_PERSONA_SEXO", "sexo IN ('M','F','O')");
                    table.CheckConstraint("CK_PERSONA_TEL_NI", "(\r\n                        numero_telefono IS NULL\r\n                        OR numero_telefono ~ '^\\+505\\d{8}$'\r\n                        OR numero_telefono ~ '^\\d{8}$'\r\n                      )");
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
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
                name: "nivel_detalle",
                columns: table => new
                {
                    nivel_detalle_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nivel_id = table.Column<int>(type: "integer", nullable: false),
                    grado_seccion_id = table.Column<int>(type: "integer", nullable: false),
                    total_vacantes = table.Column<int>(type: "integer", nullable: true),
                    vacantes_ocupadas = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nivel_detalle", x => x.nivel_detalle_id);
                    table.ForeignKey(
                        name: "FK_nivel_detalle_grado_seccion_grado_seccion_id",
                        column: x => x.grado_seccion_id,
                        principalTable: "grado_seccion",
                        principalColumn: "grado_seccion_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nivel_detalle_nivel_nivel_id",
                        column: x => x.nivel_id,
                        principalTable: "nivel",
                        principalColumn: "nivel_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alumno",
                columns: table => new
                {
                    alumno_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alumno", x => x.alumno_id);
                    table.ForeignKey(
                        name: "FK_alumno_persona_persona_id",
                        column: x => x.persona_id,
                        principalTable: "persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "apoderado",
                columns: table => new
                {
                    apoderado_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    tipo_parentesco = table.Column<string>(type: "text", nullable: true),
                    estado_civil = table.Column<string>(type: "text", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_apoderado", x => x.apoderado_id);
                    table.ForeignKey(
                        name: "FK_apoderado_persona_persona_id",
                        column: x => x.persona_id,
                        principalTable: "persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "docente",
                columns: table => new
                {
                    docente_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    persona_id = table.Column<int>(type: "integer", nullable: false),
                    grado_estudio = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docente", x => x.docente_id);
                    table.ForeignKey(
                        name: "FK_docente_persona_persona_id",
                        column: x => x.persona_id,
                        principalTable: "persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    esta_aprobado = table.Column<bool>(type: "boolean", nullable: false),
                    aprobado_por = table.Column<string>(type: "text", nullable: true),
                    fecha_aprobacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    persona_id = table.Column<int>(type: "integer", nullable: true),
                    username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    es_email_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: true),
                    es_telefono_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuarios_persona_persona_id",
                        column: x => x.persona_id,
                        principalTable: "persona",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "nivel_detalle_curso",
                columns: table => new
                {
                    nivel_detalle_curso_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nivel_detalle_id = table.Column<int>(type: "integer", nullable: false),
                    curso_id = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nivel_detalle_curso", x => x.nivel_detalle_curso_id);
                    table.ForeignKey(
                        name: "FK_nivel_detalle_curso_curso_curso_id",
                        column: x => x.curso_id,
                        principalTable: "curso",
                        principalColumn: "curso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_nivel_detalle_curso_nivel_detalle_nivel_detalle_id",
                        column: x => x.nivel_detalle_id,
                        principalTable: "nivel_detalle",
                        principalColumn: "nivel_detalle_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "alumno_apoderado",
                columns: table => new
                {
                    alumno_apoderado_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AlumnoId = table.Column<int>(type: "integer", nullable: false),
                    ApoderadoId = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    es_responsable_legal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alumno_apoderado", x => x.alumno_apoderado_id);
                    table.ForeignKey(
                        name: "FK_alumno_apoderado_alumno_AlumnoId",
                        column: x => x.AlumnoId,
                        principalTable: "alumno",
                        principalColumn: "alumno_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_alumno_apoderado_apoderado_ApoderadoId",
                        column: x => x.ApoderadoId,
                        principalTable: "apoderado",
                        principalColumn: "apoderado_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "matricula",
                columns: table => new
                {
                    matricula_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    valor_codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    situacion = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    alumno_id = table.Column<int>(type: "integer", nullable: false),
                    nivel_detalle_id = table.Column<int>(type: "integer", nullable: false),
                    apoderado_id = table.Column<int>(type: "integer", nullable: true),
                    institucion_procedencia = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    es_repitente = table.Column<bool>(type: "boolean", nullable: true),
                    periodo_id = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matricula", x => x.matricula_id);
                    table.ForeignKey(
                        name: "FK_matricula_alumno_alumno_id",
                        column: x => x.alumno_id,
                        principalTable: "alumno",
                        principalColumn: "alumno_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matricula_apoderado_apoderado_id",
                        column: x => x.apoderado_id,
                        principalTable: "apoderado",
                        principalColumn: "apoderado_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_matricula_nivel_detalle_nivel_detalle_id",
                        column: x => x.nivel_detalle_id,
                        principalTable: "nivel_detalle",
                        principalColumn: "nivel_detalle_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_matricula_periodo_periodo_id",
                        column: x => x.periodo_id,
                        principalTable: "periodo",
                        principalColumn: "periodo_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
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
                        name: "FK_AspNetUserRoles_usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_usuarios_UserId",
                        column: x => x.UserId,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "docente_nivel_detalle_curso",
                columns: table => new
                {
                    docente_nivel_detalle_curso_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nivel_detalle_curso_id = table.Column<int>(type: "integer", nullable: false),
                    docente_id = table.Column<int>(type: "integer", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_docente_nivel_detalle_curso", x => x.docente_nivel_detalle_curso_id);
                    table.ForeignKey(
                        name: "FK_docente_nivel_detalle_curso_docente_docente_id",
                        column: x => x.docente_id,
                        principalTable: "docente",
                        principalColumn: "docente_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_docente_nivel_detalle_curso_nivel_detalle_curso_nivel_detal~",
                        column: x => x.nivel_detalle_curso_id,
                        principalTable: "nivel_detalle_curso",
                        principalColumn: "nivel_detalle_curso_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "horario",
                columns: table => new
                {
                    horario_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nivel_detalle_curso_id = table.Column<int>(type: "integer", nullable: false),
                    dia_semana = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    hora_inicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    hora_fin = table.Column<TimeSpan>(type: "interval", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_horario", x => x.horario_id);
                    table.ForeignKey(
                        name: "FK_horario_nivel_detalle_curso_nivel_detalle_curso_id",
                        column: x => x.nivel_detalle_curso_id,
                        principalTable: "nivel_detalle_curso",
                        principalColumn: "nivel_detalle_curso_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "curricula",
                columns: table => new
                {
                    curricula_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    docente_nivel_detalle_curso_id = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NivelDetalleCursoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curricula", x => x.curricula_id);
                    table.ForeignKey(
                        name: "FK_curricula_docente_nivel_detalle_curso_docente_nivel_detalle~",
                        column: x => x.docente_nivel_detalle_curso_id,
                        principalTable: "docente_nivel_detalle_curso",
                        principalColumn: "docente_nivel_detalle_curso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_curricula_nivel_detalle_curso_NivelDetalleCursoId",
                        column: x => x.NivelDetalleCursoId,
                        principalTable: "nivel_detalle_curso",
                        principalColumn: "nivel_detalle_curso_id");
                });

            migrationBuilder.CreateTable(
                name: "calificacion",
                columns: table => new
                {
                    calificacion_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    curricula_id = table.Column<int>(type: "integer", nullable: false),
                    alumno_id = table.Column<int>(type: "integer", nullable: false),
                    nota = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_calificacion", x => x.calificacion_id);
                    table.CheckConstraint("ck_calificacion_nota", "nota >= 0 AND nota <= 100");
                    table.ForeignKey(
                        name: "FK_calificacion_alumno_alumno_id",
                        column: x => x.alumno_id,
                        principalTable: "alumno",
                        principalColumn: "alumno_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_calificacion_curricula_curricula_id",
                        column: x => x.curricula_id,
                        principalTable: "curricula",
                        principalColumn: "curricula_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alumno_persona_id",
                table: "alumno",
                column: "persona_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ALUMNO_APODERADO_ALUMNO_FECHAFIN",
                table: "alumno_apoderado",
                columns: new[] { "AlumnoId", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "IX_alumno_apoderado_AlumnoId",
                table: "alumno_apoderado",
                column: "AlumnoId");

            migrationBuilder.CreateIndex(
                name: "IX_alumno_apoderado_ApoderadoId",
                table: "alumno_apoderado",
                column: "ApoderadoId");

            migrationBuilder.CreateIndex(
                name: "IX_apoderado_persona_id",
                table: "apoderado",
                column: "persona_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

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
                name: "IX_calificacion_alumno_id",
                table: "calificacion",
                column: "alumno_id");

            migrationBuilder.CreateIndex(
                name: "ux_calificacion_curricula_alumno",
                table: "calificacion",
                columns: new[] { "curricula_id", "alumno_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_curricula_docente_nivel_detalle_curso_id",
                table: "curricula",
                column: "docente_nivel_detalle_curso_id");

            migrationBuilder.CreateIndex(
                name: "IX_curricula_NivelDetalleCursoId",
                table: "curricula",
                column: "NivelDetalleCursoId");

            migrationBuilder.CreateIndex(
                name: "ix_curso_codigo",
                table: "curso",
                column: "codigo");

            migrationBuilder.CreateIndex(
                name: "IX_docente_persona_id",
                table: "docente",
                column: "persona_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_docente_nivel_detalle_curso_docente_id",
                table: "docente_nivel_detalle_curso",
                column: "docente_id");

            migrationBuilder.CreateIndex(
                name: "IX_docente_nivel_detalle_curso_nivel_detalle_curso_id_docente_~",
                table: "docente_nivel_detalle_curso",
                columns: new[] { "nivel_detalle_curso_id", "docente_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_horario_nivel_detalle_curso_id",
                table: "horario",
                column: "nivel_detalle_curso_id");

            migrationBuilder.CreateIndex(
                name: "IX_matricula_alumno_id_nivel_detalle_id_periodo_id",
                table: "matricula",
                columns: new[] { "alumno_id", "nivel_detalle_id", "periodo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_matricula_apoderado_id",
                table: "matricula",
                column: "apoderado_id");

            migrationBuilder.CreateIndex(
                name: "IX_matricula_nivel_detalle_id",
                table: "matricula",
                column: "nivel_detalle_id");

            migrationBuilder.CreateIndex(
                name: "IX_matricula_periodo_id",
                table: "matricula",
                column: "periodo_id");

            migrationBuilder.CreateIndex(
                name: "IX_nivel_detalle_grado_seccion_id",
                table: "nivel_detalle",
                column: "grado_seccion_id");

            migrationBuilder.CreateIndex(
                name: "IX_nivel_detalle_nivel_id_grado_seccion_id",
                table: "nivel_detalle",
                columns: new[] { "nivel_id", "grado_seccion_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_nivel_detalle_curso_curso_id",
                table: "nivel_detalle_curso",
                column: "curso_id");

            migrationBuilder.CreateIndex(
                name: "IX_nivel_detalle_curso_nivel_detalle_id_curso_id",
                table: "nivel_detalle_curso",
                columns: new[] { "nivel_detalle_id", "curso_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_persona_documento_identidad",
                table: "persona",
                column: "documento_identidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "usuarios",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_persona_id",
                table: "usuarios",
                column: "persona_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "usuarios",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alumno_apoderado");

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
                name: "calificacion");

            migrationBuilder.DropTable(
                name: "horario");

            migrationBuilder.DropTable(
                name: "matricula");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "curricula");

            migrationBuilder.DropTable(
                name: "alumno");

            migrationBuilder.DropTable(
                name: "apoderado");

            migrationBuilder.DropTable(
                name: "periodo");

            migrationBuilder.DropTable(
                name: "docente_nivel_detalle_curso");

            migrationBuilder.DropTable(
                name: "docente");

            migrationBuilder.DropTable(
                name: "nivel_detalle_curso");

            migrationBuilder.DropTable(
                name: "persona");

            migrationBuilder.DropTable(
                name: "curso");

            migrationBuilder.DropTable(
                name: "nivel_detalle");

            migrationBuilder.DropTable(
                name: "grado_seccion");

            migrationBuilder.DropTable(
                name: "nivel");
        }
    }
}
