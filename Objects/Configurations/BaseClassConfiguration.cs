using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Vanigam.CRM.Objects.Contracts;

namespace Vanigam.CRM.Objects.Configurations
{
    public abstract class BaseClassConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseClass
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            //builder.HasKey(e => e.Id);
            ConfigureBaseProperties(builder);
        }

        protected virtual void ConfigureBaseProperties(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property(e => e.CreatedByUserId).HasMaxLength(50);

            //builder.Property(e => e.CreatedAtUtc).HasColumnType("timestamp with time zone").HasConversion(v => v.HasValue ? v.Value.ToUniversalTime() : (DateTimeOffset?)null, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : DateTime.MinValue);

            builder.Property(e => e.UpdatedByUserId).HasMaxLength(50);

            //builder.Property(e => e.UpdatedAtUtc).HasColumnType("timestamp with time zone").HasConversion(v => v.HasValue ? v.Value.ToUniversalTime() : (DateTimeOffset?)null, v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : DateTime.MinValue);

            builder.Property(e => e.IsNotDeleted).HasDefaultValue(true);
        }
    }
}

