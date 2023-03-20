using Microsoft.EntityFrameworkCore;

namespace WebBlog.Data;

// Inherit DbContext to accessing and manipulating databases
public partial class WebBlogContext : DbContext
{
    public WebBlogContext()
    {
    }

    public WebBlogContext(DbContextOptions<WebBlogContext> options)
        : base(options)
    {
    }

    // Set Model that connect with specify Table in Database
    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Using FluentAPI to specify model configuration in Entity Framework
        // This FluentAPI is Auto Generate by Scaffold-DbContext 
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.BlogId).HasName("PK__Blogs__54379E306B43644E");

            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.User).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Blogs__UserId__2B3F6F97")
                .IsRequired(false);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CEA4FA60F");

            entity.Property(e => e.Password).HasMaxLength(50);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
