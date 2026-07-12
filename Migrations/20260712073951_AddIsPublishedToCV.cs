using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace talentacquisition_jobplacement_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddIsPublishedToCV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateProfileAttributes_AttributeDefinitions_AttributeDefinitionId",
                table: "CandidateProfileAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_CandidateProfileAttributes_CandidateProfiles_CandidateProfileId",
                table: "CandidateProfileAttributes");

            migrationBuilder.DropIndex(
                name: "IX_AttributeDefinitions_Name",
                table: "AttributeDefinitions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CandidateProfileAttributes",
                table: "CandidateProfileAttributes");

            migrationBuilder.RenameTable(
                name: "CandidateProfileAttributes",
                newName: "CandidateProfileAttribute");

            migrationBuilder.RenameIndex(
                name: "IX_CandidateProfileAttributes_CandidateProfileId",
                table: "CandidateProfileAttribute",
                newName: "IX_CandidateProfileAttribute_CandidateProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_CandidateProfileAttributes_AttributeDefinitionId",
                table: "CandidateProfileAttribute",
                newName: "IX_CandidateProfileAttribute_AttributeDefinitionId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Positions",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<int>(
                name: "MaxProjects",
                table: "Positions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublished",
                table: "CVs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CandidateProfileAttribute",
                table: "CandidateProfileAttribute",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateProfileAttribute_AttributeDefinitions_AttributeDefinitionId",
                table: "CandidateProfileAttribute",
                column: "AttributeDefinitionId",
                principalTable: "AttributeDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateProfileAttribute_CandidateProfiles_CandidateProfileId",
                table: "CandidateProfileAttribute",
                column: "CandidateProfileId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateProfileAttribute_AttributeDefinitions_AttributeDefinitionId",
                table: "CandidateProfileAttribute");

            migrationBuilder.DropForeignKey(
                name: "FK_CandidateProfileAttribute_CandidateProfiles_CandidateProfileId",
                table: "CandidateProfileAttribute");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CandidateProfileAttribute",
                table: "CandidateProfileAttribute");

            migrationBuilder.DropColumn(
                name: "IsPublished",
                table: "CVs");

            migrationBuilder.RenameTable(
                name: "CandidateProfileAttribute",
                newName: "CandidateProfileAttributes");

            migrationBuilder.RenameIndex(
                name: "IX_CandidateProfileAttribute_CandidateProfileId",
                table: "CandidateProfileAttributes",
                newName: "IX_CandidateProfileAttributes_CandidateProfileId");

            migrationBuilder.RenameIndex(
                name: "IX_CandidateProfileAttribute_AttributeDefinitionId",
                table: "CandidateProfileAttributes",
                newName: "IX_CandidateProfileAttributes_AttributeDefinitionId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Positions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "MaxProjects",
                table: "Positions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CandidateProfileAttributes",
                table: "CandidateProfileAttributes",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_AttributeDefinitions_Name",
                table: "AttributeDefinitions",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateProfileAttributes_AttributeDefinitions_AttributeDefinitionId",
                table: "CandidateProfileAttributes",
                column: "AttributeDefinitionId",
                principalTable: "AttributeDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateProfileAttributes_CandidateProfiles_CandidateProfileId",
                table: "CandidateProfileAttributes",
                column: "CandidateProfileId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
