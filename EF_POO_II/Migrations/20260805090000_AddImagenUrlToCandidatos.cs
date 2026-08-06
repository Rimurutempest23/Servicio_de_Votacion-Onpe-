using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    public partial class AddImagenUrlToCandidatos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                table: "Candidatos",
                type: "varchar(300)",
                unicode: false,
                maxLength: 300,
                nullable: true);

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_InsertCandidato
                    @Nombre varchar(100),
                    @ImagenUrl varchar(300) = NULL
                AS
                BEGIN
                    INSERT INTO Candidatos (Nombre, ImagenUrl, TotalVotos)
                    VALUES (@Nombre, @ImagenUrl, 0);
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_UpdateCandidato
                    @Id int,
                    @Nombre varchar(100),
                    @ImagenUrl varchar(300) = NULL
                AS
                BEGIN
                    UPDATE Candidatos
                    SET Nombre = @Nombre,
                        ImagenUrl = @ImagenUrl
                    WHERE Id = @Id;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_InsertCandidato
                    @Nombre varchar(100)
                AS
                BEGIN
                    INSERT INTO Candidatos (Nombre, TotalVotos)
                    VALUES (@Nombre, 0);
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_UpdateCandidato
                    @Id int,
                    @Nombre varchar(100)
                AS
                BEGIN
                    UPDATE Candidatos
                    SET Nombre = @Nombre
                    WHERE Id = @Id;
                END
            ");

            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                table: "Candidatos");
        }
    }
}
