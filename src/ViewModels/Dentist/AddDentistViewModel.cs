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
    public class AddDentistViewModel : BaseViewModel
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

        private string _specialization = string.Empty;
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

        public ObservableCollection<Models.Room> Rooms { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddDentistViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Rooms = new ObservableCollection<Models.Room>(_context.Room.ToList());

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);
        }

        private bool CanSave()
        {
            return !HasErrors &&
                   !string.IsNullOrWhiteSpace(FirstName) &&
                   !string.IsNullOrWhiteSpace(LastName) &&
                   !string.IsNullOrWhiteSpace(Specialization) &&
                   Regex.IsMatch(PhoneNumber, @"^\+48\d{9}$") &&
                   (!string.IsNullOrWhiteSpace(Email) && Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) &&
                   RoomID != 0;
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
                var dentist = new Models.Dentist
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Specialization = Specialization,
                    PhoneNumber = PhoneNumber,
                    Email = Email,
                    RoomID = RoomID
                };

                _context.Dentist.Add(dentist);
                _context.SaveChanges();

                MessageBox.Show("Dentist added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving dentist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    else if (FirstName.Length > 50)
                        AddError(propertyName, "First Name cannot exceed 50 characters.");
                    break;

                case nameof(LastName):
                    if (string.IsNullOrWhiteSpace(LastName))
                        AddError(propertyName, "Last Name is required.");
                    else if (LastName.Length > 50)
                        AddError(propertyName, "Last Name cannot exceed 50 characters.");
                    break;

                case nameof(Specialization):
                    if (string.IsNullOrWhiteSpace(Specialization))
                        AddError(propertyName, "Specialization is required.");
                    break;

                case nameof(PhoneNumber):
                    if (!Regex.IsMatch(PhoneNumber, @"^\+48\d{9}$"))
                        AddError(propertyName, "Phone Number must start with +48 and contain 9 digits.");
                    break;

                case nameof(Email):
                    if (!string.IsNullOrWhiteSpace(Email) && !Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                        AddError(propertyName, "Invalid email format.");
                    break;

                case nameof(RoomID):
                    if (RoomID == 0)
                        AddError(propertyName, "Please select a room.");
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
