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
    public class EditMedicalRecordViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public Models.MedicalRecord MedicalRecord { get; private set; }

        public ObservableCollection<Models.Patient> Patients { get; }
        public ObservableCollection<Models.Dentist> Dentists { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UploadAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }

        public EditMedicalRecordViewModel(DentalClinicContext context, Models.MedicalRecord medicalRecord)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            MedicalRecord = medicalRecord ?? throw new ArgumentNullException(nameof(medicalRecord));

            Patients = new ObservableCollection<Models.Patient>(_context.Patient.ToList());
            Dentists = new ObservableCollection<Models.Dentist>(_context.Dentist.ToList());

            SaveCommand = new RelayCommand(Save);
            CancelCommand = new RelayCommand(Cancel);
            UploadAttachmentCommand = new RelayCommand(UploadAttachment);
            OpenAttachmentCommand = new RelayCommand(OpenAttachment);
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
                _context.MedicalRecord.Update(MedicalRecord);
                _context.SaveChanges();

                MessageBox.Show("Medical record updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating medical record: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

        private void OpenAttachment()
        {
            if (MedicalRecord.Attachments == null || MedicalRecord.Attachments.Length == 0)
            {
                MessageBox.Show("No attachment available for this record.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                string tempFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Attachment_" + Guid.NewGuid().ToString());
                System.IO.File.WriteAllBytes(tempFilePath, MedicalRecord.Attachments);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening attachment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
