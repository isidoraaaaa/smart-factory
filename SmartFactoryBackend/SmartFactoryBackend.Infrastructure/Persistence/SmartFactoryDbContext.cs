using Microsoft.EntityFrameworkCore;
using SmartFactoryBackend.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFactoryBackend.Infrastructure.Persistence
{
    public class SmartFactoryDbContext : DbContext
    {
        public SmartFactoryDbContext(DbContextOptions<SmartFactoryDbContext> options)
            : base(options)
        {
        }

        public DbSet<ChocolateMachine> Machines => Set<ChocolateMachine>();
        public DbSet<TelemetryReading> Readings => Set<TelemetryReading>();
        public DbSet<User> Users => Set<User>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TelemetryReading>()
                .HasOne(r => r.Machine)
                .WithMany(m => m.Readings)
                .HasForeignKey(r => r.MachineId);
        }
    }
}
