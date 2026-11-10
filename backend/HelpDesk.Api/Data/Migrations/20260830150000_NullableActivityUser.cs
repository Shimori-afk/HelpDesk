using System;
using HelpDesk.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HelpDesk.Api.Data.Migrations
{
    // Written by hand, see InitialCreate. Makes TicketActivities.UserId nullable so the
    // SLA background service can log system-generated activity (e.g. auto-escalation on breach)
    // without attributing it to a human user.
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260830150000_NullableActivityUser")]
    public partial class NullableActivityUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketActivities_Users_UserId",
                table: "TicketActivities");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "TicketActivities",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketActivities_Users_UserId",
                table: "TicketActivities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketActivities_Users_UserId",
                table: "TicketActivities");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "TicketActivities",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int?),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TicketActivities_Users_UserId",
                table: "TicketActivities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
