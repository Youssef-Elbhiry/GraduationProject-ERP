using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP.Infrastructure.Migrations;

public partial class AddO2C_Phase1 : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "Name",
            table: "AspNetUsers",
            type: "nvarchar(max)",
            nullable: false,
            defaultValue: "");

        migrationBuilder.CreateTable(
            name: "Warehouses",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Warehouses", x => x.Id));

        migrationBuilder.CreateTable(
            name: "CustomerAddresses",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                CustomerUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                Line1 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                Line2 = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                IsDefault = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CustomerAddresses", x => x.Id);
                table.ForeignKey(
                    name: "FK_CustomerAddresses_AspNetUsers_CustomerUserId",
                    column: x => x.CustomerUserId,
                    principalTable: "AspNetUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Factories",
            columns: table => new
            {
                Id = table.Column<int>(type: "int", nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                WarehouseId = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Factories", x => x.Id);
                table.ForeignKey(
                    name: "FK_Factories_Warehouses_WarehouseId",
                    column: x => x.WarehouseId,
                    principalTable: "Warehouses",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CustomerAddresses_CustomerUserId",
            table: "CustomerAddresses",
            column: "CustomerUserId",
            unique: true,
            filter: "[IsDefault] = 1");

        migrationBuilder.CreateIndex(
            name: "IX_Factories_WarehouseId",
            table: "Factories",
            column: "WarehouseId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "CustomerAddresses");
        migrationBuilder.DropTable(name: "Factories");
        migrationBuilder.DropTable(name: "Warehouses");
        migrationBuilder.DropColumn(name: "Name", table: "AspNetUsers");
    }
}
