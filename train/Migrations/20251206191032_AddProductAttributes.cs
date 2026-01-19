using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace train.Migrations
{
    /// <inheritdoc />
    public partial class AddProductAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Colors",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sizes",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Colors",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "Sizes",
                table: "Products");
        }
    }
}
