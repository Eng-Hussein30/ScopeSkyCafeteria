using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ScopeSkyCafeteria.Migrations
{
    /// <inheritdoc />
    public partial class AddDepartmentAndDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveredAt",
                table: "Orders",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeliveredByAdminId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "Orders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DepartmentName",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DeliveredByAdminId",
                table: "Orders",
                column: "DeliveredByAdminId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_DeliveredByAdminId",
                table: "Orders",
                column: "DeliveredByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_DeliveredByAdminId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DeliveredByAdminId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveredAt",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DeliveredByAdminId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DepartmentName",
                table: "AspNetUsers");
        }
    }
}
