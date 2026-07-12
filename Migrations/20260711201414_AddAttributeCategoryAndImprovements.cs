using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace talentacquisition_jobplacement_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddAttributeCategoryAndImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AttributeDefinitions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "AttributeDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "AttributeDefinitions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_Name",
                table: "AttributeDefinitions",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AttributeDefinitions_Name",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "AttributeDefinitions");

            migrationBuilder.DropColumn(
                name: "Icon",
                table: "AttributeDefinitions");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AttributeDefinitions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
