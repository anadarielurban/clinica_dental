using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.Schedule
{
    public class EditScheduleViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        private Models.Schedule _schedule;
        public Models.Schedule Schedule
        {
            get => _schedule;
            set
            {
                _schedule = value;
                OnPropertyChanged();
            }
        }

        private List<Models.Dentist> _dentists;
        public List<Models.Dentist> Dentists
        {
            get => _dentists;
            set
            {
                _dentists = value;
                OnPropertyChanged();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditScheduleViewModel(DentalClinicContext context, Models.Schedule schedule)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));

            // Load dentists
            Dentists = _context.Dentist.ToList();

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete, CanDelete);
        }

        private bool CanDelete() => Schedule != null;

        private void Save()
        {
            try
            {
                // Validate that work dates are Monday to Friday
                if (Schedule.StartDate.DayOfWeek == DayOfWeek.Saturday || Schedule.StartDate.DayOfWeek == DayOfWeek.Sunday ||
                    Schedule.EndDate.DayOfWeek == DayOfWeek.Saturday || Schedule.EndDate.DayOfWeek == DayOfWeek.Sunday)
                {
                    MessageBox.Show("Los empleados no trabajan los fines de semana.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Ensure the date range is logical
                if (Schedule.StartDate > Schedule.EndDate)
                {
                    MessageBox.Show("La fecha de inicio debe ser anterior a la fecha de fin.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if a schedule for this dentist and date range already exists, excluding the current schedule
                var overlappingSchedule = _context.Schedule
                    .Where(s => s.DentistID == Schedule.DentistID && s.ScheduleID != Schedule.ScheduleID)
                    .Any(s => s.StartDate <= Schedule.EndDate && s.EndDate >= Schedule.StartDate);

                if (overlappingSchedule)
                {
                    MessageBox.Show("Ya existe un horario para este dentista en ese rango de fechas.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate the selected dentist
                if (!Dentists.Any(d => d.DentistID == Schedule.DentistID))
                {
                    MessageBox.Show("El dentista seleccionado no existe.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Update the schedule
                _context.Schedule.Update(Schedule);
                _context.SaveChanges();

                MessageBox.Show("Horario actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                SetDialogResult(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el horario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            // Set DialogResult to false to indicate cancellation
            SetDialogResult(false);
        }

        private void Delete()
        {
            var result = MessageBox.Show(
                "¿Seguro que quieres eliminar este horario?",
                "Confirmar Eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _context.Schedule.Remove(Schedule);
                    _context.SaveChanges();

                    MessageBox.Show("Horario eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    SetDialogResult(true);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al eliminar el horario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void SetDialogResult(bool result)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = result;
                    window.Close();
                    break;
                }
            }
        }
    }
}
