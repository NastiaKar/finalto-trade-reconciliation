using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeReconciliation.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountNumber = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReconciliationResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TradeIdentifier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DiscrepancyDetails = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReconciledAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReconciliationResults", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TradeIdentifier = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Side = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    TradeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Counterparty = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_AccountNumber",
                table: "Accounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationResults_ReconciledAt",
                table: "ReconciliationResults",
                column: "ReconciledAt");

            migrationBuilder.CreateIndex(
                name: "IX_ReconciliationResults_TradeIdentifier",
                table: "ReconciliationResults",
                column: "TradeIdentifier");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_AccountId",
                table: "Trades",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TradeDate",
                table: "Trades",
                column: "TradeDate");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_TradeIdentifier_Source",
                table: "Trades",
                columns: new[] { "TradeIdentifier", "Source" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReconciliationResults");

            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "Accounts");
        }
    }
}
