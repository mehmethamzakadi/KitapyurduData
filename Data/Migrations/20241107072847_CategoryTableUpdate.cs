using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitapyurduData.Data.Migrations
{
    /// <inheritdoc />
    public partial class CategoryTableUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "KitapyurduCategoryId",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KitapyurduCategoryUrl",
                table: "Categories",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KitapyurduCategoryId",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "KitapyurduCategoryUrl",
                table: "Categories");
        }
    }
}
