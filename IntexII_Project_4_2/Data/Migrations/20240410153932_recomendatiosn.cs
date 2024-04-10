using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntexII_Project_4_2.Data.Migrations
{
    /// <inheritdoc />
    public partial class recomendatiosn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerRecommendations",
                columns: table => new
                {
                    CustomerID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Recommendation1 = table.Column<int>(type: "int", nullable: false),
                    Recommendation2 = table.Column<int>(type: "int", nullable: false),
                    Recommendation3 = table.Column<int>(type: "int", nullable: false),
                    Recommendation4 = table.Column<int>(type: "int", nullable: false),
                    Recommendation5 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerRecommendations", x => x.CustomerID);
                });

            migrationBuilder.CreateTable(
                name: "ItemsRecommendations",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Recommendation1 = table.Column<int>(type: "int", nullable: false),
                    Recommendation2 = table.Column<int>(type: "int", nullable: false),
                    Recommendation3 = table.Column<int>(type: "int", nullable: false),
                    Recommendation4 = table.Column<int>(type: "int", nullable: false),
                    Recommendation5 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemsRecommendations", x => x.ProductID);
                });

            migrationBuilder.CreateTable(
                name: "TopRecommendations",
                columns: table => new
                {
                    ProductID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rating = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TopRecommendations", x => x.ProductID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerRecommendations");

            migrationBuilder.DropTable(
                name: "ItemsRecommendations");

            migrationBuilder.DropTable(
                name: "TopRecommendations");
        }
    }
}
