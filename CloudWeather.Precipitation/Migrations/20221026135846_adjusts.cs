using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CloudWeather.Precipitation.Migrations
{
    public partial class adjusts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AmountIncher",
                table: "precipitation",
                newName: "AmountInches");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AmountInches",
                table: "precipitation",
                newName: "AmountIncher");
        }
    }
}
