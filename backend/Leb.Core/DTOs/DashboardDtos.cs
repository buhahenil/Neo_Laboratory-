using System;
using System.Collections.Generic;
using Leb.Core.Entities;

namespace Leb.Core.DTOs
{
    public class PopularTestStatsDto
    {
        public string TestName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public decimal RevenueGenerated { get; set; }
    }

    public class MonthlyRevenueStatsDto
    {
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
    }

    public class RecentActivityStatsDto
    {
        public int AppointmentId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PatientName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    public class MonthlyPatientsStatsDto
    {
        public string MonthName { get; set; } = string.Empty;
        public int PatientsCount { get; set; }
    }

    public class BranchCollectionStatsDto
    {
        public string BranchName { get; set; } = string.Empty;
        public decimal Collection { get; set; }
    }

    public class BranchPatientStatsDto
    {
        public string BranchName { get; set; } = string.Empty;
        public int PatientsCount { get; set; }
    }

    public class AdminDashboardStatsDto
    {
        public int TotalPatients { get; set; }
        public int TotalAppointments { get; set; }
        public int TodayBookings { get; set; }
        public int PendingReports { get; set; }
        public decimal TotalRevenue { get; set; }
        
        public int NewPatients { get; set; }
        public int ReturningPatients { get; set; }
        public decimal AverageRevenuePerPatient { get; set; }

        public List<PopularTestStatsDto> PopularTests { get; set; } = new List<PopularTestStatsDto>();
        public List<MonthlyRevenueStatsDto> MonthlyRevenue { get; set; } = new List<MonthlyRevenueStatsDto>();
        public List<MonthlyPatientsStatsDto> MonthlyPatients { get; set; } = new List<MonthlyPatientsStatsDto>();
        public List<BranchCollectionStatsDto> BranchCollections { get; set; } = new List<BranchCollectionStatsDto>();
        public List<BranchPatientStatsDto> BranchPatients { get; set; } = new List<BranchPatientStatsDto>();
        public List<RecentActivityStatsDto> RecentActivity { get; set; } = new List<RecentActivityStatsDto>();
    }

    public class PatientDashboardDto
    {
        public Appointment? UpcomingAppointment { get; set; }
        public List<Appointment> RecentAppointments { get; set; } = new List<Appointment>();
        public List<Report> RecentReports { get; set; } = new List<Report>();
        public List<Payment> RecentPayments { get; set; } = new List<Payment>();
        public List<Notification> Notifications { get; set; } = new List<Notification>();
    }
}
