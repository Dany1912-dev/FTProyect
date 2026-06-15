using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FtpCloud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTrashSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "folders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "files",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_folders_deleted_at",
                table: "folders",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "ix_files_deleted_at",
                table: "files",
                column: "deleted_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_folders_deleted_at",
                table: "folders");

            migrationBuilder.DropIndex(
                name: "ix_files_deleted_at",
                table: "files");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "folders");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "files");
        }
    }
}
