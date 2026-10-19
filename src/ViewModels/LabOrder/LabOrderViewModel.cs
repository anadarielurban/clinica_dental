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

namespace DentalClinicWPF.ViewModels.LabOrder
{
    public class LabOrderViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.LabOrder> LabOrders { get; private set; }

        private Models.LabOrder _selectedLabOrder;
        public Models.LabOrder SelectedLabOrder
        {
            get => _selectedLabOrder;
            set
            {
                _selectedLabOrder = value;
                OnPropertyChanged();
                (EditOrderCommand as RelayCommand<Models.LabOrder>)?.NotifyCanExecuteChanged();
                (DeleteOrderCommand as RelayCommand<Models.LabOrder>)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Fecha de Orden", "Paciente", "Dentista"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }
        public ICommand AddOrderCommand { get; }
        public ICommand EditOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public LabOrderViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            LabOrders = new ObservableCollection<Models.LabOrder>();

            AddOrderCommand = new RelayCommand(OpenAdd);
            EditOrderCommand = new RelayCommand<Models.LabOrder>(OpenEditWithParam, o => o != null);
            DeleteOrderCommand = new RelayCommand<Models.LabOrder>(DeleteWithParam, o => o != null);
            ReloadCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchData);
            SortCommand = new RelayCommand(SortData);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var list = _context.LabOrder.Include(l => l.Patient).Include(l => l.Dentist).ToList();
                LabOrders.Clear();
                foreach (var o in list) LabOrders.Add(o);
            }
            catch (Exception ex) { MessageBox.Show($"Error al cargar: {ex.Message}"); }
        }

        private void SearchData()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadData(); return; }
            var term = SearchText.ToLower();
            var filtered = _context.LabOrder.Include(l => l.Patient).Include(l => l.Dentist).ToList()
                .Where(l => (l.Patient?.FirstName?.ToLower().Contains(term) ?? false) ||
                            (l.Patient?.LastName?.ToLower().Contains(term) ?? false) ||
                            (l.Description?.ToLower().Contains(term) ?? false)).ToList();
            LabOrders.Clear();
            foreach (var o in filtered) LabOrders.Add(o);
        }

        private void SortData()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "Fecha de Orden" => LabOrders.OrderBy(l => l.OrderDate).ToList(),
                "Paciente" => LabOrders.OrderBy(l => l.Patient?.LastName).ToList(),
                "Dentista" => LabOrders.OrderBy(l => l.Dentist?.LastName).ToList(),
                _ => LabOrders.ToList()
            };
            LabOrders.Clear();
            foreach (var o in sorted) LabOrders.Add(o);
        }

        private void OpenAdd()
        {
            var w = new Views.LabOrder.AddLabOrderView { DataContext = new AddLabOrderViewModel(_context) };
            if (w.ShowDialog() == true) LoadData();
        }

        private void OpenEditWithParam(Models.LabOrder order)
        {
            if (order == null) return;
            SelectedLabOrder = order;
            var w = new Views.LabOrder.EditLabOrderView { DataContext = new EditLabOrderViewModel(_context, order) };
            if (w.ShowDialog() == true) LoadData();
        }

        private void DeleteWithParam(Models.LabOrder order)
        {
            if (order == null) return;
            if (MessageBox.Show($"¿Seguro que quieres eliminar esta orden de laboratorio?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try { _context.LabOrder.Remove(order); _context.SaveChanges(); LoadData(); }
                catch (Exception ex) { MessageBox.Show($"Error al eliminar: {ex.Message}"); }
            }
        }
    }
}
