using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Models;
using Microsoft.Win32;
using ClosedXML.Excel;
using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QColors = QuestPDF.Helpers.Colors;

namespace DentalClinicWPF.ViewModels.Dashboard
{
    public class DentistRevenue
    {
        public string DentistName { get; set; }
        public decimal Revenue { get; set; }
    }

    public class DashboardViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Action<string, BaseViewModel> _openTabAction;

        // Properties
        private ObservableCollection<object> _upcomingAppointments = new();
        public ObservableCollection<object> UpcomingAppointments
        {
            get => _upcomingAppointments;
            private set => SetProperty(ref _upcomingAppointments, value);
        }

        private decimal _totalRevenue;
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            private set => SetProperty(ref _totalRevenue, value);
        }

        private int _unpaidInvoices;
        public int UnpaidInvoices
        {
            get => _unpaidInvoices;
            private set => SetProperty(ref _unpaidInvoices, value);
        }

        private ObservableCollection<DentistRevenue> _dentistRevenue = new();
        public ObservableCollection<DentistRevenue> DentistRevenue
        {
            get => _dentistRevenue;
            private set => SetProperty(ref _dentistRevenue, value);
        }

        // LiveCharts Properties
        private SeriesCollection _revenueSeries;
        public SeriesCollection RevenueSeries
        {
            get => _revenueSeries;
            private set => SetProperty(ref _revenueSeries, value);
        }

        private List<string> _revenueLabels;
        public List<string> RevenueLabels
        {
            get => _revenueLabels;
            private set => SetProperty(ref _revenueLabels, value);
        }

        private Func<double, string> _yFormatter;
        public Func<double, string> YFormatter
        {
            get => _yFormatter;
            private set => SetProperty(ref _yFormatter, value);
        }

        // Commands
        public ICommand AddAppointmentCommand { get; }
        public ICommand RegisterPatientCommand { get; }
        public ICommand ExportReportCommand { get; }
        public ICommand ExportToExcelCommand { get; }

        public DashboardViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTabAction)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction ?? throw new ArgumentNullException(nameof(openTabAction));

            AddAppointmentCommand = new RelayCommand(OpenAddAppointment);
            RegisterPatientCommand = new RelayCommand(OpenRegisterPatient);
            ExportReportCommand = new RelayCommand(ExportReportAsPdf);
            ExportToExcelCommand = new RelayCommand(ExportReportToExcel);

            // Inicializar licencia de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                var now = DateTime.Now;
                var today = now.Date;
                var currentTime = now.TimeOfDay;

                var appointments = _context.Appointment
                    .Include(a => a.Patient)
                    .Include(a => a.Dentist)
                    .AsEnumerable().Where(a => a.AppointmentDate.Date > today ||
                                (a.AppointmentDate.Date == today && a.StartTime > currentTime))
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.StartTime)
                    .Take(10)
                    .Select(a => new
                    {
                        AppointmentDate = a.AppointmentDate.ToString("d"),
                        AppointmentTime = a.StartTime.ToString(@"hh\:mm"),
                        PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        DentistName = a.Dentist.FirstName + " " + a.Dentist.LastName
                    })
                    .ToList();

                UpcomingAppointments = new ObservableCollection<object>(appointments);

                TotalRevenue = _context.Invoice.Sum(i => i.TotalAmount);
                UnpaidInvoices = _context.Invoice.Count(i => i.AmountPaid < i.TotalAmount);

                var allDentists = _context.Dentist
                    .Include(d => d.Appointments)
                        .ThenInclude(a => a.Treatment)
                    .AsEnumerable()
                    .ToList();

                var revenuePerDentist = allDentists
                    .Select(d => new DentistRevenue
                    {
                        DentistName = d.FirstName + " " + d.LastName,
                        Revenue = d.Appointments
                            .Where(a => a.AppointmentDate.Date > today ||
                                        (a.AppointmentDate.Date == today && a.StartTime > currentTime))
                            .Sum(a => a.Treatment.Price)
                    })
                    .ToList();

                DentistRevenue = new ObservableCollection<DentistRevenue>(revenuePerDentist);

                LoadRevenueData();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while loading dashboard data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadRevenueData()
        {
            try
            {
                var sixMonthsAgo = DateTime.Now.AddMonths(-5);
                var revenueData = _context.Invoice
                    .AsEnumerable()
                    .Where(i => i.InvoiceDate >= new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1))
                    .GroupBy(i => new { i.InvoiceDate.Year, i.InvoiceDate.Month })
                    .OrderBy(g => g.Key.Year)
                    .ThenBy(g => g.Key.Month)
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Total = g.Sum(i => i.TotalAmount)
                    })
                    .ToList();

                RevenueLabels = new List<string>();
                var values = new ChartValues<double>();

                var current = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);
                var end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

                var revenueDict = revenueData.ToDictionary(
                    x => new DateTime(x.Year, x.Month, 1),
                    x => (double)x.Total
                );

                while (current <= end)
                {
                    RevenueLabels.Add(current.ToString("MMM yyyy"));
                    if (revenueDict.TryGetValue(current, out double total))
                    {
                        values.Add(total);
                    }
                    else
                    {
                        values.Add(0);
                    }
                    current = current.AddMonths(1);
                }

                RevenueSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Revenue",
                        Values = values,
                        Fill = System.Windows.Media.Brushes.SteelBlue
                    }
                };

                YFormatter = value => value.ToString("C");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while loading revenue data: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenAddAppointment()
        {
            try
            {
                var addAppointmentView = new Views.Appointment.AddAppointmentView
                {
                    DataContext = new ViewModels.Appointment.AddAppointmentViewModel(_context)
                };
                if (addAppointmentView.ShowDialog() == true)
                {
                    LoadDashboardData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while opening Add Appointment window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenRegisterPatient()
        {
            try
            {
                var addPatientWindow = new Views.Patient.AddPatientWindow
                {
                    DataContext = new ViewModels.Patient.AddPatientViewModel(_context)
                };
                if (addPatientWindow.ShowDialog() == true)
                {
                    LoadDashboardData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while opening Register Patient window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // EXPORTACIÓN A PDF (QuestPDF - Profesional)
        // ═══════════════════════════════════════════════════════════════
        private void ExportReportAsPdf()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Reporte_Dashboard_{DateTime.Now:yyyyMMdd_HHmm}",
                    DefaultExt = ".pdf",
                    Filter = "PDF Documents (.pdf)|*.pdf"
                };

                if (saveFileDialog.ShowDialog() != true) return;
                string fileName = saveFileDialog.FileName;

                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(QColors.White);
                        page.DefaultTextStyle(x => x.FontSize(11).FontFamily(QuestPDF.Helpers.Fonts.SegoeUI));

                        // ENCABEZADO
                        page.Header().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Clínica Dental").FontSize(24).SemiBold().FontColor(QColors.Teal.Medium);
                                    c.Item().Text("Reporte del Dashboard").FontSize(16).FontColor(QColors.Grey.Darken2);
                                });
                                row.ConstantItem(150).AlignRight().Column(c =>
                                {
                                    c.Item().Text($"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(10).FontColor(QColors.Grey.Medium);
                                });
                            });
                            col.Item().PaddingTop(10).LineHorizontal(2).LineColor(QColors.Teal.Medium);
                        });

                        // CONTENIDO
                        page.Content().PaddingVertical(20).Column(col =>
                        {
                            // KPIs
                            col.Item().PaddingBottom(20).Row(row =>
                            {
                                row.RelativeItem().Background(QColors.Grey.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Ingresos Totales").FontSize(10).FontColor(QColors.Grey.Darken1);
                                    c.Item().Text($"{TotalRevenue:C}").FontSize(18).Bold().FontColor(QColors.Green.Darken2);
                                });
                                row.ConstantItem(15);
                                row.RelativeItem().Background(QColors.Grey.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Facturas sin Pagar").FontSize(10).FontColor(QColors.Grey.Darken1);
                                    c.Item().Text($"{UnpaidInvoices}").FontSize(18).Bold().FontColor(QColors.Orange.Darken2);
                                });
                                row.ConstantItem(15);
                                row.RelativeItem().Background(QColors.Grey.Lighten4).Padding(10).Column(c =>
                                {
                                    c.Item().Text("Próximas Citas").FontSize(10).FontColor(QColors.Grey.Darken1);
                                    c.Item().Text($"{UpcomingAppointments.Count}").FontSize(18).Bold().FontColor(QColors.Blue.Darken2);
                                });
                            });

                            // TABLA CITAS
                            col.Item().PaddingBottom(5).Text("Próximas Citas").FontSize(14).SemiBold().FontColor(QColors.Teal.Darken2);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80); // Fecha
                                    columns.ConstantColumn(60); // Hora
                                    columns.RelativeColumn();   // Paciente
                                    columns.RelativeColumn();   // Dentista
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(QColors.Teal.Lighten4).Padding(5).Text("Fecha").SemiBold();
                                    header.Cell().Background(QColors.Teal.Lighten4).Padding(5).Text("Hora").SemiBold();
                                    header.Cell().Background(QColors.Teal.Lighten4).Padding(5).Text("Paciente").SemiBold();
                                    header.Cell().Background(QColors.Teal.Lighten4).Padding(5).Text("Dentista").SemiBold();
                                });

                                foreach (var appt in UpcomingAppointments)
                                {
                                    string date = ((dynamic)appt).AppointmentDate?.ToString() ?? "";
                                    string time = ((dynamic)appt).AppointmentTime?.ToString() ?? "";
                                    string patient = ((dynamic)appt).PatientName?.ToString() ?? "";
                                    string dentist = ((dynamic)appt).DentistName?.ToString() ?? "";

                                    table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Padding(5).Text(date);
                                    table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Padding(5).Text(time);
                                    table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Padding(5).Text(patient);
                                    table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Padding(5).Text(dentist);
                                }
                            });

                            // TABLA INGRESOS
                            col.Item().PaddingTop(20).PaddingBottom(5).Text("Ingresos por Dentista").FontSize(14).SemiBold().FontColor(QColors.Teal.Darken2);
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(100);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(QColors.Teal.Lighten4).Padding(5).Text("Dentista").SemiBold();
                                    header.Cell().Background(QColors.Teal.Lighten4).Padding(5).Text("Ingresos").SemiBold();
                                });

                                foreach (var d in DentistRevenue)
                                {
                                    table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Padding(5).Text(d.DentistName);
                                    table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten2).Padding(5).Text($"{d.Revenue:C}");
                                }
                            });
                        });

                        // PIE DE PÁGINA
                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("Página ");
                            x.CurrentPageNumber();
                            x.Span(" de ");
                            x.TotalPages();
                        });
                    });
                }).GeneratePdf(fileName);

                MessageBox.Show($"Reporte PDF generado correctamente:\n{fileName}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ═══════════════════════════════════════════════════════════════
        // EXPORTACIÓN A EXCEL (ClosedXML - Profesional)
        // ═══════════════════════════════════════════════════════════════
        private void ExportReportToExcel()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Reporte_Dashboard_{DateTime.Now:yyyyMMdd_HHmm}",
                    DefaultExt = ".xlsx",
                    Filter = "Excel Workbook (.xlsx)|*.xlsx"
                };

                if (saveFileDialog.ShowDialog() != true) return;
                string fileName = saveFileDialog.FileName;

                using (var workbook = new XLWorkbook())
                {
                    var ws = workbook.Worksheets.Add("Dashboard");

                    // TÍTULO PRINCIPAL
                    ws.Cell(1, 1).Value = "Clínica Dental - Reporte del Dashboard";
                    ws.Range(1, 1, 1, 6).Merge();
                    ws.Cell(1, 1).Style.Font.Bold = true;
                    ws.Cell(1, 1).Style.Font.FontSize = 16;
                    ws.Cell(1, 1).Style.Font.FontColor = XLColor.White;
                    ws.Cell(1, 1).Style.Fill.BackgroundColor = XLColor.Teal;
                    ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Row(1).Height = 30;

                    ws.Cell(2, 1).Value = $"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}";
                    ws.Range(2, 1, 2, 6).Merge();
                    ws.Cell(2, 1).Style.Font.Italic = true;
                    ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    // KPIs
                    ws.Cell(4, 1).Value = "Ingresos Totales";
                    ws.Cell(4, 2).Value = TotalRevenue;
                    ws.Cell(4, 2).Style.NumberFormat.Format = "$#,##0.00";
                    ws.Cell(4, 2).Style.Font.Bold = true;
                    ws.Cell(4, 2).Style.Font.FontColor = XLColor.Green;

                    ws.Cell(5, 1).Value = "Facturas sin Pagar";
                    ws.Cell(5, 2).Value = UnpaidInvoices;
                    ws.Cell(5, 2).Style.Font.Bold = true;
                    ws.Cell(5, 2).Style.Font.FontColor = XLColor.Orange;

                    // TABLA CITAS
                    int row = 7;
                    ws.Cell(row, 1).Value = "Próximas Citas";
                    ws.Range(row, 1, row, 4).Merge();
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Font.FontSize = 12;
                    ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    row++;

                    ws.Cell(row, 1).Value = "Fecha";
                    ws.Cell(row, 2).Value = "Hora";
                    ws.Cell(row, 3).Value = "Paciente";
                    ws.Cell(row, 4).Value = "Dentista";
                    ws.Range(row, 1, row, 4).Style.Font.Bold = true;
                    ws.Range(row, 1, row, 4).Style.Fill.BackgroundColor = XLColor.Teal;
                    ws.Range(row, 1, row, 4).Style.Font.FontColor = XLColor.White;
                    ws.Range(row, 1, row, 4).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    row++;

                    foreach (var appt in UpcomingAppointments)
                    {
                        ws.Cell(row, 1).Value = ((dynamic)appt).AppointmentDate.ToString();
                        ws.Cell(row, 2).Value = ((dynamic)appt).AppointmentTime.ToString();
                        ws.Cell(row, 3).Value = ((dynamic)appt).PatientName.ToString();
                        ws.Cell(row, 4).Value = ((dynamic)appt).DentistName.ToString();
                        row++;
                    }

                    // TABLA INGRESOS
                    row += 2;
                    ws.Cell(row, 1).Value = "Ingresos por Dentista";
                    ws.Range(row, 1, row, 2).Merge();
                    ws.Cell(row, 1).Style.Font.Bold = true;
                    ws.Cell(row, 1).Style.Font.FontSize = 12;
                    ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
                    row++;

                    ws.Cell(row, 1).Value = "Dentista";
                    ws.Cell(row, 2).Value = "Ingresos";
                    ws.Range(row, 1, row, 2).Style.Font.Bold = true;
                    ws.Range(row, 1, row, 2).Style.Fill.BackgroundColor = XLColor.Teal;
                    ws.Range(row, 1, row, 2).Style.Font.FontColor = XLColor.White;
                    ws.Range(row, 1, row, 2).Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                    row++;

                    foreach (var d in DentistRevenue)
                    {
                        ws.Cell(row, 1).Value = d.DentistName;
                        ws.Cell(row, 2).Value = d.Revenue;
                        ws.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                        row++;
                    }

                    // Ajustes finales
                    ws.Columns().AdjustToContents();
                    ws.SheetView.FreezeRows(7);

                    workbook.SaveAs(fileName);
                    MessageBox.Show($"Reporte Excel generado correctamente:\n{fileName}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

