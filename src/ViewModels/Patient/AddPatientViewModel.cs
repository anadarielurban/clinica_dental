using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Patient
{
    public class AddPatientViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        private string _firstName = string.Empty;
        public string FirstName
        {
            get => _firstName;
            set
            {
                if (SetProperty(ref _firstName, value))
                {
                    ValidateProperty(nameof(FirstName));
                    NotifySaveCommand();
                }
            }
        }

        private string _lastName = string.Empty;
        public string LastName
        {
            get => _lastName;
            set
            {
                if (SetProperty(ref _lastName, value))
                {
                    ValidateProperty(nameof(LastName));
                    NotifySaveCommand();
                }
            }
        }

        private DateTime _dateOfBirth = DateTime.Today.AddYears(-18);
        public DateTime DateOfBirth
        {
            get => _dateOfBirth;
            set
            {
                if (SetProperty(ref _dateOfBirth, value))
                {
                    ValidateProperty(nameof(DateOfBirth));
                    NotifySaveCommand();
                }
            }
        }

        private string _gender = "Male";
        public string Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        private string _phoneNumber = "+48";
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                if (SetProperty(ref _phoneNumber, value))
                {
                    ValidateProperty(nameof(PhoneNumber));
                    NotifySaveCommand();
                }
            }
        }

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateProperty(nameof(Email));
                    NotifySaveCommand();
                }
            }
        }

        private string _address = string.Empty;
        public string Address
        {
            get => _address;
            set
            {
                if (SetProperty(ref _address, value))
                {
                    ValidateProperty(nameof(Address));
                    NotifySaveCommand();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddPatientViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanSave()
        {
            return !HasErrors &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   CalculateAge(DateOfBirth) >= 18 &&
                   Regex.IsMatch(PhoneNumber, @"^\+48\d{9}$") &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") &&
                   ValidateAddress(Address);
        }

        private void Save()
        {
            if (!CanSave())
            {
                MessageBox.Show("Please fix the validation errors before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var patient = new Models.Patient
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    DateOfBirth = DateOfBirth,
                    Gender = Gender,
                    PhoneNumber = PhoneNumber,
                    Email = Email,
                    Address = Address
                };

                _context.Patient.Add(patient);
                _context.SaveChanges();

                MessageBox.Show("Patient added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving patient: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseWindow(false);

        protected override void ValidateProperty(string propertyName)
        {
            base.ValidateProperty(propertyName);

            switch (propertyName)
            {
                case nameof(FirstName):
                    if (string.IsNullOrWhiteSpace(FirstName))
                        AddError(propertyName, "First Name is required.");
                    else if (!StartsWithUppercase(FirstName))
                        AddError(propertyName, "First Name must start with an uppercase letter.");
                    break;

                case nameof(LastName):
                    if (string.IsNullOrWhiteSpace(LastName))
                        AddError(propertyName, "Last Name is required.");
                    else if (!StartsWithUppercase(LastName))
                        AddError(propertyName, "Last Name must start with an uppercase letter.");
                    break;

                case nameof(PhoneNumber):
                    if (string.IsNullOrWhiteSpace(PhoneNumber))
                        AddError(propertyName, "Phone Number is required.");
                    else if (!Regex.IsMatch(PhoneNumber, @"^\+48\d{9}$"))
                        AddError(propertyName, "Phone Number must start with +48 and contain 9 digits.");
                    break;

                case nameof(Email):
                    if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        AddError(propertyName, "Invalid Email Address.");
                    break;

                case nameof(DateOfBirth):
                    if (CalculateAge(DateOfBirth) < 18)
                        AddError(propertyName, "Patient must be at least 18 years old.");
                    break;

                case nameof(Address):
                    if (!ValidateAddress(Address))
                        AddError(propertyName, "Address must start with a postal code (00-000), followed by city and street.");
                    break;
            }
        }

        private bool ValidateAddress(string address)
        {
            return Regex.IsMatch(address, @"^\d{2}-\d{3} .+");
        }

        private bool StartsWithUppercase(string value)
        {
            return !string.IsNullOrEmpty(value) && char.IsUpper(value[0]);
        }

        private int CalculateAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            int age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;
            return age;
        }

        private void NotifySaveCommand()
        {
            ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();
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
    }
}
