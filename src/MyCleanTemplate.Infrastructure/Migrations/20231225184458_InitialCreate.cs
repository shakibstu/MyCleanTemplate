using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyCleanTemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
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
                    TestValueObjectList = table.Column<string>(type: "TEXT", nullable: true),
                    Created = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tests", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Tests");
        }
    }
}
