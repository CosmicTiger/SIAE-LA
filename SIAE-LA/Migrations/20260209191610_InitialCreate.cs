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
                name: "anio_lectivo",
                columns: table => new
                {
                    anio_lectivo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    anio = table.Column<int>(type: "integer", nullable: false),
                    descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_anio_lectivo", x => x.anio_lectivo_id);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_roles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_asp_net_roles", x => x.id);
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_curso", x => x.curso_id);
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_grado_seccion", x => x.grado_seccion_id);
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_nivel", x => x.nivel_id);
                });

            migrationBuilder.CreateTable(
                name: "persona",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_persona", x => x.id);
                    table.CheckConstraint("CK_PERSONA_DOCID_FORMAT", "(\r\n                        documento_identidad ~ '^\\d{3}-\\d{6}-\\d{3,4}[A-Z]?$'\r\n                        OR documento_identidad ~ '^TUTOR-\\d{3}-\\d{6}-\\d{3,4}[A-Z]?$'\r\n                      )");
                    table.CheckConstraint("CK_PERSONA_SEXO", "sexo IN ('M','F','O')");
                    table.CheckConstraint("CK_PERSONA_TEL_NI", "(\r\n                        numero_telefono IS NULL\r\n                        OR numero_telefono ~ '^\\+505\\d{8}$'\r\n                        OR numero_telefono ~ '^\\d{8}$'\r\n                      )");
                });

            migrationBuilder.CreateTable(
                name: "periodo",
                columns: table => new
                {
                    periodo_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    descripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    anio_lectivo_id = table.Column<int>(type: "integer", nullable: true),
                    orden = table.Column<int>(type: "integer", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_periodo", x => x.periodo_id);
                    table.ForeignKey(
                        name: "f_k_periodo_anio_lectivo_anio_lectivo_id",
                        column: x => x.anio_lectivo_id,
                        principalTable: "anio_lectivo",
                        principalColumn: "anio_lectivo_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_role_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_asp_net_role_claims", x => x.id);
                    table.ForeignKey(
                        name: "f_k_asp_net_role_claims_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_nivel_detalle", x => x.nivel_detalle_id);
                    table.ForeignKey(
                        name: "f_k_nivel_detalle_grado_seccion_grado_seccion_id",
                        column: x => x.grado_seccion_id,
                        principalTable: "grado_seccion",
                        principalColumn: "grado_seccion_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_nivel_detalle_nivel_nivel_id",
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_alumno", x => x.alumno_id);
                    table.ForeignKey(
                        name: "f_k_alumno_persona_persona_id",
                        column: x => x.persona_id,
                        principalTable: "persona",
                        principalColumn: "id",
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_apoderado", x => x.apoderado_id);
                    table.ForeignKey(
                        name: "f_k_apoderado_persona_persona_id",
                        column: x => x.persona_id,
                        principalTable: "persona",
                        principalColumn: "id",
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_docente", x => x.docente_id);
                    table.ForeignKey(
                        name: "f_k_docente_persona_persona_id",
                        column: x => x.persona_id,
                        principalTable: "persona",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    full_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    esta_aprobado = table.Column<bool>(type: "boolean", nullable: false),
                    aprobado_por = table.Column<string>(type: "text", nullable: true),
                    fecha_aprobacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    persona_id = table.Column<int>(type: "integer", nullable: true),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_user_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalized_email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    es_email_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    telefono = table.Column<string>(type: "text", nullable: true),
                    es_telefono_confirmado = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_usuarios", x => x.id);
                    table.ForeignKey(
                        name: "f_k_usuarios_persona_persona_id",
                        column: x => x.persona_id,
                        principalTable: "persona",
                        principalColumn: "id",
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_nivel_detalle_curso", x => x.nivel_detalle_curso_id);
                    table.ForeignKey(
                        name: "f_k_nivel_detalle_curso_curso_curso_id",
                        column: x => x.curso_id,
                        principalTable: "curso",
                        principalColumn: "curso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_nivel_detalle_curso_nivel_detalle_nivel_detalle_id",
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
                    alumno_id = table.Column<int>(type: "integer", nullable: false),
                    apoderado_id = table.Column<int>(type: "integer", nullable: false),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    es_responsable_legal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_alumno_apoderado", x => x.alumno_apoderado_id);
                    table.ForeignKey(
                        name: "f_k_alumno_apoderado_alumno_alumno_id",
                        column: x => x.alumno_id,
                        principalTable: "alumno",
                        principalColumn: "alumno_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_alumno_apoderado_apoderado_apoderado_id",
                        column: x => x.apoderado_id,
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
                    anio_lectivo_id = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_matricula", x => x.matricula_id);
                    table.ForeignKey(
                        name: "f_k_matricula_alumno_alumno_id",
                        column: x => x.alumno_id,
                        principalTable: "alumno",
                        principalColumn: "alumno_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_matricula_anio_lectivo_anio_lectivo_id",
                        column: x => x.anio_lectivo_id,
                        principalTable: "anio_lectivo",
                        principalColumn: "anio_lectivo_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_matricula_apoderado_apoderado_id",
                        column: x => x.apoderado_id,
                        principalTable: "apoderado",
                        principalColumn: "apoderado_id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "f_k_matricula_nivel_detalle_nivel_detalle_id",
                        column: x => x.nivel_detalle_id,
                        principalTable: "nivel_detalle",
                        principalColumn: "nivel_detalle_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_claims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    claim_type = table.Column<string>(type: "text", nullable: true),
                    claim_value = table.Column<string>(type: "text", nullable: true),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_asp_net_user_claims", x => x.id);
                    table.ForeignKey(
                        name: "f_k_asp_net_user_claims_usuarios_user_id",
                        column: x => x.user_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_logins",
                columns: table => new
                {
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    provider_key = table.Column<string>(type: "text", nullable: false),
                    provider_display_name = table.Column<string>(type: "text", nullable: true),
                    user_id = table.Column<string>(type: "text", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_asp_net_user_logins", x => new { x.login_provider, x.provider_key });
                    table.ForeignKey(
                        name: "f_k_asp_net_user_logins_usuarios_user_id",
                        column: x => x.user_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_roles",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_asp_net_user_roles", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "f_k_asp_net_user_roles_asp_net_roles_role_id",
                        column: x => x.role_id,
                        principalTable: "asp_net_roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_asp_net_user_roles_usuarios_user_id",
                        column: x => x.user_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "asp_net_user_tokens",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "text", nullable: false),
                    login_provider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_asp_net_user_tokens", x => new { x.user_id, x.login_provider, x.name });
                    table.ForeignKey(
                        name: "f_k_asp_net_user_tokens_usuarios_user_id",
                        column: x => x.user_id,
                        principalTable: "usuarios",
                        principalColumn: "id",
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_docente_nivel_detalle_curso", x => x.docente_nivel_detalle_curso_id);
                    table.ForeignKey(
                        name: "f_k_docente_nivel_detalle_curso_docente_docente_id",
                        column: x => x.docente_id,
                        principalTable: "docente",
                        principalColumn: "docente_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_docente_nivel_detalle_curso_nivel_detalle_curso_nivel_detal~",
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_horario", x => x.horario_id);
                    table.ForeignKey(
                        name: "f_k_horario_nivel_detalle_curso_nivel_detalle_curso_id",
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
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    nivel_detalle_curso_id = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_curricula", x => x.curricula_id);
                    table.ForeignKey(
                        name: "f_k_curricula_docente_nivel_detalle_curso_docente_nivel_detalle~",
                        column: x => x.docente_nivel_detalle_curso_id,
                        principalTable: "docente_nivel_detalle_curso",
                        principalColumn: "docente_nivel_detalle_curso_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_curricula_nivel_detalle_curso_nivel_detalle_curso_id",
                        column: x => x.nivel_detalle_curso_id,
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
                    periodo_id = table.Column<int>(type: "integer", nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    fecha_registro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    creado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true),
                    fecha_modificacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    modificado_por = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_calificacion", x => x.calificacion_id);
                    table.CheckConstraint("ck_calificacion_nota", "nota >= 0 AND nota <= 100");
                    table.ForeignKey(
                        name: "f_k_calificacion_alumno_alumno_id",
                        column: x => x.alumno_id,
                        principalTable: "alumno",
                        principalColumn: "alumno_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_calificacion_curricula_curricula_id",
                        column: x => x.curricula_id,
                        principalTable: "curricula",
                        principalColumn: "curricula_id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "f_k_calificacion_periodo_periodo_id",
                        column: x => x.periodo_id,
                        principalTable: "periodo",
                        principalColumn: "periodo_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "i_x_alumno_persona_id",
                table: "alumno",
                column: "persona_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x__a_l_u_m_n_o__a_p_o_d_e_r_a_d_o__a_l_u_m_n_o__f_e_c_h_a_f_i_n",
                table: "alumno_apoderado",
                columns: new[] { "alumno_id", "fecha_fin" });

            migrationBuilder.CreateIndex(
                name: "i_x_alumno_apoderado_alumno_id",
                table: "alumno_apoderado",
                column: "alumno_id");

            migrationBuilder.CreateIndex(
                name: "i_x_alumno_apoderado_apoderado_id",
                table: "alumno_apoderado",
                column: "apoderado_id");

            migrationBuilder.CreateIndex(
                name: "i_x_apoderado_persona_id",
                table: "apoderado",
                column: "persona_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_asp_net_role_claims_role_id",
                table: "asp_net_role_claims",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "role_name_index",
                table: "asp_net_roles",
                column: "normalized_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_asp_net_user_claims_user_id",
                table: "asp_net_user_claims",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_asp_net_user_logins_user_id",
                table: "asp_net_user_logins",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "i_x_asp_net_user_roles_role_id",
                table: "asp_net_user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "i_x_calificacion_alumno_id",
                table: "calificacion",
                column: "alumno_id");

            migrationBuilder.CreateIndex(
                name: "i_x_calificacion_periodo_id",
                table: "calificacion",
                column: "periodo_id");

            migrationBuilder.CreateIndex(
                name: "ux_calificacion_curricula_alumno",
                table: "calificacion",
                columns: new[] { "curricula_id", "alumno_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_curricula_docente_nivel_detalle_curso_id",
                table: "curricula",
                column: "docente_nivel_detalle_curso_id");

            migrationBuilder.CreateIndex(
                name: "i_x_curricula_nivel_detalle_curso_id",
                table: "curricula",
                column: "nivel_detalle_curso_id");

            migrationBuilder.CreateIndex(
                name: "ix_curso_codigo",
                table: "curso",
                column: "codigo");

            migrationBuilder.CreateIndex(
                name: "i_x_docente_persona_id",
                table: "docente",
                column: "persona_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_docente_nivel_detalle_curso_docente_id",
                table: "docente_nivel_detalle_curso",
                column: "docente_id");

            migrationBuilder.CreateIndex(
                name: "i_x_docente_nivel_detalle_curso_nivel_detalle_curso_id_docente_~",
                table: "docente_nivel_detalle_curso",
                columns: new[] { "nivel_detalle_curso_id", "docente_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_horario_nivel_detalle_curso_id",
                table: "horario",
                column: "nivel_detalle_curso_id");

            migrationBuilder.CreateIndex(
                name: "i_x_matricula_alumno_id_nivel_detalle_id_anio_lectivo_id",
                table: "matricula",
                columns: new[] { "alumno_id", "nivel_detalle_id", "anio_lectivo_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_matricula_anio_lectivo_id",
                table: "matricula",
                column: "anio_lectivo_id");

            migrationBuilder.CreateIndex(
                name: "i_x_matricula_apoderado_id",
                table: "matricula",
                column: "apoderado_id");

            migrationBuilder.CreateIndex(
                name: "i_x_matricula_nivel_detalle_id",
                table: "matricula",
                column: "nivel_detalle_id");

            migrationBuilder.CreateIndex(
                name: "i_x_nivel_detalle_grado_seccion_id",
                table: "nivel_detalle",
                column: "grado_seccion_id");

            migrationBuilder.CreateIndex(
                name: "i_x_nivel_detalle_nivel_id_grado_seccion_id",
                table: "nivel_detalle",
                columns: new[] { "nivel_id", "grado_seccion_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "i_x_nivel_detalle_curso_curso_id",
                table: "nivel_detalle_curso",
                column: "curso_id");

            migrationBuilder.CreateIndex(
                name: "i_x_nivel_detalle_curso_nivel_detalle_id_curso_id",
                table: "nivel_detalle_curso",
                columns: new[] { "nivel_detalle_id", "curso_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_periodo_anio_orden",
                table: "periodo",
                columns: new[] { "anio_lectivo_id", "orden" });

            migrationBuilder.CreateIndex(
                name: "i_x_persona_documento_identidad",
                table: "persona",
                column: "documento_identidad",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "email_index",
                table: "usuarios",
                column: "normalized_email");

            migrationBuilder.CreateIndex(
                name: "i_x_usuarios_persona_id",
                table: "usuarios",
                column: "persona_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "user_name_index",
                table: "usuarios",
                column: "normalized_user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "alumno_apoderado");

            migrationBuilder.DropTable(
                name: "asp_net_role_claims");

            migrationBuilder.DropTable(
                name: "asp_net_user_claims");

            migrationBuilder.DropTable(
                name: "asp_net_user_logins");

            migrationBuilder.DropTable(
                name: "asp_net_user_roles");

            migrationBuilder.DropTable(
                name: "asp_net_user_tokens");

            migrationBuilder.DropTable(
                name: "calificacion");

            migrationBuilder.DropTable(
                name: "horario");

            migrationBuilder.DropTable(
                name: "matricula");

            migrationBuilder.DropTable(
                name: "asp_net_roles");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "curricula");

            migrationBuilder.DropTable(
                name: "periodo");

            migrationBuilder.DropTable(
                name: "alumno");

            migrationBuilder.DropTable(
                name: "apoderado");

            migrationBuilder.DropTable(
                name: "docente_nivel_detalle_curso");

            migrationBuilder.DropTable(
                name: "anio_lectivo");

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
