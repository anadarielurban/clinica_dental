using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.Win32;

namespace DentalClinicWPF.ViewModels.MedicalRecord
{
    public class AddMedicalRecordViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.MedicalRecord MedicalRecord { get; set; }

        public ObservableCollection<Models.Patient> Patients { get; }
        public ObservableCollection<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UploadAttachmentCommand { get; }

        public AddMedicalRecordViewModel(DentalClinicContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            MedicalRecord = new Models.MedicalRecord
            {
                Date = DateTime.Now,
                Notes = string.Empty,
                Attachments = Array.Empty<byte>()
            };

            Patients = new ObservableCollection<Models.Patient>(_context.Patient.ToList());
            Dentists = new ObservableCollection<Models.Dentist>(_context.Dentist.ToList());

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            UploadAttachmentCommand = new RelayCommand(UploadAttachment);
        }

        private void Save()
        {
            if (MedicalRecord.PatientID == 0 || MedicalRecord.DentistID == 0)
            {
                MessageBox.Show("Please select both a patient and a dentist.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _context.MedicalRecord.Add(MedicalRecord);
                _context.SaveChanges();

                MessageBox.Show("Medical record saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving medical record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseWindow(false);

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

        private void UploadAttachment()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Attachment",
                Filter = "All Files (*.*)|*.*"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                MedicalRecord.Attachments = System.IO.File.ReadAllBytes(openFileDialog.FileName);
                MessageBox.Show("Attachment uploaded successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
