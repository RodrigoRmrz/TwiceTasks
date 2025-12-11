using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwiceTasks.Migrations
{
    /// <inheritdoc />
    public partial class Collections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "FileResources",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "FileResources",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CollectionId",
                table: "FileResources",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Collections",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Collections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Collections_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FileResources_ApplicationUserId",
                table: "FileResources",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FileResources_CollectionId",
                table: "FileResources",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Collections_UserId",
                table: "Collections",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FileResources_AspNetUsers_ApplicationUserId",
                table: "FileResources",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_FileResources_Collections_CollectionId",
                table: "FileResources",
                column: "CollectionId",
                principalTable: "Collections",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FileResources_AspNetUsers_ApplicationUserId",
                table: "FileResources");

            migrationBuilder.DropForeignKey(
                name: "FK_FileResources_Collections_CollectionId",
                table: "FileResources");

            migrationBuilder.DropTable(
                name: "Collections");

            migrationBuilder.DropIndex(
                name: "IX_FileResources_ApplicationUserId",
                table: "FileResources");

            migrationBuilder.DropIndex(
                name: "IX_FileResources_CollectionId",
                table: "FileResources");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "FileResources");

            migrationBuilder.DropColumn(
                name: "CollectionId",
                table: "FileResources");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "FileResources",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255);
        }
    }
}
