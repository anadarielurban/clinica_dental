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
    public class EditEmployeeViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        #region Właściwości Pracownika

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

        private string _position;
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

        // Oryginalny obiekt pracownika, który edytujemy.
        public Models.Employee OriginalEmployee { get; }

        // Komendy
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditEmployeeViewModel(Models.Employee employee, DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            OriginalEmployee = employee ?? throw new ArgumentNullException(nameof(employee));

            // Inicjalizacja kolekcji
            Dentists = new ObservableCollection<Models.Dentist>(_context.Dentist.ToList());

            // Inicjalizacja pól z istniejącego Employee
            _firstName = OriginalEmployee.FirstName;
            _lastName = OriginalEmployee.LastName;
            _position = OriginalEmployee.Position;
            _phoneNumber = OriginalEmployee.PhoneNumber;
            _email = OriginalEmployee.Email;
            _dentistID = OriginalEmployee.DentistID ?? 0;

            // Inicjalizacja RelayCommand z metodami
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);

            // Subskrypcja PropertyChanged do walidacji
            PropertyChanged += (s, e) => ValidateProperty(e.PropertyName);
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
                    else if (FirstName.Length > 50)
                        AddError(propertyName, "El nombre no puede superar 50 caracteres.");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(LastName):
                    if (string.IsNullOrWhiteSpace(LastName))
                        AddError(propertyName, "El apellido es obligatorio.");
                    else if (LastName.Length > 50)
                        AddError(propertyName, "El apellido no puede superar 50 caracteres.");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(Position):
                    if (string.IsNullOrWhiteSpace(Position))
                        AddError(propertyName, "El puesto es obligatorio.");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(PhoneNumber):
                    if (string.IsNullOrWhiteSpace(PhoneNumber))
                        AddError(propertyName, "El teléfono es obligatorio.");
                    else if (!Regex.IsMatch(PhoneNumber, @"^[\d\s\+\-\(\)]{7,}$"))
                        AddError(propertyName, "El teléfono debe contener al menos 7 dígitos. Se permiten espacios, +, -, (, ).");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(Email):
                    if (string.IsNullOrWhiteSpace(Email))
                        AddError(propertyName, "El correo es obligatorio.");
                    else if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        AddError(propertyName, "Correo electrónico inválido.");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(DentistID):
                    // Jeśli pozycja to Dental Assistant, wymagamy wybrania dentysty (wartość > 0)
                    if (Position.Equals("Asistente Dental", StringComparison.OrdinalIgnoreCase) && DentistID == 0)
                        AddError(propertyName, "La selección del dentista es obligatoria para Asistente Dental.");
                    else
                        ClearErrors(propertyName);
                    break;
            }
        }

        private bool CanSave()
        {
            return !HasErrors &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Position) &&
                   Regex.IsMatch(PhoneNumber, @"^[\d\s\+\-\(\)]{7,}$") &&
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
                // Aktualizacja oryginalnego obiektu z danych z ViewModelu
                OriginalEmployee.FirstName = FirstName;
                OriginalEmployee.LastName = LastName;
                OriginalEmployee.Position = Position;
                OriginalEmployee.PhoneNumber = PhoneNumber;
                OriginalEmployee.Email = Email;
                OriginalEmployee.DentistID = Position.Equals("Asistente Dental", StringComparison.OrdinalIgnoreCase) ? DentistID : (int?)null;

                _context.Employee.Update(OriginalEmployee);
                _context.SaveChanges();

                MessageBox.Show("¡Empleado actualizado correctamente!",
                                "Éxito",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar el empleado: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
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
                var result = MessageBox.Show(
                    "¿Seguro que quieres eliminar este empleado?",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Employee.Remove(OriginalEmployee);
                    _context.SaveChanges();

                    MessageBox.Show("¡Empleado eliminado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el empleado: {ex.Message}",
                                "Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
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
