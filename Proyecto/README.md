# Portal de Microcursos y Tutoriales

## Integrantes del equipo

- **Armando Michel Arce**
- **Reyly Anze**

---

## Descripción del proyecto

El Portal de Microcursos y Tutoriales es una aplicación web desarrollada en ASP.NET Core MVC con Entity Framework Core y ASP.NET Identity. Su objetivo es ofrecer un espacio donde los usuarios puedan explorar un catálogo de cursos, inscribirse en los que les interesen y consumir el contenido de sus lecciones de forma ordenada.

El sistema distingue dos tipos de usuario:

- **Administrador:** puede crear, editar, eliminar y publicar/despublicar cursos; gestionar las lecciones de cada curso (incluyendo cambiar su orden); y consultar la lista de usuarios inscritos en cada curso.
- **Usuario registrado:** puede ver el catálogo de cursos publicados, inscribirse en un curso y acceder al contenido de sus lecciones únicamente si está inscrito.

La base de datos maneja cinco entidades principales: `Curso`, `Leccion`, `Inscripcion`, `ApplicationUser` y `LeccionCompletada`. Las relaciones son:
- Un `Curso` tiene muchas `Lecciones` e `Inscripciones`.
- Una `Inscripcion` vincula a un `ApplicationUser` con un `Curso`.

---

## Justificación de la arquitectura: MVC vs Razor Pages

Elegimos el patrón **MVC (Model-View-Controller)** en lugar de Razor Pages por las siguientes razones:

### 1. Separación clara de responsabilidades
Nuestro proyecto tiene múltiples entidades (Cursos, Lecciones, Inscripciones) con lógica de negocio diferente en cada una. MVC nos permite tener un controlador dedicado por entidad (`CursosController`, `LeccionesController`, `InscripcionesController`, `AdminController`), lo que mantiene el código organizado y fácil de mantener.

### 2. Área de administración con lógica concentrada
El `AdminController` agrupa toda la funcionalidad del panel de administración: CRUD de cursos, gestión de lecciones con reordenamiento y consulta de inscritos. Con MVC, toda esta lógica queda en un solo controlador decorado con `[Authorize(Roles = "Administrador")]`, controlando el acceso de forma centralizada.

### 3. Rutas más expresivas
MVC genera rutas del tipo `/Admin/CrearCurso`, `/Admin/Lecciones/5`, `/Cursos/Details/3` que describen la acción y el recurso de forma natural. Razor Pages produciría rutas basadas en el nombre del archivo de página, lo cual resulta menos intuitivo para un portal con múltiples secciones.

### 4. Reutilización de vistas y lógica compartida
Con MVC podemos pasar diferentes modelos a una misma vista o compartir parciales entre controladores con facilidad, algo útil por ejemplo para el catálogo de cursos que es visible tanto para usuarios autenticados como para visitantes anónimos.

### Conclusión
Razor Pages es una buena opción para aplicaciones simples con páginas CRUD independientes. Sin embargo, nuestro portal tiene interacciones entre entidades, control de roles y flujos de negocio más complejos (inscripción → acceso a lección → validación de inscripción) que se gestionan de forma más clara y mantenible con la arquitectura MVC.

---

## Control de acceso a las lecciones

Cuando un usuario solicita ver una lección (acción `Details` del `LeccionesController`), el sistema debe comprobar si ese usuario está inscrito en el curso al que pertenece esa lección antes de mostrarle el contenido.

El flujo es el siguiente:

1. Se recibe el `id` de la lección solicitada.
2. Se consulta la base de datos para obtener la lección junto con su curso (`Include(l => l.Curso)`).
3. Se obtiene el identificador del usuario autenticado usando `User.FindFirstValue(ClaimTypes.NameIdentifier)`.
4. Se consulta la tabla `Inscripciones` para verificar si existe un registro donde `UsuarioId` coincida con el usuario actual **y** `CursoId` coincida con el curso de esa lección:

```csharp
var inscripcion = await _context.Inscripciones
    .FirstOrDefaultAsync(i => i.UsuarioId == usuarioId
                            && i.CursoId == leccion.CursoId);
```

5. Si la inscripción existe, se muestra la lección normalmente.
6. Si no existe inscripción, se aplica la respuesta de seguridad (ver sección siguiente).

---

## Seguridad y autorización

Si el usuario no tiene una inscripción activa para el curso al que pertenece la lección solicitada, el sistema responde de una de las siguientes formas:

```csharp
if (inscripcion == null)
{
    return Forbid(); // HTTP 403 – el usuario está autenticado pero no tiene permiso
}
```

O bien redirige a una página de acceso denegado:

```csharp
if (inscripcion == null)
{
    return RedirectToAction("AccesoDenegado", "Home");
}
```

Ambas opciones impiden que el usuario acceda al contenido aunque esté autenticado. El método `Forbid()` devuelve un HTTP 403 y es la respuesta semánticamente correcta cuando el usuario está identificado pero no autorizado para ese recurso en particular.

Adicionalmente, las acciones de administración están protegidas por `[Authorize(Roles = "Administrador")]` a nivel de controlador, lo que impide que usuarios regulares accedan al panel de gestión.

---

## Protección contra acceso por URL directa

Sin esta validación, cualquier usuario que conozca o descubra la URL de una lección (por ejemplo `/Lecciones/Details/7`) podría acceder a su contenido simplemente copiando o escribiendo esa dirección en el navegador, sin importar si está inscrito o no.

La validación de inscripción en la base de datos bloquea este escenario porque:

- **No depende del lado del cliente.** No importa si el usuario llegó a través de un enlace en la vista o si escribió la URL a mano; en ambos casos la solicitud llega al servidor y la consulta a `Inscripciones` se ejecuta siempre.
- **No es configurable por el usuario.** A diferencia de ocultar un botón o enlace en la vista (que solo afecta a la interfaz), la comprobación en el controlador es obligatoria y el usuario no puede saltársela.
- **Funciona con cualquier método de acceso.** Ya sea una petición GET directa desde la barra de direcciones, un enlace externo o una solicitud programática, la validación se aplica en todos los casos.

Este patrón es una práctica estándar de seguridad en aplicaciones web: la interfaz puede ocultar o mostrar opciones según el estado del usuario, pero la **autorización real siempre ocurre en el servidor**.
