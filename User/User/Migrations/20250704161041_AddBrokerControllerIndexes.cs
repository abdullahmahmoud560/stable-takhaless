using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace User.Migrations
{
    /// <inheritdoc />
    public partial class AddBrokerControllerIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_NewOrders_Status_Date",
                table: "newOrders",
                newName: "IX_NewOrders_Status_Date_Extended");

            migrationBuilder.AlterColumn<string>(
                name: "BrokerID",
                table: "values",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "step3",
                table: "newOrders",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "step2",
                table: "newOrders",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "step1",
                table: "newOrders",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AcceptCustomerService",
                table: "newOrders",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Values_BrokerID",
                table: "values",
                column: "BrokerID");

            migrationBuilder.CreateIndex(
                name: "IX_Values_BrokerID_NewOrderId",
                table: "values",
                columns: new[] { "BrokerID", "newOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_Values_NewOrderId_Accept",
                table: "values",
                columns: new[] { "newOrderId", "Accept" });

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Accept_Status_Date",
                table: "newOrders",
                columns: new[] { "Accept", "statuOrder", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_CustomerService_Accept",
                table: "newOrders",
                columns: new[] { "AcceptCustomerService", "Accept" });

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Status_Accept",
                table: "newOrders",
                columns: new[] { "statuOrder", "Accept" });

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Status_Date_Accept_Composite",
                table: "newOrders",
                columns: new[] { "statuOrder", "Date", "Accept" });

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Status_Steps",
                table: "newOrders",
                columns: new[] { "statuOrder", "step1", "step2", "step3" });

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_Status_UserId",
                table: "newOrders",
                columns: new[] { "statuOrder", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_NewOrders_UserId_Status_Date",
                table: "newOrders",
                columns: new[] { "UserId", "statuOrder", "Date" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Values_BrokerID",
                table: "values");

            migrationBuilder.DropIndex(
                name: "IX_Values_BrokerID_NewOrderId",
                table: "values");

            migrationBuilder.DropIndex(
                name: "IX_Values_NewOrderId_Accept",
                table: "values");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Accept_Status_Date",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_CustomerService_Accept",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Status_Accept",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Status_Date_Accept_Composite",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Status_Steps",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_Status_UserId",
                table: "newOrders");

            migrationBuilder.DropIndex(
                name: "IX_NewOrders_UserId_Status_Date",
                table: "newOrders");

            migrationBuilder.RenameIndex(
                name: "IX_NewOrders_Status_Date_Extended",
                table: "newOrders",
                newName: "IX_NewOrders_Status_Date");

            migrationBuilder.AlterColumn<string>(
                name: "BrokerID",
                table: "values",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "step3",
                table: "newOrders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "step2",
                table: "newOrders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "step1",
                table: "newOrders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AcceptCustomerService",
                table: "newOrders",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true);
        }
    }
}
