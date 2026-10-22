using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Pos;

namespace DentalClinicWPF.Services
{
    public class TicketPdfService
    {
        public static void Generate(Invoice invoice, List<PosItemViewModel> items, string outputPath)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            

            var clinic = GetClinicInfo();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(new PageSize(226, 800));
                    page.Margin(10);
                    page.DefaultTextStyle(x => x.FontFamily("Consolas").FontSize(9));

                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text("CLINICA DENTAL").Bold().FontSize(12);
                        col.Item().AlignCenter().Text(clinic.Name);
                        col.Item().AlignCenter().Text(clinic.Address);
                        col.Item().AlignCenter().Text($"Tel: {clinic.Phone}");
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
                    });

                    
                    page.Content().PaddingVertical(5).Column(col =>
                    {
                        col.Item().Text($"TICKET: #{invoice.InvoiceID:D6}");
                        col.Item().Text($"FECHA: {invoice.InvoiceDate:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"PACIENTE: {invoice.Patient?.FullName ?? "Venta Mostrador"}");
                        col.Item().Text($"ATENDIO: {invoice.Appointment?.Dentist?.FullName ?? "Venta Mostrador"}");
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);

                        
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(1).Text("CANT");
                            row.RelativeItem(4).Text("DESCRIPCION");
                            row.RelativeItem(2).AlignRight().Text("P.UNIT");
                            row.RelativeItem(2).AlignRight().Text("TOTAL");
                        });
                        col.Item().LineHorizontal(1).LineColor(Colors.Black);

                        // ITEMS
                        foreach (var item in items)
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem(1).Text(item.Quantity.ToString());
                                row.RelativeItem(4).Text(item.Description);
                                row.RelativeItem(2).AlignRight().Text($"${item.UnitPrice:N2}");
                                row.RelativeItem(2).AlignRight().Text($"${(item.Quantity * item.UnitPrice):N2}");
                            });
                        }

                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);

                        // TOTAL EN GRANDE Y NEGRITAS
                        col.Item().Row(row =>
                        {
                            row.RelativeItem(6).AlignRight().Text("TOTAL:").Bold().FontSize(11);
                            row.RelativeItem(2).AlignRight().Text($"${invoice.TotalAmount:N2}").Bold().FontSize(12);
                        });

                        col.Item().PaddingTop(5).Text($"PAGO CON: {invoice.PaymentMethod}");
                        
                        if (invoice.PaymentMethod == "Efectivo")
                        {
                            col.Item().Text($"RECIBIDO: ${invoice.AmountReceived:N2}");
                            col.Item().Text($"CAMBIO: ${invoice.Change:N2}");
                        }
                    });

                    // PIE DE PÁGINA
                    page.Footer().Column(col =>
                    {
                        col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Black);
                        col.Item().AlignCenter().Text("¡Gracias por su preferencia!").Bold();
                        col.Item().AlignCenter().Text("Conserve este ticket");
                    });
                });
            }).GeneratePdf(outputPath);
        }

        private static (string Name, string Address, string Phone) GetClinicInfo()
        {
            try
            {
                using var context = new DentalClinicWPF.Data.DentalClinicContext();
                var room = context.Set<Room>().FirstOrDefault();
                
                if (room != null)
                {
                    return (
                        string.IsNullOrWhiteSpace(room.RoomNumber) ? "Consultorio Principal" : room.RoomNumber,
                        string.IsNullOrWhiteSpace(room.Address) ? "Dirección no registrada" : room.Address,
                        string.IsNullOrWhiteSpace(room.Phone) ? "Sin teléfono" : room.Phone
                    );
                }
            }
            catch { }
            return ("Consultorio Principal", "Dirección no registrada", "Sin teléfono");
        }
    }
}