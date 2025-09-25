using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeliveryService.Migrations
{
    /// <inheritdoc />
    public partial class VerticalSliceBirth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "delivery_person",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_person", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "delivery_order",
                columns: table => new
                {
                    order_id = table.Column<int>(type: "integer", nullable: false),
                    customer_id = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Available"),
                    delivery_person_id = table.Column<int>(type: "integer", nullable: true),
                    snapshot = table.Column<string>(type: "jsonb", nullable: true),
                    idempotency_token = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_delivery_order", x => new { x.order_id, x.customer_id });
                    table.ForeignKey(
                        name: "FK_delivery_order_delivery_person_delivery_person_id",
                        column: x => x.delivery_person_id,
                        principalTable: "delivery_person",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_delivery_order_category",
                table: "delivery_order",
                column: "category");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_order_delivery_person_id",
                table: "delivery_order",
                column: "delivery_person_id");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_order_idempotency_token",
                table: "delivery_order",
                column: "idempotency_token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_delivery_order_status",
                table: "delivery_order",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_delivery_person_email",
                table: "delivery_person",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "delivery_order");

            migrationBuilder.DropTable(
                name: "delivery_person");
        }
    }
}
