using Microsoft.AspNetCore.Mvc;
using ManejoPresupuesto.Models;
using System.Data;
using ClosedXML.Excel;

namespace ManejoPresupuesto.Servicios
{
    public class GenerarExcel
    {
        public FileContentResult GenerarArchivoExcel(string nombreArchivo, IEnumerable<Transaccion> transacciones)
        {
            DataTable dt = new DataTable("Transacciones");
            dt.Columns.AddRange(new DataColumn[]
            {
                new DataColumn("Fecha"),
                new DataColumn("Cuenta"),
                new DataColumn("Categoria"),
                new DataColumn("Nota"),
                new DataColumn("Monto"),
                new DataColumn("Ingreso/Gasto"),
            });

            foreach (var transaccion in transacciones)
            {
                dt.Rows.Add(
                    transaccion.FechaTransaccion.ToString("dd/MM/yyyy"),
                    transaccion.Cuenta,
                    transaccion.Categoria,
                    transaccion.Nota,
                    transaccion.Monto,
                    transaccion.TipoOperacionId.ToString());
            }

            using (var workbook = new XLWorkbook())
            {
                workbook.Worksheets.Add(dt);

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);

                    return new FileContentResult(stream.ToArray(),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = nombreArchivo
                    };
                }
            }
        }
    }
}
