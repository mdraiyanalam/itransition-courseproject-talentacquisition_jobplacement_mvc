using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace talentacquisition_jobplacement_mvc.Migrations
{
    /// <inheritdoc />
    public partial class FixCVAttributeBinding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserAchievement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AwardedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CandidateProfileId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievement_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievement_CandidateProfiles_CandidateProfileId",
                        column: x => x.CandidateProfileId,
                        principalTable: "CandidateProfiles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievement_CandidateProfileId",
                table: "UserAchievement",
                column: "CandidateProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievement_UserId",
                table: "UserAchievement",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserAchievement");
        }
    }
}
