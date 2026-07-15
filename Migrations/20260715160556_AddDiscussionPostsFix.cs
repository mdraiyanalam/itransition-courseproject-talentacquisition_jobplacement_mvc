using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace talentacquisition_jobplacement_mvc.Migrations
{
    /// <inheritdoc />
    public partial class AddDiscussionPostsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddPrimaryKey(
                name: "PK_CandidateProfileAttributes",
                table: "CandidateProfileAttributes",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "DiscussionPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PositionId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscussionPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DiscussionPosts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DiscussionPosts_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionPosts_PositionId",
                table: "DiscussionPosts",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscussionPosts_UserId",
                table: "DiscussionPosts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateProfileAttributes_AttributeDefinitions_AttributeDefinitionId",
                table: "CandidateProfileAttributes",
                column: "AttributeDefinitionId",
                principalTable: "AttributeDefinitions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CandidateProfileAttributes_CandidateProfiles_CandidateProfileId",
                table: "CandidateProfileAttributes",
                column: "CandidateProfileId",
                principalTable: "CandidateProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CandidateProfileAttributes_AttributeDefinitions_AttributeDefinitionId",
                table: "CandidateProfileAttributes");

            migrationBuilder.DropForeignKey(
                name: "FK_CandidateProfileAttributes_CandidateProfiles_CandidateProfileId",
                table: "CandidateProfileAttributes");

            migrationBuilder.DropTable(
                name: "DiscussionPosts");

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
    }
}
