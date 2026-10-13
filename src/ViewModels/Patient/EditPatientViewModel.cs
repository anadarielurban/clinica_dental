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
    public class EditPatientViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Action<string, BaseViewModel> _openTabAction;

        // Obiekt oryginalny (z bazy) – będziemy go aktualizować w Save().
        public Models.Patient EditingPatient { get; }

        // Pola do bindowania – identyczne jak w AddPatientViewModel.
        private string _firstName;
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

        private string _lastName;
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

        private DateTime _dateOfBirth;
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

        private string _gender;
        public string Gender
        {
            get => _gender;
            set
            {
                if (SetProperty(ref _gender, value))
                {
                    ValidateProperty(nameof(Gender));
                    NotifySaveCommand();
                }
            }
        }

        private string _phoneNumber;
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

        private string _email;
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

        private string _address;
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

        // Komendy
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditPatientViewModel(
            DentalClinicContext context, 
            Models.Patient patient, 
            Action<string, BaseViewModel> openTabAction)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _openTabAction = openTabAction; 

            // Zapamiętujemy obiekt, który potem aktualizujemy w bazie
            EditingPatient = patient;

            // Wczytujemy dane z pacjenta do lokalnych pól (po to, żeby je walidować tak samo jak w Add)
            _firstName = patient.FirstName;
            _lastName = patient.LastName;
            _dateOfBirth = patient.DateOfBirth;
            _gender = patient.Gender;
            _phoneNumber = patient.PhoneNumber;
            _email = patient.Email;
            _address = patient.Address;

            // Inicjalizacja RelayCommand
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
        }

        private void Cancel()
        {
            CloseWindow(false);
        }

        private void Delete()
        {
            try
            {
                _context.Patient.Remove(EditingPatient);
                _context.SaveChanges();

                MessageBox.Show("¡Paciente eliminado correctamente!",
                                "Éxito",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                CloseWindow(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el paciente: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void Save()
        {
            if (!CanSave())
            {
                MessageBox.Show("Corrige los errores de validación antes de guardar.",
                                "Error de Validación",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Przepisujemy z ViewModelu do obiektu pacjenta
                EditingPatient.FirstName = FirstName;
                EditingPatient.LastName = LastName;
                EditingPatient.DateOfBirth = DateOfBirth;
                EditingPatient.Gender = Gender;
                EditingPatient.PhoneNumber = PhoneNumber;
                EditingPatient.Email = Email;
                EditingPatient.Address = Address;

                // Aktualizujemy w bazie
                _context.Patient.Update(EditingPatient);
                _context.SaveChanges();

                MessageBox.Show("¡Paciente actualizado correctamente!",
                                "Éxito",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);

                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el paciente: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        // Metoda decydująca, czy przycisk Save jest aktywny
        private bool CanSave()
        {
            return !HasErrors
                   && !string.IsNullOrWhiteSpace(FirstName)
                   && !string.IsNullOrWhiteSpace(LastName)
                   && CalculateAge(DateOfBirth) >= 18
                   && Regex.IsMatch(PhoneNumber, @"^[\d\s\+\-\(\)]{7,}$")
                   && !string.IsNullOrWhiteSpace(Email)
                   && Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
                   && ValidateAddress(Address);
        }

        protected override void ValidateProperty(string propertyName)
        {
            // Najpierw czyścimy stare błędy (zawsze, gdy wywoływana jest walidacja)
            base.ValidateProperty(propertyName);

            switch (propertyName)
            {
                case nameof(FirstName):
                    if (string.IsNullOrWhiteSpace(FirstName))
                        AddError(propertyName, "El nombre es obligatorio.");
                    else if (!char.IsUpper(FirstName[0]))
                        AddError(propertyName, "El nombre debe comenzar con mayúscula.");
                    break;

                case nameof(LastName):
                    if (string.IsNullOrWhiteSpace(LastName))
                        AddError(propertyName, "El apellido es obligatorio.");
                    else if (!char.IsUpper(LastName[0]))
                        AddError(propertyName, "El apellido debe comenzar con mayúscula.");
                    break;

                case nameof(PhoneNumber):
                    if (string.IsNullOrWhiteSpace(PhoneNumber))
                        AddError(propertyName, "El teléfono es obligatorio.");
                    else if (!Regex.IsMatch(PhoneNumber, @"^[\d\s\+\-\(\)]{7,}$"))
                        AddError(propertyName, "El teléfono debe contener al menos 7 dígitos. Se permiten espacios, +, -, (, ).");
                    break;

                case nameof(Email):
                    if (!string.IsNullOrWhiteSpace(Email) 
                        && !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    {
                        AddError(propertyName, "Correo electrónico inválido.");
                    }
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
            // np. "00-000 Warszawa, ul. Przykładowa 10"
            return !string.IsNullOrWhiteSpace(address);
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
            // Zmusza RelayCommand, aby ponownie wywołać CanExecute
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
