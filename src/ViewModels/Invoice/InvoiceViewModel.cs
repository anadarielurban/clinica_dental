using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Invoice
{
    public class InvoiceViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Action<string, BaseViewModel> _openTabAction;  // Akcja otwierająca zakładkę

        public ObservableCollection<Models.Invoice> Invoices { get; set; }
        private Models.Invoice? _selectedInvoice;
        public Models.Invoice? SelectedInvoice
        {
            get => _selectedInvoice;
            set
            {
                _selectedInvoice = value;
                OnPropertyChanged();
                (EditInvoiceCommand as RelayCommand)?.NotifyCanExecuteChanged();
                (OpenInvoiceDetailCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public string SearchText { get; set; } = string.Empty;

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Invoice ID",
            "Patient",
            "Dentist",
            "Total Amount",
            "Appointment Date"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddInvoiceCommand { get; }
        public ICommand EditInvoiceCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        // Nowa komenda otwierania szczegółów faktury
        public ICommand OpenInvoiceDetailCommand { get; }

        // Konstruktor – dodatkowy parametr openTabAction
        public InvoiceViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTabAction)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction ?? throw new ArgumentNullException(nameof(openTabAction));

            AddInvoiceCommand = new RelayCommand(OpenAddInvoice);
            EditInvoiceCommand = new RelayCommand(OpenEditInvoice, () => SelectedInvoice != null);
            ReloadCommand = new RelayCommand(LoadInvoices);
            SearchCommand = new RelayCommand(SearchInvoices);
            SortCommand = new RelayCommand(SortInvoices);
            OpenInvoiceDetailCommand = new RelayCommand(OpenInvoiceDetail, () => SelectedInvoice != null);

            LoadInvoices();
        }

        private void LoadInvoices()
        {
            try
            {
                Invoices = new ObservableCollection<Models.Invoice>(_context.Invoice
                    .Include(i => i.Appointment)
                        .ThenInclude(a => a.Patient)
                    .Include(i => i.Appointment.Dentist)
                    .Include(i => i.Appointment.Treatment)
                    .ToList());
                OnPropertyChanged(nameof(Invoices));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading invoices: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenAddInvoice()
        {
            var addInvoiceView = new Views.Invoice.AddInvoiceView
            {
                DataContext = new AddInvoiceViewModel(_context)
            };

            if (addInvoiceView.ShowDialog() == true)
            {
                LoadInvoices();
            }
        }

        private void OpenEditInvoice()
        {
            if (SelectedInvoice == null) return;
            var editInvoiceView = new Views.Invoice.EditInvoiceView
            {
                DataContext = new EditInvoiceViewModel(_context, SelectedInvoice)
            };

            if (editInvoiceView.ShowDialog() == true)
            {
                LoadInvoices();
            }
        }

        private void SearchInvoices()
        {
            try
            {
                var filtered = _context.Invoice
                    .Include(i => i.Appointment)
                        .ThenInclude(a => a.Patient)
                    .Include(i => i.Appointment.Dentist)
                    .Include(i => i.Appointment.Treatment)
                    .Where(i => i.Appointment.Patient.FirstName.Contains(SearchText) ||
                                i.Appointment.Patient.LastName.Contains(SearchText) ||
                                i.Appointment.Dentist.FirstName.Contains(SearchText) ||
                                i.Appointment.Dentist.LastName.Contains(SearchText) ||
                                i.Appointment.Treatment.Name.Contains(SearchText) ||
                                i.InvoiceID.ToString().Contains(SearchText))
                    .ToList();

                Invoices = new ObservableCollection<Models.Invoice>(filtered);
                OnPropertyChanged(nameof(Invoices));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortInvoices()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Invoices == null)
                return;

            IEnumerable<Models.Invoice> sortedInvoices = SelectedSortOption switch
            {
                "Invoice ID" => Invoices.OrderBy(i => i.InvoiceID),
                "Patient" => Invoices.OrderBy(i => i.Appointment.Patient.LastName)
                                      .ThenBy(i => i.Appointment.Patient.FirstName),
                "Dentist" => Invoices.OrderBy(i => i.Appointment.Dentist.LastName)
                                      .ThenBy(i => i.Appointment.Dentist.FirstName),
                "Total Amount" => Invoices.OrderBy(i => i.TotalAmount),
                "Appointment Date" => Invoices.OrderBy(i => i.Appointment.AppointmentDate),
                _ => Invoices
            };

            Invoices = new ObservableCollection<Models.Invoice>(sortedInvoices);
            OnPropertyChanged(nameof(Invoices));
        }

        // Metoda otwierająca szczegóły faktury jako zakładkę
        private void OpenInvoiceDetail()
        {
            if (SelectedInvoice == null)
                return;
            var detailViewModel = new InvoiceDetailViewModel(_context, SelectedInvoice);
            _openTabAction.Invoke($"Invoice: {SelectedInvoice.InvoiceID}", detailViewModel);
        }
    }
}
