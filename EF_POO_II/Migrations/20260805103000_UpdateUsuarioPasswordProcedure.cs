using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    public partial class UpdateUsuarioPasswordProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_UpdateUsuario
                    @Id int,
                    @Username varchar(50),
                    @PasswordHash varchar(255),
                    @RolId int
                AS
                BEGIN
                    UPDATE Usuarios
                    SET Username = @Username,
                        PasswordHash = @PasswordHash,
                        RolId = @RolId
                    WHERE Id = @Id;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_UpdateUsuario
                    @Id int,
                    @Username varchar(50),
                    @RolId int
                AS
                BEGIN
                    UPDATE Usuarios
                    SET Username = @Username,
                        RolId = @RolId
                    WHERE Id = @Id;
                END
            ");
        }
    }
}
