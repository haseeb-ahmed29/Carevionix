using Carevionix.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Carevionix.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<DoctorAvailability> DoctorAvailabilities => Set<DoctorAvailability>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionMedicine> PrescriptionMedicines => Set<PrescriptionMedicine>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<Consultation> Consultations => Set<Consultation>();
    public DbSet<DoctorReview> DoctorReviews => Set<DoctorReview>();
    public DbSet<HealthRecordShare> HealthRecordShares => Set<HealthRecordShare>();
    public DbSet<ContactSubmission> ContactSubmissions => Set<ContactSubmission>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.PatientProfile)
            .WithOne(p => p.User)
            .HasForeignKey<Patient>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ApplicationUser>()
            .HasOne(u => u.DoctorProfile)
            .WithOne(d => d.User)
            .HasForeignKey<Doctor>(d => d.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Appointment>()
            .HasOne(a => a.Patient)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Doctor)
            .WithMany(d => d.Appointments)
            .HasForeignKey(a => a.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasIndex(a => new { a.DoctorId, a.ScheduledAt })
            .IsUnique()
            .HasFilter("[Status] <> 2 AND [Status] <> 4");

        builder.Entity<Prescription>()
            .HasOne(p => p.Appointment)
            .WithOne(a => a.Prescription)
            .HasForeignKey<Prescription>(p => p.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Invoice>()
            .HasOne(i => i.Appointment)
            .WithOne(a => a.Invoice)
            .HasForeignKey<Invoice>(i => i.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Consultation>()
            .HasOne(c => c.Appointment)
            .WithOne(a => a.Consultation)
            .HasForeignKey<Consultation>(c => c.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DoctorReview>()
            .HasOne(r => r.Doctor)
            .WithMany(d => d.Reviews)
            .HasForeignKey(r => r.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DoctorReview>()
            .HasOne(r => r.Patient)
            .WithMany(p => p.DoctorReviews)
            .HasForeignKey(r => r.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<DoctorReview>()
            .HasOne(r => r.Appointment)
            .WithOne()
            .HasForeignKey<DoctorReview>(r => r.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<DoctorReview>()
            .HasIndex(r => new { r.AppointmentId, r.PatientId })
            .IsUnique();

        builder.Entity<HealthRecordShare>()
            .HasOne(s => s.HealthRecord)
            .WithMany(r => r.Shares)
            .HasForeignKey(s => s.HealthRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<HealthRecordShare>()
            .HasOne(s => s.Patient)
            .WithMany(p => p.SharedHealthRecords)
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HealthRecordShare>()
            .HasOne(s => s.Doctor)
            .WithMany(d => d.SharedRecords)
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<HealthRecordShare>()
            .HasIndex(s => new { s.HealthRecordId, s.DoctorId })
            .IsUnique();

        builder.Entity<ChatMessage>()
            .HasOne(m => m.Sender)
            .WithMany()
            .HasForeignKey(m => m.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ChatMessage>()
            .HasOne(m => m.Receiver)
            .WithMany()
            .HasForeignKey(m => m.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Doctor>().Property(d => d.ConsultationFee).HasPrecision(18, 2);
        builder.Entity<Doctor>().Property(d => d.DoctorName).HasMaxLength(160);
        builder.Entity<Patient>().Property(p => p.PatientName).HasMaxLength(160);
        builder.Entity<Appointment>().Property(a => a.PatientName).HasMaxLength(160);
        builder.Entity<Appointment>().Property(a => a.DoctorName).HasMaxLength(160);
        builder.Entity<ContactSubmission>().Property(c => c.Name).HasMaxLength(160);
        builder.Entity<ContactSubmission>().Property(c => c.Email).HasMaxLength(256);
        builder.Entity<ContactSubmission>().Property(c => c.Subject).HasMaxLength(180);
        builder.Entity<Invoice>().Property(i => i.ConsultationFee).HasPrecision(18, 2);
        builder.Entity<Invoice>().Property(i => i.InsuranceAmount).HasPrecision(18, 2);
        builder.Entity<Invoice>().Property(i => i.Discount).HasPrecision(18, 2);
        builder.Entity<Invoice>().Property(i => i.Total).HasPrecision(18, 2);
        builder.Entity<Invoice>().Property(i => i.DemoPayerName).HasMaxLength(80);
        builder.Entity<Invoice>().Property(i => i.DemoPaymentMethod).HasMaxLength(40);
        builder.Entity<Invoice>().Property(i => i.DemoPaymentReceipt).HasMaxLength(80);
    }
}
