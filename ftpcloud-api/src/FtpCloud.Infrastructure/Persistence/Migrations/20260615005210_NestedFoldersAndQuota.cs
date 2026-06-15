using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FtpCloud.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class NestedFoldersAndQuota : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "storage_quota_bytes",
                table: "users",
                type: "bigint",
                nullable: false,
                defaultValue: 21474836480L);

            migrationBuilder.AddColumn<Guid>(
                name: "parent_folder_id",
                table: "folders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "root_folder_id",
                table: "folders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.Sql("UPDATE folders SET root_folder_id = id;");

            migrationBuilder.CreateIndex(
                name: "ix_folders_parent_folder_id",
                table: "folders",
                column: "parent_folder_id");

            migrationBuilder.CreateIndex(
                name: "ix_folders_root_folder_id",
                table: "folders",
                column: "root_folder_id");

            migrationBuilder.AddForeignKey(
                name: "fk_folders_folders_parent_folder_id",
                table: "folders",
                column: "parent_folder_id",
                principalTable: "folders",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_folders_folders_parent_folder_id",
                table: "folders");

            migrationBuilder.DropIndex(
                name: "ix_folders_parent_folder_id",
                table: "folders");

            migrationBuilder.DropIndex(
                name: "ix_folders_root_folder_id",
                table: "folders");

            migrationBuilder.DropColumn(
                name: "storage_quota_bytes",
                table: "users");

            migrationBuilder.DropColumn(
                name: "parent_folder_id",
                table: "folders");

            migrationBuilder.DropColumn(
                name: "root_folder_id",
                table: "folders");
        }
    }
}
