using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KitapyurduData.Data.Migrations
{
    /// <inheritdoc />
    public partial class CategoryTableUpdate5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransferInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnSonKaldigiSayfa = table.Column<int>(type: "int", nullable: false),
                    EnSonKaldigiKategoriId = table.Column<int>(type: "int", nullable: false),
                    TransferDurum = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransferInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransferInfos");
        }
    }
}
