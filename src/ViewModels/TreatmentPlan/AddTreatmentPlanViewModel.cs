using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.TreatmentPlan
{
    public class AddTreatmentPlanViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Models.Patient _patient;

        public ObservableCollection<Models.Treatment> AvailableTreatments { get; set; }
        public ObservableCollection<Models.Dentist> AvailableDentists { get; set; }
        public ObservableCollection<TreatmentPlanItemViewModel> Items { get; set; }

        private Models.Treatment _selectedTreatment;
        public Models.Treatment SelectedTreatment
        {
            get => _selectedTreatment;
            set { _selectedTreatment = value; OnPropertyChanged(nameof(SelectedTreatment)); }
        }

        private Models.Dentist _selectedDentist;
        public Models.Dentist SelectedDentist
        {
            get => _selectedDentist;
            set { _selectedDentist = value; OnPropertyChanged(nameof(SelectedDentist)); }
        }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(nameof(Quantity)); }
        }

        private string _observations;
        public string Observations
        {
            get => _observations;
            set { _observations = value; OnPropertyChanged(nameof(Observations)); }
        }

        public decimal TotalAmount => Items.Sum(x => x.Subtotal);

        public IRelayCommand AddItemCommand { get; }
        public IRelayCommand<TreatmentPlanItemViewModel> RemoveItemCommand { get; }
        public IRelayCommand SaveCommand { get; }

        public AddTreatmentPlanViewModel(DentalClinicContext context, Models.Patient patient)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _patient = patient ?? throw new ArgumentNullException(nameof(patient));

            Items = new ObservableCollection<TreatmentPlanItemViewModel>();
            AvailableTreatments = new ObservableCollection<Models.Treatment>(_context.Treatment.ToList());
            AvailableDentists = new ObservableCollection<Models.Dentist>(_context.Dentist.ToList());
            SelectedDentist = AvailableDentists.FirstOrDefault();

            AddItemCommand = new RelayCommand(AddItem);
            RemoveItemCommand = new RelayCommand<TreatmentPlanItemViewModel>(RemoveItem);
            SaveCommand = new RelayCommand(Save);
        }

        private void AddItem()
        {
            if (SelectedTreatment == null || Quantity <= 0)
            {
                MessageBox.Show("Seleccione un tratamiento y una cantidad válida.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = Items.FirstOrDefault(x => x.Treatment.TreatmentID == SelectedTreatment.TreatmentID);
            if (existing != null)
            {
                existing.Quantity += Quantity;
                // Forzar actualización en UI
                var index = Items.IndexOf(existing);
                Items[index] = existing;
            }
            else
            {
                Items.Add(new TreatmentPlanItemViewModel(SelectedTreatment, Quantity));
            }

            Quantity = 1;
            OnPropertyChanged(nameof(TotalAmount));
        }

        private void RemoveItem(TreatmentPlanItemViewModel item)
        {
            if (item != null && Items.Contains(item))
            {
                Items.Remove(item);
                OnPropertyChanged(nameof(TotalAmount));
            }
        }

        private void Save()
        {
            if (!Items.Any())
            {
                MessageBox.Show("Debe agregar al menos un tratamiento al presupuesto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedDentist == null)
            {
                MessageBox.Show("Debe seleccionar un odontólogo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var plan = new Models.TreatmentPlan
                {
                    PatientID = _patient.PatientID,
                    DentistID = SelectedDentist.DentistID,
                    CreationDate = DateTime.Now,
                    Status = "Borrador",
                    TotalAmount = TotalAmount,
                    Observations = Observations,
                    Details = string.Empty // <- Requerido por el esquema antiguo de SQLite
                };

                // Guardar el plan primero para generar el TreatmentPlanID
                _context.TreatmentPlan.Add(plan);
                _context.SaveChanges();

                // Ahora guardar los items con el ID del plan ya generado
                foreach (var item in Items)
                {
                    var entity = item.ToEntity();
                    entity.TreatmentPlanID = plan.TreatmentPlanID;
                    _context.TreatmentPlanItem.Add(entity);
                }
                _context.SaveChanges();
                
                MessageBox.Show("Presupuesto guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                // Cerrar ventana (requiere que el code-behind escuche este evento o se use un DialogResult)
                if (Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive) is Window w) { w.DialogResult = true; }
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? ex.InnerException.Message : "";
                MessageBox.Show($"Error al guardar:\n{ex.Message}\n\nDetalle:\n{innerMsg}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}



