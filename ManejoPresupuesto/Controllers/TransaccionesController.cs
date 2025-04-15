using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc.Rendering; // ✔ Este es el correcto para ASP.NET Core
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace ManejoPresupuesto.Controllers
{
    public class TransaccionesController : Controller
    {
        private readonly IServicioUsuarios servicioUsuarios;
        private readonly IRepositorioCuentas repositorioCuentas;
        private readonly IRepositorioCategorias repositorioCategorias;
        private readonly IRepositorioTransacciones repositorioTransacciones;
        private readonly IMapper mapper;
        private readonly IServicioReportes servicioReportes;

        public TransaccionesController(
            IServicioUsuarios servicioUsuarios,
            IRepositorioCuentas repositorioCuentas,
            IRepositorioCategorias repositorioCategorias, IRepositorioTransacciones repositorioTransacciones,
            IMapper mapper, IServicioReportes servicioReportes)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
        }

        public async Task<IActionResult> Index(int mes, int ano)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();


            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, ano, ViewBag);

            return View(modelo);
        }

        public async Task<IActionResult> Semanal()
        {
            return View();
        }
        public async Task<IActionResult> Mensual()
        {
            return View();
        }
        public async Task<IActionResult> ExcelReporte()
        {
            return View();
        }
        public async Task<IActionResult> Calendario()
        {
            return View();
        }


        public async Task<IActionResult> Crear()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var modelo = new TransaccionCreacionViewModel();
            modelo.Cuentas = await ObtenerCuentas(usuarioId); 
            modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorId(id , usuarioId);

            if (transacciones is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            } 

            var modelo = mapper.Map<TransaccionActualizacionViewModel>(transacciones);

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.MontoAnterior = modelo.Monto *-1;
            }
            modelo.CuentaAnteriorId = transacciones.CuentaId;
            modelo.Categorias = await ObtenerCategorias(usuarioId, transacciones.TipoOperacionId);
            modelo.Cuentas = await ObtenerCuentas(usuarioId); 
            modelo.UrlRetorno = urlRetorno;
            return View(modelo);

        }

        [HttpPost] 

        public async Task<IActionResult> Editar(TransaccionActualizacionViewModel modelo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);

            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if (categoria is null) 
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            var transaccion = mapper.Map<Transaccion>(modelo);
            modelo.MontoAnterior = modelo.Monto;

            if (modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                transaccion.Monto *= -1; 
            }

            await repositorioTransacciones.Actualizar(transaccion, modelo.MontoAnterior, modelo.CuentaAnteriorId);

            if (string.IsNullOrEmpty(modelo.UrlRetorno))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return LocalRedirect(modelo.UrlRetorno);
            }

            

        }


        [HttpPost]

        public async Task<IActionResult> Borrar(int id, string urlRetorno = null)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();

            var transaccion = await repositorioTransacciones.ObtenerPorId(id, usuarioId);   
            if (transaccion is null) { 
                return RedirectToAction("List", "Home");
            }

            await repositorioTransacciones.Borrar(id);

            if (string.IsNullOrEmpty(urlRetorno))
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return LocalRedirect(urlRetorno);
            }

        }


        private async Task<IEnumerable<SelectListItem>> ObtenerCuentas(int usuarioId)
        {
            var cuentas = await repositorioCuentas.Buscar(usuarioId);
            return cuentas?.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()))
                   ?? Enumerable.Empty<SelectListItem>(); // Si es null, devuelve lista vacía
        }

        private async Task<IEnumerable<SelectListItem>> ObtenerCategorias(int usuarioId, TipoOperacion tipoOperacion)
        {
            var categorias = await repositorioCategorias.Obtener(usuarioId, tipoOperacion);
            return categorias.Select(x => new SelectListItem(x.Nombre, x.Id.ToString()));   
        }

        [HttpPost]
        public async Task<IActionResult> Crear(TransaccionCreacionViewModel modelo)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if (!ModelState.IsValid)
            {
                modelo.Cuentas = await ObtenerCuentas(usuarioId);
                modelo.Categorias = await ObtenerCategorias(usuarioId, modelo.TipoOperacionId);
                return View(modelo);
            }

            var cuenta = await repositorioCuentas.ObtenerPorId(modelo.CuentaId, usuarioId);
            if (cuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");

            }

            var categoria = await repositorioCategorias.ObtenerPorId(modelo.CategoriaId, usuarioId);

            if(categoria is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }

            modelo.UsuarioId = usuarioId;

            if(modelo.TipoOperacionId == TipoOperacion.Gasto)
            {
                modelo.Monto *= -1;
            }
            await repositorioTransacciones.Crear(modelo);
            return RedirectToAction("Index");
        }


        [HttpPost]  // Elimina [HttpGet]
        public async Task<IActionResult> ObtenerCategorias([FromBody] int tipoOperacionId)

        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var categorias = await repositorioCategorias.Obtener(usuarioId, (TipoOperacion)tipoOperacionId);

            if (!categorias.Any())
            {
                return NotFound("No se encontraron categorías para el tipo de operación seleccionado.");
            }

            return Ok(categorias.Select(c => new { value = c.Id, text = c.Nombre }));
        }


    }


}