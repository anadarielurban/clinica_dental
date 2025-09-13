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
    public class AddLabOrderViewModel : BaseViewModel, INotifyDataErrorInfo
    {
        private readonly DentalClinicContext _context;

        public Models.LabOrder LabOrder { get; set; }
        public ObservableCollection<Models.Patient> Patients { get; }
        public ObservableCollection<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // Validation
        private readonly Dictionary<string, List<string>> _propertyErrors = new();

        public bool HasErrors => _propertyErrors.Count > 0;

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        public AddLabOrderViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            LabOrder = new Models.LabOrder
            {
                OrderDate = DateTime.Now,
                Status = "Pending",
                Description = string.Empty
            };

            Patients = new ObservableCollection<Models.Patient>(_context.Patient.ToList());
            Dentists = new ObservableCollection<Models.Dentist>(_context.Dentist.ToList());

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
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

                _context.LabOrder.Add(LabOrder);
                _context.SaveChanges();

                MessageBox.Show("Lab order created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while creating the lab order: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            CloseWindow(false);
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
                // Add more validations as needed
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
