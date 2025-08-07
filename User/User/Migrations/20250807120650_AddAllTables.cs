using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace User.Migrations
{
    /// <inheritdoc />
    public partial class AddAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "newOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Location = table.Column<string>(type: "longtext", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UserId = table.Column<string>(type: "longtext", nullable: false),
                    statuOrder = table.Column<string>(type: "longtext", nullable: false),
                    numberOfLicense = table.Column<string>(type: "longtext", nullable: false),
                    Accept = table.Column<string>(type: "longtext", nullable: true),
                    AcceptCustomerService = table.Column<string>(type: "longtext", nullable: true),
                    AcceptAccount = table.Column<string>(type: "longtext", nullable: true),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    City = table.Column<string>(type: "longtext", nullable: true),
                    Town = table.Column<string>(type: "longtext", nullable: true),
                    zipCode = table.Column<string>(type: "longtext", nullable: true),
                    step1 = table.Column<string>(type: "longtext", nullable: true),
                    step2 = table.Column<string>(type: "longtext", nullable: true),
                    step3 = table.Column<string>(type: "longtext", nullable: true),
                    JopID = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_newOrders", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "paymentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    OrderId = table.Column<string>(type: "longtext", nullable: true),
                    UserId = table.Column<string>(type: "longtext", nullable: true),
                    Status = table.Column<string>(type: "longtext", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DateTime = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    TransactionId = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_paymentDetails", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "saberCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Subject = table.Column<string>(type: "longtext", nullable: false),
                    Description = table.Column<string>(type: "longtext", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_saberCertificates", x => x.Id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notesAccountings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    UserID = table.Column<string>(type: "longtext", nullable: true),
                    newOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notesAccountings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notesAccountings_newOrders_newOrderId",
                        column: x => x.newOrderId,
                        principalTable: "newOrders",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notesCustomerServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Notes = table.Column<string>(type: "longtext", nullable: true),
                    fileName = table.Column<string>(type: "longtext", nullable: true),
                    fileUrl = table.Column<string>(type: "longtext", nullable: true),
                    ContentType = table.Column<string>(type: "longtext", nullable: true),
                    newOrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notesCustomerServices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_notesCustomerServices_newOrders_newOrderId",
                        column: x => x.newOrderId,
                        principalTable: "newOrders",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "typeOrders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    typeOrder = table.Column<string>(type: "longtext", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<float>(type: "float", nullable: true),
                    Size = table.Column<string>(type: "longtext", nullable: true),
                    newOrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_typeOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_typeOrders_newOrders_newOrderId",
                        column: x => x.newOrderId,
                        principalTable: "newOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "uploadFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    fileName = table.Column<string>(type: "longtext", nullable: false),
                    fileUrl = table.Column<string>(type: "longtext", nullable: false),
                    ContentType = table.Column<string>(type: "longtext", nullable: false),
                    newOrderId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_uploadFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_uploadFiles_newOrders_newOrderId",
                        column: x => x.newOrderId,
                        principalTable: "newOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "value",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    BrokerID = table.Column<string>(type: "longtext", nullable: true),
                    newOrderId = table.Column<int>(type: "int", nullable: true),
                    Value = table.Column<double>(type: "double", nullable: true),
                    Accept = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    JopID = table.Column<string>(type: "longtext", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_value", x => x.Id);
                    table.ForeignKey(
                        name: "FK_value_newOrders_newOrderId",
                        column: x => x.newOrderId,
                        principalTable: "newOrders",
                        principalColumn: "Id");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_notesAccountings_newOrderId",
                table: "notesAccountings",
                column: "newOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_notesCustomerServices_newOrderId",
                table: "notesCustomerServices",
                column: "newOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_typeOrders_newOrderId",
                table: "typeOrders",
                column: "newOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_uploadFiles_newOrderId",
                table: "uploadFiles",
                column: "newOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_value_newOrderId",
                table: "value",
                column: "newOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notesAccountings");

            migrationBuilder.DropTable(
                name: "notesCustomerServices");

            migrationBuilder.DropTable(
                name: "paymentDetails");

            migrationBuilder.DropTable(
                name: "saberCertificates");

            migrationBuilder.DropTable(
                name: "typeOrders");

            migrationBuilder.DropTable(
                name: "uploadFiles");

            migrationBuilder.DropTable(
                name: "value");

            migrationBuilder.DropTable(
                name: "newOrders");
        }
    }
}
