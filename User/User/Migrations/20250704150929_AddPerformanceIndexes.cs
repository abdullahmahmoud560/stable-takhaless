using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.Migrations
{
    /// <inheritdoc />
    public partial class AddPerformanceIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_values_newOrderId",
                table: "values",
                newName: "IX_Values_NewOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_notesCustomerServices_newOrderId",
                table: "notesCustomerServices",
                newName: "IX_NotesCustomerService_NewOrderId");

            migrationBuilder.AlterColumn<string>(
                name: "statuOrder",
                table: "newOrders",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "newOrders",
                type: "varchar(255)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext");

            migrationBuilder.AlterColumn<string>(
                name: "Accept",
                table: "newOrders",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Values_Accept",
                table: "values",
                column: "Accept");

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Accept",
                table: "newOrders",
                column: "Accept");

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Accept_Status",
                table: "newOrders",
                columns: new[] { "Accept", "statuOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Date",
                table: "newOrders",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Status",
                table: "newOrders",
                column: "statuOrder");

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Status_Date",
                table: "newOrders",
                columns: new[] { "statuOrder", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_UserId",
                table: "newOrders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_UserId_Status",
                table: "newOrders",
                columns: new[] { "UserId", "statuOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Values_Accept",
                table: "values");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Accept",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Accept_Status",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Date",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Status",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Status_Date",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_UserId",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_UserId_Status",
                table: "newOrders");

            migrationBuilder.RenameIndex(
                name: "IX_Values_NewOrderId",
                table: "values",
                newName: "IX_values_newOrderId");

            migrationBuilder.RenameIndex(
                name: "IX_NotesCustomerService_NewOrderId",
                table: "notesCustomerServices",
                newName: "IX_notesCustomerServices_newOrderId");

            migrationBuilder.AlterColumn<string>(
                name: "statuOrder",
                table: "newOrders",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "newOrders",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)");

            migrationBuilder.AlterColumn<string>(
                name: "Accept",
                table: "newOrders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);
        }
    }
}
