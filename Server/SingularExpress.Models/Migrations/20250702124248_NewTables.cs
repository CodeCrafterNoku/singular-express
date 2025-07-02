using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SingularExpress.Models.Migrations
{
    /// <inheritdoc />
    public partial class NewTables : Migration
    {
        /// <inheritdoc />
      protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.CreateTable(
        name: "Users",
        columns: table => new
        {
            UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
            FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
            CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
            ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
            FailedLoginAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
            LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_Users", x => x.UserId);
        });

    migrationBuilder.CreateTable(
        name: "PasswordResetTokens",
        columns: table => new
        {
            Id = table.Column<int>(type: "int", nullable: false)
                .Annotation("SqlServer:Identity", "1, 1"),
            Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
            Otp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
            ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_PasswordResetTokens", x => x.Id);
        });
}


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropTable(name: "PasswordResetTokens");
    migrationBuilder.DropTable(name: "Users");
}

    }
}
