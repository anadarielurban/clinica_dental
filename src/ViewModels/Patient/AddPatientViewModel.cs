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

        private string _phoneNumber = "+52";
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
                   Regex.IsMatch(PhoneNumber, @"^[\d\s\+\-\(\)]{7,}$") &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") &&
                   ValidateAddress(Address);
        }

        private void Save()
        {
            if (!CanSave())
            {
                MessageBox.Show("Corrige los errores de validación antes de guardar.", "Error de Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                MessageBox.Show("¡Paciente agregado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el paciente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                        AddError(propertyName, "El nombre es obligatorio.");
                    else if (!StartsWithUppercase(FirstName))
                        AddError(propertyName, "El nombre debe comenzar con mayúscula.");
                    break;

                case nameof(LastName):
                    if (string.IsNullOrWhiteSpace(LastName))
                        AddError(propertyName, "El apellido es obligatorio.");
                    else if (!StartsWithUppercase(LastName))
                        AddError(propertyName, "El apellido debe comenzar con mayúscula.");
                    break;

                case nameof(PhoneNumber):
                    if (string.IsNullOrWhiteSpace(PhoneNumber))
                        AddError(propertyName, "El teléfono es obligatorio.");
                    else if (!Regex.IsMatch(PhoneNumber, @"^[\d\s\+\-\(\)]{7,}$"))
                        AddError(propertyName, "El teléfono debe contener al menos 7 dígitos. Se permiten espacios, +, -, (, ).");
                    break;

                case nameof(Email):
                    if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        AddError(propertyName, "Correo electrónico inválido.");
                    break;

                case nameof(DateOfBirth):
                    if (CalculateAge(DateOfBirth) < 18)
                        AddError(propertyName, "El paciente debe tener al menos 18 años.");
                    break;

                case nameof(Address):
                    if (!ValidateAddress(Address))
                        AddError(propertyName, "La dirección es obligatoria.");
                    break;
            }
        }

        private bool ValidateAddress(string address)
        {
            return !string.IsNullOrWhiteSpace(address);
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
