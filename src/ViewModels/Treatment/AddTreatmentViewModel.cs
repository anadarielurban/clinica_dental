using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Treatment
{
    public class AddEditTreatmentViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        // Properties for Binding
        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    Treatment.Name = value;
                    ValidateProperty(nameof(Name));
                    NotifySaveCommand();
                }
            }
        }

        private string _description = string.Empty;
        public string Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    Treatment.Description = value;
                    ValidateProperty(nameof(Description));
                    NotifySaveCommand();
                }
            }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set
            {
                if (SetProperty(ref _price, value))
                {
                    Treatment.Price = value;
                    ValidateProperty(nameof(Price));
                    NotifySaveCommand();
                }
            }
        }

        private int _durationMinutes;
        public int DurationMinutes
        {
            get => _durationMinutes;
            set
            {
                if (SetProperty(ref _durationMinutes, value))
                {
                    Treatment.Duration = TimeSpan.FromMinutes(value);
                    ValidateProperty(nameof(DurationMinutes));
                    NotifySaveCommand();
                }
            }
        }

        public Models.Treatment Treatment { get; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AddEditTreatmentViewModel(DentalClinicContext context, Models.Treatment? treatment = null)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Treatment = treatment ?? new Models.Treatment
            {
                Name = string.Empty,
                Description = string.Empty,
                Price = 1,
                Duration = TimeSpan.FromMinutes(1)
            };

            Name = Treatment.Name;
            Description = Treatment.Description;
            Price = Treatment.Price;
            DurationMinutes = (int)Treatment.Duration.TotalMinutes;

            SaveCommand = new RelayCommand(Save, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            PropertyChanged += (s, e) => ValidateProperty(e.PropertyName);
        }

        protected override void ValidateProperty(string propertyName)
        {
            base.ValidateProperty(propertyName);

            switch (propertyName)
            {
                case nameof(Name):
                    if (string.IsNullOrWhiteSpace(Name))
                        AddError(propertyName, "Name is required.");
                    else if (Name.Length > 100)
                        AddError(propertyName, "Name cannot exceed 100 characters.");
                    break;

                case nameof(Description):
                    if (string.IsNullOrWhiteSpace(Description))
                        AddError(propertyName, "Description is required.");
                    else if (Description.Length > 500)
                        AddError(propertyName, "Description cannot exceed 500 characters.");
                    break;

                case nameof(Price):
                    if (Price <= 0)
                        AddError(propertyName, "Price must be greater than 0.");
                    break;

                case nameof(DurationMinutes):
                    if (DurationMinutes < 1)
                        AddError(propertyName, "Duration must be at least 1 minute.");
                    break;
            }
        }

        private bool CanSave()
        {
            return !HasErrors &&
                   !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Description) &&
                   Price > 0 &&
                   DurationMinutes > 0;
        }

        private void Save()
        {
            if (!CanSave())
            {
                MessageBox.Show("Please fix validation errors before saving.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (Treatment.TreatmentID == 0)
                    _context.Treatment.Add(Treatment);
                else
                    _context.Treatment.Update(Treatment);

                _context.SaveChanges();
                MessageBox.Show("Treatment saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                CloseWindow(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving treatment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel() => CloseWindow(false);

        private void NotifySaveCommand()
        {
            if (SaveCommand is RelayCommand relayCommand)
                relayCommand.NotifyCanExecuteChanged();
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
