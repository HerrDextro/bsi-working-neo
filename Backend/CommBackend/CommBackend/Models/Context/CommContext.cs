using CommBackend.Models.Data;
using Livekit.Server.Sdk.Dotnet;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata;

namespace CommBackend.Models.Context
{
    public class CommContext : DbContext
    {
        public CommContext(DbContextOptions<CommContext> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<TeamsCall>()
                .HasMany(e => e.Users)
                .WithOne(e => e.TeamsCall)
                .HasForeignKey(e => e.TeamsId);
        }

        public DbSet<User> Users { get; set; }
        public DbSet<TeamsCall> TeamsCalls { get; set; }

    }
}
