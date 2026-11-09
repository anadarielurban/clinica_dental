using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QColors = QuestPDF.Helpers.Colors;

namespace DentalClinicWPF.ViewModels.Invoice
{
    public class InvoiceViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.Invoice> Invoices { get; private set; }

        private Models.Invoice _selectedInvoice;
        public Models.Invoice SelectedInvoice
        {
            get => _selectedInvoice;
            set { _selectedInvoice = value; OnPropertyChanged(); }
        }

        // Filtros por fecha
        private DateTime _startDate = DateTime.Today.AddDays(-30);
        public DateTime StartDate
        {
            get => _startDate;
            set { if (SetProperty(ref _startDate, value)) LoadInvoices(); }
        }

        private DateTime _endDate = DateTime.Today;
        public DateTime EndDate
        {
            get => _endDate;
            set { if (SetProperty(ref _endDate, value)) LoadInvoices(); }
        }

        // Búsqueda de texto
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    LoadInvoices(); // Filtra al escribir
                }
            }
        }

        // Paginación
        private List<Models.Invoice> _allFilteredInvoices = new();
        private int _currentPage = 1;
        private int _pageSize = 15;
        public string PageInfo => $"Página {_currentPage} de {Math.Max(1, (int)Math.Ceiling((double)_allFilteredInvoices.Count / _pageSize))}";
        public bool CanGoNextPage => _currentPage < (int)Math.Ceiling((double)_allFilteredInvoices.Count / _pageSize);
        public bool CanGoPrevPage => _currentPage > 1;

        // Comandos
        public ICommand ReloadCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PrevPageCommand { get; }
        public ICommand PrintTicketCommand { get; }

        public InvoiceViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Invoices = new ObservableCollection<Models.Invoice>();

            ReloadCommand = new RelayCommand(LoadInvoices);
            NextPageCommand = new RelayCommand(() => ChangePage(1), () => CanGoNextPage);
            PrevPageCommand = new RelayCommand(() => ChangePage(-1), () => CanGoPrevPage);
            
            PrintTicketCommand = new RelayCommand<Models.Invoice>(PrintTicket);

            LoadInvoices();
        }

        private void ChangePage(int direction)
        {
            _currentPage += direction;
            UpdatePagedInvoices();
            OnPropertyChanged(nameof(PageInfo));
            OnPropertyChanged(nameof(CanGoNextPage));
            OnPropertyChanged(nameof(CanGoPrevPage));
            (NextPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
            (PrevPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
        }

        private void UpdatePagedInvoices()
        {
            Invoices.Clear();
            var paged = _allFilteredInvoices.Skip((_currentPage - 1) * _pageSize).Take(_pageSize).ToList();
            foreach (var inv in paged) Invoices.Add(inv);
        }

        private void LoadInvoices()
        {
            try
            {
                var query = _context.Invoice
                    .Include(i => i.Patient)
                    .Include(i => i.Appointment)
                        .ThenInclude(a => a.Dentist)
                    .Include(i => i.Appointment)
                        .ThenInclude(a => a.Treatment)
                    .Where(i => i.InvoiceDate >= StartDate.Date && i.InvoiceDate <= EndDate.Date.AddDays(1).AddSeconds(-1))
                    .ToList();

                // Filtro de búsqueda de texto
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var term = SearchText.Trim().ToLower();
                    query = query.Where(i =>
                        (i.Patient?.FirstName?.ToLower().Contains(term) ?? false) ||
                        (i.Patient?.LastName?.ToLower().Contains(term) ?? false) ||
                        i.InvoiceID.ToString().Contains(term)).ToList();
                }

                _allFilteredInvoices = query.OrderByDescending(i => i.InvoiceDate).ToList();
                _currentPage = 1;
                UpdatePagedInvoices();

                OnPropertyChanged(nameof(PageInfo));
                OnPropertyChanged(nameof(CanGoNextPage));
                OnPropertyChanged(nameof(CanGoPrevPage));
                (NextPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (PrevPageCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar facturas: {ex.Message}");
            }
        }

        private void PrintTicket(Models.Invoice invoice)
        {
            if (invoice == null) return;

            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var saveFileDialog = new SaveFileDialog
                {
                    FileName = $"Factura_{invoice.InvoiceID:D6}",
                    DefaultExt = ".pdf",
                    Filter = "PDF Documents (.pdf)|*.pdf"
                };

                if (saveFileDialog.ShowDialog() != true) return;
                string fileName = saveFileDialog.FileName;

                QuestPDF.Fluent.Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A5); // Tamaño media carta (A5) ideal para facturas
                        page.Margin(1.5f, Unit.Centimetre);
                        page.PageColor(QColors.White);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily(QuestPDF.Helpers.Fonts.SegoeUI));

                        // ENCABEZADO
                        page.Header().Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Clínica Dental").FontSize(20).SemiBold().FontColor(QColors.Teal.Medium);
                                    c.Item().Text("Factura de Venta").FontSize(12).FontColor(QColors.Grey.Darken2);
                                });
                                row.ConstantItem(120).AlignRight().Column(c =>
                                {
                                    c.Item().Text($"Folio: #{invoice.InvoiceID:D6}").Bold();
                                    c.Item().Text($"Fecha: {invoice.InvoiceDate:dd/MM/yyyy HH:mm}");
                                });
                            });
                            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(QColors.Grey.Lighten2);
                        });

                        // CONTENIDO
                        page.Content().PaddingVertical(15).Column(col =>
                        {
                            // Datos del Paciente
                            col.Item().PaddingBottom(10).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("DATOS DEL PACIENTE").FontSize(9).FontColor(QColors.Grey.Medium).SemiBold();
                                    c.Item().Text($"{invoice.Patient?.FirstName} {invoice.Patient?.LastName}").FontSize(11).Bold();
                                    c.Item().Text($"Tel: {invoice.Patient?.PhoneNumber}");
                                });
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("DATOS DE ATENCIÓN").FontSize(9).FontColor(QColors.Grey.Medium).SemiBold();
                                    c.Item().Text($"Dentista: {invoice.Appointment?.Dentist?.FirstName} {invoice.Appointment?.Dentist?.LastName}");
                                    c.Item().Text($"Tratamiento: {invoice.Appointment?.Treatment?.Name ?? "N/A"}");
                                });
                            });

                            // Tabla
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3); // Concepto
                                    columns.RelativeColumn(1); // Total
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Background(QColors.Teal.Lighten4).Padding(5).Text("CONCEPTO").SemiBold();
                                    header.Cell().Background(QColors.Teal.Lighten4).Padding(5).AlignRight().Text("TOTAL").SemiBold();
                                });

                                table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten3).Padding(8).Text(invoice.Appointment?.Treatment?.Name ?? "Servicios Dentales");
                                table.Cell().BorderBottom(1).BorderColor(QColors.Grey.Lighten3).Padding(8).AlignRight().Text($"{invoice.TotalAmount:C}");
                            });

                            // Totales
                            col.Item().PaddingTop(15).AlignRight().Column(c =>
                            {
                                c.Item().Text($"Subtotal: {invoice.TotalAmount:C}");
                                c.Item().Text($"Pagado: {invoice.AmountPaid:C}");
                                c.Item().PaddingTop(5).Text($"TOTAL: {invoice.TotalAmount:C}").FontSize(14).Bold().FontColor(QColors.Teal.Darken2);
                            });
                        });

                        page.Footer().AlignCenter().Text(x =>
                        {
                            x.Span("¡Gracias por su preferencia!  |  Página ");
                            x.CurrentPageNumber();
                        });
                    });
                }).GeneratePdf(fileName);

                MessageBox.Show($"Factura generada correctamente:\n{fileName}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar factura: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}