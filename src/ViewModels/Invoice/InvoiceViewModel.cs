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

        public InvoiceViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Invoices = new ObservableCollection<Models.Invoice>();

            ReloadCommand = new RelayCommand(LoadInvoices);
            NextPageCommand = new RelayCommand(() => ChangePage(1), () => CanGoNextPage);
            PrevPageCommand = new RelayCommand(() => ChangePage(-1), () => CanGoPrevPage);

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
                    .Where(i => i.InvoiceDate >= StartDate.Date && i.InvoiceDate <= EndDate.Date.AddDays(1).AddSeconds(-1))
                    .ToList();

                // Filtro de búsqueda de texto
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    var term = SearchText.Trim().ToLower();
                    query = query.Where(i => 
                        (i.Patient?.FirstName?.ToLower().Contains(term) ?? false) ||
                        (i.Patient?.LastName?.ToLower().Contains(term) ?? false)).ToList();
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
    }
}