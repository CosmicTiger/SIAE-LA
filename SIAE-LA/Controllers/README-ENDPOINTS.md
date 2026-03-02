# Lista de Endpoints de API

GET /health/db → { canConnect: true }


GET /api/Alumnos?search=&page=&pageSize= → listado paginado
GET /api/Alumnos/{id} → detalle
POST /api/Alumnos → crear (AlumnoCreateDto)
PUT /api/Alumnos/{id} → actualizar (AlumnoUpdateDto)
DELETE /api/Alumnos/{id} → desactivar (soft)


GET /api/Cursos?search=&page=&pageSize= → listado
POST /api/Cursos → crear
PUT /api/Cursos/{id} → actualizar
DELETE /api/Cursos/{id} → desactivar (soft)


POST /api/Matriculas → matricular (MatriculaCreateDto)
GET /api/Matriculas/by-alumno/{alumnoId} → matrículas por alumno (opcional ?periodoId=, ahora también acepta ?anioLectivoId=; `anioLectivoId` tiene preferencia si se proporciona)
GET /api/Matriculas/by-nivel-detalle/{id} → matrículas por nivelDetalle (opcional ?periodoId=, ahora también acepta ?anioLectivoId=; `anioLectivoId` tiene preferencia si se proporciona)


POST /api/Calificaciones → crear (CalificacionCreateDto)
PUT /api/Calificaciones/{id} → editar (CalificacionUpdateDto)
GET /api/Calificaciones/by-alumno/{alumnoId} → por alumno (opcional ?periodoId=, ahora también acepta ?anioLectivoId=; `anioLectivoId` tiene preferencia si se proporciona)


GET /api/Periodos → lista
POST /api/Periodos → crear


Notas:
- Agrega [Authorize] cuando actives autenticación para proteger módulos.
- CORS: permite http://localhost:4200 para Angular.
- Si quieres DTOs y controladores para Niveles/GradoSeccion/NivelDetalle/NivelDetalleCurso y Docente/Asignaciones/Currícula, puedo generarlos igual que los anteriores.