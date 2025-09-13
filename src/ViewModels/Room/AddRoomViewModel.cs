using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Room
{
    public class AddRoomViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        public Models.Room Room { get; set; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddRoomViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            Room = new Models.Room
            {
                RoomNumber = string.Empty,
                IsAvailable = true
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(Room.RoomNumber))
                {
                    MessageBox.Show("Room Number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                var exists = _context.Room.Any(r => r.RoomNumber == Room.RoomNumber);
                if (exists)
                {
                    MessageBox.Show("Room Number already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                _context.Room.Add(Room);
                _context.SaveChanges();

                MessageBox.Show("Room added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while saving room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
