using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.Views.Employee;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Employee
{
    public class EmployeeViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly DispatcherTimer _searchTimer;

        public ObservableCollection<Models.Employee> Employees { get; private set; }

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

        private Models.Employee _selectedEmployee;
        public Models.Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
                (EditEmployeeCommand as RelayCommand<Models.Employee>)?.NotifyCanExecuteChanged();
                (DeleteEmployeeCommand as RelayCommand<Models.Employee>)?.NotifyCanExecuteChanged();
            }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Nombre", "Apellido", "Puesto"
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
                    SortEmployees();
                }
            }
        }

        public ICommand SortCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand AddEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }
        public ICommand DeleteEmployeeCommand { get; }

        public EmployeeViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Employees = new ObservableCollection<Models.Employee>();

            _searchTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
            _searchTimer.Tick += (s, e) => { _searchTimer.Stop(); ApplyFilters(); };

            SearchCommand = new RelayCommand(ApplyFilters);
            ReloadCommand = new RelayCommand(ReloadAll);
            AddEmployeeCommand = new RelayCommand(OpenAddEmployee);
            EditEmployeeCommand = new RelayCommand<Models.Employee>(OpenEditEmployeeWithParam, e => e != null);
            DeleteEmployeeCommand = new RelayCommand<Models.Employee>(DeleteEmployeeWithParam, e => e != null);
            SortCommand = new RelayCommand(SortEmployees);

            LoadEmployees();
        }

        private void LoadEmployees()
        {
            try
            {
                var list = _context.Employee.Include(e => e.Dentist).ToList();
                Employees.Clear();
                foreach (var e in list) Employees.Add(e);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los empleados: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ReloadAll()
        {
            _searchTimer.Stop();
            _searchText = string.Empty;
            OnPropertyChanged(nameof(SearchText));
            LoadEmployees();
        }

        private void ApplyFilters()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                LoadEmployees();
                return;
            }

            var term = SearchText.Trim().ToLower();
            var filtered = _context.Employee.Include(e => e.Dentist).ToList()
                .Where(e =>
                    (e.FirstName?.ToLower().Contains(term) ?? false) ||
                    (e.LastName?.ToLower().Contains(term) ?? false) ||
                    (e.Position?.ToLower().Contains(term) ?? false))
                .ToList();

            Employees.Clear();
            foreach (var e in filtered) Employees.Add(e);
        }

        private void SortEmployees()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "Nombre" => Employees.OrderBy(e => e.FirstName).ToList(),
                "Apellido" => Employees.OrderBy(e => e.LastName).ToList(),
                "Puesto" => Employees.OrderBy(e => e.Position).ToList(),
                _ => Employees.ToList()
            };
            Employees.Clear();
            foreach (var e in sorted) Employees.Add(e);
        }

        private void OpenAddEmployee()
        {
            var w = new AddEmployeeView { DataContext = new AddEmployeeViewModel(_context) };
            if (w.ShowDialog() == true) LoadEmployees();
        }

        private void OpenEditEmployeeWithParam(Models.Employee emp)
        {
            if (emp == null) return;
            SelectedEmployee = emp;
            var w = new EditEmployeeView { DataContext = new EditEmployeeViewModel(emp, _context) };
            if (w.ShowDialog() == true) LoadEmployees();
        }

        private void DeleteEmployeeWithParam(Models.Employee emp)
        {
            if (emp == null) return;
            if (MessageBox.Show($"¿Seguro que quieres eliminar al empleado {emp.FirstName} {emp.LastName}?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Employee.Remove(emp);
                    _context.SaveChanges();
                    LoadEmployees();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el empleado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
