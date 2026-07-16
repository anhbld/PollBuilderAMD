using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PollBuilder.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Polls",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Question = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Options = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Polls", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "TextQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PollCode = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Upvotes = table.Column<int>(type: "int", nullable: false),
                    IsPinned = table.Column<bool>(type: "bit", nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TextQuestions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TextQuestions_Polls_PollCode",
                        column: x => x.PollCode,
                        principalTable: "Polls",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Votes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PollCode = table.Column<string>(type: "nvarchar(10)", nullable: false),
                    OptionIndex = table.Column<int>(type: "int", nullable: true),
                    TextResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoterToken = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    VotedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Votes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Votes_Polls_PollCode",
                        column: x => x.PollCode,
                        principalTable: "Polls",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TextQuestions_PollCode",
                table: "TextQuestions",
                column: "PollCode");

            migrationBuilder.CreateIndex(
                name: "IX_Votes_PollCode_VoterToken",
                table: "Votes",
                columns: new[] { "PollCode", "VoterToken" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TextQuestions");

            migrationBuilder.DropTable(
                name: "Votes");

            migrationBuilder.DropTable(
                name: "Polls");
        }
    }
}
