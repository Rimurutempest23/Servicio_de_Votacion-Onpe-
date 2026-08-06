using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    public partial class AddReportesExportados : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReportesExportados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime", nullable: false, defaultValueSql: "GETDATE()"),
                    RutaGuardado = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportesExportados", x => x.Id);
                });

            migrationBuilder.Sql(@"
                CREATE OR ALTER PROCEDURE sp_InsertReporteExportado
                    @Nombre varchar(150),
                    @RutaGuardado varchar(300)
                AS
                BEGIN
                    SET NOCOUNT ON;

                    INSERT INTO ReportesExportados (Nombre, FechaCreacion, RutaGuardado)
                    VALUES (@Nombre, GETDATE(), @RutaGuardado);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS sp_InsertReporteExportado;");
            migrationBuilder.DropTable(name: "ReportesExportados");
        }
    }
}
