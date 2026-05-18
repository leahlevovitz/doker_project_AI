using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace serverAPI.Migrations
{
    /// <inheritdoc />
    public partial class addQ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "Gifts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Gifts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "Gifts");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Gifts");
        }
    }
}
