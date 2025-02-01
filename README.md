# DentalClinic

The DentalClinicWPF application is a desktop application built using WPF and MVVM for managing a dental clinic’s operations. It includes functionalities for handling appointments, patients, dentists, invoices, medical records, treatments, prescriptions, inventory, and staff management.

Technologies used in the project:

.NET (C#)

WPF (Windows Presentation Foundation)

Entity Framework Core – ORM for database management.

CommunityToolkit.Mvvm – Simplifies MVVM implementation.

XAML – Defines the user interface.

SQL Server – Database system.

-------------------------------------

![Zrzut ekranu 2025-02-01 152547](https://github.com/user-attachments/assets/116ce3ef-67df-4714-aace-fcb1fe19eca3)

Upcoming Appointments: A table listing scheduled appointments with patient names, assigned dentists, and appointment times.

Financial Statistics: Displays total revenue and the number of unpaid invoices.

Revenue per Dentist: A breakdown of earnings by each dentist in the clinic.

Revenue Over Time Chart: A bar graph illustrating revenue trends over recent months.

Export Options: Buttons for exporting the dashboard data to PDF or Excel.

Quick Actions: Buttons for adding new appointments and patients.

![Zrzut ekranu 2025-02-01 152953](https://github.com/user-attachments/assets/bcc92f8b-65f8-42ea-9974-07517c3dcc66)
![Zrzut ekranu 2025-02-01 153000](https://github.com/user-attachments/assets/bff15de1-e51a-4737-95eb-272fcbca7879)


Patient List View:

![Zrzut ekranu 2025-02-01 152633](https://github.com/user-attachments/assets/c0a6d4e4-de2b-4035-9cb2-3f033214257e)

Displays a table of registered patients with their first name, last name, phone number, email, and date of birth.

Provides search, reload, add, edit, and view details functionalities.

Sorting options allow organization by various criteria.

Add New Patient Form:

![Zrzut ekranu 2025-02-01 152643](https://github.com/user-attachments/assets/e3a6c835-ef23-4c39-8733-9b1facbd7e44)

A form for entering new patient details including first name, last name, date of birth, gender, phone number, email, and address.

Dropdown selection for gender and automatic validation of phone numbers and emails.

Edit/Delete Patient Form:

![Zrzut ekranu 2025-02-01 152649](https://github.com/user-attachments/assets/0316c1ec-1006-47fe-955f-afbe5985d711)

Enables updating patient details with options to cancel, save, or delete.

Date of birth selection via a DatePicker.

Validation Errors:

![Zrzut ekranu 2025-02-01 152704](https://github.com/user-attachments/assets/e35ad1ef-a910-42f8-a31e-f8d257d9a717)

Shows red error messages when invalid data is entered.

Includes rules such as name starting with an uppercase letter, valid email format, phone number structure, and address format.

Prevents adding underage patients.

Search & Sorting Functionality:

![Zrzut ekranu 2025-02-01 152741](https://github.com/user-attachments/assets/1a067a3a-12ba-4040-818a-b44745a77972)

![Zrzut ekranu 2025-02-01 152748](https://github.com/user-attachments/assets/0d59329b-b824-4e81-8ec9-f40b713c7577)
Allows filtering patients by name and sorting them by different attributes (e.g., Date of Birth).

The patient list updates dynamically based on search criteria.

Patient Details View:

![Zrzut ekranu 2025-02-01 152715](https://github.com/user-attachments/assets/1ca3177b-27f8-4e10-ae0f-ddd933424191)

Displays patient information:

Name, date of birth, phone number, email, and address.

Statistics Section:

Total number of visits.

Payment status for invoices.

Tabbed Interface:

Appointments: List of scheduled appointments with dates, times, assigned dentists, treatments, and notes.

Prescriptions, Treatment Plans, Invoices, and Medical Documentation tabs for comprehensive patient records.

Search & Export Functionality:

Search/filter patient history.

Export patient details to PDF.

Action Buttons:

Add, Edit, and Preview options for managing appointments.

Appointment Preview:

![Zrzut ekranu 2025-02-01 152722](https://github.com/user-attachments/assets/0f44207c-29bd-4d23-b702-12f2080b1b12)

A detailed preview window for a selected appointment.

Displays:

Appointment ID, date, and time.

Patient details (name and DOB).

Assigned dentist and treatment.

Additional notes.

Weekly Appointment Overview:

![Zrzut ekranu 2025-02-01 152801](https://github.com/user-attachments/assets/8e58ad33-a917-499a-b1d1-a3f1472adad4)
![Zrzut ekranu 2025-02-01 152808](https://github.com/user-attachments/assets/c2688839-08b2-4706-9ae2-6591ec6c15c6)

The calendar presents scheduled appointments for the selected week, organized by date.

Each entry includes:

Time of appointment

Patient name

Treatment type

Additional treatment details (if applicable)

Displays the dentist’s details, including:

Specialization (e.g., Orthodontics)

Phone number & email

Availability (e.g., Monday–Friday, 9:00–18:00)

Key Performance Metrics:

Total number of appointments this week

Most frequent procedure performed

"Previous Week" and "Next Week" buttons allow easy navigation between weeks.

Invoice Details View

![Zrzut ekranu 2025-02-01 152824](https://github.com/user-attachments/assets/f66beb7f-a1d8-4c82-ab97-118db545edf3)

Shows billing details for a patient's dental visit.

Includes:

Invoice ID

Patient details (Name & DOB)

Dentist’s name

Treatment type

Appointment date

Total amount billed

Payment status (Paid/Unpaid)

Action Buttons:

Preview: Displays a print-friendly version of the invoice.

Print: Allows direct printing.

Export to PDF: Saves the invoice as a PDF for record-keeping or sharing with the patient.

Printed Invoice PDF Preview

![Zrzut ekranu 2025-02-01 153004](https://github.com/user-attachments/assets/fdc36836-3334-44cb-a37f-b3738897329f)

A formatted document containing:

Clinic name, address, and phone number at the top.

Invoice details, including bank account number for payments.

Patient and treatment details.

Signatures section for clinic and client authorization.










