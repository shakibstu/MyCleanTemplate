using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCleanTemplate.Domain.Test;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace MyCleanTemplate.Infrastructure.Persistence.Configurations;

internal class TestEntityConfiguration : IEntityTypeConfiguration<TestEntity>
{
    public void Configure(EntityTypeBuilder<TestEntity> builder)
    {
        builder.Property(t => t.Name)
            .HasMaxLength(50)
            .IsRequired();

        // These columns are nullable in the existing schema. Domain was compiled without nullable
        // reference types when the migration was created, so keep them optional explicitly.
        builder.Property(a => a.TestValueObjectJson)
            .HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions)null),
                y => JsonSerializer.Deserialize<TestValueObject>(y, (JsonSerializerOptions)null))
            .IsRequired(false);

        builder.OwnsOne(t => t.TestValueObject, sb =>
        {
            sb.Property(x => x.Country).HasMaxLength(100).IsRequired(false);
            sb.Property(x => x.State).HasMaxLength(100).IsRequired(false);
            sb.Property(x => x.City).HasMaxLength(100).IsRequired(false);
            sb.Property(x => x.Street).HasMaxLength(100).IsRequired(false);
            sb.Property(x => x.ZipCode).HasMaxLength(100).IsRequired(false);
        });


        builder.Property(a => a.TestValueObjectList)
            .HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions)null),
                y => JsonSerializer.Deserialize<List<TestValueObject>>(y, (JsonSerializerOptions)null))
            .IsRequired(false);
    }
}