using System.ComponentModel.DataAnnotations;
using ManejoPresupuesto.Validaciones;
using Microsoft.AspNetCore.Mvc;


public class TipoCuenta: IValidatableObject
{
    public int Id { get; set; }
    [Remote(action: "VerificarExisteTipoCuenta", controller: "TiposCuentasController")]
    public string Nombre { get; set; }
    
    public int UsuarioId { get; set; }
    public int Orden { get; set; }


    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Nombre != null && Nombre.Length >0)
        {
            var primeraLetra = Nombre[0].ToString();
            if ( primeraLetra != primeraLetra.ToUpper())
            {
                yield return new ValidationResult("La primera letra debe ser mayuscula");
            }
        }
    }
}
