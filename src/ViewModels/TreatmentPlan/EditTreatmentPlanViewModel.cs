using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using DentalClinicWPF.Data;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.TreatmentPlan
{
    public class EditTreatmentPlanViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        private readonly Models.TreatmentPlan _plan;

        public ObservableCollection<Models.Treatment> AvailableTreatments { get; set; }
        public ObservableCollection<TreatmentPlanItemViewModel> Items { get; set; }

        private Models.Treatment _selectedTreatment;
        public Models.Treatment SelectedTreatment
        {
            get => _selectedTreatment;
            set { _selectedTreatment = value; OnPropertyChanged(nameof(SelectedTreatment)); }
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

        public EditTreatmentPlanViewModel(DentalClinicContext context, Models.TreatmentPlan plan)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _plan = plan ?? throw new ArgumentNullException(nameof(plan));

            Items = new ObservableCollection<TreatmentPlanItemViewModel>();
            AvailableTreatments = new ObservableCollection<Models.Treatment>(_context.Treatment.ToList());
            Observations = _plan.Observations;

            // Cargar items existentes
            var dbPlan = _context.TreatmentPlan.Include(p => p.Items).ThenInclude(i => i.Treatment).FirstOrDefault(p => p.TreatmentPlanID == _plan.TreatmentPlanID);
            if (dbPlan != null && dbPlan.Items != null)
            {
                foreach (var item in dbPlan.Items)
                {
                    Items.Add(new TreatmentPlanItemViewModel(item.Treatment, item.Quantity));
                }
            }

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
                MessageBox.Show("Debe haber al menos un tratamiento en el presupuesto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _plan.Observations = Observations;
                _plan.TotalAmount = TotalAmount;

                // Eliminar items viejos
                var oldItems = _context.TreatmentPlanItem.Where(i => i.TreatmentPlanID == _plan.TreatmentPlanID).ToList();
                _context.TreatmentPlanItem.RemoveRange(oldItems);

                // Agregar nuevos
                foreach (var item in Items)
                {
                    var entity = item.ToEntity();
                    entity.TreatmentPlanID = _plan.TreatmentPlanID;
                    _context.TreatmentPlanItem.Add(entity);
                }

                _context.SaveChanges();
                MessageBox.Show("Presupuesto actualizado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                if (Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive) is Window w) { w.DialogResult = true; }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar: {ex.Message}\n{ex.InnerException?.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

