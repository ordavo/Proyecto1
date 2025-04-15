using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ManejoPresupuesto.Models
{
    public class TransaccionCreacionViewModel : Transaccion
    {
        public IEnumerable<SelectListItem> Cuentas { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();

        [Display(Name = "Tipo operación")]
        public new TipoOperacion TipoOperacionId { get; set; }
    }

}