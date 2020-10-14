using Microsoft.EntityFrameworkCore.Migrations;

namespace Warehouse.Migrations
{
    public partial class seed : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "d71bec83-8585-4174-a730-bd912c17c5f0", "2adf252f-ce0b-4067-9f9f-ad856f373a70", "User", null },
                    { "3eb97023-903b-4617-a47b-de2928b1c251", "b105dc11-5491-4f58-a82a-90db39bae91a", "Staff", null }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3eb97023-903b-4617-a47b-de2928b1c251");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "d71bec83-8585-4174-a730-bd912c17c5f0");
        }
    }
}
