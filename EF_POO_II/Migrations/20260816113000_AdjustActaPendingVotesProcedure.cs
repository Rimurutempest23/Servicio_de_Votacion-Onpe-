using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    public partial class AdjustActaPendingVotesProcedure : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }
    }
}
