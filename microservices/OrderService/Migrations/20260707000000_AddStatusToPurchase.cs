using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrderService.Migrations;

public partial class AddStatusToPurchase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Status",
            table: "Purchases",
            type: "nvarchar(20)",
            maxLength: 20,
            nullable: false,
            defaultValue: "Pending");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "Status", table: "Purchases");
    }
}
