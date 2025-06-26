using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.Migrations
{
    /// <inheritdoc />
    public partial class updateFileupload : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fileData",
                table: "uploadFiles");

            migrationBuilder.AddColumn<string>(
                name: "fileUrl",
                table: "uploadFiles",
                type: "longtext",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fileUrl",
                table: "uploadFiles");

            migrationBuilder.AddColumn<byte[]>(
                name: "fileData",
                table: "uploadFiles",
                type: "longblob",
                nullable: false);
        }
    }
}
