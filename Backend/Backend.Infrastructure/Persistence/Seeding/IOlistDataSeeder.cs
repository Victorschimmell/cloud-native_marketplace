namespace Backend.Infrastructure.Persistence.Seeding;

public interface IOlistDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken = default);
}
