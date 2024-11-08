using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitapyurduData.Data.Migrations
{
    /// <inheritdoc />
    public partial class CategoryTableUpdate2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PageCount",
                table: "Categories",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ToplamKitapSayisi",
                table: "Categories",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PageCount",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "ToplamKitapSayisi",
                table: "Categories");
        }
    }
}
