using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Equipment
{
    public class EditEquipmentViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly DentalClinicContext _context;
        private readonly Dictionary<string, List<string>> _errors = new();

        public Models.Equipment EditingEquipment { get; set; }
        public ObservableCollection<Models.Room> Rooms { get; set; }
        public Models.Room SelectedRoom { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public bool HasErrors => _errors.Any();

        public EditEquipmentViewModel(Models.Equipment equipment, DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            EditingEquipment = equipment ?? throw new ArgumentNullException(nameof(equipment));

            Rooms = new ObservableCollection<Models.Room>(_context.Room.ToList());
            SelectedRoom = Rooms.FirstOrDefault(r => r.RoomID == equipment.RoomID);

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return _errors.SelectMany(err => err.Value);
            else if (_errors.ContainsKey(propertyName))
                return _errors[propertyName];

            return Enumerable.Empty<string>();
        }

        private void ValidateProperty(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return;

            _errors.Remove(propertyName);

            switch (propertyName)
            {
                case nameof(EditingEquipment.Name):
                    if (string.IsNullOrWhiteSpace(EditingEquipment.Name))
                        AddError(propertyName, "El nombre es obligatorio.");
                    break;
                case nameof(EditingEquipment.Model):
                    if (string.IsNullOrWhiteSpace(EditingEquipment.Model))
                        AddError(propertyName, "El modelo es obligatorio.");
                    break;
                case nameof(EditingEquipment.SerialNumber):
                    if (!string.IsNullOrWhiteSpace(EditingEquipment.SerialNumber) &&
                        !Regex.IsMatch(EditingEquipment.SerialNumber, @"^[a-zA-Z0-9-]+$"))
                        AddError(propertyName, "El número de serie solo puede contener caracteres alfanuméricos o guiones.");
                    break;
                case nameof(EditingEquipment.PurchaseDate):
                    if (EditingEquipment.PurchaseDate > DateTime.Now)
                        AddError(propertyName, "La fecha de compra no puede ser futura.");
                    break;
                case nameof(EditingEquipment.LastServiceDate):
                    if (EditingEquipment.LastServiceDate > DateTime.Now)
                        AddError(propertyName, "La fecha del último servicio no puede ser futura.");
                    break;
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
        }

        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            _errors[propertyName].Add(error);
        }

        private bool CanSave() => !HasErrors;

        private void Save()
        {
            if (HasErrors)
            {
                MessageBox.Show("Corrige los errores de validación antes de guardar.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                EditingEquipment.RoomID = SelectedRoom?.RoomID;

                _context.Equipment.Update(EditingEquipment);
                _context.SaveChanges();

                MessageBox.Show("¡Equipo actualizado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar el equipo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseWindow();

        private void Delete()
        {
            try
            {
                if (MessageBox.Show("¿Seguro que quieres eliminar este equipo?", "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    _context.Equipment.Remove(EditingEquipment);
                    _context.SaveChanges();
                    MessageBox.Show("¡Equipo eliminado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el equipo: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = true;
                    window.Close();
                    break;
                }
            }
        }
    }
}
