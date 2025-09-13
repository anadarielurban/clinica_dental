using System;
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
            private set
            {
                _schedules = value;
                OnPropertyChanged();
            }
        }

        private Models.Schedule _selectedSchedule;
        public Models.Schedule SelectedSchedule
        {
            get => _selectedSchedule;
            set
            {
                _selectedSchedule = value;
                OnPropertyChanged();
                (EditScheduleCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Dentist",
            "Start Date",
            "End Date",
            "Day Off"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddScheduleCommand { get; }
        public ICommand EditScheduleCommand { get; }
        public ICommand ReloadSchedulesCommand { get; }
        public ICommand SearchCommand { get; }

        public ScheduleViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Schedules = new ObservableCollection<Models.Schedule>();

            AddScheduleCommand = new RelayCommand(OpenAddSchedule);
            EditScheduleCommand = new RelayCommand(OpenEditSchedule, CanEditOrDeleteSchedule);
            ReloadSchedulesCommand = new RelayCommand(ReloadSchedules);
            SearchCommand = new RelayCommand(SearchSchedules);
            SortCommand = new RelayCommand(SortSchedules); // Komenda sortowania

            ReloadSchedules();
        }

        private bool CanEditOrDeleteSchedule() => SelectedSchedule != null;

        private void ReloadSchedules()
        {
            try
            {
                var schedules = _context.Schedule
                    .Include(s => s.Dentist) // Ensure related Dentist data is loaded
                    .ToList();

                Schedules = new ObservableCollection<Models.Schedule>(schedules);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading schedules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchSchedules()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    ReloadSchedules();
                    return;
                }

                var filteredSchedules = _context.Schedule
                    .Include(s => s.Dentist)
                    .AsEnumerable()
                    .Where(s =>
                        (!string.IsNullOrWhiteSpace(s.Dentist?.FirstName) &&
                         s.Dentist.FirstName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (!string.IsNullOrWhiteSpace(s.Dentist?.LastName) &&
                         s.Dentist.LastName.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                        (s.StartDate.ToString("yyyy-MM-dd").Contains(SearchText)) ||
                        (s.EndDate.ToString("yyyy-MM-dd").Contains(SearchText)) ||
                        (s.StartTime.ToString(@"hh\:mm").Contains(SearchText)) ||
                        (s.EndTime.ToString(@"hh\:mm").Contains(SearchText)) ||
                        (s.IsDayOff && "Day Off".Contains(SearchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                Schedules = new ObservableCollection<Models.Schedule>(filteredSchedules);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching schedules: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void SortSchedules()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Schedules == null)
                return;

            IEnumerable<Models.Schedule> sortedSchedules = SelectedSortOption switch
            {
                "Dentist" => Schedules.OrderBy(s => s.Dentist.LastName).ThenBy(s => s.Dentist.FirstName),
                "Start Date" => Schedules.OrderBy(s => s.StartDate),
                "End Date" => Schedules.OrderBy(s => s.EndDate),
                "Day Off" => Schedules.OrderBy(s => s.IsDayOff),
                _ => Schedules
            };

            Schedules = new ObservableCollection<Models.Schedule>(sortedSchedules);
            OnPropertyChanged(nameof(Schedules));
        }

        private void OpenAddSchedule()
        {
            var addScheduleWindow = new AddScheduleView
            {
                DataContext = new AddScheduleViewModel(_context)
            };

            if (addScheduleWindow.ShowDialog() == true)
            {
                ReloadSchedules();
            }
        }

        private void OpenEditSchedule()
        {
            if (SelectedSchedule == null) return;

            var editScheduleWindow = new EditScheduleView
            {
                DataContext = new EditScheduleViewModel(_context, SelectedSchedule)
            };

            if (editScheduleWindow.ShowDialog() == true)
            {
                ReloadSchedules();
            }
        }

        private bool WorkDaysInRange(Models.Schedule schedule)
        {
            DateTime current = schedule.StartDate.Date;
            while (current <= schedule.EndDate.Date)
            {
                if (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday)
                    return false;
                current = current.AddDays(1);
            }
            return true;
        }
    }
}
