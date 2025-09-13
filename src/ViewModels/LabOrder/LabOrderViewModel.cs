using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.LabOrder;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.LabOrder
{
    public class LabOrderViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.LabOrder> LabOrders { get; private set; } = new();
        private Models.LabOrder? _selectedOrder;

        public Models.LabOrder? SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged();
                (EditOrderCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public string SearchText { get; set; } = string.Empty;

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Patient",
            "Dentist",
            "Order Date",
            "Status"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public LabOrderViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            AddOrderCommand = new RelayCommand(OpenAddOrder);
            EditOrderCommand = new RelayCommand(OpenEditOrder, CanEditOrder);
            ReloadCommand = new RelayCommand(LoadLabOrders);
            SearchCommand = new RelayCommand(SearchOrders);
            SortCommand = new RelayCommand(SortOrders); // Komenda sortowania

            LoadLabOrders();
        }

        private void LoadLabOrders()
        {
            try
            {
                LabOrders = new ObservableCollection<Models.LabOrder>(_context.LabOrder
                    .Include(lo => lo.Patient)
                    .Include(lo => lo.Dentist)
                    .ToList());
                OnPropertyChanged(nameof(LabOrders));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading lab orders: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditOrder() => SelectedOrder != null;

        private void OpenAddOrder()
        {
            var addOrderView = new AddLabOrderView
            {
                DataContext = new AddLabOrderViewModel(_context)
            };

            if (addOrderView.ShowDialog() == true)
            {
                LoadLabOrders();
            }
        }

        private void OpenEditOrder()
        {
            if (SelectedOrder == null) return;

            var editOrderView = new EditLabOrderView
            {
                DataContext = new EditLabOrderViewModel(_context, SelectedOrder)
            };

            if (editOrderView.ShowDialog() == true)
            {
                LoadLabOrders();
            }
        }

        private void SearchOrders()
        {
            try
            {
                var filtered = _context.LabOrder
                    .Include(lo => lo.Patient)
                    .Include(lo => lo.Dentist)
                    .Where(lo => lo.Patient.FirstName.Contains(SearchText) ||
                                lo.Patient.LastName.Contains(SearchText) ||
                                lo.Description.Contains(SearchText) ||
                                lo.Status.Contains(SearchText))
                    .ToList();

                LabOrders = new ObservableCollection<Models.LabOrder>(filtered);
                OnPropertyChanged(nameof(LabOrders));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortOrders()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || LabOrders == null)
                return;

            IEnumerable<Models.LabOrder> sortedOrders = SelectedSortOption switch
            {
                "Patient" => LabOrders.OrderBy(lo => lo.Patient.LastName).ThenBy(lo => lo.Patient.FirstName),
                "Dentist" => LabOrders.OrderBy(lo => lo.Dentist.LastName).ThenBy(lo => lo.Dentist.FirstName),
                "Order Date" => LabOrders.OrderBy(lo => lo.OrderDate),
                "Status" => LabOrders.OrderBy(lo => lo.Status),
                _ => LabOrders
            };

            LabOrders = new ObservableCollection<Models.LabOrder>(sortedOrders);
            OnPropertyChanged(nameof(LabOrders));
        }
    }
}
