using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using DentalClinicWPF.Data;
using DentalClinicWPF.Helpers;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;

namespace DentalClinicWPF.ViewModels.Budget
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        // --- CABECERA ---
        public string Folio { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        
        public ObservableCollection<Models.Patient> AvailablePatients { get; set; } = new();
        private Models.Patient _selectedPatient;
        public Models.Patient SelectedPatient 
        { 
            get => _selectedPatient; 
            set 
            { 
                _selectedPatient = value; 
                OnPropertyChanged(nameof(SelectedPatient));
                OnPropertyChanged(nameof(PatientName));
                OnPropertyChanged(nameof(PatientId));
            } 
        }
        public string PatientName => SelectedPatient?.FullName ?? "Seleccione un paciente...";
        public string PatientId => SelectedPatient?.PatientID.ToString() ?? "---";

        public string Observations { get; set; } = "";
        public double ExchangeRate { get; set; } = 18.00;
        public string Status { get; set; } = "Prospecto";

        // --- TRATAMIENTOS ---
        public ObservableCollection<Models.Treatment> AvailableTreatments { get; set; } = new();
        private Models.Treatment _selectedTreatment;
        public Models.Treatment SelectedTreatment { get => _selectedTreatment; set => _selectedTreatment = value; }

        public ObservableCollection<BudgetItemViewModel> Items { get; set; } = new();

        // --- TOTALES ---
        public decimal SubTotalMXN => Items.Sum(i => i.TotalMXN);
        public decimal SubTotalUSD => SubTotalMXN / (decimal)ExchangeRate;
        public decimal TotalMXN => SubTotalMXN;
        public decimal TotalUSD => SubTotalUSD;

        // --- PLAN DE PAGOS ---
        private decimal _downPayment;
        public decimal DownPayment 
        { 
            get => _downPayment; 
            set 
            { 
                _downPayment = value; 
                OnPropertyChanged(nameof(DownPayment));
                OnPropertyChanged(nameof(RemainingBalance));
                OnPropertyChanged(nameof(PaymentAmount));
            } 
        }

        private int _numberOfPayments = 1;
        public int NumberOfPayments 
        { 
            get => _numberOfPayments; 
            set 
            { 
                _numberOfPayments = value < 1 ? 1 : value; 
                OnPropertyChanged(nameof(NumberOfPayments));
                OnPropertyChanged(nameof(PaymentAmount));
            } 
        }

        public decimal RemainingBalance => TotalMXN - DownPayment;
        public decimal PaymentAmount => NumberOfPayments > 0 ? RemainingBalance / NumberOfPayments : 0;

        // --- COMANDOS ---
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand CancelCommand { get; }

        public BudgetViewModel(DentalClinicContext context)
        {
            _context = context;
            Folio = $"PRES-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
            LoadData();

            AddItemCommand = new RelayCommand(AddItem);
            RemoveItemCommand = new RelayCommand<BudgetItemViewModel>(RemoveItem);
            SaveCommand = new RelayCommand(SaveBudget);
            PrintCommand = new RelayCommand(PrintBudget);
            CancelCommand = new RelayCommand(ClearForm);
        }

        private void LoadData()
        {
            var patients = _context.Patient.ToList();
            foreach (var p in patients) AvailablePatients.Add(p);

            var treatments = _context.Treatment.ToList();
            foreach (var t in treatments) AvailableTreatments.Add(t);
        }

        private void AddItem()
        {
            if (SelectedTreatment == null) return;

            var newItem = new BudgetItemViewModel
            {
                Treatment = SelectedTreatment,
                Quantity = 1,
                Description = SelectedTreatment.Name,
                UnitPriceMXN = SelectedTreatment.Price,
                DiscountPercentage = 0
            };

            newItem.PropertyChanged += (s, e) => RecalculateTotals();
            Items.Add(newItem);
            RecalculateTotals();
        }

        private void RemoveItem(BudgetItemViewModel item)
        {
            if (item != null)
            {
                Items.Remove(item);
                RecalculateTotals();
            }
        }

        private void RecalculateTotals()
        {
            OnPropertyChanged(nameof(SubTotalMXN));
            OnPropertyChanged(nameof(SubTotalUSD));
            OnPropertyChanged(nameof(TotalMXN));
            OnPropertyChanged(nameof(TotalUSD));
            OnPropertyChanged(nameof(RemainingBalance));
            OnPropertyChanged(nameof(PaymentAmount));
        }

        private void SaveBudget()
        {
            if (SelectedPatient == null)
            {
                MessageBox.Show("Por favor, seleccione un paciente antes de guardar.", "Atención");
                return;
            }
            if (!Items.Any())
            {
                MessageBox.Show("El presupuesto debe tener al menos un tratamiento.", "Atención");
                return;
            }

            try
            {
                var newBudget = new Models.Budget
                {
                    Folio = this.Folio,
                    Date = this.Date,
                    PatientId = SelectedPatient.PatientID, // <--- Guardar el ID del paciente
                    Observations = this.Observations ?? "",
                    TotalMXN = this.TotalMXN,
                    TotalUSD = this.TotalUSD,
                    Deposit = this.DownPayment,
                    NumberOfPayments = this.NumberOfPayments,
                    Status = this.Status ?? "Prospecto"
                };

                foreach (var item in Items)
                {
                    newBudget.Items.Add(new Models.BudgetItem
                    {
                        Quantity = item.Quantity,
                        Description = item.Description,
                        UnitPriceMXN = item.UnitPriceMXN,
                        DiscountPercentage = item.DiscountPercentage
                    });
                }

                _context.Budgets.Add(newBudget);
                _context.SaveChanges();

                MessageBox.Show($"Presupuesto {Folio} guardado exitosamente para el paciente {PatientName}.", "Éxito");
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar: {ex.Message}", "Error");
            }
        }

        private void PrintBudget()
        {
            if (SelectedPatient == null || !Items.Any())
            {
                MessageBox.Show("Seleccione un paciente y agregue al menos un tratamiento para imprimir.", "Atención");
                return;
            }

            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("      DENTAL NEXUP      ");
                sb.AppendLine("------------------------------");
                sb.AppendLine($"Folio: {Folio}");
                sb.AppendLine($"Fecha: {Date:d}");
                sb.AppendLine($"Paciente: {PatientName}");
                sb.AppendLine("------------------------------");
                
                foreach (var item in Items)
                {
                    sb.AppendLine($"{item.Quantity} x {item.Description}");
                    sb.AppendLine($"   {item.TotalMXN:C}");
                }
                
                sb.AppendLine("------------------------------");
                sb.AppendLine($"Total MXN: {TotalMXN:C}");
                sb.AppendLine($"Total USD: {TotalUSD:C}");
                sb.AppendLine($"Anticipo: {DownPayment:C}");
                sb.AppendLine($"Resto: {RemainingBalance:C}");
                sb.AppendLine("------------------------------");
                sb.AppendLine("   ¡Gracias por su preferencia!   ");
                sb.AppendLine(" ");
                sb.AppendLine(" ");

                // IMPRESIÓN DIRECTA A LA IMPRESORA DE TICKETS
                // ⚠️ CAMBIA "POS-80" POR EL NOMBRE EXACTO DE TU IMPRESORA
                string printerName = "POS-80"; 
                RawPrinterHelper.SendStringToPrinter(printerName, sb.ToString());
                
                MessageBox.Show("Ticket enviado a la impresora.", "Imprimir");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al imprimir: {ex.Message}\n\nVerifica que el nombre de la impresora sea correcto.", "Error");
            }
        }

        private void ClearForm()
        {
            Items.Clear();
            SelectedPatient = null;
            Observations = "";
            DownPayment = 0;
            NumberOfPayments = 1;
            RecalculateTotals();
            Folio = $"PRES-{DateTime.Now:yyyyMMdd}-{new Random().Next(100, 999)}";
        }
    }

    public class BudgetItemViewModel : INotifyPropertyChanged
    {
        public Models.Treatment Treatment { get; set; }
        
        private int _quantity = 1;
        public int Quantity 
        { 
            get => _quantity; 
            set { _quantity = value; OnPropertyChanged(nameof(Quantity)); OnPropertyChanged(nameof(TotalMXN)); OnPropertyChanged(nameof(TotalUSD)); } 
        }

        private string _description;
        public string Description 
        { 
            get => _description; 
            set { _description = value; OnPropertyChanged(nameof(Description)); } 
        }

        private decimal _unitPriceMXN;
        public decimal UnitPriceMXN 
        { 
            get => _unitPriceMXN; 
            set { _unitPriceMXN = value; OnPropertyChanged(nameof(UnitPriceMXN)); OnPropertyChanged(nameof(TotalMXN)); OnPropertyChanged(nameof(TotalUSD)); } 
        }

        private decimal _discountPercentage;
        public decimal DiscountPercentage 
        { 
            get => _discountPercentage; 
            set { _discountPercentage = value; OnPropertyChanged(nameof(DiscountPercentage)); OnPropertyChanged(nameof(TotalMXN)); OnPropertyChanged(nameof(TotalUSD)); } 
        }

        public decimal TotalMXN => (UnitPriceMXN * Quantity) * (1 - (DiscountPercentage / 100));
        public decimal TotalUSD => TotalMXN / 18.00m;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

