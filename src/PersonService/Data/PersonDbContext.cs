using Microsoft.EntityFrameworkCore;
using PersonService.Domain;

namespace PersonService.Data;

public class PersonDbContext(DbContextOptions<PersonDbContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("persons");
            entity.HasKey(person => person.Id);

            entity.Property(person => person.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(person => person.Name)
                .HasColumnName("name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(person => person.Age)
                .HasColumnName("age");

            entity.Property(person => person.Address)
                .HasColumnName("address")
                .HasMaxLength(255);

            entity.Property(person => person.Work)
                .HasColumnName("work")
                .HasMaxLength(255);
        });
    }
}
