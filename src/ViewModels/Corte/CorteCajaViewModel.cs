using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Helpers;
using DentalClinicWPF.ViewModels.Base;
using Microsoft.EntityFrameworkCore;

// ALIAS para evitar el conflicto con el namespace "Invoice"
using InvoiceEntity = DentalClinicWPF.Models.Invoice;

namespace DentalClinicWPF.ViewModels.Corte
{
    public class CorteCajaViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;
        
        private DateTime _selectedDate;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged(nameof(SelectedDate));
                CalcularCorte();
            }
        }

        private decimal _totalIngresos;
        public decimal TotalIngresos
        {
            get => _totalIngresos;
            set { _totalIngresos = value; OnPropertyChanged(nameof(TotalIngresos)); }
        }

        private int _cantidadTickets;
        public int CantidadTickets
        {
            get => _cantidadTickets;
            set { _cantidadTickets = value; OnPropertyChanged(nameof(CantidadTickets)); }
        }

        public ObservableCollection<InvoiceEntity> TicketsDelDia { get; set; }

        public ICommand GenerarCorteCommand { get; }

        public CorteCajaViewModel(DentalClinicContext context)
        {
            _context = context;
            TicketsDelDia = new ObservableCollection<InvoiceEntity>();
            SelectedDate = DateTime.Today;
            
            GenerarCorteCommand = new RelayCommand(CalcularCorte);
        }

        private void CalcularCorte()
        {
            try
            {
                TicketsDelDia.Clear();
                
                var todasLasFacturas = _context.Invoice
                    .Include(i => i.Patient)
                    .ToList();

                // ========================================================
                // NOTA: Como tu entidad Invoice no tiene una propiedad
                // de fecha estándar (Date, CreatedAt, etc.) ni un Id,
                // por ahora mostramos TODAS las facturas pagadas.
                //
                // Cuando me pases el código de Models/Invoice.cs,
                // podremos filtrar por el día seleccionado.
                // ========================================================
                var filtradas = todasLasFacturas
                    .Where(i => i.IsPaid) 
                    .ToList();

                foreach (var ticket in filtradas)
                {
                    TicketsDelDia.Add(ticket);
                }

                CantidadTickets = TicketsDelDia.Count;
                TotalIngresos = TicketsDelDia.Sum(i => i.TotalAmount);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error al generar el corte: {ex.Message}", "Error");
            }
        }
    }
}
