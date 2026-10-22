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
                (EditRoomCommand as RelayCommand<Models.Room>)?.NotifyCanExecuteChanged();
                (DeleteRoomCommand as RelayCommand<Models.Room>)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "NÃºmero", "Disponibilidad", "Equipos"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand AddRoomCommand { get; }
        public ICommand EditRoomCommand { get; }
        public ICommand DeleteRoomCommand { get; }

        public RoomViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            SearchCommand = new RelayCommand(SearchRooms);
            ReloadCommand = new RelayCommand(LoadRooms);
            AddRoomCommand = new RelayCommand(OpenAddRoom);
            EditRoomCommand = new RelayCommand<Models.Room>(OpenEditRoomWithParam, r => r != null);
            DeleteRoomCommand = new RelayCommand<Models.Room>(DeleteRoomWithParam, r => r != null);
            SortCommand = new RelayCommand(SortRooms);

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
                MessageBox.Show($"Error al cargar los consultorios: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SearchRooms()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText)) { LoadRooms(); return; }
                var filteredRooms = _context.Room
                    .Where(r => r.RoomNumber.Contains(SearchText))
                    .ToList();
                Rooms = new ObservableCollection<Models.Room>(filteredRooms);
                OnPropertyChanged(nameof(Rooms));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error durante la bÃºsqueda: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortRooms()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Rooms == null) return;
            var sorted = SelectedSortOption switch
            {
                "NÃºmero" => Rooms.OrderBy(r => r.RoomNumber).ToList(),
                "Disponibilidad" => Rooms.OrderBy(r => r.IsAvailable).ToList(),
                "Equipos" => Rooms.OrderBy(r => r.Equipments?.Count ?? 0).ToList(),
                _ => Rooms.ToList()
            };
            Rooms = new ObservableCollection<Models.Room>(sorted);
            OnPropertyChanged(nameof(Rooms));
        }

        private void OpenAddRoom()
        {
            var v = new AddRoomView { DataContext = new AddRoomViewModel(_context), Owner = Application.Current.MainWindow };
            if (v.ShowDialog() == true) LoadRooms();
        }

        private void OpenEditRoomWithParam(Models.Room room)
        {
            if (room == null) return;
            SelectedRoom = room;
            var v = new EditRoomView { DataContext = new EditRoomViewModel(_context, SelectedRoom), Owner = Application.Current.MainWindow };
            if (v.ShowDialog() == true) LoadRooms();
        }

        private void DeleteRoomWithParam(Models.Room room)
        {
            if (room == null) return;

            var result = MessageBox.Show($"¿Estás seguro de eliminar el consultorio {room.RoomNumber}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                // 1. Verificar si hay Dentistas o Equipos asignados
                var hasDentists = _context.Set<Models.Dentist>().Any(d => d.RoomID == room.RoomID);
                var hasEquipments = _context.Set<Models.Equipment>().Any(e => e.RoomID == room.RoomID);

                if (hasDentists || hasEquipments)
                {
                    MessageBox.Show("No se puede eliminar el consultorio porque tiene dentistas o equipos asignados. Por favor, reasígnalos a otro consultorio primero.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // 2. Si está vacío, borrar
                _context.Set<Models.Room>().Remove(room);
                _context.SaveChanges();

                MessageBox.Show("Consultorio eliminado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRooms(); // Recargar la tabla
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException?.InnerException?.Message ?? ex.InnerException?.Message ?? ex.Message;
                MessageBox.Show($"Error al eliminar: {innerMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
