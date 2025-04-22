using System.Collections.Generic;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudyJet.API.Data.Entities;

namespace StudyJet.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Custom DbSets 
        public DbSet<Category> Categories { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Wishlist> Wishlists { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<UserPurchaseCourse> UserPurchaseCourse { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<CourseUpdate> CourseUpdates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Identity mode
            base.OnModelCreating(modelBuilder);

            // User table
            modelBuilder.Entity<User>()
              .HasMany(u => u.CoursesTaught)
              .WithOne(c => c.Instructor)
              .HasForeignKey(c => c.InstructorID)
              .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.CoursesEnrolled)
                .WithMany(c => c.EnrolledStudents)
                .UsingEntity<Dictionary<string, object>>(
                     "UserCourse", // Name of the join table
                         j => j.HasOne<Course>().WithMany().HasForeignKey("CourseId"),
                         j => j.HasOne<User>().WithMany().HasForeignKey("UserId")
            );

            modelBuilder.Entity<User>()
                .HasMany(u => u.Wishlists)
                .WithOne(w => w.User)
                .HasForeignKey(w => w.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Carts)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasMany(u => u.Transactions)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserID)
                .OnDelete(DeleteBehavior.Cascade);


            // Category table
            modelBuilder.Entity<Category>()
                .HasIndex(u => u.Name)
                .IsUnique();

            // Course table relationships
            modelBuilder.Entity<Course>()
                .HasOne(c => c.Instructor)
                .WithMany(i => i.CoursesTaught)
                .HasForeignKey(c => c.InstructorID);

            modelBuilder.Entity<Course>()
                .Property(c => c.Status)
                .HasConversion<string>();


            // Wishlist table
            modelBuilder.Entity<Wishlist>()
                .HasIndex(w => new { w.UserID, w.CourseID })
                .IsUnique();

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserID);

            modelBuilder.Entity<Wishlist>()
                .HasOne(w => w.Course)
                .WithMany(c => c.Wishlists)
                .HasForeignKey(w => w.CourseID);


            // Cart table
            modelBuilder.Entity<Cart>()
                .HasIndex(c => new { c.UserID, c.CourseID })
                .IsUnique();

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserID);

            modelBuilder.Entity<Cart>()
                .HasOne(c => c.Course)
                .WithMany(c => c.Carts)
                .HasForeignKey(c => c.CourseID);


            // Transaction table
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserID);


            // UserPurchaseCourse table
            modelBuilder.Entity<UserPurchaseCourse>()
                .HasKey(upc => upc.ID);

            modelBuilder.Entity<UserPurchaseCourse>()
                .HasIndex(upc => new { upc.UserID, upc.CourseID })
                .IsUnique();


            modelBuilder.Entity<UserPurchaseCourse>()
                .HasOne(upc => upc.User)
                .WithMany(u => u.UserCoursePurchases)
                .HasForeignKey(upc => upc.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPurchaseCourse>()
                .HasOne(upc => upc.Course)
                .WithMany(c => c.UserPurchaseCourses)
                .HasForeignKey(upc => upc.CourseID)
                .OnDelete(DeleteBehavior.Restrict);


            // Notification
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserID);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Course)
                .WithMany()
                .HasForeignKey(n => n.CourseID);

        }



    }
}
