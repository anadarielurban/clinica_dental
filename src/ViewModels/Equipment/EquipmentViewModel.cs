using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Equipment
{
    public class EquipmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly DispatcherTimer _searchTimer;

        public ObservableCollection<Models.Equipment> EquipmentList { get; private set; }

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

        private Models.Equipment _selectedEquipment;
        public Models.Equipment SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                _selectedEquipment = value;
                OnPropertyChanged();
                (EditEquipmentCommand as RelayCommand<Models.Equipment>)?.NotifyCanExecuteChanged();
                (DeleteEquipmentCommand as RelayCommand<Models.Equipment>)?.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Nombre", "Modelo", "Número de Serie", "Fecha de Compra", "Último Servicio"
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
                    SortEquipment();
                }
            }
        }

        public ICommand SortCommand { get; }
        public ICommand AddEquipmentCommand { get; }
        public ICommand EditEquipmentCommand { get; }
        public ICommand DeleteEquipmentCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public EquipmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            EquipmentList = new ObservableCollection<Models.Equipment>();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); ApplyFilters(); };

            AddEquipmentCommand = new RelayCommand(OpenAddEquipmentView);
            EditEquipmentCommand = new RelayCommand<Models.Equipment>(OpenEditEquipmentWithParam, e => e != null);
            DeleteEquipmentCommand = new RelayCommand<Models.Equipment>(DeleteEquipmentWithParam, e => e != null);
            ReloadCommand = new RelayCommand(ReloadAll);
            SearchCommand = new RelayCommand(ApplyFilters);
            SortCommand = new RelayCommand(SortEquipment);

            LoadEquipment();
        }

        private void LoadEquipment()
        {
            try
            {
                var list = _context.Equipment.Include(e => e.Room).ToList();
                EquipmentList.Clear();
                foreach (var e in list) EquipmentList.Add(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los equipos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadAll()
        {
            _searchTimer.Stop();
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            LoadEquipment();
        }

        private void ApplyFilters()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadEquipment();
                return;
            }

            var term = SearchText.Trim().ToLower();
            var filtered = _context.Equipment.Include(e => e.Room).ToList()
                .Where(e =>
                    (e.Name?.ToLower().Contains(term) ?? false) ||
                    (e.Model?.ToLower().Contains(term) ?? false) ||
                    (e.SerialNumber?.ToLower().Contains(term) ?? false))
                .ToList();

            EquipmentList.Clear();
            foreach (var e in filtered) EquipmentList.Add(e);
        }

        private void SortEquipment()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "Nombre" => EquipmentList.OrderBy(e => e.Name).ToList(),
                "Modelo" => EquipmentList.OrderBy(e => e.Model).ToList(),
                "Número de Serie" => EquipmentList.OrderBy(e => e.SerialNumber).ToList(),
                "Fecha de Compra" => EquipmentList.OrderBy(e => e.PurchaseDate).ToList(),
                "Último Servicio" => EquipmentList.OrderBy(e => e.LastServiceDate).ToList(),
                _ => EquipmentList.ToList()
            };
            EquipmentList.Clear();
            foreach (var e in sorted) EquipmentList.Add(e);
        }

        private void OpenAddEquipmentView()
        {
            var w = new Views.Equipment.AddEquipmentView { DataContext = new AddEquipmentViewModel(_context) };
            if (w.ShowDialog() == true) LoadEquipment();
        }

        private void OpenEditEquipmentWithParam(Models.Equipment eq)
        {
            if (eq == null) return;
            SelectedEquipment = eq;
            var w = new Views.Equipment.EditEquipmentView { DataContext = new EditEquipmentViewModel(eq, _context) };
            if (w.ShowDialog() == true) LoadEquipment();
        }

        private void DeleteEquipmentWithParam(Models.Equipment eq)
        {
            if (eq == null) return;
            if (MessageBox.Show($"¿Seguro que quieres eliminar el equipo '{eq.Name}'?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Equipment.Remove(eq);
                    _context.SaveChanges();
                    LoadEquipment();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el equipo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
