using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using SkiaSharp;
using DentalClinicWPF.ViewModels.Patient.PatientDetail;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QColors = QuestPDF.Helpers.Colors;
using MediaBrush = System.Windows.Media.Brush;

namespace DentalClinicWPF.Services
{
    public static class ExpedientePdfService
    {
        static ExpedientePdfService()
        {
            QuestPDF.Settings.License = LicenseType.Community;
             // Habilita Segoe UI y otras fuentes de Windows
        }

        public static void Generate(PatientDetailViewModel vm, string outputPath)
        {
            var patient = vm.Patient;

            Document.Create(doc =>
            {
                doc.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontFamily("Segoe UI").FontSize(9));
                    page.PageColor(QColors.White);

                    page.Content().Column(col =>
                    {
                        col.Spacing(8);

                        // ═══ HEADER ═══
                        col.Item().Background("#14B8A6").Padding(12).Column(h =>
                        {
                            h.Item().AlignCenter().Text("CLINICA DENTAL")
                                .FontSize(18).Bold().FontColor(QColors.White);
                            h.Item().AlignCenter().Text("EXPEDIENTE CLINICO")
                                .FontSize(10).SemiBold().FontColor(QColors.White);
                        });

                        // ═══ DATOS DEL PACIENTE ═══
                        SectionTitle(col, "DATOS DEL PACIENTE");
                        col.Item().Border(0.5f).BorderColor("#E2E8F0").Table(table =>
                        {
                            table.ColumnsDefinition(c => { c.ConstantColumn(130); c.RelativeColumn(); });
                            void Row(string lbl, string val, bool zebra)
                            {
                                var bg = zebra ? "#F8FAFC" : "#FFFFFF";
                                table.Cell().Background(bg).Padding(5).Text(lbl).SemiBold().FontSize(9).FontColor("#475569");
                                table.Cell().Background(bg).Padding(5).Text(string.IsNullOrWhiteSpace(val) ? "—" : val).FontSize(9).FontColor("#1E293B");
                            }
                            Row("Paciente", patient.FullName, false);
                            Row("F. Nacimiento", patient.DateOfBirth.ToString("dd/MM/yyyy"), true);
                            Row("Telefono", patient.PhoneNumber, false);
                            Row("Correo", patient.Email, true);
                            Row("Direccion", patient.Address, false);
                        });

                        // ═══ HISTORIA CLINICA ═══
                        var conditions = new List<string>();
                        if (patient.HasDiabetes) conditions.Add("Diabetes");
                        if (patient.HasHypertension) conditions.Add("Hipertension");
                        if (patient.HasHeartConditions) conditions.Add("Problemas cardiacos");
                        if (patient.HasCoagulationIssues) conditions.Add("Coagulacion");
                        if (patient.TakesAnticoagulants) conditions.Add("Anticoagulantes");
                        if (patient.IsPregnant) conditions.Add("Embarazo");
                        if (patient.IsSmoker) conditions.Add("Fumador");

                        bool hasHist = !string.IsNullOrWhiteSpace(patient.BloodType)
                            || conditions.Count > 0
                            || patient.HasAllergies
                            || !string.IsNullOrWhiteSpace(patient.EmergencyContactName)
                            || !string.IsNullOrWhiteSpace(patient.MedicalNotes);

                        if (hasHist)
                        {
                            SectionTitle(col, "HISTORIA CLINICA");
                            col.Item().Border(0.5f).BorderColor("#E2E8F0").Padding(8).Column(hc =>
                            {
                                hc.Spacing(5);

                                if (!string.IsNullOrWhiteSpace(patient.BloodType))
                                {
                                    hc.Item().Row(r =>
                                    {
                                        r.AutoItem().Text("Tipo de sangre: ").SemiBold().FontColor("#475569");
                                        r.AutoItem().Background("#DC2626").PaddingHorizontal(8).PaddingVertical(2)
                                            .Text(patient.BloodType!).Bold().FontColor(QColors.White).FontSize(9);
                                    });
                                }

                                if (conditions.Count > 0)
                                {
                                    hc.Item().Text(t =>
                                    {
                                        t.Span("Condiciones: ").SemiBold().FontColor("#475569");
                                        t.Span(string.Join("  |  ", conditions)).FontColor("#1E293B");
                                    });
                                }

                                if (patient.HasAllergies)
                                {
                                    hc.Item().Background("#FEE2E2").Border(0.5f).BorderColor("#DC2626").Padding(6).Column(a =>
                                    {
                                        a.Item().Text("! ALERGIAS").Bold().FontColor("#991B1B").FontSize(9);
                                        a.Item().Text(string.IsNullOrWhiteSpace(patient.AllergiesDetails) ? "No especificadas" : patient.AllergiesDetails)
                                            .FontColor("#7F1D1D").FontSize(9);
                                    });
                                }

                                if (!string.IsNullOrWhiteSpace(patient.EmergencyContactName) || !string.IsNullOrWhiteSpace(patient.EmergencyContactPhone))
                                {
                                    var parts = new List<string>();
                                    if (!string.IsNullOrWhiteSpace(patient.EmergencyContactName)) parts.Add(patient.EmergencyContactName);
                                    if (!string.IsNullOrWhiteSpace(patient.EmergencyContactPhone)) parts.Add(patient.EmergencyContactPhone);
                                    hc.Item().Text(t =>
                                    {
                                        t.Span("Contacto de emergencia: ").SemiBold().FontColor("#475569");
                                        t.Span(string.Join("  |  ", parts)).FontColor("#1E293B");
                                    });
                                }

                                if (!string.IsNullOrWhiteSpace(patient.MedicalNotes))
                                {
                                    hc.Item().Text(t =>
                                    {
                                        t.Span("Notas medicas: ").SemiBold().FontColor("#475569");
                                        t.Span(patient.MedicalNotes!).FontColor("#1E293B");
                                    });
                                }
                            });
                        }

                        // ═══ ODONTOGRAMA ═══
                        SectionTitle(col, "ODONTOGRAMA");
                        col.Item().Border(0.5f).BorderColor("#E2E8F0").Padding(8).Column(odo =>
                        {
                            odo.Spacing(4);
                            odo.Item().Row(r =>
                            {
                                r.RelativeItem().Element(c => ToothGrid(c, vm.TeethUpperRight.ToList(), "SUPERIOR DERECHO", false));
                                r.ConstantItem(10);
                                r.RelativeItem().Element(c => ToothGrid(c, vm.TeethUpperLeft.ToList(), "SUPERIOR IZQUIERDO", false));
                            });
                            odo.Item().PaddingVertical(3).LineHorizontal(0.5f).LineColor("#E2E8F0");
                            odo.Item().Row(r =>
                            {
                                r.RelativeItem().Element(c => ToothGrid(c, vm.TeethLowerRight.ToList(), "INFERIOR DERECHO", true));
                                r.ConstantItem(10);
                                r.RelativeItem().Element(c => ToothGrid(c, vm.TeethLowerLeft.ToList(), "INFERIOR IZQUIERDO", true));
                            });
                        });

                        // ═══ LEYENDA ═══
                        col.Item().Background("#F8FAFC").Border(0.5f).BorderColor("#E2E8F0").Padding(6).Row(r =>
                        {
                            r.AutoItem().Text("LEYENDA  ").Bold().FontSize(7.5f).FontColor("#475569");
                            void LegendItem(string name, string hex)
                            {
                                r.AutoItem().PaddingLeft(6).Row(lr =>
                                {
                                    lr.AutoItem().Width(8).Height(8).Background(hex).Border(0.5f).BorderColor("#CBD5E1");
                                    lr.AutoItem().PaddingLeft(2).Text(name).FontSize(7.5f).FontColor("#475569");
                                });
                            }
                            LegendItem("Caries", "#EF4444");
                            LegendItem("Resina", "#3B82F6");
                            LegendItem("Endodoncia", "#8B5CF6");
                            LegendItem("Corona", "#F59E0B");
                            LegendItem("Extraido", "#94A3B8");
                            LegendItem("Implante", "#10B981");
                        });

                        // ═══ HALLAZGOS ═══
                        var findings = vm.TeethUpperRight.Concat(vm.TeethUpperLeft)
                            .Concat(vm.TeethLowerRight).Concat(vm.TeethLowerLeft)
                            .Where(t => t.Condition != "Sano").ToList();

                        if (findings.Count > 0)
                        {
                            SectionTitle(col, "DETALLE DE HALLAZGOS");
                            col.Item().Border(0.5f).BorderColor("#E2E8F0").Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(55);
                                    c.ConstantColumn(90);
                                    c.RelativeColumn();
                                });
                                table.Header(h =>
                                {
                                    h.Cell().Background("#14B8A6").Padding(4).Text("DIENTE").Bold().FontColor(QColors.White).FontSize(8);
                                    h.Cell().Background("#14B8A6").Padding(4).Text("ESTADO").Bold().FontColor(QColors.White).FontSize(8);
                                    h.Cell().Background("#14B8A6").Padding(4).Text("NOTAS").Bold().FontColor(QColors.White).FontSize(8);
                                });
                                int i = 0;
                                foreach (var t in findings)
                                {
                                    var bg = i % 2 == 1 ? "#F8FAFC" : "#FFFFFF";
                                    table.Cell().Background(bg).Padding(3).Text(t.ToothNumber.ToString()).SemiBold().FontSize(8);
                                    table.Cell().Background(bg).Padding(3).Text(t.Condition).FontColor("#0D9488").FontSize(8);
                                    table.Cell().Background(bg).Padding(3).Text(string.IsNullOrWhiteSpace(t.Record.Notes) ? "—" : t.Record.Notes).FontColor("#475569").FontSize(8);
                                    i++;
                                }
                            });
                        }

