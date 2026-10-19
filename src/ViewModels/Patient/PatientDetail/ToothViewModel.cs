using System;
using System.Windows.Media;
using System.Windows.Input;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;

namespace DentalClinicWPF.ViewModels.Patient.PatientDetail
{
    public class ToothViewModel : BaseViewModel
    {
        public ToothRecord Record { get; }
        public int ToothNumber => Record.ToothNumber;

        private string _condition;
        public string Condition
        {
            get => _condition;
            set
            {
                if (SetProperty(ref _condition, value))
                {
                    Record.Condition = value;
                    OnPropertyChanged(nameof(BackgroundColor));
                    OnPropertyChanged(nameof(TextColor));
                }
            }
        }

        public Brush BackgroundColor
        {
            get
            {
                return Condition switch
                {
                    "Caries" => new SolidColorBrush(Color.FromRgb(239, 68, 68)),     // Rojo
                    "Resina" => new SolidColorBrush(Color.FromRgb(59, 130, 246)),    // Azul
                    "Endodoncia" => new SolidColorBrush(Color.FromRgb(168, 85, 247)),// Morado
                    "Corona" => new SolidColorBrush(Color.FromRgb(234, 179, 8)),     // Dorado
                    "Extraido" => new SolidColorBrush(Color.FromRgb(107, 114, 128)), // Gris
                    "Implante" => new SolidColorBrush(Color.FromRgb(20, 184, 166)),  // Teal
                    _ => new SolidColorBrush(Colors.White)                           // Sano
                };
            }
        }

        public Brush TextColor => Condition == "Sano" ? new SolidColorBrush(Colors.Black) : new SolidColorBrush(Colors.White);

        public ICommand ChangeStateCommand { get; }

        // ─── Selección visual (resaltado teal) ───
        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected == value) return; _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        // ─── Notas editables por diente ───
        public string Notes
        {
            get => Record.Notes ?? string.Empty;
            set
            {
                if (Record.Notes == value) return;
                Record.Notes = value;
                OnPropertyChanged(nameof(Notes));
                OnPropertyChanged(nameof(HasNotes));
            }
        }

        public bool HasNotes => !string.IsNullOrWhiteSpace(Record.Notes);

        public ToothViewModel(ToothRecord record)
        {
            Record = record;
            _condition = record.Condition;
            ChangeStateCommand = new RelayCommand<string>(newState => Condition = newState);
        }
    }
}

