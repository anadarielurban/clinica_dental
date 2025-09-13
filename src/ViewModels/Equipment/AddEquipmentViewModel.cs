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
    public class AddEquipmentViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly DentalClinicContext _context;
        private readonly Dictionary<string, List<string>> _errors = new();

        public Models.Equipment Equipment { get; set; }
        public ObservableCollection<Models.Room> Rooms { get; set; }
        public Models.Room SelectedRoom { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public bool HasErrors => _errors.Any();

        public AddEquipmentViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Equipment = new Models.Equipment
            {
                Name = string.Empty,
                Model = string.Empty,
                SerialNumber = string.Empty,
                PurchaseDate = null,
                LastServiceDate = null
            };

            Rooms = new ObservableCollection<Models.Room>(_context.Room.ToList());
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
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
                case nameof(Equipment.Name):
                    if (string.IsNullOrWhiteSpace(Equipment.Name))
                        AddError(propertyName, "Name is required.");
                    break;
                case nameof(Equipment.Model):
                    if (string.IsNullOrWhiteSpace(Equipment.Model))
                        AddError(propertyName, "Model is required.");
                    break;
                case nameof(Equipment.SerialNumber):
                    if (!string.IsNullOrWhiteSpace(Equipment.SerialNumber) &&
                        !Regex.IsMatch(Equipment.SerialNumber, @"^[a-zA-Z0-9-]+$"))
                        AddError(propertyName, "Serial Number must contain only alphanumeric characters or dashes.");
                    break;
                case nameof(Equipment.PurchaseDate):
                    if (Equipment.PurchaseDate > DateTime.Now)
                        AddError(propertyName, "Purchase Date cannot be in the future.");
                    break;
                case nameof(Equipment.LastServiceDate):
                    if (Equipment.LastServiceDate > DateTime.Now)
                        AddError(propertyName, "Last Service Date cannot be in the future.");
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
                MessageBox.Show("Please fix validation errors before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Equipment.RoomID = SelectedRoom?.RoomID;

                _context.Equipment.Add(Equipment);
                _context.SaveChanges();

                MessageBox.Show("Equipment added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving equipment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseWindow();

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
