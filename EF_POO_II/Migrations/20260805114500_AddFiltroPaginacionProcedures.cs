using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    public partial class AddFiltroPaginacionProcedures : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_ListarUsuariosPaginado
                    @Filtro varchar(100) = NULL,
                    @Page int = 1,
                    @PageSize int = 5
                AS
                BEGIN
                    SET NOCOUNT ON;

                    ;WITH Base AS
                    (
                        SELECT
                            u.Id,
                            u.Username,
                            u.PasswordHash,
                            u.RolId,
                            r.Nombre AS RolNombre,
                            COUNT(*) OVER() AS TotalRegistros
                        FROM Usuarios u
                        INNER JOIN Roles r ON r.Id = u.RolId
                        WHERE @Filtro IS NULL
                           OR u.Username LIKE '%' + @Filtro + '%'
                           OR r.Nombre LIKE '%' + @Filtro + '%'
                    )
                    SELECT *
                    FROM Base
                    ORDER BY Username
                    OFFSET (@Page - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_ListarCandidatosPaginado
                    @Filtro varchar(100) = NULL,
                    @Page int = 1,
                    @PageSize int = 5
                AS
                BEGIN
                    SET NOCOUNT ON;

                    ;WITH Base AS
                    (
                        SELECT
                            Id,
                            Nombre,
                            ImagenUrl,
                            TotalVotos,
                            COUNT(*) OVER() AS TotalRegistros
                        FROM Candidatos
                        WHERE @Filtro IS NULL
                           OR Nombre LIKE '%' + @Filtro + '%'
                    )
                    SELECT *
                    FROM Base
                    ORDER BY Nombre
                    OFFSET (@Page - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_ListarResultadosPaginado
                    @Filtro varchar(100) = NULL,
                    @Page int = 1,
                    @PageSize int = 5
                AS
                BEGIN
                    SET NOCOUNT ON;

                    ;WITH Base AS
                    (
                        SELECT
                            Id,
                            Nombre,
                            ImagenUrl,
                            TotalVotos,
                            COUNT(*) OVER() AS TotalRegistros
                        FROM Candidatos
                        WHERE @Filtro IS NULL
                           OR Nombre LIKE '%' + @Filtro + '%'
                    )
                    SELECT *
                    FROM Base
                    ORDER BY Nombre
                    OFFSET (@Page - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ListarResultadosPaginado;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ListarCandidatosPaginado;");
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ListarUsuariosPaginado;");
        }
    }
}
