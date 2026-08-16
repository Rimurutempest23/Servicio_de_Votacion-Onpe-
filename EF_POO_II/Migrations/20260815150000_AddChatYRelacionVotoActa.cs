using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    public partial class AddChatYRelacionVotoActa : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ActaElectoralId",
                table: "Votos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChatMensajes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Mensaje = table.Column<string>(type: "varchar(240)", maxLength: 240, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMensajes", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Votos_ActaElectoralId",
                table: "Votos",
                column: "ActaElectoralId");

            migrationBuilder.AddForeignKey(
                name: "FK_Votos_ActasElectorales_ActaElectoralId",
                table: "Votos",
                column: "ActaElectoralId",
                principalTable: "ActasElectorales",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

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
                            LEFT JOIN Candidatos c ON c.Id = d.CandidatoId
                            WHERE c.Id IS NULL
                        )
                            THROW 51004, 'Uno o mas candidatos no existen.', 1;

                        INSERT INTO ActasElectorales (EleccionId, MesaElectoralId, FechaRegistro, Estado, UsuarioRegistro, Observaciones)
                        VALUES (@EleccionId, @MesaElectoralId, GETDATE(), 'Procesada', @UsuarioRegistro, @Observaciones);

                        DECLARE @ActaId int = SCOPE_IDENTITY();

                        INSERT INTO DetalleActas (ActaElectoralId, CandidatoId, Votos)
                        SELECT @ActaId, CandidatoId, Votos
                        FROM @Detalle;

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

                        UPDATE MesasElectorales
                        SET Estado = 'Procesada'
                        WHERE Id = @MesaElectoralId;

                        INSERT INTO Auditorias (Usuario, Accion, TablaAfectada, Fecha, Descripcion)
                        VALUES (
                            @UsuarioRegistro,
                            'Procesar acta electoral',
                            'ActasElectorales',
                            GETDATE(),
                            CONCAT('Acta ', @ActaId, ' procesada para mesa ', @MesaElectoralId, ' con ', (SELECT SUM(Votos) FROM @Detalle), ' votos.')
                        );

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
            migrationBuilder.DropForeignKey(
                name: "FK_Votos_ActasElectorales_ActaElectoralId",
                table: "Votos");

            migrationBuilder.DropTable(name: "ChatMensajes");

            migrationBuilder.DropIndex(
                name: "IX_Votos_ActaElectoralId",
                table: "Votos");

            migrationBuilder.DropColumn(
                name: "ActaElectoralId",
                table: "Votos");
        }
    }
}
