using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using Microsoft.AspNetCore.Mvc.Rendering; // ✔ Este es el correcto para ASP.NET Core
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AspNetCoreGeneratedDocument;

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
        private readonly GenerarExcel generarExcel;

        public TransaccionesController(
            IServicioUsuarios servicioUsuarios,
            IRepositorioCuentas repositorioCuentas,
            IRepositorioCategorias repositorioCategorias, IRepositorioTransacciones repositorioTransacciones,
            IMapper mapper, IServicioReportes servicioReportes, GenerarExcel generarExcel)
        {
            this.servicioUsuarios = servicioUsuarios;
            this.repositorioCuentas = repositorioCuentas;
            this.repositorioCategorias = repositorioCategorias;
            this.repositorioTransacciones = repositorioTransacciones;
            this.mapper = mapper;
            this.servicioReportes = servicioReportes;
            this.generarExcel = generarExcel;
        }

        public async Task<IActionResult> Index(int mes, int ano)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();


            var modelo = await servicioReportes.ObtenerReporteTransaccionesDetalladas(usuarioId, mes, ano, ViewBag);

            return View(modelo);
        }

        public async Task<IActionResult> Semanal(int mes, int ano)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            IEnumerable<ResultadoObtenerPorSemana> transaccionesPorSemana
                = await servicioReportes.ObtenerReporteSemanal(usuarioId, mes, ano, ViewBag);

            var agrupado = transaccionesPorSemana.GroupBy(x=> x.Semana).Select(x=> new ResultadoObtenerPorSemana()
            {
                Semana=x.Key,
                Ingresos = x.Where(x=> x.TipoOperacionId == TipoOperacion.Ingreso).Select(x=> x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault(),
            }).ToList();   
            
            if(ano == 0 || mes == 0)
            {
                var hoy = DateTime.Today;
                ano = hoy.Year;
                mes = hoy.Month;
            }
            var fechaReferencia = new DateTime(ano, mes, 1);
            var diaDelMes = Enumerable.Range(1, fechaReferencia.AddMonths(1).AddDays(-1).Day);

            var diasSegmentados = diaDelMes.Chunk(7).ToList();

            for (int i = 0; i < diasSegmentados.Count(); i++)
            {
                var semana = i - 1;
                var fechaInicio = new DateTime(ano, mes, diasSegmentados[i].First());
                var fechaFin = new DateTime(ano, mes, diasSegmentados[i].Last());
                var grupoSemana = agrupado.FirstOrDefault(x=>x.Semana == semana);
                if (grupoSemana is null)
                {
                    agrupado.Add(new ResultadoObtenerPorSemana()
                    {
                        Semana = semana,
                        FechaInicio = fechaInicio,
                        FechaFin = fechaFin

                    });
                }
                else
                {
                    grupoSemana.FechaInicio = fechaInicio;
                    grupoSemana.FechaFin = fechaFin;
                }
            }
            agrupado.OrderByDescending(x=> x.Semana).ToList();

            var modelo = new ReportesSemanalViewModel();
            modelo.TransaccionesPorSemana = agrupado;
            modelo.FechaReferencia = fechaReferencia;


            return View(modelo);
        }
        public async Task<IActionResult> Mensual(int año)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            if(año == 0)
            {
                año = DateTime.Today.Year;
            }

            var transaccionesPorMes = await repositorioTransacciones.ObtenerPorMes(usuarioId, año);

            var transaccionesAgrupadas = transaccionesPorMes.GroupBy(x=>x.Mes).Select(x=> new ResultadoObtenerPorMes()
            {
                Mes=x.Key,
                Ingresos = x.Where(x => x.TipoOperacionId == TipoOperacion.Ingreso).Select(x => x.Monto).FirstOrDefault(),
                Gastos = x.Where(x => x.TipoOperacionId == TipoOperacion.Gasto).Select(x => x.Monto).FirstOrDefault(),
            }).ToList();

            for (int mes = 1; mes <=12; mes++)
            {
                var transaccion = transaccionesAgrupadas.FirstOrDefault(x => x.Mes == mes);
                var fechaReferencia = new DateTime(año, mes, 1);
                if(transaccion is null)
                {
                    transaccionesAgrupadas.Add(new ResultadoObtenerPorMes()
                    {
                        Mes = mes,
                        FechaReferencia = fechaReferencia,

                    });
                }
                else
                {
                    transaccion.FechaReferencia = fechaReferencia;
                }

            }

            transaccionesAgrupadas = transaccionesAgrupadas.OrderByDescending(x=>x.Mes).ToList();

            var modelo = new ReporteMensualViewModel();
            modelo.Año = año;
            modelo.TransaccionesPorMes = transaccionesAgrupadas;

            return View(modelo);
        }
        public async Task<IActionResult> ExcelReporte()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            return View();
        }

        [HttpGet]

        public async Task<FileResult> ExportarExcelPorMes (int mes, int ano)
        {
            var fechaInicio = new DateTime(ano, mes, 1);
            var fechaFin = fechaInicio.AddMonths(1).AddDays(-1);
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var transacciones = await repositorioTransacciones.ObtenerPorUsuarioId(new ParametroObtenerTransaccionesPorUsuario
            {
                UsuarioId = usuarioId,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            });

            var nombreArchivo = $"Manejo presupuestos - {fechaInicio.ToString("MMM yyyy")}.xlsx";

            return generarExcel.GenerarArchivoExcel(nombreArchivo, transacciones);


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