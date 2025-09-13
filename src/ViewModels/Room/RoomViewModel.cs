using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.Room;

namespace DentalClinicWPF.ViewModels.Room
{
    public class RoomViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.Room> Rooms { get; set; } = new ObservableCollection<Models.Room>();
        private Models.Room _selectedRoom;

        public Models.Room SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                _selectedRoom = value;
                OnPropertyChanged();
                (EditRoomCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Room Number",
            "Availability",
            "Equipment Count"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand SearchCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand AddRoomCommand { get; }
        public ICommand EditRoomCommand { get; }

        public RoomViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            SearchCommand = new RelayCommand(SearchRooms);
            ReloadCommand = new RelayCommand(LoadRooms);
            AddRoomCommand = new RelayCommand(OpenAddRoom);
            EditRoomCommand = new RelayCommand(OpenEditRoom, CanEditRoom);
            SortCommand = new RelayCommand(SortRooms); // Komenda sortowania

            LoadRooms();
        }

        private void LoadRooms()
        {
            try
            {
                var rooms = _context.Room.ToList();
                Rooms = new ObservableCollection<Models.Room>(rooms);
                OnPropertyChanged(nameof(Rooms));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading rooms: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchRooms()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    LoadRooms();
                    return;
                }

                var filteredRooms = _context.Room
                    .Where(r => r.RoomNumber.Contains(SearchText))
                    .ToList();

                Rooms = new ObservableCollection<Models.Room>(filteredRooms);
                OnPropertyChanged(nameof(Rooms));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortRooms()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Rooms == null)
                return;

            IEnumerable<Models.Room> sortedRooms = SelectedSortOption switch
            {
                "Room Number" => Rooms.OrderBy(r => r.RoomNumber),
                "Availability" => Rooms.OrderBy(r => r.IsAvailable),
                "Equipment Count" => Rooms.OrderBy(r => r.Equipments?.Count ?? 0),
                _ => Rooms
            };

            Rooms = new ObservableCollection<Models.Room>(sortedRooms);
            OnPropertyChanged(nameof(Rooms));
        }

        private void OpenAddRoom()
        {
            var addRoomView = new AddRoomView
            {
                DataContext = new AddRoomViewModel(_context),
                Owner = Application.Current.MainWindow
            };
            if (addRoomView.ShowDialog() == true)
            {
                LoadRooms();
            }
        }

        private void OpenEditRoom()
        {
            if (SelectedRoom == null) return;

            var editRoomView = new EditRoomView
            {
                DataContext = new EditRoomViewModel(SelectedRoom, _context),
                Owner = Application.Current.MainWindow
            };
            if (editRoomView.ShowDialog() == true)
            {
                LoadRooms();
            }
        }

        private bool CanEditRoom()
        {
            return SelectedRoom != null;
        }
    }
}
