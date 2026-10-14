using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
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
        private readonly Action<string, BaseViewModel> _openTabAction;
        private readonly DispatcherTimer _searchTimer;

        public ObservableCollection<Models.Invoice> Invoices { get; private set; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    _searchTimer.Stop();
                    _searchTimer.Start();
                }
            }
        }

        private string _resultsLabel = string.Empty;
        public string ResultsLabel { get => _resultsLabel; set { _resultsLabel = value; OnPropertyChanged(); } }

        private Models.Invoice _selectedInvoice;
        public Models.Invoice SelectedInvoice
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

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "ID Factura", "Paciente", "Dentista", "Monto Total", "Fecha de Cita"
        };

        private string _selectedSortOption;
        public string SelectedSortOption
        {
            get => _selectedSortOption;
            set
            {
                if (_selectedSortOption != value)
                {
                    _selectedSortOption = value;
                    OnPropertyChanged();
                    SortInvoices();
                }
            }
        }

        public ICommand SortCommand { get; }
        public ICommand AddInvoiceCommand { get; }
        public ICommand EditInvoiceCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand OpenInvoiceDetailCommand { get; }

        public InvoiceViewModel(DentalClinicContext context, Action<string, BaseViewModel> openTabAction)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction ?? throw new ArgumentNullException(nameof(openTabAction));
            Invoices = new ObservableCollection<Models.Invoice>();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); SearchInvoices(); };

            AddInvoiceCommand = new RelayCommand(OpenAddInvoice);
            EditInvoiceCommand = new RelayCommand(OpenEditInvoice, () => SelectedInvoice != null);
            ReloadCommand = new RelayCommand(ReloadAll);
            SearchCommand = new RelayCommand(SearchInvoices);
            SortCommand = new RelayCommand(SortInvoices);
            OpenInvoiceDetailCommand = new RelayCommand(OpenInvoiceDetail, () => SelectedInvoice != null);

            LoadInvoices();
        }

        private void LoadInvoices()
        {
            try
            {
                var list = _context.Invoice
                    .Include(i => i.Appointment).ThenInclude(a => a.Patient)
                    .Include(i => i.Appointment.Dentist)
                    .Include(i => i.Appointment.Treatment)
                    .ToList();
                Invoices.Clear();
                foreach (var i in list) Invoices.Add(i);
                UpdateResultsLabel(list.Count, false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar las facturas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadAll()
        {
            _searchTimer.Stop();
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            LoadInvoices();
        }

        private void UpdateResultsLabel(int count, bool filtered)
        {
            if (count == 0) ResultsLabel = "Sin resultados";
            else if (count == 1) ResultsLabel = filtered ? "1 resultado" : "1 factura";
            else ResultsLabel = filtered ? $"{count} resultados" : $"{count} facturas";
        }

        private void SortInvoices()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "ID Factura" => Invoices.OrderBy(i => i.InvoiceID).ToList(),
                "Paciente" => Invoices.OrderBy(i => i.Appointment?.Patient?.LastName).ThenBy(i => i.Appointment?.Patient?.FirstName).ToList(),
                "Dentista" => Invoices.OrderBy(i => i.Appointment?.Dentist?.LastName).ThenBy(i => i.Appointment?.Dentist?.FirstName).ToList(),
                "Monto Total" => Invoices.OrderBy(i => i.TotalAmount).ToList(),
                "Fecha de Cita" => Invoices.OrderBy(i => i.Appointment?.AppointmentDate).ToList(),
                _ => Invoices.ToList()
            };
            Invoices.Clear();
            foreach (var i in sorted) Invoices.Add(i);
        }

        private void SearchInvoices()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadInvoices(); return; }
            var term = SearchText.Trim().ToLower();
            var filtered = _context.Invoice
                .Include(i => i.Appointment).ThenInclude(a => a.Patient)
                .Include(i => i.Appointment.Dentist)
                .Include(i => i.Appointment.Treatment)
                .ToList()
                .Where(i =>
                    (i.Appointment?.Patient?.FirstName?.ToLower().Contains(term) ?? false) ||
                    (i.Appointment?.Patient?.LastName?.ToLower().Contains(term) ?? false) ||
                    (i.Appointment?.Dentist?.FirstName?.ToLower().Contains(term) ?? false) ||
                    (i.Appointment?.Dentist?.LastName?.ToLower().Contains(term) ?? false) ||
                    (i.Appointment?.Treatment?.Name?.ToLower().Contains(term) ?? false) ||
                    i.InvoiceID.ToString().Contains(term))
                .ToList();
            Invoices.Clear();
            foreach (var i in filtered) Invoices.Add(i);
            UpdateResultsLabel(filtered.Count, true);
        }

        private void OpenAddInvoice()
        {
            var v = new Views.Invoice.AddInvoiceView { DataContext = new AddInvoiceViewModel(_context) };
            if (v.ShowDialog() == true) LoadInvoices();
        }

        private void OpenEditInvoice()
        {
            if (SelectedInvoice == null) return;
            var v = new Views.Invoice.EditInvoiceView { DataContext = new EditInvoiceViewModel(_context, SelectedInvoice) };
            if (v.ShowDialog() == true) LoadInvoices();
        }

        private void OpenInvoiceDetail()
        {
            if (SelectedInvoice == null) return;
            var detailVm = new InvoiceDetailViewModel(_context, SelectedInvoice);
            _openTabAction.Invoke($"Factura: {SelectedInvoice.InvoiceID}", detailVm);
        }
    }
}
