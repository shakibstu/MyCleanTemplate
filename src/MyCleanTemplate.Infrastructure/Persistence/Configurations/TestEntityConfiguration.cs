using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyCleanTemplate.Domain.Test;
using Newtonsoft.Json;
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

        builder.Property(a => a.TestValueObjectJson)
            .HasConversion(x => JsonSerializer.Serialize(x, (JsonSerializerOptions)null),
                y => JsonConvert.DeserializeObject<TestValueObject>(y));

        builder.OwnsOne(t => t.TestValueObject, sb =>
        {
            sb.Property(x => x.Country).HasMaxLength(100);
            sb.Property(x => x.State).HasMaxLength(100);
            sb.Property(x => x.City).HasMaxLength(100);
            sb.Property(x => x.Street).HasMaxLength(100);
            sb.Property(x => x.ZipCode).HasMaxLength(100);
        });


        builder.Property(a => a.TestValueObjectList)
            .HasConversion(x => JsonConvert.SerializeObject(x),
                y => JsonConvert.DeserializeObject<List<TestValueObject>>(y));
    }
}