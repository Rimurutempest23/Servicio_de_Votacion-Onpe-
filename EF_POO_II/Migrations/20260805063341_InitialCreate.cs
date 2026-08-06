using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Candidatos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    TotalVotos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Candidat__3214EC073219DE9E", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Roles__3214EC07165DD0FC", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Votos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Votos__3214EC07B1DB355B", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Votos__Candidato__534D60F1",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuarios__3214EC073E252F66", x => x.Id);
                    table.ForeignKey(
                        name: "FK__Usuarios__RolId__4D94879B",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "UQ__Usuarios__536C85E4BD901214",
                table: "Usuarios",
                column: "Username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Votos_CandidatoId",
                table: "Votos",
                column: "CandidatoId");

            // Stored procedures for Candidatos and Usuarios
            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_InsertCandidato @Nombre varchar(100)
                AS
                BEGIN
                    INSERT INTO Candidatos (Nombre, TotalVotos) VALUES (@Nombre, 0);
                END
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_UpdateCandidato @Id int, @Nombre varchar(100)
                AS
                BEGIN
                    UPDATE Candidatos SET Nombre = @Nombre WHERE Id = @Id;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_DeleteCandidato @Id int
                AS
                BEGIN
                    DELETE FROM Candidatos WHERE Id = @Id;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_InsertUsuario @Username varchar(50), @PasswordHash varchar(255), @RolId int
                AS
                BEGIN
                    INSERT INTO Usuarios (Username, PasswordHash, RolId) VALUES (@Username, @PasswordHash, @RolId);
                END
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_UpdateUsuario @Id int, @Username varchar(50), @RolId int
                AS
                BEGIN
                    UPDATE Usuarios SET Username = @Username, RolId = @RolId WHERE Id = @Id;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE PROCEDURE sp_DeleteUsuario @Id int
                AS
                BEGIN
                    DELETE FROM Usuarios WHERE Id = @Id;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop stored procedures
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_DeleteUsuario;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_UpdateUsuario;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_InsertUsuario;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_DeleteCandidato;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_UpdateCandidato;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_InsertCandidato;");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Votos");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Candidatos");
        }
    }
}
