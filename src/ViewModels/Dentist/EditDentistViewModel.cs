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

namespace DentalClinicWPF.ViewModels.Dentist
{
    public class EditDentistViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        // Prywatne pola z publicznymi właściwościami
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

        private string _specialization;
        public string Specialization
        {
            get => _specialization;
            set
            {
                if (SetProperty(ref _specialization, value))
                {
                    ValidateProperty(nameof(Specialization));
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

        private int _roomID;
        public int RoomID
        {
            get => _roomID;
            set
            {
                if (SetProperty(ref _roomID, value))
                {
                    ValidateProperty(nameof(RoomID));
                    NotifySaveCommand();
                }
            }
        }

        // Kolekcje do ComboBoxów
        public ObservableCollection<Models.Room> Rooms { get; }

        // Obiekt Dentist, który edytujemy
        public Models.Dentist OriginalDentist { get; }

        // Komendy
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditDentistViewModel(Models.Dentist dentist, DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            OriginalDentist = dentist ?? throw new ArgumentNullException(nameof(dentist));

            // Inicjalizacja kolekcji
            Rooms = new ObservableCollection<Models.Room>(_context.Room.ToList());

            // Inicjalizacja pól z istniejącego Dentist
            _firstName = OriginalDentist.FirstName;
            _lastName = OriginalDentist.LastName;
            _specialization = OriginalDentist.Specialization;
            _phoneNumber = OriginalDentist.PhoneNumber;
            _email = OriginalDentist.Email;
            _roomID = OriginalDentist.RoomID;

            // Inicjalizacja RelayCommand z metodami
            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);

            // Subskrypcja PropertyChanged do walidacji
            PropertyChanged += (s, e) => ValidateProperty(e.PropertyName);
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
                // Przepisujemy z ViewModelu do obiektu OriginalDentist
                OriginalDentist.FirstName = FirstName;
                OriginalDentist.LastName = LastName;
                OriginalDentist.Specialization = Specialization;
                OriginalDentist.PhoneNumber = PhoneNumber;
                OriginalDentist.Email = Email;
                OriginalDentist.RoomID = RoomID;

                // Aktualizacja w bazie danych
                _context.Dentist.Update(OriginalDentist);
                _context.SaveChanges();

                MessageBox.Show("¡Dentista actualizado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el dentista: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    "¿Seguro que quieres eliminar este dentista?",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Dentist.Remove(OriginalDentist);
                    _context.SaveChanges();

                    MessageBox.Show("¡Dentista eliminado correctamente!", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el dentista: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanSave()
        {
            return !HasErrors &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Specialization) &&
                   Regex.IsMatch(PhoneNumber, @"^[\d\s\+\-\(\)]{7,}$") &&
                   (string.IsNullOrWhiteSpace(Email) || Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) &&
                   RoomID != 0;
        }

        protected override void ValidateProperty(string propertyName)
        {
            // Najpierw czyścimy stare błędy
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

                case nameof(Specialization):
                    if (string.IsNullOrWhiteSpace(Specialization))
                        AddError(propertyName, "La especialización es obligatoria.");
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
                    if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        AddError(propertyName, "Formato de correo inválido.");
                    else
                        ClearErrors(propertyName);
                    break;

                case nameof(RoomID):
                    if (RoomID == 0)
                        AddError(propertyName, "Selecciona un consultorio.");
                    else
                        ClearErrors(propertyName);
                    break;
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
