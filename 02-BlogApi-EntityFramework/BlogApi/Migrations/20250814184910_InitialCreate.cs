using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogApi.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "POSTS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    TITLE = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CONTENT = table.Column<string>(type: "CLOB", nullable: false),
                    SUMMARY = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true),
                    AUTHOR = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    UPDATED_AT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    IS_PUBLISHED = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    PUBLISHED_AT = table.Column<DateTime>(type: "TIMESTAMP", nullable: true),
                    TAGS = table.Column<string>(type: "NVARCHAR2(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_POSTS", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "COMMENTS",
                columns: table => new
                {
                    ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                        .Annotation("Oracle:Identity", "START WITH 1 INCREMENT BY 1"),
                    CONTENT = table.Column<string>(type: "NVARCHAR2(1000)", maxLength: 1000, nullable: false),
                    AUTHOR_NAME = table.Column<string>(type: "NVARCHAR2(100)", maxLength: 100, nullable: false),
                    AUTHOR_EMAIL = table.Column<string>(type: "NVARCHAR2(200)", maxLength: 200, nullable: false),
                    CREATED_AT = table.Column<DateTime>(type: "TIMESTAMP", nullable: false),
                    IS_APPROVED = table.Column<bool>(type: "NUMBER(1)", nullable: false),
                    POST_ID = table.Column<int>(type: "NUMBER(10)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COMMENTS", x => x.ID);
                    table.ForeignKey(
                        name: "FK_COMMENTS_POST_ID",
                        column: x => x.POST_ID,
                        principalTable: "POSTS",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_COMMENTS_CREATED_AT",
                table: "COMMENTS",
                column: "CREATED_AT");

            migrationBuilder.CreateIndex(
                name: "IX_COMMENTS_IS_APPROVED",
                table: "COMMENTS",
                column: "IS_APPROVED");

            migrationBuilder.CreateIndex(
                name: "IX_COMMENTS_POST_ID",
                table: "COMMENTS",
                column: "POST_ID");

            migrationBuilder.CreateIndex(
                name: "IX_POSTS_CREATED_AT",
                table: "POSTS",
                column: "CREATED_AT");

            migrationBuilder.CreateIndex(
                name: "IX_POSTS_IS_PUBLISHED",
                table: "POSTS",
                column: "IS_PUBLISHED");

            migrationBuilder.CreateIndex(
                name: "IX_POSTS_TITLE",
                table: "POSTS",
                column: "TITLE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "COMMENTS");

            migrationBuilder.DropTable(
                name: "POSTS");
        }
    }
}
