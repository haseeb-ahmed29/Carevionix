using System;
using Carevionix.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Carevionix.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260522043000_ProfileAdminContactExpansion")]
    public partial class ProfileAdminContactExpansion : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfileImagePath",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Doctors",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Qualifications",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "Doctors",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                table: "Patients",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmergencyContact",
                table: "Patients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientName",
                table: "Appointments",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Appointments",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ContactSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactSubmissions", x => x.Id);
                });

            migrationBuilder.Sql("""
                UPDATE d
                SET DoctorName = u.FullName
                FROM Doctors d
                INNER JOIN AspNetUsers u ON d.UserId = u.Id
                WHERE d.DoctorName = '';

                UPDATE p
                SET PatientName = u.FullName
                FROM Patients p
                INNER JOIN AspNetUsers u ON p.UserId = u.Id
                WHERE p.PatientName = '';

                UPDATE a
                SET
                    PatientName = COALESCE(NULLIF(p.PatientName, ''), pu.FullName, ''),
                    DoctorName = COALESCE(NULLIF(d.DoctorName, ''), du.FullName, '')
                FROM Appointments a
                INNER JOIN Patients p ON a.PatientId = p.Id
                INNER JOIN Doctors d ON a.DoctorId = d.Id
                INNER JOIN AspNetUsers pu ON p.UserId = pu.Id
                INNER JOIN AspNetUsers du ON d.UserId = du.Id
                WHERE a.PatientName = '' OR a.DoctorName = '';
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ContactSubmissions");
            migrationBuilder.DropColumn(name: "Address", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "ProfileImagePath", table: "AspNetUsers");
            migrationBuilder.DropColumn(name: "DoctorName", table: "Doctors");
            migrationBuilder.DropColumn(name: "Qualifications", table: "Doctors");
            migrationBuilder.DropColumn(name: "Bio", table: "Doctors");
            migrationBuilder.DropColumn(name: "PatientName", table: "Patients");
            migrationBuilder.DropColumn(name: "EmergencyContact", table: "Patients");
            migrationBuilder.DropColumn(name: "PatientName", table: "Appointments");
            migrationBuilder.DropColumn(name: "DoctorName", table: "Appointments");
        }
    }
}
