using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCleanTemplate.Domain.Test;
using MyCleanTemplate.Domain.ValueObjects;

namespace MyCleanTemplate.Infrastructure.Persistence.Configurations
{
    public class TestEntityConfiguration : IEntityTypeConfiguration<TestEntity>
    {
        public void Configure(EntityTypeBuilder<TestEntity> builder)
        {
            builder.Property(t => t.Name)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(a => a.TestValueObjectJson)
                .HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions)null), 
                    y => JsonSerializer.Deserialize<TestValueObject>(y, (JsonSerializerOptions)null));

            builder.OwnsOne(t => t.TestValueObject, sb =>
            {
                sb.Property(x => x.Country).HasMaxLength(100);
                sb.Property(x => x.State).HasMaxLength(100);
                sb.Property(x => x.City).HasMaxLength(100);
                sb.Property(x => x.Street).HasMaxLength(100);
                sb.Property(x => x.ZipCode).HasMaxLength(100);
            });


            builder.Property(a => a.TestValueObjectList)
                .HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions)null),
                    y => JsonSerializer.Deserialize<List<TestValueObject>>(y, (JsonSerializerOptions)null));
        }
    }
}
