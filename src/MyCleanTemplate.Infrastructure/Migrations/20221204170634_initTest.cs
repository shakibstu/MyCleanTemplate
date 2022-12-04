using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCleanTemplate.Infrastructure.Migrations
{
    public partial class initTest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Tests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TestEnum = table.Column<int>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    TestValueObject_Street = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TestValueObject_City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TestValueObject_State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TestValueObject_Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TestValueObject_ZipCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    TestValueObjectJson = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tests_TestValueObjectList",
                columns: table => new
                {
                    TestEntityId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Street = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests_TestValueObjectList", x => new { x.TestEntityId, x.Id });
                    table.ForeignKey(
                        name: "FK_Tests_TestValueObjectList_Tests_TestEntityId",
                        column: x => x.TestEntityId,
                        principalTable: "Tests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tests_TestValueObjectList");

            migrationBuilder.DropTable(
                name: "Tests");
        }
    }
}
