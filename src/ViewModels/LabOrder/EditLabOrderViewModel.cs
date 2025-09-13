using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using System.ComponentModel;

namespace DentalClinicWPF.ViewModels.LabOrder
{
    public class EditLabOrderViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly DentalClinicContext _context;

        public Models.LabOrder LabOrder { get; set; }
        public ObservableCollection<Models.Patient> Patients { get; }
        public ObservableCollection<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        // Validation
        private readonly Dictionary<string, List<string>> _propertyErrors = new();

        public bool HasErrors => _propertyErrors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public EditLabOrderViewModel(DentalClinicContext context, Models.LabOrder labOrder)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            LabOrder = labOrder ?? throw new ArgumentNullException(nameof(labOrder));

            Patients = new ObservableCollection<Models.Patient>(_context.Patient.ToList());
            Dentists = new ObservableCollection<Models.Dentist>(_context.Dentist.ToList());

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
        }

        private void Save()
        {
            try
            {
                if (!IsLabOrderDataValid())
                {
                    MessageBox.Show("Please fill in all required fields correctly!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.LabOrder.Update(LabOrder);
                _context.SaveChanges();

                MessageBox.Show("Lab order updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while updating the lab order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void Delete()
        {
            try
            {
                var result = MessageBox.Show("Are you sure you want to delete this lab order?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _context.LabOrder.Remove(LabOrder);
                    _context.SaveChanges();

                    MessageBox.Show("Lab order deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while deleting the lab order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow(bool dialogResult)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = dialogResult;
                    window.Close();
                    break;
                }
            }
        }

        private bool IsLabOrderDataValid()
        {
            ValidateProperty(nameof(LabOrder.PatientID));
            ValidateProperty(nameof(LabOrder.DentistID));
            ValidateProperty(nameof(LabOrder.Description));

            return !HasErrors;
        }

        private void ValidateProperty(string propertyName)
        {
            _propertyErrors.Remove(propertyName);

            switch (propertyName)
            {
                case nameof(LabOrder.PatientID):
                    if (LabOrder.PatientID == 0)
                        AddError(propertyName, "Patient must be selected.");
                    break;
                case nameof(LabOrder.DentistID):
                    if (LabOrder.DentistID == 0)
                        AddError(propertyName, "Dentist must be selected.");
                    break;
                case nameof(LabOrder.Description):
                    if (string.IsNullOrWhiteSpace(LabOrder.Description))
                        AddError(propertyName, "Description is required.");
                    break;
            }

            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        private void AddError(string propertyName, string error)
        {
            if (!_propertyErrors.ContainsKey(propertyName))
                _propertyErrors[propertyName] = new List<string>();

            _propertyErrors[propertyName].Add(error);
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null!;

            return _propertyErrors.ContainsKey(propertyName) ? _propertyErrors[propertyName] : null!;
        }
    }
}
