using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.MedicalRecord;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.ViewModels.MedicalRecord
{
    public class MedicalRecordViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.MedicalRecord> MedicalRecords { get; set; } = new();

        private Models.MedicalRecord _selectedMedicalRecord;
        public Models.MedicalRecord SelectedMedicalRecord
        {
            get => _selectedMedicalRecord;
            set
            {
                _selectedMedicalRecord = value;
                OnPropertyChanged();
                (EditCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
            }
        }

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Record ID",
            "Patient",
            "Dentist",
            "Date"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public MedicalRecordViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            AddCommand = new RelayCommand(OpenAddView);
            EditCommand = new RelayCommand(OpenEditView, () => SelectedMedicalRecord != null);
            ReloadCommand = new RelayCommand(LoadMedicalRecords);
            SearchCommand = new RelayCommand(Search);
            SortCommand = new RelayCommand(SortMedicalRecords); // Komenda sortowania

            LoadMedicalRecords();
        }

        private void LoadMedicalRecords()
        {
            try
            {
                var records = _context.MedicalRecord
                    .Include(mr => mr.Patient)
                    .Include(mr => mr.Dentist)
                    .ToList();

                MedicalRecords = new ObservableCollection<Models.MedicalRecord>(
                    records.Select(mr => new Models.MedicalRecord
                    {
                        RecordID = mr.RecordID,
                        PatientID = mr.PatientID,
                        Patient = mr.Patient ?? new Models.Patient
                        {
                            FirstName = "Unknown",
                            LastName = "Patient",
                            DateOfBirth = DateTime.MinValue
                        },
                        DentistID = mr.DentistID,
                        Dentist = mr.Dentist ?? new Models.Dentist
                        {
                            FirstName = "Unknown",
                            LastName = "Dentist",
                            Specialization = "N/A"
                        },
                        Date = mr.Date,
                        Notes = string.IsNullOrEmpty(mr.Notes) ? "No notes available" : mr.Notes,
                        Attachments = mr.Attachments ?? Array.Empty<byte>()
                    })
                );

                OnPropertyChanged(nameof(MedicalRecords));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading medical records: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SortMedicalRecords()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || MedicalRecords == null)
                return;

            IEnumerable<Models.MedicalRecord> sortedRecords = SelectedSortOption switch
            {
                "Record ID" => MedicalRecords.OrderBy(mr => mr.RecordID),
                "Patient" => MedicalRecords.OrderBy(mr => mr.Patient.LastName)
                                           .ThenBy(mr => mr.Patient.FirstName),
                "Dentist" => MedicalRecords.OrderBy(mr => mr.Dentist.LastName)
                                           .ThenBy(mr => mr.Dentist.FirstName),
                "Date" => MedicalRecords.OrderBy(mr => mr.Date),
                _ => MedicalRecords
            };

            MedicalRecords = new ObservableCollection<Models.MedicalRecord>(sortedRecords);
            OnPropertyChanged(nameof(MedicalRecords));
        }

        private void OpenAddView()
        {
            var addView = new AddMedicalRecordView
            {
                DataContext = new AddMedicalRecordViewModel(_context),
                Owner = Application.Current.MainWindow
            };

            if (addView.ShowDialog() == true)
            {
                LoadMedicalRecords();
            }
        }

        private void OpenEditView()
        {
            if (SelectedMedicalRecord == null) return;

            var editView = new EditMedicalRecordView
            {
                DataContext = new EditMedicalRecordViewModel(_context, SelectedMedicalRecord),
                Owner = Application.Current.MainWindow
            };

            if (editView.ShowDialog() == true)
            {
                LoadMedicalRecords();
            }
        }

        private void Search()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(SearchText))
                {
                    LoadMedicalRecords();
                    return;
                }

                var filtered = _context.MedicalRecord
                    .Include(mr => mr.Patient)
                    .Where(mr =>
                        (mr.Patient != null && (
                            mr.Patient.FirstName.Contains(SearchText) ||
                            mr.Patient.LastName.Contains(SearchText)
                        )) ||
                        (!string.IsNullOrEmpty(mr.Notes) && mr.Notes.Contains(SearchText)) ||
                        mr.RecordID.ToString().Contains(SearchText))
                    .ToList();

                MedicalRecords = new ObservableCollection<Models.MedicalRecord>(filtered.Select(mr => new Models.MedicalRecord
                {
                    RecordID = mr.RecordID,
                    PatientID = mr.PatientID,
                    Patient = mr.Patient ?? new Models.Patient
                    {
                        FirstName = "Unknown",
                        LastName = "Patient",
                        DateOfBirth = DateTime.MinValue
                    },
                    Date = mr.Date,
                    Notes = mr.Notes ?? "No notes available",
                    Attachments = mr.Attachments ?? Array.Empty<byte>()
                }));

                OnPropertyChanged(nameof(MedicalRecords));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during search: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
