using Carevionix.DTOs;
using Carevionix.Models;

namespace Carevionix.ViewModels;

public class HomeIndexViewModel
{
    public int ActiveDoctors { get; set; }
    public int Patients { get; set; }
    public int Appointments { get; set; }
    public int CompletedConsultations { get; set; }
    public bool DatabaseConnected { get; set; }
}

public class PatientDashboardViewModel
{
    public Patient Patient { get; set; } = null!;
    public IReadOnlyList<Appointment> Appointments { get; set; } = [];
    public IReadOnlyList<Notification> Notifications { get; set; } = [];
}

public class DoctorDashboardViewModel
{
    public Doctor Doctor { get; set; } = null!;
    public IReadOnlyList<Appointment> Appointments { get; set; } = [];
}

public class AdminDashboardViewModel
{
    public int Doctors { get; set; }
    public int Patients { get; set; }
    public int Appointments { get; set; }
    public int Messages { get; set; }
    public int HealthRecords { get; set; }
    public int ContactSubmissions { get; set; }
    public int PendingAppointments { get; set; }
    public int CompletedConsultations { get; set; }
    public decimal Revenue { get; set; }
}

public class AdminAppointmentHistoryViewModel
{
    public IReadOnlyList<Appointment> Appointments { get; set; } = [];
    public string? Search { get; set; }
    public string Status { get; set; } = "All";
    public string Invoice { get; set; } = "All";
}

public class AdminSystemControlViewModel : AdminDashboardViewModel
{
    public int ActiveDoctors { get; set; }
    public int InactiveDoctors { get; set; }
    public int ActivePatients { get; set; }
    public int InactivePatients { get; set; }
    public int OpenContactSubmissions { get; set; }
    public DateTime ServerTimeUtc { get; set; }
}

public class AdminUsersViewModel<T>
{
    public IReadOnlyList<T> Items { get; set; } = [];
    public AdminDoctorDto DoctorForm { get; set; } = new(null, string.Empty, string.Empty, null, string.Empty, null, null, null, null, 0, 0, true, null);
    public AdminPatientDto PatientForm { get; set; } = new(null, string.Empty, string.Empty, null, null, null, null, null, null, null, true, null);
}

public class DoctorProfileViewModel
{
    public Doctor Doctor { get; set; } = null!;
    public DoctorProfileDto Profile { get; set; } = new(string.Empty, null, null, string.Empty, null, null, null, null, 0, 0);
    public ChangePasswordDto Password { get; set; } = new(string.Empty, string.Empty, string.Empty);
}

public class PatientProfileViewModel
{
    public Patient Patient { get; set; } = null!;
    public PatientProfileDto Profile { get; set; } = new(string.Empty, null, null, null, null, null, null, null);
    public ChangePasswordDto Password { get; set; } = new(string.Empty, string.Empty, string.Empty);
}

public class AdminProfileViewModel
{
    public ApplicationUser Admin { get; set; } = null!;
    public AdminProfileDto Profile { get; set; } = new(string.Empty, string.Empty, null, null);
    public ChangePasswordDto Password { get; set; } = new(string.Empty, string.Empty, string.Empty);
}

public class FindDoctorsViewModel
{
    public DoctorSearchDto Filter { get; set; } = new(null, null, null, null, null);
    public IReadOnlyList<Doctor> Doctors { get; set; } = [];
}

public class BookAppointmentViewModel
{
    public BookAppointmentDto Appointment { get; set; } = new(0, DateTime.Now.AddDays(1), ConsultationType.Video, string.Empty, null);
    public IReadOnlyList<Doctor> Doctors { get; set; } = [];
}

public record ReportRowViewModel(string Label, int Count);

public class AiDiagnosisViewModel
{
    public AiDiagnosisRequestDto Request { get; set; } = new(string.Empty, 0, null, null);
    public AiDiagnosisResultDto? Result { get; set; }
}
