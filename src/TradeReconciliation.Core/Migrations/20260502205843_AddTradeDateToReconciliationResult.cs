using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeReconciliation.Core.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeDateToReconciliationResult : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TradeDate",
                table: "ReconciliationResults",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationResults_TradeDate",
                table: "ReconciliationResults",
                column: "TradeDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReconciliationResults_TradeDate",
                table: "ReconciliationResults");

            migrationBuilder.DropColumn(
                name: "TradeDate",
                table: "ReconciliationResults");
        }
    }
}
