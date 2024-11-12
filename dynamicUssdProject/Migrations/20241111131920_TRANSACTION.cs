using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dynamicUssdProject.Migrations
{
    /// <inheritdoc />
    public partial class TRANSACTION : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Commented out the creation of Menus and SubMenus tables if they already exist.

            // migrationBuilder.CreateTable(
            //     name: "Menus",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         DisplayText = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         ActionUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         ParentId = table.Column<int>(type: "int", nullable: true)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_Menus", x => x.Id);
            //     });

            // migrationBuilder.CreateTable(
            //     name: "SubMenus",
            //     columns: table => new
            //     {
            //         Id = table.Column<int>(type: "int", nullable: false)
            //             .Annotation("SqlServer:Identity", "1, 1"),
            //         DisplayText = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         ActionUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //         ParentMenuId = table.Column<int>(type: "int", nullable: false)
            //     },
            //     constraints: table =>
            //     {
            //         table.PrimaryKey("PK_SubMenus", x => x.Id);
            //         table.ForeignKey(
            //             name: "FK_SubMenus_Menus_ParentMenuId",
            //             column: x => x.ParentMenuId,
            //             principalTable: "Menus",
            //             principalColumn: "Id",
            //             onDelete: ReferentialAction.Cascade);
            //     });

            // migrationBuilder.CreateIndex(
            //     name: "IX_SubMenus_ParentMenuId",
            //     table: "SubMenus",
            //     column: "ParentMenuId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubMenus");

            migrationBuilder.DropTable(
                name: "Menus");
        }
    }
}
