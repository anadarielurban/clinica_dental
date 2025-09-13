using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Models;
using Microsoft.Win32;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
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
        public ICommand ExportToExcelCommand { get; } // Added Excel export command

        public DashboardViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTabAction)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction ?? throw new ArgumentNullException(nameof(openTabAction));

            AddAppointmentCommand = new RelayCommand(OpenAddAppointment);
            RegisterPatientCommand = new RelayCommand(OpenRegisterPatient);
            ExportReportCommand = new RelayCommand(ExportReportAsPdf);
            ExportToExcelCommand = new RelayCommand(ExportReportToExcel); // Initialize command

            // Load Dashboard data
            LoadDashboardData();
        }

        private void LoadDashboardData()
        {
            try
            {
                // Upcoming Appointments (max 10)
                var appointments = _context.Appointment
                    .Include(a => a.Patient)
                    .Include(a => a.Dentist)
                    .Where(a => a.AppointmentDate.Date > DateTime.Now.Date ||
                                (a.AppointmentDate.Date == DateTime.Now.Date && a.StartTime > DateTime.Now.TimeOfDay))
                    .OrderBy(a => a.AppointmentDate)
                    .ThenBy(a => a.StartTime)
                    .Take(10)
                    .Select(a => new
                    {
                        AppointmentDate = a.AppointmentDate.ToString("d"),
                        AppointmentTime = a.StartTime.ToString(@"hh\:mm"),
                        // Alternatively, for 12-hour format with AM/PM:
                        // AppointmentTime = DateTime.Today.Add(a.StartTime).ToString("hh:mm tt"),
                        PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                        DentistName = a.Dentist.FirstName + " " + a.Dentist.LastName
                    })
                    .ToList();

                UpcomingAppointments = new ObservableCollection<object>(appointments);

                // Financial Statistics
                TotalRevenue = _context.Invoice.Sum(i => i.TotalAmount);
                UnpaidInvoices = _context.Invoice.Count(i => !i.IsPaid);

                // Revenue per Dentist
                var revenuePerDentist = _context.Dentist
                    .Select(d => new DentistRevenue
                    {
                        DentistName = d.FirstName + " " + d.LastName,
                        Revenue = d.Appointments
                            .Where(a => a.AppointmentDate.Date > DateTime.Now.Date ||
                                        (a.AppointmentDate.Date == DateTime.Now.Date && a.StartTime > DateTime.Now.TimeOfDay))
                            .Sum(a => a.Treatment.Price)
                    })
                    .ToList();

                DentistRevenue = new ObservableCollection<DentistRevenue>(revenuePerDentist);

                // Load Revenue Chart
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
                // Show revenue for the last 6 months
                var sixMonthsAgo = DateTime.Now.AddMonths(-5);
                var revenueData = _context.Invoice
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

                // Prepare labels and data
                RevenueLabels = new List<string>();
                var values = new ChartValues<double>();

                // Fill missing months with zero revenue
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

                // Initialize chart series
                RevenueSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Revenue",
                        Values = values,
                        Fill = System.Windows.Media.Brushes.SteelBlue
                    }
                };

                // Initialize Y-axis formatter
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
                    // Refresh dashboard data after adding a new appointment
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
                    // Refresh dashboard data after registering a new patient
                    LoadDashboardData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while opening Register Patient window: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportReportAsPdf()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = "DashboardReport",
                    DefaultExt = ".pdf",
                    Filter = "PDF Documents (.pdf)|*.pdf"
                };

                bool? result = saveFileDialog.ShowDialog();
                if (result != true) return;

                string fileName = saveFileDialog.FileName;

                using (var pdfDocument = new PdfDocument())
                {
                    pdfDocument.Info.Title = "Dashboard Report";
                    var page = pdfDocument.AddPage();
                    var gfx = XGraphics.FromPdfPage(page);

                    // Fonts
                    var fontTitle = new XFont("Segoe UI", 20, XFontStyle.Bold);
                    var fontHeader = new XFont("Segoe UI", 14, XFontStyle.Bold);
                    var fontRegular = new XFont("Segoe UI", 12, XFontStyle.Regular);

                    double yPoint = 40;

                    // Title
                    gfx.DrawString("Dashboard Report", fontTitle, XBrushes.Black,
                        new XRect(0, yPoint, page.Width, page.Height),
                        XStringFormats.TopCenter);
                    yPoint += 40;

                    // Financial Statistics
                    gfx.DrawString($"Total Revenue: {TotalRevenue:C}", fontHeader, XBrushes.Black, 40, yPoint);
                    yPoint += 30;
                    gfx.DrawString($"Unpaid Invoices: {UnpaidInvoices}", fontHeader, XBrushes.Black, 40, yPoint);
                    yPoint += 40;

                    // Add Upcoming Appointments Table
                    gfx.DrawString("Upcoming Appointments:", fontHeader, XBrushes.Black, 40, yPoint);
                    yPoint += 20;

                    // Table Headers
                    gfx.DrawString("Date", fontRegular, XBrushes.Black, 40, yPoint);
                    gfx.DrawString("Time", fontRegular, XBrushes.Black, 140, yPoint);
                    gfx.DrawString("Patient", fontRegular, XBrushes.Black, 240, yPoint);
                    gfx.DrawString("Dentist", fontRegular, XBrushes.Black, 340, yPoint);
                    yPoint += 20;

                    // Table Data
                    foreach (var appointment in UpcomingAppointments)
                    {
                        var date = ((dynamic)appointment).AppointmentDate;
                        var time = ((dynamic)appointment).AppointmentTime;
                        var patient = ((dynamic)appointment).PatientName;
                        var dentist = ((dynamic)appointment).DentistName;

                        gfx.DrawString(date, fontRegular, XBrushes.Black, 40, yPoint);
                        gfx.DrawString(time, fontRegular, XBrushes.Black, 140, yPoint);
                        gfx.DrawString(patient, fontRegular, XBrushes.Black, 240, yPoint);
                        gfx.DrawString(dentist, fontRegular, XBrushes.Black, 340, yPoint);
                        yPoint += 20;
                    }

                    // Add Revenue per Dentist Table
                    yPoint += 20;
                    gfx.DrawString("Revenue per Dentist:", fontHeader, XBrushes.Black, 40, yPoint);
                    yPoint += 20;

                    // Table Headers
                    gfx.DrawString("Dentist", fontRegular, XBrushes.Black, 40, yPoint);
                    gfx.DrawString("Revenue", fontRegular, XBrushes.Black, 240, yPoint);
                    yPoint += 20;

                    // Table Data
                    foreach (var dentist in DentistRevenue)
                    {
                        gfx.DrawString(dentist.DentistName, fontRegular, XBrushes.Black, 40, yPoint);
                        gfx.DrawString(dentist.Revenue.ToString("C"), fontRegular, XBrushes.Black, 240, yPoint);
                        yPoint += 20;
                    }

                    // Save PDF
                    pdfDocument.Save(fileName);
                    MessageBox.Show($"Report exported successfully:\n{fileName}", "Export to PDF", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report to PDF: {ex.Message}", "Export PDF Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportReportToExcel()
        {
            try
            {
                var saveFileDialog = new SaveFileDialog
                {
                    FileName = "DashboardReport",
                    DefaultExt = ".xlsx",
                    Filter = "Excel Workbook (.xlsx)|*.xlsx"
                };

                bool? result = saveFileDialog.ShowDialog();
                if (result != true) return;

                string fileName = saveFileDialog.FileName;

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Dashboard Report");

                    // Title
                    worksheet.Cell(1, 1).Value = "Dashboard Report";
                    worksheet.Range(1, 1, 1, 4).Merge().AddToNamed("Titles");
                    worksheet.Row(1).Height = 20;
                    worksheet.Cell(1, 1).Style.Font.Bold = true;
                    worksheet.Cell(1, 1).Style.Font.FontSize = 16;
                    worksheet.Cell(1, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;

                    // Financial Statistics
                    worksheet.Cell(3, 1).Value = "Total Revenue";
                    worksheet.Cell(3, 2).Value = TotalRevenue;
                    worksheet.Cell(3, 2).Style.NumberFormat.Format = "$#,##0.00";

                    worksheet.Cell(4, 1).Value = "Unpaid Invoices";
                    worksheet.Cell(4, 2).Value = UnpaidInvoices;

                    // Upcoming Appointments
                    worksheet.Cell(6, 1).Value = "Upcoming Appointments";
                    worksheet.Cell(6, 1).Style.Font.Bold = true;
                    int row = 7;

                    // Table Headers
                    worksheet.Cell(row, 1).Value = "Date";
                    worksheet.Cell(row, 2).Value = "Time";
                    worksheet.Cell(row, 3).Value = "Patient";
                    worksheet.Cell(row, 4).Value = "Dentist";
                    row++;

                    // Table Data
                    foreach (var appointment in UpcomingAppointments)
                    {
                        var date = ((dynamic)appointment).AppointmentDate;
                        var time = ((dynamic)appointment).AppointmentTime;
                        var patient = ((dynamic)appointment).PatientName;
                        var dentist = ((dynamic)appointment).DentistName;

                        worksheet.Cell(row, 1).Value = date;
                        worksheet.Cell(row, 2).Value = time;
                        worksheet.Cell(row, 3).Value = patient;
                        worksheet.Cell(row, 4).Value = dentist;
                        row++;
                    }

                    // Revenue per Dentist
                    row += 2;
                    worksheet.Cell(row, 1).Value = "Revenue per Dentist";
                    worksheet.Cell(row, 1).Style.Font.Bold = true;
                    row++;

                    // Table Headers
                    worksheet.Cell(row, 1).Value = "Dentist";
                    worksheet.Cell(row, 2).Value = "Revenue";
                    row++;

                    // Table Data
                    foreach (var dentist in DentistRevenue)
                    {
                        worksheet.Cell(row, 1).Value = dentist.DentistName;
                        worksheet.Cell(row, 2).Value = dentist.Revenue;
                        worksheet.Cell(row, 2).Style.NumberFormat.Format = "$#,##0.00";
                        row++;
                    }

                    // Save Excel
                    workbook.SaveAs(fileName);
                    MessageBox.Show($"Report exported successfully:\n{fileName}", "Export to Excel", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report to Excel: {ex.Message}", "Export Excel Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
