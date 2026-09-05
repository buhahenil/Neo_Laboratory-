# 🔬 Neo Laboratory — Enterprise Pathology & Clinical Management System
A full-stack medical laboratory and pathology platform built to streamline multi-branch operations, diagnostic test parameter entry, patient scheduling, interactive analytics, and automated PDF report generation with QR verification.

## 🌟 Key Features
* **📊 Multi-Branch Analytics & Insights:** Real-time Chart.js visualizers tracking monthly collections, patient volume growth, revenue per patient, and branch-wise performance comparisons.
* **🔒 Role-Based Access Control (RBAC):** Strict security boundaries across Super Admin, Branch Managers, Lab Technologists, Doctors, and Patients.
* **🧪 Diagnostic Parameter Management:** Pre-configured support for multi-parameter clinical panels including **CBC, LFT, KFT/RFT, Lipid Profile, Thyroid, and HbA1c**.
* **📄 Automated PDF & QR Verification:** Server-side PDF report and receipt generation featuring digital pathologist signatures and scannable QR verification codes.
* **🌙 Modern Glassmorphic & Accessible UI:** 100% responsive layout with instant light/dark mode switching and mobile drawer navigation.

## 🛠 Tech Stack
### **Backend**
* **Language & Framework:** C# | .NET 8 Web API
* **Database & Persistence:** Microsoft SQL Server | Dapper | Stored Procedures
* **Authentication:** JWT (JSON Web Tokens) with Password Hashing & Salt
* **PDF Generation:** PdfSharp Library
### **Frontend**
* **Framework:** Angular (Standalone Components & Signals)
* **Styling & UI:** Bootstrap 5 | SCSS | Glassmorphic Design System
* **Data Visualization:** Chart.js
* **Asynchronous Flow:** RxJS & HTTP Interceptors
