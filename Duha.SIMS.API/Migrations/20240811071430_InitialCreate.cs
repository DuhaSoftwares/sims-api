using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Duha.SIMS.API.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ApplicationUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleType = table.Column<int>(type: "int", nullable: false),
                    LoginId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmailId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    IsPhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    LoginStatus = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUserAddress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUserAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationUserAddress_ApplicationUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "ApplicationUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientCompany",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CompanyMobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyWebsite = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyLogoPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyDateOfEstablishment = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClientCompanyAddressId = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCompany", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientCompanyAddress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    State = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    City = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address1 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Address2 = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ClientCompanyDetailId = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientCompanyAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientCompanyAddress_ClientCompany_ClientCompanyDetailId",
                        column: x => x.ClientCompanyDetailId,
                        principalTable: "ClientCompany",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    PersonalEmailId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClientCompanyDetailId = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoleType = table.Column<int>(type: "int", nullable: false),
                    LoginId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmailId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsEmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    IsPhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    LoginStatus = table.Column<int>(type: "int", nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientUsers_ClientCompany_ClientCompanyDetailId",
                        column: x => x.ClientCompanyDetailId,
                        principalTable: "ClientCompany",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ClientUserAddress",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClientUserId = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModifiedOnUTC = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientUserAddress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientUserAddress_ClientUsers_ClientUserId",
                        column: x => x.ClientUserId,
                        principalTable: "ClientUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "ClientCompany",
                columns: new[] { "Id", "ClientCompanyAddressId", "CompanyCode", "CompanyDateOfEstablishment", "CompanyLogoPath", "CompanyMobileNumber", "CompanyWebsite", "ContactEmail", "CreatedBy", "CreatedOnUTC", "Description", "LastModifiedBy", "LastModifiedOnUTC", "Name" },
                values: new object[] { 1, null, "123", new DateTime(1990, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "wwwroot/content/companies/logos/company.jpg", "9876542341", "www.duhasoftwares.com", "DuhaSoftwares@outlook.com", "SeedAdmin", new DateTime(2024, 8, 11, 7, 14, 29, 985, DateTimeKind.Utc).AddTicks(8991), "Software Development Company", null, null, "Duha-Softwares" });

            migrationBuilder.InsertData(
                table: "ClientCompany",
                columns: new[] { "Id", "ClientCompanyAddressId", "CompanyCode", "CompanyDateOfEstablishment", "CompanyLogoPath", "CompanyMobileNumber", "CompanyWebsite", "ContactEmail", "CreatedBy", "CreatedOnUTC", "Description", "LastModifiedBy", "LastModifiedOnUTC", "Name" },
                values: new object[] { 2, null, "124", new DateTime(2009, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "wwwroot/content/companies/logos/company.jpg", "1234567890", "www.wmart.com", "weddingmart@gmail.com", "SeedAdmin", new DateTime(2024, 8, 11, 7, 14, 29, 985, DateTimeKind.Utc).AddTicks(8993), "Software Development Company", null, null, "Wedding-Mart" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUserAddress_ApplicationUserId",
                table: "ApplicationUserAddress",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_EmailId",
                table: "ApplicationUsers",
                column: "EmailId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUsers_LoginId",
                table: "ApplicationUsers",
                column: "LoginId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompany_ClientCompanyAddressId",
                table: "ClientCompany",
                column: "ClientCompanyAddressId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompany_CompanyCode",
                table: "ClientCompany",
                column: "CompanyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientCompanyAddress_ClientCompanyDetailId",
                table: "ClientCompanyAddress",
                column: "ClientCompanyDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientUserAddress_ClientUserId",
                table: "ClientUserAddress",
                column: "ClientUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientUsers_ClientCompanyDetailId",
                table: "ClientUsers",
                column: "ClientCompanyDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientUsers_EmailId",
                table: "ClientUsers",
                column: "EmailId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientUsers_LoginId",
                table: "ClientUsers",
                column: "LoginId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientCompany_ClientCompanyAddress_ClientCompanyAddressId",
                table: "ClientCompany",
                column: "ClientCompanyAddressId",
                principalTable: "ClientCompanyAddress",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientCompany_ClientCompanyAddress_ClientCompanyAddressId",
                table: "ClientCompany");

            migrationBuilder.DropTable(
                name: "ApplicationUserAddress");

            migrationBuilder.DropTable(
                name: "ClientUserAddress");

            migrationBuilder.DropTable(
                name: "ApplicationUsers");

            migrationBuilder.DropTable(
                name: "ClientUsers");

            migrationBuilder.DropTable(
                name: "ClientCompanyAddress");

            migrationBuilder.DropTable(
                name: "ClientCompany");
        }
    }
}
