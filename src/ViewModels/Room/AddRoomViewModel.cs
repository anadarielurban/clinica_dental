using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using System;
using System.Windows;

namespace DentalClinicWPF.ViewModels.Room
{
    public partial class AddRoomViewModel : ObservableObject
    {
        private readonly DentalClinicContext _context;

        [ObservableProperty]
        private string? roomNumber;

        [ObservableProperty]
        private string? address;

        [ObservableProperty]
        private string? phone;

        [ObservableProperty]
        private bool isAvailable = true;

        public AddRoomViewModel(DentalClinicContext context)
        {
            _context = context;
        }

        [RelayCommand]
        private void Save(Window window)
        {
            if (string.IsNullOrWhiteSpace(RoomNumber))
            {
                MessageBox.Show("El nombre/número de consultorio es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var newRoom = new Models.Room
                {
                    RoomNumber = RoomNumber,
                    Address = Address,
                    Phone = Phone,
                    IsAvailable = IsAvailable
                };

                _context.Set<Models.Room>().Add(newRoom);
                _context.SaveChanges();
                
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error");
            }
        }

        [RelayCommand]
        private void Cancel(Window window)
        {
            window.DialogResult = false;
            window.Close();
        }
    }
}