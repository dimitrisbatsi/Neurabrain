using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Neurabrain.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHybridMultiTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SourceMaterials_Users_UploadedByUserId",
                table: "SourceMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_Users_UserId",
                table: "StudentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "Condition",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "TargetCondition",
                table: "GeneratedExercises");

            migrationBuilder.RenameColumn(
                name: "UploadedByUserId",
                table: "SourceMaterials",
                newName: "OwnerUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SourceMaterials_UploadedByUserId",
                table: "SourceMaterials",
                newName: "IX_SourceMaterials_OwnerUserId");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<Guid>(
                name: "LearningConditionId",
                table: "StudentProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "StudentProfiles",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SourceMaterials",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<Guid>(
                name: "LearningConditionId",
                table: "GeneratedExercises",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "LearningConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    AiPromptInstruction = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LearningConditions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TaxId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MaxStudents = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserOrganizations",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserOrganizations", x => new { x.UserId, x.OrganizationId });
                    table.ForeignKey(
                        name: "FK_UserOrganizations_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserOrganizations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_LearningConditionId",
                table: "StudentProfiles",
                column: "LearningConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_OrganizationId",
                table: "StudentProfiles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_GeneratedExercises_LearningConditionId",
                table: "GeneratedExercises",
                column: "LearningConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserOrganizations_OrganizationId",
                table: "UserOrganizations",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneratedExercises_LearningConditions_LearningConditionId",
                table: "GeneratedExercises",
                column: "LearningConditionId",
                principalTable: "LearningConditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SourceMaterials_Users_OwnerUserId",
                table: "SourceMaterials",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_LearningConditions_LearningConditionId",
                table: "StudentProfiles",
                column: "LearningConditionId",
                principalTable: "LearningConditions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_Organizations_OrganizationId",
                table: "StudentProfiles",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_Users_UserId",
                table: "StudentProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneratedExercises_LearningConditions_LearningConditionId",
                table: "GeneratedExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_SourceMaterials_Users_OwnerUserId",
                table: "SourceMaterials");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_LearningConditions_LearningConditionId",
                table: "StudentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_Organizations_OrganizationId",
                table: "StudentProfiles");

            migrationBuilder.DropForeignKey(
                name: "FK_StudentProfiles_Users_UserId",
                table: "StudentProfiles");

            migrationBuilder.DropTable(
                name: "LearningConditions");

            migrationBuilder.DropTable(
                name: "UserOrganizations");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_LearningConditionId",
                table: "StudentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_OrganizationId",
                table: "StudentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles");

            migrationBuilder.DropIndex(
                name: "IX_GeneratedExercises_LearningConditionId",
                table: "GeneratedExercises");

            migrationBuilder.DropColumn(
                name: "LearningConditionId",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "LearningConditionId",
                table: "GeneratedExercises");

            migrationBuilder.RenameColumn(
                name: "OwnerUserId",
                table: "SourceMaterials",
                newName: "UploadedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_SourceMaterials_OwnerUserId",
                table: "SourceMaterials",
                newName: "IX_SourceMaterials_UploadedByUserId");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Users",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Users",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "StudentProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "SourceMaterials",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AddColumn<int>(
                name: "TargetCondition",
                table: "GeneratedExercises",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles",
                column: "UserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_SourceMaterials_Users_UploadedByUserId",
                table: "SourceMaterials",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StudentProfiles_Users_UserId",
                table: "StudentProfiles",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
