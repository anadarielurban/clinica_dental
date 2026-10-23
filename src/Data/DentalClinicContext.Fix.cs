using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace DentalClinicWPF.Data
{
    public partial class DentalClinicContext
    {
        public override int SaveChanges()
        {
            NormalizeStringsBeforeSave();
            ValidateBeforeSave();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            NormalizeStringsBeforeSave();
            ValidateBeforeSave();
            return base.SaveChangesAsync(cancellationToken);
        }

        // 1) Rellena "" los strings NOT NULL que vienen null
        private void NormalizeStringsBeforeSave()
        {
            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                    continue;

                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.ClrType == typeof(string)
                        && !property.Metadata.IsNullable
                        && property.CurrentValue == null)
                    {
                        property.CurrentValue = "";
                    }
                }
            }
        }

        // 2) Valida [Required], [EmailAddress], [RegularExpression]
        private void ValidateBeforeSave()
        {
            var problems = new List<string>();

            foreach (var entry in ChangeTracker.Entries())
            {
                if (entry.State != EntityState.Added && entry.State != EntityState.Modified)
                    continue;

                var entityType = entry.Metadata.ClrType;
                var entityName = GetEntitySpanishName(entityType.Name);

                foreach (var property in entry.Properties)
                {
                    if (property.Metadata.IsPrimaryKey()) continue;

                    var propInfo = entityType.GetProperty(property.Metadata.Name);
                    if (propInfo == null) continue;

                    var value = property.CurrentValue;
                    var clrType = property.Metadata.ClrType;
                    var fieldName = GetFieldSpanishName(property.Metadata.Name);
                    string? error = null;

                    if (propInfo.GetCustomAttribute<RequiredAttribute>() != null)
                    {
                        if (clrType == typeof(string) && string.IsNullOrWhiteSpace((string?)value))
                            error = "falta rellenar";
                        else if (clrType == typeof(int) && (int)value! == 0)
                            error = "debe seleccionarse";
                        else if (clrType == typeof(DateTime) && (DateTime)value! == default)
                            error = "falta la fecha";
                    }

                    if (error == null && clrType == typeof(string)
                        && propInfo.GetCustomAttribute<EmailAddressAttribute>() != null
                        && !string.IsNullOrWhiteSpace((string?)value)
                        && !new EmailAddressAttribute().IsValid(value))
                    {
                        error = "no es un correo válido";
                    }

                    if (error == null && clrType == typeof(string))
                    {
                        var regexAttr = propInfo.GetCustomAttribute<RegularExpressionAttribute>();
                        if (regexAttr != null && !string.IsNullOrWhiteSpace((string?)value)
                            && !regexAttr.IsValid(value))
                        {
                            error = "formato inválido";
                        }
                    }

                    if (error != null)
                        problems.Add("• " + entityName + " → " + fieldName + ": " + error);
                }
            }

            if (problems.Count > 0)
            {
                throw new ValidationException(
                    "Revisa los siguientes campos antes de guardar:\n\n"
                    + string.Join("\n", problems)
                );
            }
        }

        private static string GetEntitySpanishName(string typeName) => typeName switch
        {
            "Supplier" => "Proveedor",
            "Employee" => "Empleado",
            "Equipment" => "Equipo",
            "Appointment" => "Cita",
            "Patient" => "Paciente",
            "Dentist" => "Odontólogo",
            "Prescription" => "Receta",
            "LabOrder" => "Orden de laboratorio",
            "MedicalRecord" => "Expediente",
            "Treatment" => "Tratamiento",
            "TreatmentPlan" => "Plan de tratamiento",
            "Inventory" => "Inventario",
            "Invoice" => "Factura",
            "Room" => "Consultorio",
            "Schedule" => "Horario",
            _ => typeName
        };

        private static string GetFieldSpanishName(string propName) => propName switch
        {
            "CompanyName" => "Nombre de la empresa",
            "ContactName" => "Nombre de contacto",
            "PhoneNumber" => "Teléfono",
            "Email" => "Correo electrónico",
            "Address" => "Dirección",
            "FirstName" => "Nombre",
            "LastName" => "Apellido",
            "Position" => "Cargo",
            "Name" => "Nombre",
            "Model" => "Modelo",
            "SerialNumber" => "Número de serie",
            "ProductName" => "Nombre del producto",
            "Medication" => "Medicamento",
            "Dosage" => "Dosis",
            "Instructions" => "Instrucciones",
            "Description" => "Descripción",
            "Notes" => "Notas",
            "Specialization" => "Especialidad",
            "Status" => "Estado",
            "RoomNumber" => "Número de consultorio",
            "PatientID" => "Paciente",
            "DentistID" => "Odontólogo",
            "TreatmentID" => "Tratamiento",
            "SupplierID" => "Proveedor",
            "RoomID" => "Consultorio",
            "AppointmentDate" => "Fecha de cita",
            "DateOfBirth" => "Fecha de nacimiento",
            "Gender" => "Género",
            "DateIssued" => "Fecha de emisión",
            "OrderDate" => "Fecha de orden",
            "StartDate" => "Fecha de inicio",
            "EndDate" => "Fecha de fin",
            "Price" => "Precio",
            "Duration" => "Duración",
            _ => propName
        };
    }
}
