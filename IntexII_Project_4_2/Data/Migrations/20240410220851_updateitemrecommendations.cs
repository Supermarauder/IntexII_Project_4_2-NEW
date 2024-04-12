using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntexII_Project_4_2.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateitemrecommendations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemsRecommendations",
                table: "ItemsRecommendations");

            migrationBuilder.RenameTable(
                name: "ItemsRecommendations",
                newName: "ItemRecommendations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemRecommendations",
                table: "ItemRecommendations",
                column: "ProductID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemRecommendations",
                table: "ItemRecommendations");

            migrationBuilder.RenameTable(
                name: "ItemRecommendations",
                newName: "ItemsRecommendations");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemsRecommendations",
                table: "ItemsRecommendations",
                column: "ProductID");
        }
    }
}
