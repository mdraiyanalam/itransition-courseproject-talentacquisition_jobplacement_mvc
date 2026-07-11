using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace talentacquisition_jobplacement_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddAllowedRolesToPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedRoles",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedRoles",
                table: "Positions");
        }
    }
}
