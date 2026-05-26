using Microsoft.EntityFrameworkCore;
using Tutorial8.Models;

namespace Tutorial8.Data;

public partial class HospitalContext : DbContext
{
    public HospitalContext(DbContextOptions<HospitalContext> options) : base(options) { }

    public virtual DbSet<Admission> Admissions { get; set; }
    public virtual DbSet<BedAssignment> BedAssignments { get; set; }
    public virtual DbSet<BedType> BedTypes { get; set; }
    public virtual DbSet<Bed> Beds { get; set; }
    public virtual DbSet<Patient> Patients { get; set; }
    public virtual DbSet<Room> Rooms { get; set; }
    public virtual DbSet<Ward> Wards { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Admission>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientPesel).HasColumnType("char(11)");
            entity.Property(e => e.AdmissionDate).HasColumnType("datetime");
            entity.Property(e => e.DischargeDate).HasColumnType("datetime");

            entity.HasOne(e => e.PatientPeselNavigation)
                .WithMany(p => p.Admissions)
                .HasForeignKey(e => e.PatientPesel)
                .HasConstraintName("Admissions_Patients");

            entity.HasOne(e => e.Ward)
                .WithMany(w => w.Admissions)
                .HasForeignKey(e => e.WardId)
                .HasConstraintName("Admissions_Wards");
        });

        modelBuilder.Entity<BedAssignment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PatientPesel).HasColumnType("char(11)");
            entity.Property(e => e.From).HasColumnName("From").HasColumnType("datetime");
            entity.Property(e => e.To).HasColumnName("To").HasColumnType("datetime");

            entity.HasOne(e => e.PatientPeselNavigation)
                .WithMany(p => p.BedAssignments)
                .HasForeignKey(e => e.PatientPesel)
                .HasConstraintName("BedAssignments_Patients");

            entity.HasOne(e => e.BedNavigation)
                .WithMany(b => b.BedAssignments)
                .HasForeignKey(e => e.BedId)
                .HasConstraintName("BedAssignments_Beds");
        });

        modelBuilder.Entity<BedType>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(300);
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<Bed>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.RoomId).HasMaxLength(4).IsUnicode(false);

            entity.HasOne(e => e.BedType)
                .WithMany(bt => bt.Beds)
                .HasForeignKey(e => e.BedTypeId)
                .HasConstraintName("Beds_BedTypes");

            entity.HasOne(e => e.Room)
                .WithMany(r => r.Beds)
                .HasForeignKey(e => e.RoomId)
                .HasConstraintName("Beds_Rooms");
        });

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(e => e.Pesel);
            entity.Property(e => e.Pesel).HasColumnType("char(11)");
            entity.Property(e => e.FirstName).HasMaxLength(50);
            entity.Property(e => e.LastName).HasMaxLength(100);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(4).IsUnicode(false);

            entity.HasOne(e => e.Ward)
                .WithMany(w => w.Rooms)
                .HasForeignKey(e => e.WardId)
                .HasConstraintName("Room_Ward");
        });

        modelBuilder.Entity<Ward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(300);
            entity.Property(e => e.Description).HasColumnType("nvarchar(max)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
