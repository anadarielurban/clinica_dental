using System;
using DentalClinicWPF.Models;

namespace DentalClinicWPF.Events
{
    /// <summary>
    /// Evento que se publica cuando el usuario selecciona una receta
    /// desde el detalle del paciente para navegar al módulo de recetas
    /// con esa receta seleccionada.
    /// </summary>
    public class PrescriptionSelectedEvent
    {
        private static readonly Lazy<PrescriptionSelectedEvent> _instance =
            new Lazy<PrescriptionSelectedEvent>(() => new PrescriptionSelectedEvent());

        public static PrescriptionSelectedEvent Instance => _instance.Value;

        private PrescriptionSelectedEvent() { }

        public event Action<Prescription> PrescriptionSelected;

        public void Publish(Prescription prescription)
        {
            PrescriptionSelected?.Invoke(prescription);
        }

        public void Subscribe(Action<Prescription> handler)
        {
            PrescriptionSelected += handler;
        }

        public void Unsubscribe(Action<Prescription> handler)
        {
            PrescriptionSelected -= handler;
        }
    }
}