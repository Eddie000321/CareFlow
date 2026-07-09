using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AlignRuntimeModelForRetention : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicalNotes_Pets_PetId",
                table: "ClinicalNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_LabReports_Pets_PetId",
                table: "LabReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Owners_OwnerId",
                table: "Pets");

            migrationBuilder.DropIndex(
                name: "IX_LabResults_LabReportId",
                table: "LabResults");

            migrationBuilder.DropIndex(
                name: "IX_LabReports_PetId",
                table: "LabReports");

            migrationBuilder.DropIndex(
                name: "IX_ClinicalNotes_PetId",
                table: "ClinicalNotes");

            migrationBuilder.Sql(
                """
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM "LabResults" WHERE char_length("Units") > 32) THEN
                        RAISE EXCEPTION 'CareFlow migration blocked: LabResults.Units contains values longer than 32 characters.';
                    END IF;

                    IF EXISTS (SELECT 1 FROM "LabResults" WHERE char_length("Flag") > 8) THEN
                        RAISE EXCEPTION 'CareFlow migration blocked: LabResults.Flag contains values longer than 8 characters.';
                    END IF;

                    IF EXISTS (SELECT 1 FROM "LabReports" WHERE char_length("Status") > 32) THEN
                        RAISE EXCEPTION 'CareFlow migration blocked: LabReports.Status contains values longer than 32 characters.';
                    END IF;

                    IF EXISTS (SELECT 1 FROM "LabReports" WHERE char_length("LabName") > 128) THEN
                        RAISE EXCEPTION 'CareFlow migration blocked: LabReports.LabName contains values longer than 128 characters.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Units",
                table: "LabResults",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Flag",
                table: "LabResults",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LabReports",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LabName",
                table: "LabReports",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_LabReportId_AnalyteName",
                table: "LabResults",
                columns: new[] { "LabReportId", "AnalyteName" });

            migrationBuilder.CreateIndex(
                name: "IX_LabReports_PetId_ReportedAt",
                table: "LabReports",
                columns: new[] { "PetId", "ReportedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_PetId_RecordedAt",
                table: "ClinicalNotes",
                columns: new[] { "PetId", "RecordedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicalNotes_Pets_PetId",
                table: "ClinicalNotes",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabReports_Pets_PetId",
                table: "LabReports",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Owners_OwnerId",
                table: "Pets",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A downgrade deliberately restores the previous cascade-delete contract.
            // Review clinical-history retention impact before running this path.
            migrationBuilder.DropForeignKey(
                name: "FK_ClinicalNotes_Pets_PetId",
                table: "ClinicalNotes");

            migrationBuilder.DropForeignKey(
                name: "FK_LabReports_Pets_PetId",
                table: "LabReports");

            migrationBuilder.DropForeignKey(
                name: "FK_Pets_Owners_OwnerId",
                table: "Pets");

            migrationBuilder.DropIndex(
                name: "IX_LabResults_LabReportId_AnalyteName",
                table: "LabResults");

            migrationBuilder.DropIndex(
                name: "IX_LabReports_PetId_ReportedAt",
                table: "LabReports");

            migrationBuilder.DropIndex(
                name: "IX_ClinicalNotes_PetId_RecordedAt",
                table: "ClinicalNotes");

            migrationBuilder.AlterColumn<string>(
                name: "Units",
                table: "LabResults",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Flag",
                table: "LabResults",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(8)",
                oldMaxLength: 8,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "LabReports",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "LabName",
                table: "LabReports",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_LabReportId",
                table: "LabResults",
                column: "LabReportId");

            migrationBuilder.CreateIndex(
                name: "IX_LabReports_PetId",
                table: "LabReports",
                column: "PetId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_PetId",
                table: "ClinicalNotes",
                column: "PetId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClinicalNotes_Pets_PetId",
                table: "ClinicalNotes",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LabReports_Pets_PetId",
                table: "LabReports",
                column: "PetId",
                principalTable: "Pets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pets_Owners_OwnerId",
                table: "Pets",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
