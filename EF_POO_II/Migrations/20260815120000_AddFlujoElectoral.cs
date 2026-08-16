using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    public partial class AddFlujoElectoral : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Auditorias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Usuario = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Accion = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    TablaAfectada = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    Descripcion = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Auditorias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Elecciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime", nullable: false),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Elecciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MesasElectorales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CodigoMesa = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    LocalVotacion = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false),
                    Distrito = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: false),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MesasElectorales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ActasElectorales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EleccionId = table.Column<int>(type: "int", nullable: false),
                    MesaElectoralId = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    UsuarioRegistro = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Observaciones = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActasElectorales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActasElectorales_Elecciones_EleccionId",
                        column: x => x.EleccionId,
                        principalTable: "Elecciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ActasElectorales_MesasElectorales_MesaElectoralId",
                        column: x => x.MesaElectoralId,
                        principalTable: "MesasElectorales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DetalleActas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActaElectoralId = table.Column<int>(type: "int", nullable: false),
                    CandidatoId = table.Column<int>(type: "int", nullable: false),
                    Votos = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleActas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetalleActas_ActasElectorales_ActaElectoralId",
                        column: x => x.ActaElectoralId,
                        principalTable: "ActasElectorales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleActas_Candidatos_CandidatoId",
                        column: x => x.CandidatoId,
                        principalTable: "Candidatos",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActasElectorales_EleccionId",
                table: "ActasElectorales",
                column: "EleccionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActasElectorales_MesaElectoralId",
                table: "ActasElectorales",
                column: "MesaElectoralId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleActas_ActaElectoralId",
                table: "DetalleActas",
                column: "ActaElectoralId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleActas_CandidatoId",
                table: "DetalleActas",
                column: "CandidatoId");

            migrationBuilder.CreateIndex(
                name: "IX_MesasElectorales_CodigoMesa",
                table: "MesasElectorales",
                column: "CodigoMesa",
                unique: true);

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

                        DECLARE @Detalle TABLE
                        (
                            CandidatoId int NOT NULL,
                            Votos int NOT NULL
                        );

                        INSERT INTO @Detalle (CandidatoId, Votos)
                        SELECT CandidatoId, Votos
                        FROM OPENJSON(@DetalleJson)
                        WITH
                        (
                            CandidatoId int '$.CandidatoId',
                            Votos int '$.Votos'
                        );

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

                        INSERT INTO Votos (CandidatoId, Cantidad, Fecha)
                        SELECT CandidatoId, Votos, GETDATE()
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
                        IF @@TRANCOUNT > 0
                            ROLLBACK TRANSACTION;

                        THROW;
                    END CATCH
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_ProcesarActaElectoral;");
            migrationBuilder.DropTable(name: "Auditorias");
            migrationBuilder.DropTable(name: "DetalleActas");
            migrationBuilder.DropTable(name: "ActasElectorales");
            migrationBuilder.DropTable(name: "Elecciones");
            migrationBuilder.DropTable(name: "MesasElectorales");
        }
    }
}
