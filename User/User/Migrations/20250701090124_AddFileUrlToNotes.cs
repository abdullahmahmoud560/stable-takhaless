using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.Migrations
{
    /// <inheritdoc />
    public partial class AddFileUrlToNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // حذف هذا السطر لأنه يسبب خطأ عند التنفيذ
            // migrationBuilder.DropColumn(
            //     name: "fileData",
            //     table: "notesCustomerServices");

            migrationBuilder.AddColumn<string>(
                name: "fileUrl",
                table: "notesCustomerServices",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fileUrl",
                table: "notesCustomerServices");

            // هذا السطر يعيد العمود fileData لو رجعت في Migration
            migrationBuilder.AddColumn<byte[]>(
                name: "fileData",
                table: "notesCustomerServices",
                type: "longblob",
                nullable: true);
        }
    }
}
