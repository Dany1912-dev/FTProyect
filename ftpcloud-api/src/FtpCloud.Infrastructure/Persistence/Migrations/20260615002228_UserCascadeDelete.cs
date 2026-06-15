using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FtpCloud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UserCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_files_users_uploaded_by_id",
                table: "files");

            migrationBuilder.DropForeignKey(
                name: "fk_folders_users_owner_id",
                table: "folders");

            migrationBuilder.AlterColumn<Guid>(
                name: "uploaded_by_id",
                table: "files",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "fk_files_users_uploaded_by_id",
                table: "files",
                column: "uploaded_by_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_folders_users_owner_id",
                table: "folders",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_files_users_uploaded_by_id",
                table: "files");

            migrationBuilder.DropForeignKey(
                name: "fk_folders_users_owner_id",
                table: "folders");

            migrationBuilder.AlterColumn<Guid>(
                name: "uploaded_by_id",
                table: "files",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "fk_files_users_uploaded_by_id",
                table: "files",
                column: "uploaded_by_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_folders_users_owner_id",
                table: "folders",
                column: "owner_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
