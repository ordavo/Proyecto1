﻿using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Validaciones
{
    public class PrimeraLetraMayusculaAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString().Trim()))
            {
                return ValidationResult.Success;
            }

            var primeraLetra = value.ToString().Trim()[0].ToString();

            if (primeraLetra != primeraLetra.ToUpper())
            {
                return new ValidationResult("La primera letra debe de ser mayúscula");
            }
            return ValidationResult.Success;
        }
    }
}
