using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GraphOfOrders.Repo.Migrations
{
    /// <inheritdoc />
    public partial class DeliveryPersonCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliverPersonId",
                table: "Orders",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryStatus",
                table: "Orders",
                type: "text",
                nullable: true,
                defaultValue: "Pending");

            migrationBuilder.CreateTable(
                name: "DeliverPersons",
                columns: table => new
                {
                    DeliverPersonId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliverPersons", x => x.DeliverPersonId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliverPersonId",
                table: "Orders",
                column: "DeliverPersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_DeliverPersons_DeliverPersonId",
                table: "Orders",
                column: "DeliverPersonId",
                principalTable: "DeliverPersons",
                principalColumn: "DeliverPersonId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_DeliverPersons_DeliverPersonId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "DeliverPersons");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliverPersonId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliverPersonId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveryStatus",
                table: "Orders");
        }
    }
}
