using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace EF_POO_II.Models;

public partial class SistemaVotacionContext : DbContext
{
    public SistemaVotacionContext()
    {
    }

    public SistemaVotacionContext(DbContextOptions<SistemaVotacionContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Candidato> Candidatos { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    public virtual DbSet<Voto> Votos { get; set; }

    public virtual DbSet<ReporteExportado> ReportesExportados { get; set; }

    public virtual DbSet<Eleccion> Elecciones { get; set; }

    public virtual DbSet<MesaElectoral> MesasElectorales { get; set; }

    public virtual DbSet<ActaElectoral> ActasElectorales { get; set; }

    public virtual DbSet<DetalleActa> DetalleActas { get; set; }

    public virtual DbSet<Auditoria> Auditorias { get; set; }

    public virtual DbSet<ChatMensaje> ChatMensajes { get; set; }

    public virtual DbSet<OperadorMesaAsignacion> OperadorMesaAsignaciones { get; set; }

    

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidato>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Candidat__3214EC073219DE9E");

            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.ImagenUrl)
                .HasMaxLength(300)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC07165DD0FC");

            entity.Property(e => e.Nombre)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Usuarios__3214EC073E252F66");

            entity.HasIndex(e => e.Username, "UQ__Usuarios__536C85E4BD901214").IsUnique();

            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Rol).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.RolId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Usuarios__RolId__4D94879B");
        });

        modelBuilder.Entity<Voto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Votos__3214EC07B1DB355B");

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Candidato).WithMany(p => p.Votos)
                .HasForeignKey(d => d.CandidatoId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Votos__Candidato__534D60F1");

            entity.HasOne(d => d.ActaElectoral)
                .WithMany()
                .HasForeignKey(d => d.ActaElectoralId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ReporteExportado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ReportesExportados");

            entity.Property(e => e.Nombre)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("GETDATE()")
                .HasColumnType("datetime");

            entity.Property(e => e.RutaGuardado)
                .HasMaxLength(300)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Eleccion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Elecciones");

            entity.Property(e => e.Nombre)
                .HasMaxLength(120)
                .IsUnicode(false);

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.FechaInicio).HasColumnType("datetime");
            entity.Property(e => e.FechaFin).HasColumnType("datetime");
        });

        modelBuilder.Entity<MesaElectoral>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_MesasElectorales");

            entity.HasIndex(e => e.CodigoMesa).IsUnique();

            entity.Property(e => e.CodigoMesa)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.LocalVotacion)
                .HasMaxLength(120)
                .IsUnicode(false);

            entity.Property(e => e.Distrito)
                .HasMaxLength(80)
                .IsUnicode(false);

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ActaElectoral>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ActasElectorales");

            entity.Property(e => e.FechaRegistro)
                .HasDefaultValueSql("GETDATE()")
                .HasColumnType("datetime");

            entity.Property(e => e.Estado)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.Property(e => e.UsuarioRegistro)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Observaciones)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(e => e.Eleccion)
                .WithMany(e => e.Actas)
                .HasForeignKey(e => e.EleccionId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.MesaElectoral)
                .WithMany(e => e.Actas)
                .HasForeignKey(e => e.MesaElectoralId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<MesaElectoral>()
            .HasOne(e => e.Usuario)
            .WithMany(e => e.MesasAsignadas)
            .HasForeignKey(e => e.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<OperadorMesaAsignacion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_OperadorMesaAsignaciones");

            entity.Property(e => e.FechaAsignacion)
                .HasDefaultValueSql("GETDATE()")
                .HasColumnType("datetime");

            entity.Property(e => e.FechaCierre)
                .HasColumnType("datetime");

            entity.Property(e => e.Observacion)
                .HasMaxLength(120)
                .IsUnicode(false);

            entity.HasOne(e => e.Usuario)
                .WithMany(e => e.AsignacionesMesa)
                .HasForeignKey(e => e.UsuarioId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.MesaElectoral)
                .WithMany()
                .HasForeignKey(e => e.MesaElectoralId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<DetalleActa>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_DetalleActas");

            entity.HasOne(e => e.ActaElectoral)
                .WithMany(e => e.Detalles)
                .HasForeignKey(e => e.ActaElectoralId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Candidato)
                .WithMany()
                .HasForeignKey(e => e.CandidatoId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Auditorias");

            entity.Property(e => e.Usuario)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Accion)
                .HasMaxLength(80)
                .IsUnicode(false);

            entity.Property(e => e.TablaAfectada)
                .HasMaxLength(80)
                .IsUnicode(false);

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("GETDATE()")
                .HasColumnType("datetime");

            entity.Property(e => e.Descripcion)
                .HasMaxLength(300)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ChatMensaje>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_ChatMensajes");

            entity.Property(e => e.Usuario)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Mensaje)
                .HasMaxLength(240)
                .IsUnicode(false);

            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("GETDATE()")
                .HasColumnType("datetime");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
