using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TwinBlade.Infrastructure.Persistence.Migrationsclear
{
    /// <inheritdoc />
    public partial class AddItemTypeAndMaterials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemMeterials_ItemMaterialId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemType_ItemTypeId",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemType",
                table: "ItemType");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemMeterials",
                table: "ItemMeterials");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ItemType");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ItemMeterials");

            migrationBuilder.RenameTable(
                name: "ItemType",
                newName: "ItemTypes");

            migrationBuilder.RenameTable(
                name: "ItemMeterials",
                newName: "ItemMaterials");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Items",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemTypes",
                table: "ItemTypes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemMaterials",
                table: "ItemMaterials",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Items_Code",
                table: "Items",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemTypes_Code",
                table: "ItemTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItemMaterials_Code",
                table: "ItemMaterials",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemMaterials_ItemMaterialId",
                table: "Items",
                column: "ItemMaterialId",
                principalTable: "ItemMaterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemTypes_ItemTypeId",
                table: "Items",
                column: "ItemTypeId",
                principalTable: "ItemTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemMaterials_ItemMaterialId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_ItemTypes_ItemTypeId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_Code",
                table: "Items");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemTypes",
                table: "ItemTypes");

            migrationBuilder.DropIndex(
                name: "IX_ItemTypes_Code",
                table: "ItemTypes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ItemMaterials",
                table: "ItemMaterials");

            migrationBuilder.DropIndex(
                name: "IX_ItemMaterials_Code",
                table: "ItemMaterials");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Items");

            migrationBuilder.RenameTable(
                name: "ItemTypes",
                newName: "ItemType");

            migrationBuilder.RenameTable(
                name: "ItemMaterials",
                newName: "ItemMeterials");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ItemType",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ItemMeterials",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemType",
                table: "ItemType",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ItemMeterials",
                table: "ItemMeterials",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemMeterials_ItemMaterialId",
                table: "Items",
                column: "ItemMaterialId",
                principalTable: "ItemMeterials",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Items_ItemType_ItemTypeId",
                table: "Items",
                column: "ItemTypeId",
                principalTable: "ItemType",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
