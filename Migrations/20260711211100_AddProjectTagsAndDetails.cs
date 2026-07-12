using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace talentacquisition_jobplacement_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectTagsAndDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Positions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "AccessRules",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Company",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxProjects",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProjectTags",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessRules",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "Company",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "MaxProjects",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "ProjectTags",
                table: "Positions");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
