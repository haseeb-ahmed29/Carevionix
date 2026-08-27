# Carevionix User Guide

Welcome to **Carevionix**! This user guide provides new users with a comprehensive understanding of the project, including how to set up and use the system, the core problem statement it addresses, how Carevionix solves these challenges, and a complete breakdown of how data flows through the application.

---

## 1. Project Overview & How to Use

### Project Overview
Carevionix is a full-stack Web application built on **ASP.NET Core (.NET 8)** designed to streamline healthcare services, virtual/in-person patient consultation management, health records storage and sharing, appointment scheduling, electronic prescriptions, and billing.

### Technical Prerequisites
- **.NET 8 SDK** (or higher)
- **SQL Server** (LocalDB, Express, or full SQL Server instance)
- A modern web browser (Chrome, Firefox, Edge, Safari)

### How to Get Started

1. **Clone the Repository**
   ```bash
   git clone <repository-url>
   cd Carevionix
   ```

2. **Configure App Settings**
   - Copy `Carevionix/appsettings.example.json` to create `Carevionix/appsettings.json`.
   - Configure your SQL Server database connection string (`DefaultConnection`) and optional JWT authentication settings.

3. **Restore & Build**
   ```bash
   dotnet restore
   dotnet build Carevionix/Carevionix.csproj
   ```

4. **Run the Application**
   ```bash
   dotnet run --project Carevionix/Carevionix.csproj
   ```

5. **Access the System**
   - Open your browser and navigate to the local HTTPS URL printed in the terminal (e.g., `https://localhost:7001`).
   - Log in using seeded default accounts or register a new Patient account.

---

## 2. Problem Statement

Modern healthcare providers and patients frequently face operational bottlenecks and communication gaps due to fragmented systems:

1. **Fragmented Patient Information**: Patient health records, medical histories, and past consultation logs are often isolated across different clinics or physical paper charts, leading to missing history during critical care decisions.
2. **Inefficient Scheduling & High No-Show Rates**: Traditional phone-in appointment booking leads to scheduling conflicts, lack of visibility into doctor availability, and missed appointments without real-time notifications.
3. **Disjointed Consultation & Prescription Workflows**: Connecting video or in-person consultations with immediate digital prescription issuance and follow-up tracking is difficult across separate platforms.
4. **Opaque Invoicing and Billing**: Patients struggle to view clear invoice breakdowns for consultations and prescribed medicines, while clinics face manual billing overhead.
5. **Security and Access Control**: Managing safe data sharing between patients, attending physicians, and administrative staff requires strict role-based access control and audit trails.

---

## 3. How Carevionix Solves the Problem

Carevionix solves these challenges through a centralized, secure, and modern digital healthcare platform:

- **Centralized Health Records (EHR/EMR)**: Patients can securely upload, organize, and store personal health records and grant access to specific doctors when necessary.
- **Smart Appointment Scheduling**: Real-time availability tracking allows patients to search for doctors by specialty, view available slots, and book appointments seamlessly.
- **Integrated Consultations & Telehealth**: Doctors and patients can interact through scheduled consultations, live video signaling integrations, and real-time chat sessions.
- **Digital Prescriptions**: Physicians can create structured digital prescriptions linked directly to completed consultations and patient records.
- **Automated Invoicing & Billing**: Invoices are generated automatically for appointments and services, providing transparency for patients and administrative oversight for healthcare managers.
- **Multi-Role Security & RBAC**: Strict separation of concerns via ASP.NET Core Identity (Admin, Doctor, Patient roles) ensuring data privacy and appropriate access rights.

---

## 4. Full Workflow & Data Flow Architecture

### High-Level Architecture Flow

Data moves through a clean layered architecture designed for maintainability, security, and scalability:

```text
[ User Interface / Web Browser ]
              │
              ▼ (HTTP / HTTPS Requests, Forms, APIs, Anti-Forgery Tokens)
[ Authentication & Authorization Middleware ]
              │ (ASP.NET Core Identity / Cookie / JWT Authentication)
              ▼
[ Controllers ] (AccountController, PatientController, DoctorController, AdminController, etc.)
              │
              ▼ (DTOs & ViewModels mapping)
[ Application Services ] (AppointmentService, HealthRecordService, ConsultationService, PrescriptionService, etc.)
              │
              ▼
[ Repositories ] (IRepository<T> / Repository<T> abstraction)
              │
              ▼
[ Entity Framework Core (ApplicationDbContext) ]
              │
              ▼
[ SQL Server Database ]
```

---

### Step-by-Step User Role Workflows

#### A. Patient Workflow
1. **Registration / Login**: Patient registers an account, logs in via Cookie or Bearer Authentication.
2. **Doctor Discovery & Scheduling**: Patient searches doctors by specialty/availability and creates an appointment request (`AppointmentService`).
3. **Consultation**: Patient joins scheduled virtual/in-person session, exchanges messages via `ChatService`, or connects via `VideoSignalController`.
4. **Records & Prescriptions**: Patient views digital prescriptions issued by the doctor (`PrescriptionService`) and manages/shares personal health records (`HealthRecordService`).
5. **Billing**: Patient reviews generated invoices and payment status (`InvoiceService`).

#### B. Doctor Workflow
1. **Availability Management**: Doctor updates working slots and consultation hours (`DoctorAvailability`).
2. **Appointment Processing**: Doctor reviews incoming appointments, accepts or reschedules them.
3. **Consultation & Diagnosis**: Doctor conducts consultations, logs medical notes, and creates structured digital prescriptions (`PrescriptionMedicine`).
4. **Record Access**: Doctor views shared health records from patients to make informed diagnoses.

#### C. Administrator Workflow
1. **User Management**: Admin oversees system user accounts, doctor verifications, and role assignments.
2. **System Auditing & Reports**: Admin monitors system analytics, overall appointment volumes, invoicing status, and platform logs.

---

## 5. System Features Summary

| Feature Module | Key Components & Responsibilities |
| :--- | :--- |
| **Authentication & RBAC** | Identity management, role checks (Patient/Doctor/Admin), Cookie & JWT schemes |
| **Appointments** | Doctor availability slots, scheduling, status updates (Pending, Confirmed, Completed, Cancelled) |
| **Consultations & Chat** | Consultation session notes, real-time chat messages, video signaling endpoints |
| **Health Records** | EHR upload, record sharing permissions, medical history tracking |
| **Prescriptions** | Digital prescriptions, medicine dosages, instruction details |
| **Invoicing** | Service billing, invoice tracking, status updates |
| **Notifications** | In-app alerts, email notifications (`EmailNotificationService`) |
