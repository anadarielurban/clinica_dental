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
using DentalClinicWPF.Views.Schedule;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Schedule
{
    public class ScheduleViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        private ObservableCollection<Models.Schedule> _schedules;
        public ObservableCollection<Models.Schedule> Schedules
        {
            get => _schedules;
            private set { _schedules = value; OnPropertyChanged(); }
        }

        private Models.Schedule _selectedSchedule;
        public Models.Schedule SelectedSchedule
        {
            get => _selectedSchedule;
            set
            {
                _selectedSchedule = value;
                OnPropertyChanged();
                (EditScheduleCommand as RelayCommand<Models.Schedule>)?.NotifyCanExecuteChanged();
                (DeleteScheduleCommand as RelayCommand<Models.Schedule>)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Dentista", "Fecha de Inicio", "Fecha de Fin", "Día Libre"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }
        public ICommand AddScheduleCommand { get; }
        public ICommand EditScheduleCommand { get; }
        public ICommand DeleteScheduleCommand { get; }
        public ICommand ReloadSchedulesCommand { get; }
        public ICommand SearchCommand { get; }

        public ScheduleViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Schedules = new ObservableCollection<Models.Schedule>();

            AddScheduleCommand = new RelayCommand(OpenAddSchedule);
            EditScheduleCommand = new RelayCommand<Models.Schedule>(OpenEditScheduleWithParam, s => s != null);
            DeleteScheduleCommand = new RelayCommand<Models.Schedule>(DeleteScheduleWithParam, s => s != null);
            ReloadSchedulesCommand = new RelayCommand(ReloadSchedules);
            SearchCommand = new RelayCommand(SearchSchedules);
            SortCommand = new RelayCommand(SortSchedules);

            ReloadSchedules();
        }

        private void ReloadSchedules()
        {
            try
            {
                var schedules = _context.Schedule.Include(s => s.Dentist).ToList();
                Schedules = new ObservableCollection<Models.Schedule>(schedules);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar los horarios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchSchedules()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText)) { ReloadSchedules(); return; }
                var term = SearchText.ToLower();
                var filtered = _context.Schedule.Include(s => s.Dentist).ToList()
                    .Where(s =>
                        (s.Dentist?.FirstName?.ToLower().Contains(term) ?? false) ||
                        (s.Dentist?.LastName?.ToLower().Contains(term) ?? false) ||
                        s.StartDate.ToString("yyyy-MM-dd").Contains(SearchText) ||
                        s.EndDate.ToString("yyyy-MM-dd").Contains(SearchText))
                    .ToList();
                Schedules = new ObservableCollection<Models.Schedule>(filtered);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar horarios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortSchedules()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Schedules == null) return;
            var sorted = SelectedSortOption switch
            {
                "Dentista" => Schedules.OrderBy(s => s.Dentist?.LastName).ThenBy(s => s.Dentist?.FirstName).ToList(),
                "Fecha de Inicio" => Schedules.OrderBy(s => s.StartDate).ToList(),
                "Fecha de Fin" => Schedules.OrderBy(s => s.EndDate).ToList(),
                "Día Libre" => Schedules.OrderBy(s => s.IsDayOff).ToList(),
                _ => Schedules.ToList()
            };
            Schedules = new ObservableCollection<Models.Schedule>(sorted);
        }

        private void OpenAddSchedule()
        {
            var w = new AddScheduleView { DataContext = new AddScheduleViewModel(_context) };
            if (w.ShowDialog() == true) ReloadSchedules();
        }

        private void OpenEditScheduleWithParam(Models.Schedule schedule)
        {
            if (schedule == null) return;
            SelectedSchedule = schedule;
            var w = new EditScheduleView { DataContext = new EditScheduleViewModel(_context, SelectedSchedule) };
            if (w.ShowDialog() == true) ReloadSchedules();
        }

        private void DeleteScheduleWithParam(Models.Schedule schedule)
        {
            if (schedule == null) return;
            var result = MessageBox.Show(
                "¿Seguro que quieres eliminar este horario?",
                "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Schedule.Remove(schedule);
                    _context.SaveChanges();
                    ReloadSchedules();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el horario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
