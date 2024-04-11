using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntexII_Project_4_2.Data.Migrations
{
    /// <inheritdoc />
    public partial class customeridadd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "AspNetUsers");
        }
    }
}
