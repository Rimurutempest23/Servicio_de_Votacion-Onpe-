using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    public partial class AddSoftDeleteAsignacionesYPasswordHasher : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActivo",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActivo",
                table: "Candidatos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "OperadorMesaAsignaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    MesaElectoralId = table.Column<int>(type: "int", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    FechaCierre = table.Column<DateTime>(type: "datetime", nullable: true),
                    IsActiva = table.Column<bool>(type: "bit", nullable: false),
                    Observacion = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperadorMesaAsignaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperadorMesaAsignaciones_MesasElectorales_MesaElectoralId",
                        column: x => x.MesaElectoralId,
                        principalTable: "MesasElectorales",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_OperadorMesaAsignaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OperadorMesaAsignaciones_MesaElectoralId",
                table: "OperadorMesaAsignaciones",
                column: "MesaElectoralId");

            migrationBuilder.CreateIndex(
                name: "IX_OperadorMesaAsignaciones_UsuarioId",
                table: "OperadorMesaAsignaciones",
                column: "UsuarioId");

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
                        SELECT u.Id, u.Username, u.PasswordHash, u.RolId, r.Nombre AS RolNombre, COUNT(*) OVER() AS TotalRegistros
                        FROM Usuarios u
                        INNER JOIN Roles r ON r.Id = u.RolId
                        WHERE u.IsActivo = 1
                          AND (@Filtro IS NULL OR u.Username LIKE '%' + @Filtro + '%' OR r.Nombre LIKE '%' + @Filtro + '%')
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
                        SELECT Id, Nombre, ImagenUrl, TotalVotos, COUNT(*) OVER() AS TotalRegistros
                        FROM Candidatos
                        WHERE IsActivo = 1
                          AND (@Filtro IS NULL OR Nombre LIKE '%' + @Filtro + '%')
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
                        SELECT Id, Nombre, ImagenUrl, TotalVotos, COUNT(*) OVER() AS TotalRegistros
                        FROM Candidatos
                        WHERE IsActivo = 1
                          AND (@Filtro IS NULL OR Nombre LIKE '%' + @Filtro + '%')
                    )
                    SELECT *
                    FROM Base
                    ORDER BY Nombre
                    OFFSET (@Page - 1) * @PageSize ROWS
                    FETCH NEXT @PageSize ROWS ONLY;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_InsertCandidato
                    @Nombre varchar(100),
                    @ImagenUrl varchar(300) = NULL
                AS
                BEGIN
                    INSERT INTO Candidatos (Nombre, ImagenUrl, TotalVotos, IsActivo)
                    VALUES (@Nombre, @ImagenUrl, 0, 1);
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_DeleteCandidato
                    @Id int
                AS
                BEGIN
                    UPDATE Candidatos SET IsActivo = 0 WHERE Id = @Id;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_InsertUsuario
                    @Username varchar(50),
                    @PasswordHash varchar(255),
                    @RolId int
                AS
                BEGIN
                    INSERT INTO Usuarios (Username, PasswordHash, RolId, IsActivo)
                    VALUES (@Username, @PasswordHash, @RolId, 1);
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_DeleteUsuario
                    @Id int
                AS
                BEGIN
                    UPDATE Usuarios SET IsActivo = 0 WHERE Id = @Id;
                    UPDATE OperadorMesaAsignaciones
                    SET IsActiva = 0, FechaCierre = GETDATE()
                    WHERE UsuarioId = @Id AND IsActiva = 1;
                END
            ");

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_ProcesarActaElectoral
                    @EleccionId int,
                    @MesaElectoralId int,
                    @UsuarioRegistro varchar(50),
                    @Observaciones varchar(250) = NULL,
                    @DetalleJson nvarchar(max)
                AS
                BEGIN
                    SET NOCOUNT ON;
                    SET XACT_ABORT ON;

                    BEGIN TRY
                        BEGIN TRANSACTION;

                        IF NOT EXISTS (SELECT 1 FROM Elecciones WHERE Id = @EleccionId AND Estado = 'Abierta')
                            THROW 51000, 'La eleccion no existe o no esta abierta.', 1;

                        IF NOT EXISTS (SELECT 1 FROM MesasElectorales WHERE Id = @MesaElectoralId AND Estado <> 'Procesada')
                            THROW 51001, 'La mesa no existe o ya fue procesada.', 1;

                        IF NOT EXISTS (
                            SELECT 1
                            FROM OperadorMesaAsignaciones a
                            INNER JOIN Usuarios u ON u.Id = a.UsuarioId AND u.IsActivo = 1
                            WHERE a.MesaElectoralId = @MesaElectoralId
                              AND a.IsActiva = 1
                              AND u.Username = @UsuarioRegistro
                        )
                            THROW 51005, 'El operador no tiene asignacion activa para esta mesa.', 1;

                        DECLARE @Detalle TABLE (CandidatoId int NOT NULL, Votos int NOT NULL);

                        INSERT INTO @Detalle (CandidatoId, Votos)
                        SELECT CandidatoId, Votos
                        FROM OPENJSON(@DetalleJson)
                        WITH (CandidatoId int '$.CandidatoId', Votos int '$.Votos');

                        IF NOT EXISTS (SELECT 1 FROM @Detalle)
                            THROW 51002, 'Debe registrar al menos un detalle de acta.', 1;

                        IF EXISTS (SELECT 1 FROM @Detalle WHERE Votos < 0)
                            THROW 51003, 'Los votos no pueden ser negativos.', 1;

                        IF EXISTS (
                            SELECT 1
                            FROM @Detalle d
                            LEFT JOIN Candidatos c ON c.Id = d.CandidatoId AND c.IsActivo = 1
                            WHERE c.Id IS NULL
                        )
                            THROW 51004, 'Uno o mas candidatos no existen o estan inactivos.', 1;

                        INSERT INTO ActasElectorales (EleccionId, MesaElectoralId, FechaRegistro, Estado, UsuarioRegistro, Observaciones)
                        VALUES (@EleccionId, @MesaElectoralId, GETDATE(), 'Procesada', @UsuarioRegistro, @Observaciones);

                        DECLARE @ActaId int = SCOPE_IDENTITY();

                        INSERT INTO DetalleActas (ActaElectoralId, CandidatoId, Votos)
                        SELECT @ActaId, CandidatoId, Votos FROM @Detalle;

                        DECLARE @Pendientes TABLE (CandidatoId int NOT NULL, TotalPendiente int NOT NULL);

                        INSERT INTO @Pendientes (CandidatoId, TotalPendiente)
                        SELECT v.CandidatoId, SUM(v.Cantidad)
                        FROM Votos v
                        INNER JOIN @Detalle d ON d.CandidatoId = v.CandidatoId
                        WHERE v.ActaElectoralId IS NULL
                        GROUP BY v.CandidatoId;

                        UPDATE c
                        SET c.TotalVotos =
                            CASE
                                WHEN c.TotalVotos >= p.TotalPendiente THEN c.TotalVotos - p.TotalPendiente
                                ELSE 0
                            END
                        FROM Candidatos c
                        INNER JOIN @Pendientes p ON p.CandidatoId = c.Id;

                        DELETE v
                        FROM Votos v
                        INNER JOIN @Detalle d ON d.CandidatoId = v.CandidatoId
                        WHERE v.ActaElectoralId IS NULL;

                        INSERT INTO Votos (CandidatoId, Cantidad, Fecha, ActaElectoralId)
                        SELECT CandidatoId, Votos, GETDATE(), @ActaId
                        FROM @Detalle
                        WHERE Votos > 0;

                        UPDATE c
                        SET c.TotalVotos = c.TotalVotos + d.Total
                        FROM Candidatos c
                        INNER JOIN (
                            SELECT CandidatoId, SUM(Votos) AS Total
                            FROM @Detalle
                            GROUP BY CandidatoId
                        ) d ON d.CandidatoId = c.Id;

                        UPDATE MesasElectorales SET Estado = 'Procesada' WHERE Id = @MesaElectoralId;

                        UPDATE a
                        SET a.IsActiva = 0, a.FechaCierre = GETDATE(), a.Observacion = 'Cerrada por procesamiento de acta'
                        FROM OperadorMesaAsignaciones a
                        INNER JOIN Usuarios u ON u.Id = a.UsuarioId
                        WHERE a.MesaElectoralId = @MesaElectoralId AND a.IsActiva = 1 AND u.Username = @UsuarioRegistro;

                        INSERT INTO Auditorias (Usuario, Accion, TablaAfectada, Fecha, Descripcion)
                        VALUES (@UsuarioRegistro, 'Procesar acta electoral', 'ActasElectorales', GETDATE(),
                                CONCAT('Acta ', @ActaId, ' procesada para mesa ', @MesaElectoralId, ' con ', (SELECT SUM(Votos) FROM @Detalle), ' votos.'));

                        COMMIT TRANSACTION;
                        SELECT @ActaId AS ActaId;
                    END TRY
                    BEGIN CATCH
                        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
                        THROW;
                    END CATCH
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "OperadorMesaAsignaciones");
            migrationBuilder.DropColumn(name: "IsActivo", table: "Usuarios");
            migrationBuilder.DropColumn(name: "IsActivo", table: "Candidatos");
        }
    }
}
