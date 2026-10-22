using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Linq;
using DentalClinicWPF.Models;

namespace DentalClinicWPF.Services
{
    public class PrescriptionPdfService
    {
        public static void Generate(Prescription prescription, string outputPath)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            
            var clinicName = "CLINICA DENTAL";
            var clinicAddress = "Dirección de la Clínica";
            var clinicPhone = "Teléfono";

            try
            {
                using var context = new DentalClinicWPF.Data.DentalClinicContext();
                var room = context.Set<Room>().FirstOrDefault();
                if (room != null)
                {
                    clinicName = room.RoomNumber;
                    if (!string.IsNullOrEmpty(room.Address)) clinicAddress = room.Address;
                    if (!string.IsNullOrEmpty(room.Phone)) clinicPhone = room.Phone;
                }
            }
            catch { }

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(11));

                    // ENCABEZADO
                    page.Header().Column(col =>
                    {
                        col.Item().AlignCenter().Text(clinicName).FontSize(20).Bold().FontColor(Colors.Teal.Darken2);
                        col.Item().AlignCenter().Text(clinicAddress).FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().AlignCenter().Text($"Tel: {clinicPhone}").FontSize(10).FontColor(Colors.Grey.Darken1);
                        col.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Teal.Medium);
                    });

                    // CUERPO
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Paciente: {prescription.Patient?.FullName ?? "Desconocido"}").FontSize(12).Bold();
                            row.ConstantItem(150).Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(12);
                        });
                        
                        col.Item().PaddingVertical(20).Text("Rx").FontSize(28).Bold().FontColor(Colors.Teal.Darken2);
                        
                        col.Item().PaddingBottom(10).Text(prescription.Medication).FontSize(16).Bold();
                        col.Item().PaddingBottom(5).Text($"Dosis / Indicaciones: {prescription.Dosage}").FontSize(12);
                        
                        if (!string.IsNullOrWhiteSpace(prescription.Instructions))
                        {
                            col.Item().Text($"Instrucciones adicionales: {prescription.Instructions}").FontSize(12);
                        }
                    });

                    // PIE DE PÁGINA (Firma)
                    page.Footer().Column(col =>
                    {
                        col.Item().PaddingTop(40).AlignRight().Text("_______________________________");
                        col.Item().AlignRight().Text($"Dr(a). {prescription.Dentist?.FullName ?? "Odontólogo"}").FontSize(12).Bold();
                        col.Item().AlignRight().Text("Cédula Profesional: 123456789").FontSize(10);
                    });
                });
            }).GeneratePdf(outputPath);
        }
    }
}