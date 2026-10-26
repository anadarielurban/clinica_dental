using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using DentalClinicWPF.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DentalClinicWPF.Services
{
    public class TicketPdfService
    {
        public void GenerarEImprimirTicket(Models.Invoice invoice, string rutaSalida)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                // Aseguramos que el directorio exista
                var dir = Path.GetDirectoryName(rutaSalida);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // 1. GENERAR EL PDF (Formato Ticket Térmico Corto)
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        // Ancho de 80mm (aprox 226 puntos) y alto dinámico
                        page.Size(new PageSize(226, 500)); 
                        page.Margin(10);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily(Fonts.Courier)); // Fuente tipo ticket

                        page.Header().Column(col =>
                        {
                            col.Item().AlignCenter().Text("CLÍNICA DENTAL").FontSize(14).Bold();
                            col.Item().AlignCenter().Text("Calle Falsa 123, Tel: 555-1234").FontSize(8);
                            col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
                        });

                        page.Content().PaddingVertical(5).Column(col =>
                        {
                            col.Item().Text($"Fecha: {invoice.InvoiceDate:dd/MM/yyyy HH:mm}");
                            col.Item().Text($"Paciente: {invoice.Patient?.FirstName} {invoice.Patient?.LastName}");
                            col.Item().Text($"Folio: #{invoice.InvoiceID}");
                            
                            col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);

                            // Tabla de Conceptos
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(30); // Cant
                                    columns.RelativeColumn();   // Desc
                                    columns.ConstantColumn(60); // Total
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("CANT").Bold();
                                    header.Cell().Text("DESCRIPCIÓN").Bold();
                                    header.Cell().AlignRight().Text("TOTAL").Bold();
                                });

                                // Si el POS genera una lista de conceptos, recórrela aquí.
                                // Como ejemplo, imprimimos el monto total como un solo concepto.
                                table.Cell().Text("1");
                                table.Cell().Text("Servicios Dentales / Venta POS");
                                table.Cell().AlignRight().Text($"{invoice.TotalAmount:C}");
                            });

                            col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);

                            col.Item().AlignRight().Text($"TOTAL: {invoice.TotalAmount:C}").FontSize(12).Bold();
                            col.Item().AlignRight().Text($"PAGÓ: {invoice.AmountPaid:C}");
                            col.Item().AlignRight().Text($"CAMBIO: {(invoice.AmountPaid - invoice.TotalAmount):C}");
                        });

                        page.Footer().AlignCenter().Column(col =>
                        {
                            col.Item().PaddingTop(5).AlignCenter().Text("¡Gracias por su preferencia!").FontSize(9);
                            col.Item().AlignCenter().Text("Conserve su ticket").FontSize(7);
                        });
                    });
                }).GeneratePdf(rutaSalida);

                // 2. IMPRIMIR AUTOMÁTICAMENTE
                // Usamos el comando "print" de Windows para enviarlo a la impresora predeterminada
                var psi = new ProcessStartInfo
                {
                    FileName = rutaSalida,
                    Verb = "print",
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };
                
                Process.Start(psi);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar/imprimir el ticket: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}