using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TwiceTasks.Models;

namespace TwiceTasks.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Workspace> Workspaces { get; set; }
        public DbSet<Page> Pages { get; set; }
        public DbSet<FileResource> FileResources { get; set; }




        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relación User -> Workspaces (1:N)
            builder.Entity<Workspace>()
                .HasOne(w => w.User)
                .WithMany(u => u.Workspaces)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación Workspace -> Pages (1:N)
            builder.Entity<Page>()
                .HasOne(p => p.Workspace)
                .WithMany(w => w.Pages)
                .HasForeignKey(p => p.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);
            // Relación FileResource -> User (N:1)
            builder.Entity<FileResource>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserId);

        }
    }
}
