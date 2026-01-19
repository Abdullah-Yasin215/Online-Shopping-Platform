using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace train.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrders3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContactName",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShippingAddress",
                table: "Orders",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Orders",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId1",
                table: "Orders",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AspNetUsers_UserId1",
                table: "Orders",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AspNetUsers_UserId1",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId1",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ContactName",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingAddress",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Orders");
        }
    }
}
