using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EF_POO_II.Migrations
{
    public partial class AddAsignacionMesaOperador : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "MesasElectorales",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MesasElectorales_UsuarioId",
                table: "MesasElectorales",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_MesasElectorales_Usuarios_UsuarioId",
                table: "MesasElectorales",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MesasElectorales_Usuarios_UsuarioId",
                table: "MesasElectorales");

            migrationBuilder.DropIndex(
                name: "IX_MesasElectorales_UsuarioId",
                table: "MesasElectorales");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "MesasElectorales");
        }
    }
}
