using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TelegramService.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.Sql("DELETE FROM \"Users\";");
            //
            // migrationBuilder.AlterColumn<int>(
            //     name: "UserId",
            //     table: "Users",
            //     type: "integer",
            //     nullable: false,
            //     oldClrType: typeof(Guid),
            //     oldType: "uuid")
            //     .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
            //
            // migrationBuilder.AlterColumn<int>(
            //     name: "UserId",
            //     table: "RegistrationCodes",
            //     type: "integer",
            //     nullable: false,
            //     oldClrType: typeof(Guid),
            //     oldType: "uuid");
            
            migrationBuilder.Sql("DELETE FROM \"Users\";");
            
            migrationBuilder.AddColumn<int>(
                name: "TempUserId",
                table: "Users",
                nullable: false);

            // 3. Удаление старого столбца UserId типа GUID
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Users");

            // 4. Переименование TempUserId в UserId
            migrationBuilder.RenameColumn(
                name: "TempUserId",
                table: "Users",
                newName: "UserId");

            // 5. Установление UserId как первичный ключ (если это необходимо)
            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "Users",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AlterColumn<Guid>(
                name: "UserId",
                table: "RegistrationCodes",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");
        }
    }
}
