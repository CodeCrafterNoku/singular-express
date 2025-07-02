using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SingularExpress.Models.Migrations
{
    public partial class AddPasswordResetToken : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PasswordResetTokens",
                columns: table => new
                {
                    Email = table.Column<string>(nullable: false),
                    Otp = table.Column<string>(nullable: false),
                    ExpiresAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordResetTokens", x => x.Email);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "PasswordResetTokens");
        }
    }
}