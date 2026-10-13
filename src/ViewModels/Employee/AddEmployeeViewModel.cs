using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Employee
{
    public class AddEmployeeViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        #region Właściwości Pracownika

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
        
        private string _position = string.Empty;
        public string Position
        {
            get => _position;
            set 
            { 
                if (SetProperty(ref _position, value))
                {
                    ValidateProperty(nameof(Position));
                    // Przy zmianie pozycji walidujemy również wybór dentysty.
                    ValidateProperty(nameof(DentistID));
                    NotifySaveCommand();
                }
            }
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

        // Jeśli dentysta nie został wybrany, przyjmujemy wartość 0.
        private int _dentistID;
        public int DentistID
        {
            get => _dentistID;
            set 
            {
                if (SetProperty(ref _dentistID, value))
                {
                    ValidateProperty(nameof(DentistID));
                    NotifySaveCommand();
                }
            }
        }

        #endregion

        // Lista dentystów – wyświetlana w ComboBoxie.
        public ObservableCollection<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEmployeeViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            // Ładujemy listę dentystów z bazy.
            Dentists = new ObservableCollection<Models.Dentist>(_context.Dentist.ToList());

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            // Opcjonalnie – przy każdej zmianie właściwości wywołujemy walidację.
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName != nameof(HasErrors))
                    ValidateProperty(e.PropertyName);
            };
        }

        /// <summary>
        /// Walidacja właściwości – jeśli nie spełnia kryteriów, dodajemy komunikat błędu.
        /// </summary>
        protected override void ValidateProperty(string propertyName)
        {
            base.ValidateProperty(propertyName);

            switch (propertyName)
            {
                case nameof(FirstName):
                    if (string.IsNullOrWhiteSpace(FirstName))
                        AddError(propertyName, "El nombre es obligatorio.");
                    break;

                case nameof(LastName):
                    if (string.IsNullOrWhiteSpace(LastName))
                        AddError(propertyName, "El apellido es obligatorio.");
                    break;

                case nameof(Position):
                    if (string.IsNullOrWhiteSpace(Position))
                        AddError(propertyName, "El puesto es obligatorio.");
                    break;

                case nameof(PhoneNumber):
                    if (string.IsNullOrWhiteSpace(PhoneNumber))
                        AddError(propertyName, "El teléfono es obligatorio.");
                    else if (!Regex.IsMatch(PhoneNumber, @"^[\d\s\+\-\(\)]{7,}$"))
                        AddError(propertyName, "El teléfono debe contener al menos 7 dígitos. Se permiten espacios, +, -, (, ).");
                    break;

                case nameof(Email):
                    if (string.IsNullOrWhiteSpace(Email))
                        AddError(propertyName, "El correo es obligatorio.");
                    else if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        AddError(propertyName, "Correo electrónico inválido.");
                    break;

                case nameof(DentistID):
                    // Jeśli pozycja to Dental Assistant, wymagamy wybrania dentysty (wartość > 0)
                    if (Position.Equals("Asistente Dental", StringComparison.OrdinalIgnoreCase) && DentistID == 0)
                        AddError(propertyName, "La selección del dentista es obligatoria para Asistente Dental.");
                    break;
            }
        }
        private bool CanSave()
        {
            return !HasErrors &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Position) &&
                   !string.IsNullOrWhiteSpace(PhoneNumber) &&
                   Regex.IsMatch(PhoneNumber, @"^[\d\s\+\-\(\)]{7,}$") &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$") &&
                   // Jeśli pozycja to Dental Assistant, wymagamy wyboru dentysty.
                   (!Position.Equals("Asistente Dental", StringComparison.OrdinalIgnoreCase) || DentistID > 0);
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
                var employee = new Models.Employee
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Position = Position,
                    PhoneNumber = PhoneNumber,
                    Email = Email,
                    DentistID = Position.Equals("Asistente Dental", StringComparison.OrdinalIgnoreCase) ? DentistID : (int?)null
                };

                _context.Employee.Add(employee);
                _context.SaveChanges();

                MessageBox.Show("¡Empleado agregado correctamente!",
                                "Éxito",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el empleado: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseWindow(false);
        
        private void NotifySaveCommand() => ((RelayCommand)SaveCommand).NotifyCanExecuteChanged();

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
