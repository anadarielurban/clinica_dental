[![.NET](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/) [![WPF](https://img.shields.io/badge/WPF-Desktop-blueviolet)]() [![Build Status](https://img.shields.io/github/actions/workflow/status/yourusername/DentalClinicWPF/ci.yml?branch=main)](https://github.com/yourusername/DentalClinicWPF/actions) [![License: MIT](https://img.shields.io/badge/License-MIT-green)](LICENSE)

# 🦷 DentalClinicWPF

A modern **WPF (C#)** desktop application for end‑to‑end management of a dental clinic. The app tracks **appointments, patients, dentists, invoices, treatments, prescriptions, inventory, and staff** with an intuitive MVVM front‑end and a robust Entity Framework Core backend.

---

## 📌 Table of Contents

* [✨ Key Features](#✨-key-features)
* [🛠️ Tech Stack](#🛠️-tech-stack)
* [🏗️ Project Structure](#🏗️-project-structure)
* [⚙️ Installation](#⚙️-installation)
* [▶️ Running the App](#▶️-running-the-app)
* [📸 Screenshots](#📸-screenshots)
* [📄 License](#📄-license)

---

## ✨ Key Features

* **Dashboard KPI Widgets** – upcoming appointments, revenue, unpaid invoices, revenue per dentist & timeline chart.
* **Appointments** – weekly calendar, dentist availability, quick add/edit/preview.
* **Patients** – full CRUD with validation (uppercase names, phone/email rules, age ≥ 18), search & sort, detailed history tabs.
* **Billing** – invoice generation, status tracking, print & PDF export.
* **Treatments & Prescriptions** – per‑patient record keeping with print‑ready docs.
* **Inventory** – stock levels, reorder alerts (optional feature flag).
* **Staff Management** – dentists & assistants, working hours, revenue breakdown.
* **MVVM & CommunityToolkit.Mvvm** – clean separation, async commands, dependency injection.
* **Export** – one‑click Excel/PDF export for dashboards and lists.

---

## 🛠️ Tech Stack

| Layer           | Technology                                      |
| --------------- | ----------------------------------------------- |
| **Language**    | C# 12                                           |
| **Runtime**     | .NET 8 LTS                                      |
| **UI**          | Windows Presentation Foundation (WPF)           |
| **Patterns**    | MVVM + CommunityToolkit.Mvvm                    |
| **Database**    | SQL Server Express (local)                      |
| **ORM**         | Entity Framework Core 8 – Code‑First Migrations |
| **Charts**      | LiveCharts 2                                    |
| **PDF / Excel** | QuestPDF & EPPlus                               |
| **Testing**     | xUnit, Moq, FluentAssertions                    |
| **CI/CD**       | GitHub Actions – build, test, code‑analysis     |

---

## 🏗️ Project Structure

```text
DentalClinicWPF.sln
│
├─ DentalClinic.Domain/          # Entities & business rules
├─ DentalClinic.Infrastructure/  # EF Core DbContext & migrations
├─ DentalClinic.Application/     # Services, DTOs, validators
├─ DentalClinic.Presentation/    # WPF views (XAML), view‑models
└─ DentalClinic.Tests/           # Unit & integration tests
```

---

## ⚙️ Installation

### 🔑 Prerequisites

* **Windows 10/11**
* **.NET SDK 8.0+**
  `winget install Microsoft.DotNet.SDK.8`
* **SQL Server Express 2019+** *(or change the connection string to another SQL Server instance)*
* *(Optional)* **Visual Studio 2022** with ".NET Desktop Development" workload.

### 🏃‍♂️ Quick Start

```bash
# 1  Clone repository
git clone https://github.com/yourusername/DentalClinicWPF.git
cd DentalClinicWPF

# 2  Restore NuGet packages
dotnet restore

# 3  Configure database
#    Edit ./DentalClinic.Infrastructure/appsettings.json
#    Default connection string uses (localdb)\\MSSQLLocalDB

# 4  Apply migrations & seed sample data
dotnet ef database update --project DentalClinic.Infrastructure

# 5  Run the application
dotnet run --project DentalClinic.Presentation
```

> **Tip:** Opening the solution in Visual Studio auto‑restores packages and sets **DentalClinic.Presentation** as the startup project.

---

## ▶️ Running the App

* Login screen is skipped; the demo database seeds two dentists and three test patients.
* Dashboard opens by default – use the left navigation bar to access Patients, Appointments, Billing, etc.
* Press **F1** anywhere for contextual help overlays.

---

## 📸 Screenshots

| #  | Screen                                                                               | Description                                                  |
| -- | ------------------------------------------------------------------------------------ | ------------------------------------------------------------ |
| 1  | ![](https://github.com/user-attachments/assets/116ce3ef-67df-4714-aace-fcb1fe19eca3) | **Dashboard** – upcoming appointments, revenue KPIs & charts |
| 2  | ![](https://github.com/user-attachments/assets/bcc92f8b-65f8-42ea-9974-07517c3dcc66) | **Appointment Form** – schedule new visit                    |
| 3  | ![](https://github.com/user-attachments/assets/bff15de1-e51a-4737-95eb-272fcbca7879) | **Appointment List** – sortable, filterable                  |
| 4  | ![](https://github.com/user-attachments/assets/c0a6d4e4-de2b-4035-9cb2-3f033214257e) | **Patients** – search & CRUD table                           |
| 5  | ![](https://github.com/user-attachments/assets/e3a6c835-ef23-4c39-8733-9b1facbd7e44) | **Add Patient** – validated form                             |
| 6  | ![](https://github.com/user-attachments/assets/0316c1ec-1006-47fe-955f-afbe5985d711) | **Edit/Delete Patient**                                      |
| 7  | ![](https://github.com/user-attachments/assets/e35ad1ef-a910-42f8-a31e-f8d257d9a717) | **Validation Errors** – inline feedback                      |
| 8  | ![](https://github.com/user-attachments/assets/1a067a3a-12ba-4040-818a-b44745a77972) | **Search Bar** – live filtering                              |
| 9  | ![](https://github.com/user-attachments/assets/0d59329b-b824-4e81-8ec9-f40b713c7577) | **Sorting Menu**                                             |
| 10 | ![](https://github.com/user-attachments/assets/1ca3177b-27f8-4e10-ae0f-ddd933424191) | **Patient Details** – tabs & stats                           |
| 11 | ![](https://github.com/user-attachments/assets/0f44207c-29bd-4d23-b702-12f2080b1b12) | **Appointment Preview** modal                                |
| 12 | ![](https://github.com/user-attachments/assets/8e58ad33-a917-499a-b1d1-a3f1472adad4) | **Weekly Calendar (left)**                                   |
| 13 | ![](https://github.com/user-attachments/assets/c2688839-08b2-4706-9ae2-6591ec6c15c6) | **Weekly Calendar (right)**                                  |
| 14 | ![](https://github.com/user-attachments/assets/f66beb7f-a1d8-4c82-ab97-118db545edf3) | **Invoice Details** – actions & status                       |
| 15 | ![](https://github.com/user-attachments/assets/fdc36836-3334-44cb-a37f-b3738897329f) | **Invoice PDF Preview**                                      |

---

## 📄 License

Released under the [MIT License](LICENSE).
