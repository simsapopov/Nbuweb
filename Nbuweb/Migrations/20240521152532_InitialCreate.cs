using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nbuweb.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "BankProducts",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductName = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ProductType = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MinAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    MaxAmount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    FixedTerm = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxTermLength = table.Column<int>(type: "int", nullable: true),
                    FlexTerm = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    InterestPaymentInterval = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CanReinvest = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankProducts", x => x.ProductId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "DepositDetails",
                columns: table => new
                {
                    DepositDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Amount = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    TermLength = table.Column<int>(type: "int", nullable: false),
                    InterestRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepositDetails", x => x.DepositDetailId);
                    table.ForeignKey(
                        name: "FK_DepositDetails_BankProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "BankProducts",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "InterestDetails",
                columns: table => new
                {
                    InterestDetailId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    BankProductId = table.Column<int>(type: "int", nullable: false),
                    TermLengthInMonths = table.Column<int>(type: "int", nullable: false),
                    UsdInterestRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    EurInterestRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    BgnInterestRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    GovernmentTaxRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    EarlyWithdrawalPenaltyRate = table.Column<decimal>(type: "decimal(65,30)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterestDetails", x => x.InterestDetailId);
                    table.ForeignKey(
                        name: "FK_InterestDetails_BankProducts_BankProductId",
                        column: x => x.BankProductId,
                        principalTable: "BankProducts",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_DepositDetails_ProductId",
                table: "DepositDetails",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_InterestDetails_BankProductId",
                table: "InterestDetails",
                column: "BankProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepositDetails");

            migrationBuilder.DropTable(
                name: "InterestDetails");

            migrationBuilder.DropTable(
                name: "BankProducts");
        }
    }
}
