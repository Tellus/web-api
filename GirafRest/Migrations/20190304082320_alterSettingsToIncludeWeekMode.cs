using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace GirafRest.Migrations
{
    public partial class alterSettingsToIncludeWeekMode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>("weekDisplayMode", "Setting");
            migrationBuilder.AddColumn<int>("weekDisplayNumOfDays", "Setting");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn("weekDisplayMode", "Setting");
            migrationBuilder.DropColumn("weekDisplayNumOfDays", "Setting");
        }
    }
}
