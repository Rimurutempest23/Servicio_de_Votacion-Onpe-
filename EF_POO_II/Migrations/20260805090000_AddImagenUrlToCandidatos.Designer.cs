using EF_POO_II.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;

#nullable disable

namespace EF_POO_II.Migrations
{
    [DbContext(typeof(SistemaVotacionContext))]
    [Migration("20260805090000_AddImagenUrlToCandidatos")]
    partial class AddImagenUrlToCandidatos
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "10.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("EF_POO_II.Models.Candidato", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                b.Property<string>("Nombre")
                    .IsRequired()
                    .HasMaxLength(100)
                    .IsUnicode(false)
                    .HasColumnType("varchar(100)");

                b.Property<string>("ImagenUrl")
                    .HasMaxLength(300)
                    .IsUnicode(false)
                    .HasColumnType("varchar(300)");

                b.Property<int>("TotalVotos")
                    .HasColumnType("int");

                b.HasKey("Id")
                    .HasName("PK__Candidat__3214EC073219DE9E");

                b.ToTable("Candidatos");
            });

            modelBuilder.Entity("EF_POO_II.Models.Role", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                b.Property<string>("Nombre")
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnType("varchar(50)");

                b.HasKey("Id")
                    .HasName("PK__Roles__3214EC07165DD0FC");

                b.ToTable("Roles");
            });

            modelBuilder.Entity("EF_POO_II.Models.Usuario", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                b.Property<string>("PasswordHash")
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false)
                    .HasColumnType("varchar(255)");

                b.Property<int>("RolId")
                    .HasColumnType("int");

                b.Property<string>("Username")
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnType("varchar(50)");

                b.HasKey("Id")
                    .HasName("PK__Usuarios__3214EC073E252F66");

                b.HasIndex("RolId");

                b.HasIndex(new[] { "Username" }, "UQ__Usuarios__536C85E4BD901214")
                    .IsUnique();

                b.ToTable("Usuarios");
            });

            modelBuilder.Entity("EF_POO_II.Models.Voto", b =>
            {
                b.Property<int>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("int");

                SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                b.Property<int>("CandidatoId")
                    .HasColumnType("int");

                b.Property<int>("Cantidad")
                    .HasColumnType("int");

                b.Property<DateTime?>("Fecha")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("(getdate())");

                b.HasKey("Id")
                    .HasName("PK__Votos__3214EC07B1DB355B");

                b.HasIndex("CandidatoId");

                b.ToTable("Votos");
            });

            modelBuilder.Entity("EF_POO_II.Models.Usuario", b =>
            {
                b.HasOne("EF_POO_II.Models.Role", "Rol")
                    .WithMany("Usuarios")
                    .HasForeignKey("RolId")
                    .IsRequired()
                    .HasConstraintName("FK__Usuarios__RolId__4D94879B");

                b.Navigation("Rol");
            });

            modelBuilder.Entity("EF_POO_II.Models.Voto", b =>
            {
                b.HasOne("EF_POO_II.Models.Candidato", "Candidato")
                    .WithMany("Votos")
                    .HasForeignKey("CandidatoId")
                    .IsRequired()
                    .HasConstraintName("FK__Votos__Candidato__534D60F1");

                b.Navigation("Candidato");
            });

            modelBuilder.Entity("EF_POO_II.Models.Candidato", b =>
            {
                b.Navigation("Votos");
            });

            modelBuilder.Entity("EF_POO_II.Models.Role", b =>
            {
                b.Navigation("Usuarios");
            });
#pragma warning restore 612, 618
        }
    }
}