using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace train.Migrations
{
    /// <inheritdoc />
    public partial class AddSizeColorToCartItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SelectedColor",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SelectedSize",
                table: "CartItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedColor",
                table: "CartItems");

            migrationBuilder.DropColumn(
                name: "SelectedSize",
                table: "CartItems");
        }
    }
}
