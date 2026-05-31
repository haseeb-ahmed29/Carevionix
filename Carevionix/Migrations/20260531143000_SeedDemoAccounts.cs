using Carevionix.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carevionix.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260531143000_SeedDemoAccounts")]
    public partial class SeedDemoAccounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DECLARE @now datetime2 = '2026-05-31T09:30:00';

                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'ADMIN')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('role-admin', 'Admin', 'ADMIN', 'seed-role-admin');
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'DOCTOR')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('role-doctor', 'Doctor', 'DOCTOR', 'seed-role-doctor');
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE NormalizedName = 'PATIENT')
                BEGIN
                    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
                    VALUES ('role-patient', 'Patient', 'PATIENT', 'seed-role-patient');
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'ADMIN@CAREVIONIX.LOCAL')
                BEGIN
                    INSERT INTO AspNetUsers
                    (
                        Id, FullName, ProfileImagePath, Address, IsActive, CreatedAt, UserName, NormalizedUserName,
                        Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
                    )
                    VALUES
                    (
                        'seed-admin-user', 'Carevionix Admin', NULL, NULL, 1, @now, 'admin@carevionix.local', 'ADMIN@CAREVIONIX.LOCAL',
                        'admin@carevionix.local', 'ADMIN@CAREVIONIX.LOCAL', 1,
                        'AQAAAAIAAYagAAAAEL/YeG7bauwfv+ikOVSbMNTO9CtU4O+axZEW9E1Q5AI6el4/avNbw7tfErzKCFNh6g==',
                        'seed-security-admin', 'seed-concurrency-admin', NULL, 0, 0, NULL, 1, 0
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'HASEEBAHMED03155@GMAIL.COM')
                BEGIN
                    INSERT INTO AspNetUsers
                    (
                        Id, FullName, ProfileImagePath, Address, IsActive, CreatedAt, UserName, NormalizedUserName,
                        Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
                    )
                    VALUES
                    (
                        'seed-doctor-haseeb', 'Haseeb Ahmed', NULL, 'Karachi, Pakistan', 1, @now, 'haseebahmed03155@gmail.com', 'HASEEBAHMED03155@GMAIL.COM',
                        'haseebahmed03155@gmail.com', 'HASEEBAHMED03155@GMAIL.COM', 1,
                        'AQAAAAIAAYagAAAAEL+OT+QV9U4xZMT/zSSqekpaKWt63El8wkpXXyVqeRjX9jgOmlM7v+KnggE7BA4YeQ==',
                        'seed-security-haseeb', 'seed-concurrency-haseeb', NULL, 0, 0, NULL, 1, 0
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'RAFAY@GMAIL.COM')
                BEGIN
                    INSERT INTO AspNetUsers
                    (
                        Id, FullName, ProfileImagePath, Address, IsActive, CreatedAt, UserName, NormalizedUserName,
                        Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
                    )
                    VALUES
                    (
                        'seed-doctor-rafay', 'Rafay', NULL, 'Karachi, Pakistan', 1, @now, 'rafay@gmail.com', 'RAFAY@GMAIL.COM',
                        'rafay@gmail.com', 'RAFAY@GMAIL.COM', 1,
                        'AQAAAAIAAYagAAAAEIDqh66k3dffPMhe0sRHSAOKggU1xJ2E7nIy8eB3SnJZUP/OllrEORZ84tKqopV9tw==',
                        'seed-security-rafay', 'seed-concurrency-rafay', NULL, 0, 0, NULL, 1, 0
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'TUBA@GMAIL.COM')
                BEGIN
                    INSERT INTO AspNetUsers
                    (
                        Id, FullName, ProfileImagePath, Address, IsActive, CreatedAt, UserName, NormalizedUserName,
                        Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
                    )
                    VALUES
                    (
                        'seed-doctor-tuba', 'Tuba', NULL, 'Karachi, Pakistan', 1, @now, 'tuba@gmail.com', 'TUBA@GMAIL.COM',
                        'tuba@gmail.com', 'TUBA@GMAIL.COM', 1,
                        'AQAAAAIAAYagAAAAEFFl8eHfUUTHc9HPoKSTClmnodeAKIvC+OmnYaVWlDFujnkyefSv6bYpeXVC6zd/sA==',
                        'seed-security-tuba', 'seed-concurrency-tuba', NULL, 0, 0, NULL, 1, 0
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'SAAD@GMAIL.COM')
                BEGIN
                    INSERT INTO AspNetUsers
                    (
                        Id, FullName, ProfileImagePath, Address, IsActive, CreatedAt, UserName, NormalizedUserName,
                        Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
                    )
                    VALUES
                    (
                        'seed-patient-saad', 'Saad', NULL, 'Karachi, Pakistan', 1, @now, 'saad@gmail.com', 'SAAD@GMAIL.COM',
                        'saad@gmail.com', 'SAAD@GMAIL.COM', 1,
                        'AQAAAAIAAYagAAAAEGkqNzFkVkwRjGW0KqoxNqxFE95wPH0fXZ8GHDJVL0K6qlus7+NjCQTZX0psIdQOoA==',
                        'seed-security-saad', 'seed-concurrency-saad', NULL, 0, 0, NULL, 1, 0
                    );
                END

                IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE NormalizedEmail = 'HASHIM@GMAIL.COM')
                BEGIN
                    INSERT INTO AspNetUsers
                    (
                        Id, FullName, ProfileImagePath, Address, IsActive, CreatedAt, UserName, NormalizedUserName,
                        Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp,
                        PhoneNumber, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnd, LockoutEnabled, AccessFailedCount
                    )
                    VALUES
                    (
                        'seed-patient-hashim', 'Hashim', NULL, 'Karachi, Pakistan', 1, @now, 'hashim@gmail.com', 'HASHIM@GMAIL.COM',
                        'hashim@gmail.com', 'HASHIM@GMAIL.COM', 1,
                        'AQAAAAIAAYagAAAAED2HRpJsGkjqGW0HsJUxs+XPSJrUKwF0WW+2Sm2vqqDtHoFuYZepGPBQONgv+NZTjA==',
                        'seed-security-hashim', 'seed-concurrency-hashim', NULL, 0, 0, NULL, 1, 0
                    );
                END

                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT u.Id, r.Id
                FROM AspNetUsers u
                CROSS JOIN AspNetRoles r
                WHERE u.NormalizedEmail = 'ADMIN@CAREVIONIX.LOCAL'
                    AND r.NormalizedName = 'ADMIN'
                    AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = r.Id);

                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT u.Id, r.Id
                FROM AspNetUsers u
                CROSS JOIN AspNetRoles r
                WHERE u.NormalizedEmail IN ('HASEEBAHMED03155@GMAIL.COM', 'RAFAY@GMAIL.COM', 'TUBA@GMAIL.COM')
                    AND r.NormalizedName = 'DOCTOR'
                    AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = r.Id);

                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT u.Id, r.Id
                FROM AspNetUsers u
                CROSS JOIN AspNetRoles r
                WHERE u.NormalizedEmail IN ('SAAD@GMAIL.COM', 'HASHIM@GMAIL.COM')
                    AND r.NormalizedName = 'PATIENT'
                    AND NOT EXISTS (SELECT 1 FROM AspNetUserRoles ur WHERE ur.UserId = u.Id AND ur.RoleId = r.Id);

                INSERT INTO Doctors (UserId, DoctorName, Specialty, Location, Languages, Qualifications, Bio, ExperienceYears, ConsultationFee, IsActive, CreatedAt)
                SELECT u.Id, u.FullName, 'General Physician', 'Karachi, Pakistan', 'English, Urdu', 'MBBS', 'General consultation and routine care.', 5, 1500, 1, @now
                FROM AspNetUsers u
                WHERE u.NormalizedEmail = 'HASEEBAHMED03155@GMAIL.COM'
                    AND NOT EXISTS (SELECT 1 FROM Doctors d WHERE d.UserId = u.Id);

                INSERT INTO Doctors (UserId, DoctorName, Specialty, Location, Languages, Qualifications, Bio, ExperienceYears, ConsultationFee, IsActive, CreatedAt)
                SELECT u.Id, u.FullName, 'Cardiologist', 'Karachi, Pakistan', 'English, Urdu', 'MBBS, FCPS Cardiology', 'Heart care and cardiovascular consultation.', 6, 2000, 1, @now
                FROM AspNetUsers u
                WHERE u.NormalizedEmail = 'RAFAY@GMAIL.COM'
                    AND NOT EXISTS (SELECT 1 FROM Doctors d WHERE d.UserId = u.Id);

                INSERT INTO Doctors (UserId, DoctorName, Specialty, Location, Languages, Qualifications, Bio, ExperienceYears, ConsultationFee, IsActive, CreatedAt)
                SELECT u.Id, u.FullName, 'Dermatologist', 'Karachi, Pakistan', 'English, Urdu', 'MBBS, Diploma Dermatology', 'Skin care and dermatology consultation.', 4, 1800, 1, @now
                FROM AspNetUsers u
                WHERE u.NormalizedEmail = 'TUBA@GMAIL.COM'
                    AND NOT EXISTS (SELECT 1 FROM Doctors d WHERE d.UserId = u.Id);

                INSERT INTO Patients (UserId, PatientName, DateOfBirth, Gender, Address, InsuranceInfo, MedicalHistory, EmergencyContact, CreatedAt)
                SELECT u.Id, u.FullName, '2000-01-15', 'Male', 'Karachi, Pakistan', 'Self Pay', 'No major medical history recorded.', '0300-0000000', @now
                FROM AspNetUsers u
                WHERE u.NormalizedEmail = 'SAAD@GMAIL.COM'
                    AND NOT EXISTS (SELECT 1 FROM Patients p WHERE p.UserId = u.Id);

                INSERT INTO Patients (UserId, PatientName, DateOfBirth, Gender, Address, InsuranceInfo, MedicalHistory, EmergencyContact, CreatedAt)
                SELECT u.Id, u.FullName, '1999-08-20', 'Male', 'Karachi, Pakistan', 'Self Pay', 'No major medical history recorded.', '0300-0000000', @now
                FROM AspNetUsers u
                WHERE u.NormalizedEmail = 'HASHIM@GMAIL.COM'
                    AND NOT EXISTS (SELECT 1 FROM Patients p WHERE p.UserId = u.Id);

                INSERT INTO DoctorAvailabilities (DoctorId, DayOfWeek, StartTime, EndTime, IsAvailable)
                SELECT d.Id, slots.DayOfWeek, slots.StartTime, slots.EndTime, 1
                FROM Doctors d
                INNER JOIN AspNetUsers u ON u.Id = d.UserId
                CROSS APPLY (VALUES
                    (1, CAST('09:00:00' AS time), CAST('13:00:00' AS time)),
                    (2, CAST('09:00:00' AS time), CAST('13:00:00' AS time)),
                    (3, CAST('14:00:00' AS time), CAST('18:00:00' AS time)),
                    (4, CAST('14:00:00' AS time), CAST('18:00:00' AS time)),
                    (5, CAST('10:00:00' AS time), CAST('14:00:00' AS time))
                ) slots (DayOfWeek, StartTime, EndTime)
                WHERE u.NormalizedEmail IN ('HASEEBAHMED03155@GMAIL.COM', 'RAFAY@GMAIL.COM', 'TUBA@GMAIL.COM')
                    AND NOT EXISTS (
                        SELECT 1
                        FROM DoctorAvailabilities da
                        WHERE da.DoctorId = d.Id
                            AND da.DayOfWeek = slots.DayOfWeek
                            AND da.StartTime = slots.StartTime
                            AND da.EndTime = slots.EndTime
                    );
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE da
                FROM DoctorAvailabilities da
                INNER JOIN Doctors d ON d.Id = da.DoctorId
                INNER JOIN AspNetUsers u ON u.Id = d.UserId
                WHERE u.NormalizedEmail IN ('HASEEBAHMED03155@GMAIL.COM', 'RAFAY@GMAIL.COM', 'TUBA@GMAIL.COM');

                DELETE FROM Patients
                WHERE UserId IN (
                    SELECT Id FROM AspNetUsers WHERE NormalizedEmail IN ('SAAD@GMAIL.COM', 'HASHIM@GMAIL.COM')
                );

                DELETE FROM Doctors
                WHERE UserId IN (
                    SELECT Id FROM AspNetUsers WHERE NormalizedEmail IN ('HASEEBAHMED03155@GMAIL.COM', 'RAFAY@GMAIL.COM', 'TUBA@GMAIL.COM')
                );

                DELETE ur
                FROM AspNetUserRoles ur
                INNER JOIN AspNetUsers u ON u.Id = ur.UserId
                WHERE u.NormalizedEmail IN (
                    'ADMIN@CAREVIONIX.LOCAL',
                    'HASEEBAHMED03155@GMAIL.COM',
                    'RAFAY@GMAIL.COM',
                    'TUBA@GMAIL.COM',
                    'SAAD@GMAIL.COM',
                    'HASHIM@GMAIL.COM'
                );

                DELETE FROM AspNetUsers
                WHERE NormalizedEmail IN (
                    'ADMIN@CAREVIONIX.LOCAL',
                    'HASEEBAHMED03155@GMAIL.COM',
                    'RAFAY@GMAIL.COM',
                    'TUBA@GMAIL.COM',
                    'SAAD@GMAIL.COM',
                    'HASHIM@GMAIL.COM'
                )
                AND Id LIKE 'seed-%';

                DELETE FROM AspNetRoles
                WHERE Id IN ('role-admin', 'role-doctor', 'role-patient');
                """);
        }
    }
}
