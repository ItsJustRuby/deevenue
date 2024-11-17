using Deevenue.Domain;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Deevenue.Infrastructure.Db;

public class DeevenueContext(DbContextOptions<DeevenueContext> options) :
    DbContext(options), IDataProtectionKeyContext
{
    // It looks odd, but the "= null!;" tells the compiler that this is implicitly non-null.
    // * adding "required" makes instantiating the context manually impossible
    // * not initializing the fields at all makes "dotnet format" add "?" to the type declaration,
    //   which is semantically incorrect and causes build warnings and thus, errors in Release.
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    public DbSet<Medium> Media { get; set; } = null!;
    public DbSet<MediumHash> MediumHashes { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<TagImplication> TagImplications { get; set; } = null!;
    public DbSet<ThumbnailSheet> ThumbnailSheets { get; set; } = null!;
    public DbSet<JobResult> JobResults { get; set; } = null!;

    // Implicitly required for the "UsingEntity" model creations below to actually be implemented
    // correctly (creation will succeed, but foreign keys will fail at insert time!)
    public DbSet<MediumTag> MediumTags { get; set; } = null!;
    public DbSet<MediumTagAbsence> MediumTagAbsences { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Note: You cannot use a custom schema name.
        // At least not without breaking migrations - they will fail because
        // they check for the presence of __EFMigrationsHistory in the default schema,
        // and yours will be in your custom one. This will work for a bit, then fail
        // when trying to apply the second ever migration. Oof!

        var ratingConversion = new ValueConverter<Rating, char>(
            r => Ratings.ToChar[r],
            c => Ratings.ToRating[c]);

        modelBuilder.Entity<Medium>()
            .HasKey(m => m.Id);
        modelBuilder.Entity<Medium>()
            .Property(m => m.ContentType)
            .IsRequired();
        modelBuilder.Entity<Medium>()
            .Property(m => m.Width)
            .IsRequired();
        modelBuilder.Entity<Medium>()
            .Property(m => m.Height)
            .IsRequired();
        modelBuilder.Entity<Medium>()
            .Property(m => m.FileSize)
            .IsRequired();
        modelBuilder.Entity<Medium>()
            .Property(m => m.InsertedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();
        modelBuilder.Entity<Medium>()
            .HasMany(m => m.KnownHashes)
            .WithOne(h => h.Medium);
        modelBuilder.Entity<Medium>()
            .HasMany(m => m.Tags)
            .WithMany(t => t.Media)
            .UsingEntity<MediumTag>();
        modelBuilder.Entity<Medium>()
            .HasMany(m => m.AbsentTags)
            .WithMany(t => t.AbsentMedia)
            .UsingEntity<MediumTagAbsence>();
        modelBuilder.Entity<Medium>()
            .Property(t => t.Rating)
            .HasConversion(ratingConversion);

        modelBuilder.Entity<MediumHash>()
            .HasKey(m => m.Id);
        modelBuilder.Entity<MediumHash>()
            .Property(m => m.Hash)
            .IsRequired()
            .HasMaxLength(32)
            .IsFixedLength(true);
        modelBuilder.Entity<MediumHash>()
            .HasIndex(m => m.Hash)
            .IsUnique();

        modelBuilder.Entity<Tag>()
            .HasKey(t => t.Id);
        modelBuilder.Entity<Tag>()
            .Property(t => t.Rating)
            .HasConversion(ratingConversion);
        modelBuilder.Entity<Tag>()
            .Property(t => t.Name)
            .IsRequired();
        modelBuilder.Entity<Tag>()
            .HasIndex(t => t.Name)
            .IsUnique();
        modelBuilder.Entity<Tag>()
            .Property(t => t.Aliases)
            .IsRequired()
            .HasDefaultValue(Array.Empty<string>());
        modelBuilder.Entity<Tag>()
            .HasMany(t => t.OutgoingImplications)
            .WithOne(ti => ti.ImplyingTag);
        modelBuilder.Entity<Tag>()
            .HasMany(t => t.IncomingImplications)
            .WithOne(ti => ti.ImpliedTag);
        modelBuilder.Entity<Tag>()
            .HasMany(t => t.ImpliedByThis)
            .WithMany(t => t.ImplyingThis)
            .UsingEntity<TagImplication>(
                left => left.HasOne<Tag>(e => e.ImpliedTag).WithMany(t => t.IncomingImplications),
                right => right.HasOne<Tag>(e => e.ImplyingTag).WithMany(t => t.OutgoingImplications)
            );

        modelBuilder.Entity<ThumbnailSheet>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<ThumbnailSheet>()
            .HasOne(s => s.Medium)
            .WithMany(m => m.ThumbnailSheets);

        modelBuilder.Entity<JobResult>()
            .HasKey(s => s.Id);
        modelBuilder.Entity<JobResult>()
            .Property(s => s.JobId);
        modelBuilder.Entity<JobResult>()
            .Property(s => s.JobTypeName);
        modelBuilder.Entity<JobResult>()
            .Property(s => s.ErrorText);
        modelBuilder.Entity<JobResult>()
            .Property(s => s.InsertedAt)
            .HasDefaultValueSql("now()")
            .IsRequired();
    }
}