                        // ═══ FECHA ═══
                        col.Item().AlignRight().Text(t =>
                        {
                            t.DefaultTextStyle(x => x.FontSize(8).FontColor("#94A3B8"));
                            t.Span($"Emitido el {DateTime.Now:dd/MM/yyyy} a las {DateTime.Now:HH:mm}");
                        });

                        // ═══ FIRMA CENTRADA ═══
                        col.Item().PaddingTop(15).AlignCenter().Width(220).Column(sig =>
                        {
                            sig.Item().LineHorizontal(0.8f).LineColor("#1E293B");
                            sig.Item().PaddingTop(3).AlignCenter().Text("Firma del Odontologo").FontSize(9).SemiBold().FontColor("#475569");
                            sig.Item().PaddingTop(1).AlignCenter().Text("Cedula Profesional: ________________").FontSize(8).FontColor("#94A3B8");
                        });
                    });

                    page.Footer().AlignCenter().Text(t =>
                    {
                        t.DefaultTextStyle(x => x.FontSize(7).FontColor("#94A3B8"));
                        t.Span("Clinica Dental  |  Documento confidencial  |  Generado automaticamente");
                    });
                });
            }).GeneratePdf(outputPath);
        }

        private static void SectionTitle(ColumnDescriptor col, string text)
        {
            col.Item().Text(t =>
            {
                t.Span("| ").FontSize(11).Bold().FontColor("#14B8A6");
                t.Span(text).FontSize(10).Bold().FontColor("#1E293B");
            });
        }

        private static void ToothGrid(IContainer container, List<ToothViewModel> teeth, string label, bool invert)
    {
        container.Column(c =>
        {
            c.Item().AlignCenter().Text(label).FontSize(6.5f).SemiBold().FontColor("#94A3B8");
            c.Item().PaddingTop(2).Table(t =>
            {
                t.ColumnsDefinition(cols =>
                {
                    for (int i = 0; i < 8; i++) cols.RelativeColumn();
                });
                foreach (var tooth in teeth)
                {
                    var hex = ToHex(tooth.BackgroundColor);
                    var txtHex = ToHex(tooth.TextColor);
                    
                    t.Cell().Padding(1).Height(24).Layers(layers =>
                    {
                        // Forzar el método Canvas de QuestPDF usando ElementExtensions
                        layers.Layer().Element(e => QuestPDF.Fluent.ElementExtensions.Canvas(e, (canvasObj, size) =>
                        {
                            var canvas = (SkiaSharp.SKCanvas)canvasObj;
                            var path = new SkiaSharp.SKPath();
                            float scaleX = (float)size.Width / 55f;
                            float scaleY = (float)size.Height / 65f;
                            canvas.Scale(scaleX, scaleY);

                            if (invert)
                            {
                                canvas.Translate(27.5f, 32.5f);
                                canvas.RotateDegrees(180);
                                canvas.Translate(-27.5f, -32.5f);
                            }

                            path.MoveTo(15, 5);
                            path.CubicTo(25, 0, 35, 0, 45, 5);
                            path.CubicTo(50, 15, 50, 30, 45, 40);
                            path.LineTo(40, 60);
                            path.CubicTo(40, 65, 35, 65, 35, 60);
                            path.LineTo(30, 45);
                            path.LineTo(30, 60);
                            path.CubicTo(30, 65, 25, 65, 25, 60);
                            path.LineTo(20, 45);
                            path.LineTo(20, 60);
                            path.CubicTo(20, 65, 15, 65, 15, 60);
                            path.LineTo(10, 40);
                            path.CubicTo(5, 30, 5, 15, 10, 5);
                            path.Close();

                            var fillColor = SkiaSharp.SKColor.Parse(hex);
                            var strokeColor = SkiaSharp.SKColor.Parse("#CBD5E1");

                            using var fillPaint = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Fill, Color = fillColor, IsAntialias = true };
                            using var strokePaint = new SkiaSharp.SKPaint { Style = SkiaSharp.SKPaintStyle.Stroke, Color = strokeColor, StrokeWidth = 1.5f, IsAntialias = true };

                            canvas.DrawPath(path, fillPaint);
                            canvas.DrawPath(path, strokePaint);
                        }));

                        var textLayer = layers.PrimaryLayer().AlignCenter().AlignMiddle();
                        if (invert)
                            textLayer = textLayer.PaddingTop(7);
                        else
                            textLayer = textLayer.PaddingBottom(7);

                        textLayer.Text(tooth.ToothNumber.ToString()).FontSize(6.5f).Bold().FontColor(txtHex);
                    });
                }
            });
        });
    }

        private static string ToHex(MediaBrush brush)
        {
            if (brush is System.Windows.Media.SolidColorBrush s)
                return $"#{s.Color.R:X2}{s.Color.G:X2}{s.Color.B:X2}";
            return "#FFFFFF";
        }
    }
}







