using System;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Room
{
    public class EditRoomViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.Room OriginalRoom { get; }
        public Models.Room EditingRoom { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand DeleteCommand { get; }

        public EditRoomViewModel(Models.Room room, DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            OriginalRoom = room ?? throw new ArgumentNullException(nameof(room));
            
            EditingRoom = new Models.Room
            {
                RoomID = room.RoomID,
                RoomNumber = room.RoomNumber,
                IsAvailable = room.IsAvailable
            };

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            DeleteCommand = new RelayCommand(Delete);
        }

        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EditingRoom.RoomNumber))
                {
                    MessageBox.Show("Room Number is required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (EditingRoom.RoomNumber != OriginalRoom.RoomNumber)
                {
                    var exists = _context.Room.Any(r => r.RoomNumber == EditingRoom.RoomNumber && r.RoomID != EditingRoom.RoomID);
                    if (exists)
                    {
                        MessageBox.Show("Room Number already exists.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }

                OriginalRoom.RoomNumber = EditingRoom.RoomNumber;
                OriginalRoom.IsAvailable = EditingRoom.IsAvailable;

                _context.Room.Update(OriginalRoom);
                _context.SaveChanges();

                MessageBox.Show("Room updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void Delete()
        {
            try
            {
                var result = MessageBox.Show(
                    "Are you sure you want to delete this room?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    var hasDentists = _context.Dentist.Any(d => d.RoomID == OriginalRoom.RoomID);
                    if (hasDentists)
                    {
                        MessageBox.Show("Cannot delete room assigned to a dentist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    _context.Room.Remove(OriginalRoom);
                    _context.SaveChanges();

                    MessageBox.Show("Room deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    CloseWindow(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error while deleting room: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseWindow(bool success)
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window.DataContext == this)
                {
                    window.DialogResult = success;
                    window.Close();
                    break;
                }
            }
        }
    }
}
