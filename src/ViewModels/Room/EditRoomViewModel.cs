using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using System;
using System.Linq;
using System.Windows;

namespace DentalClinicWPF.ViewModels.Room
{
    public partial class EditRoomViewModel : ObservableObject
    {
        private readonly DentalClinicContext _context;
        private readonly Models.Room _room;

        [ObservableProperty]
        private string? roomNumber;

        [ObservableProperty]
        private string? address;

        [ObservableProperty]
        private string? phone;

        [ObservableProperty]
        private bool isAvailable;

        public EditRoomViewModel(DentalClinicContext context, Models.Room room)
        {
            _context = context;
            _room = room;
            
            RoomNumber = _room.RoomNumber;
            Address = _room.Address;
            Phone = _room.Phone;
            IsAvailable = _room.IsAvailable;
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
                _room.RoomNumber = RoomNumber;
                _room.Address = Address;
                _room.Phone = Phone;
                _room.IsAvailable = IsAvailable;

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
        private void Delete(Window window)
        {
            var result = MessageBox.Show("¿Estás seguro de eliminar este consultorio?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                var hasDentists = _context.Set<Models.Dentist>().Any(d => d.RoomID == _room.RoomID);
                var hasEquipments = _context.Set<Models.Equipment>().Any(e => e.RoomID == _room.RoomID);

                if (hasDentists || hasEquipments)
                {
                    MessageBox.Show("No se puede eliminar el consultorio porque tiene dentistas o equipos asignados.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                _context.Set<Models.Room>().Remove(_room);
                _context.SaveChanges();
                
                window.DialogResult = true;
                window.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar: {ex.Message}", "Error");
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