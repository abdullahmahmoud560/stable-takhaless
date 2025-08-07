using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace firstProject.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleSaber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[] { "a0669889-c0ff-4a17-aaa5-eda8396b1504", "e42f786f-6f58-491b-9404-c69a253974f3", "Saber", "SABER" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "a0669889-c0ff-4a17-aaa5-eda8396b1504");
        }
    }
}
