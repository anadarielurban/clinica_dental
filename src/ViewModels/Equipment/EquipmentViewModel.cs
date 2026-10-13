using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.Views.Equipment;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Equipment
{
    public class EquipmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.Equipment> EquipmentList { get; set; }
        private Models.Equipment _selectedEquipment;
        public Models.Equipment SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                _selectedEquipment = value;
                OnPropertyChanged();
                (EditEquipmentCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public string SearchText { get; set; }

        // Nowe właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Name",
            "Model",
            "Número de Serie",
            "Fecha de Compra",
            "Último Servicio"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddEquipmentCommand { get; }
        public ICommand EditEquipmentCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public EquipmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            LoadEquipment();

            AddEquipmentCommand = new RelayCommand(OpenAddEquipmentView);
            EditEquipmentCommand = new RelayCommand(OpenEditEquipmentView, CanEditEquipment);
            ReloadCommand = new RelayCommand(LoadEquipment);
            SearchCommand = new RelayCommand(SearchEquipment);
            SortCommand = new RelayCommand(SortEquipment); // Komenda sortowania
        }

        private void LoadEquipment()
        {
            try
            {
                var equipmentQuery = _context.Equipment.Include(e => e.Room).AsQueryable();

                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    equipmentQuery = equipmentQuery.Where(e =>
                        e.Name.Contains(SearchText) ||
                        e.Model.Contains(SearchText) ||
                        e.SerialNumber.Contains(SearchText));
                }

                var equipment = equipmentQuery.ToList();
                EquipmentList = new ObservableCollection<Models.Equipment>(equipment);
                OnPropertyChanged(nameof(EquipmentList));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los equipos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanEditEquipment()
        {
            return SelectedEquipment != null;
        }

        private void OpenAddEquipmentView()
        {
            var addEquipmentView = new AddEquipmentView
            {
                DataContext = new AddEquipmentViewModel(_context)
            };
            addEquipmentView.ShowDialog();
            LoadEquipment();
        }

        private void OpenEditEquipmentView()
        {
            if (SelectedEquipment == null)
            {
                MessageBox.Show("Selecciona un equipo para editar.", "Sin Selección", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var editEquipmentView = new EditEquipmentView
            {
                DataContext = new EditEquipmentViewModel(SelectedEquipment, _context)
            };
            editEquipmentView.ShowDialog();
            LoadEquipment();
        }

        private void SearchEquipment()
        {
            LoadEquipment();
        }

        private void SortEquipment()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || EquipmentList == null)
                return;

            IEnumerable<Models.Equipment> sortedEquipment = SelectedSortOption switch
            {
                "Name" => EquipmentList.OrderBy(e => e.Name),
                "Model" => EquipmentList.OrderBy(e => e.Model),
                "Número de Serie" => EquipmentList.OrderBy(e => e.SerialNumber),
                "Fecha de Compra" => EquipmentList.OrderBy(e => e.PurchaseDate),
                "Último Servicio" => EquipmentList.OrderBy(e => e.LastServiceDate),
                _ => EquipmentList
            };

            EquipmentList = new ObservableCollection<Models.Equipment>(sortedEquipment);
            OnPropertyChanged(nameof(EquipmentList));
        }
    }
}
