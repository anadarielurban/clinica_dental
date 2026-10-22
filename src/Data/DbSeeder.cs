using System.Linq;
using DentalClinicWPF.Models;

namespace DentalClinicWPF.Data
{
    public static class DbSeeder
    {
        public static void Seed(DentalClinicContext context)
        {
            // Si no hay consultorios, creamos el único consultorio
            if (!context.Set<Room>().Any())
            {
                var mainRoom = new Room
                {
                    RoomNumber = "Consultorio 1",
                    Address = "Av. Principal #123, Col. Centro",
                    IsAvailable = true
                };
                context.Set<Room>().Add(mainRoom);
                context.SaveChanges();

                // Asignar a todos los dentistas a este consultorio
                var dentists = context.Set<Dentist>().ToList();
                foreach (var d in dentists)
                {
                    d.RoomID = mainRoom.RoomID;
                }

                // Asignar a todos los equipos a este consultorio
                var equipments = context.Set<Equipment>().ToList();
                foreach (var e in equipments)
                {
                    e.RoomID = mainRoom.RoomID;
                }

                context.SaveChanges();
            }
        }
    }
}