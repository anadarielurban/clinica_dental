using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.MedicalRecord
{
    public class MedicalRecordViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.MedicalRecord> MedicalRecords { get; private set; }

        private Models.MedicalRecord _selectedMedicalRecord;
        public Models.MedicalRecord SelectedMedicalRecord
        {
            get => _selectedMedicalRecord;
            set
            {
                _selectedMedicalRecord = value;
                OnPropertyChanged();
                (EditCommand as RelayCommand<Models.MedicalRecord>)?.NotifyCanExecuteChanged();
                (DeleteCommand as RelayCommand<Models.MedicalRecord>)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText;
        public string SearchText { get => _searchText; set { _searchText = value; OnPropertyChanged(); } }

        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "ID Registro", "Paciente", "Dentista", "Fecha"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public MedicalRecordViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            MedicalRecords = new ObservableCollection<Models.MedicalRecord>();

            AddCommand = new RelayCommand(OpenAdd);
            EditCommand = new RelayCommand<Models.MedicalRecord>(OpenEditWithParam, r => r != null);
            DeleteCommand = new RelayCommand<Models.MedicalRecord>(DeleteWithParam, r => r != null);
            ReloadCommand = new RelayCommand(LoadData);
            SearchCommand = new RelayCommand(SearchData);
            SortCommand = new RelayCommand(SortData);

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                var list = _context.MedicalRecord.Include(m => m.Patient).Include(m => m.Dentist).ToList();
                MedicalRecords.Clear();
                foreach (var r in list) MedicalRecords.Add(r);
            }
            catch (Exception ex) { MessageBox.Show($"Error al cargar: {ex.Message}"); }
        }

        private void SearchData()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { LoadData(); return; }
            var term = SearchText.ToLower();
            var filtered = _context.MedicalRecord.Include(m => m.Patient).Include(m => m.Dentist).ToList()
                .Where(m => (m.Patient?.FirstName?.ToLower().Contains(term) ?? false) ||
                            (m.Patient?.LastName?.ToLower().Contains(term) ?? false) ||
                            (m.Notes?.ToLower().Contains(term) ?? false) ||
                            m.RecordID.ToString().Contains(term)).ToList();
            MedicalRecords.Clear();
            foreach (var r in filtered) MedicalRecords.Add(r);
        }

        private void SortData()
        {
            if (string.IsNullOrEmpty(SelectedSortOption)) return;
            var sorted = SelectedSortOption switch
            {
                "ID Registro" => MedicalRecords.OrderBy(m => m.RecordID).ToList(),
                "Paciente" => MedicalRecords.OrderBy(m => m.Patient?.LastName).ToList(),
                "Dentista" => MedicalRecords.OrderBy(m => m.Dentist?.LastName).ToList(),
                "Fecha" => MedicalRecords.OrderBy(m => m.Date).ToList(),
                _ => MedicalRecords.ToList()
            };
            MedicalRecords.Clear();
            foreach (var r in sorted) MedicalRecords.Add(r);
        }

        private void OpenAdd()
        {
            var w = new Views.MedicalRecord.AddMedicalRecordView { DataContext = new AddMedicalRecordViewModel(_context) };
            if (w.ShowDialog() == true) LoadData();
        }

        private void OpenEditWithParam(Models.MedicalRecord record)
        {
            if (record == null) return;
            SelectedMedicalRecord = record;
            var w = new Views.MedicalRecord.EditMedicalRecordView { DataContext = new EditMedicalRecordViewModel(_context, record) };
            if (w.ShowDialog() == true) LoadData();
        }

        private void DeleteWithParam(Models.MedicalRecord record)
        {
            if (record == null) return;
            if (MessageBox.Show($"¿Seguro que quieres eliminar el expediente #{record.RecordID}?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try { _context.MedicalRecord.Remove(record); _context.SaveChanges(); LoadData(); }
                catch (Exception ex) { MessageBox.Show($"Error al eliminar: {ex.Message}"); }
            }
        }
    }
}
