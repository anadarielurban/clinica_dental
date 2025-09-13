using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
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

        public ObservableCollection<Models.Employee> Employees { get; private set; } = new();
        private Models.Employee _selectedEmployee;

        public Models.Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                OnPropertyChanged();
                (EditEmployeeCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public string SearchText { get; set; } = string.Empty;

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "First Name",
            "Last Name",
            "Position"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand SearchCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand AddEmployeeCommand { get; }
        public ICommand EditEmployeeCommand { get; }

        public EmployeeViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            SearchCommand = new RelayCommand(SearchEmployees);
            ReloadCommand = new RelayCommand(LoadEmployees);
            AddEmployeeCommand = new RelayCommand(OpenAddEmployee);
            EditEmployeeCommand = new RelayCommand(OpenEditEmployee, CanEditEmployee);
            SortCommand = new RelayCommand(SortEmployees); // Komenda sortowania

            LoadEmployees();
        }

        private void LoadEmployees()
        {
            Employees = new ObservableCollection<Models.Employee>(
                _context.Employee
                        .Include(e => e.Dentist)
                        .ToList()
            );
            OnPropertyChanged(nameof(Employees));
        }

        private void SearchEmployees()
        {
            var filteredEmployees = _context.Employee
                .Include(e => e.Dentist)
                .Where(e => e.FirstName.Contains(SearchText) || 
                            e.LastName.Contains(SearchText) ||
                            e.Position.Contains(SearchText))
                .ToList();

            Employees = new ObservableCollection<Models.Employee>(filteredEmployees);
            OnPropertyChanged(nameof(Employees));
        }

        private void SortEmployees()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Employees == null)
                return;

            IEnumerable<Models.Employee> sortedEmployees = SelectedSortOption switch
            {
                "First Name" => Employees.OrderBy(e => e.FirstName),
                "Last Name" => Employees.OrderBy(e => e.LastName),
                "Position" => Employees.OrderBy(e => e.Position),
                _ => Employees
            };

            Employees = new ObservableCollection<Models.Employee>(sortedEmployees);
            OnPropertyChanged(nameof(Employees));
        }

        private void OpenAddEmployee()
        {
            var addEmployeeView = new AddEmployeeView
            {
                DataContext = new AddEmployeeViewModel(_context)
            };
            addEmployeeView.ShowDialog();
            LoadEmployees();
        }

        private void OpenEditEmployee()
        {
            if (SelectedEmployee == null) return;

            var editEmployeeView = new EditEmployeeView
            {
                DataContext = new EditEmployeeViewModel(SelectedEmployee, _context)
            };
            editEmployeeView.ShowDialog();
            LoadEmployees();
        }

        private bool CanEditEmployee() => SelectedEmployee != null;
    }
}
