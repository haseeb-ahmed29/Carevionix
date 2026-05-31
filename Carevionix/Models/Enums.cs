namespace Carevionix.Models;

public enum AppointmentStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2,
    Completed = 3,
    Cancelled = 4,
    Rescheduled = 5
}

public enum ConsultationType
{
    Video = 0,
    Audio = 1,
    Chat = 2
}

public enum HealthRecordType
{
    MedicalHistory = 0,
    LabReport = 1,
    XrayReport = 2,
    Prescription = 3,
    ConsultationHistory = 4,
    Document = 5
}

public enum NotificationType
{
    AppointmentBooked = 0,
    AppointmentReminder = 1,
    PrescriptionAdded = 2,
    FollowUpReminder = 3,
    InvoiceGenerated = 4,
    RecordUploaded = 5
}

public enum PaymentStatus
{
    Unpaid = 0,
    Submitted = 1,
    Approved = 2
}
