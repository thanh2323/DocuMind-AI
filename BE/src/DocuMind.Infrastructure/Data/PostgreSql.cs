using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuMind.Core.Entities;
using DocuMind.Core.Enum;
using Microsoft.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Data
{
    public class PostgreSql : DbContext
    {
        public PostgreSql(DbContextOptions<PostgreSql> options)
       : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<ChatSession> ChatSessions { get; set; }
        public DbSet<SessionDocument> SessionDocuments { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===================================================================
            // USER CONFIGURATION
            // ===================================================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FullName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.HasIndex(e => e.Email)
                    .IsUnique();

                entity.Property(e => e.PasswordHash)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.Role)
                     .IsRequired()
                     .HasMaxLength(50)
                     .HasDefaultValue("User");


                entity.Property(e => e.IsLocked)
                    .HasDefaultValue(false);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Changed from GETUTCDATE() to CURRENT_TIMESTAMP for Postgres

                // Relationships
                entity.HasMany(e => e.Documents)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.ChatSessions)
                    .WithOne(e => e.User)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================================================================
            // DOCUMENT CONFIGURATION
            // ===================================================================
            modelBuilder.Entity<Document>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.FileName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.FilePath)
                    .IsRequired()
                    .HasMaxLength(1000);

                entity.Property(e => e.Summary)
                    .HasMaxLength(2000);

                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasConversion<int>()
                    .HasDefaultValue(DocumentStatus.Pending);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Changed from GETUTCDATE()

                // Indexes for performance
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.CreatedAt);
            });

            // ===================================================================
            // CHAT SESSION CONFIGURATION
            // ===================================================================
            modelBuilder.Entity<ChatSession>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Changed from GETUTCDATE()

                entity.Property(e => e.LastActiveAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Changed from GETUTCDATE()

                // Indexes
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.LastActiveAt);

                // Relationships
                entity.HasMany(e => e.Messages)
                    .WithOne(e => e.Session)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ===================================================================
            // SESSION DOCUMENT CONFIGURATION (Many-to-Many Junction Table)
            // ===================================================================
            modelBuilder.Entity<SessionDocument>(entity =>
            {
                // Composite Primary Key
                entity.HasKey(e => new { e.SessionId, e.DocumentId });

                entity.Property(e => e.AddedAt)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Changed from GETUTCDATE()

                // Relationships
                entity.HasOne(e => e.Session)
                    .WithMany(e => e.SessionDocuments)
                    .HasForeignKey(e => e.SessionId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Document)
                    .WithMany(e => e.SessionDocuments)
                    .HasForeignKey(e => e.DocumentId)
                    .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete conflicts
            });

            // ===================================================================
            // CHAT MESSAGE CONFIGURATION
            // ===================================================================
            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Content)
                    .IsRequired();

                entity.Property(e => e.IsUser)
                    .IsRequired();

                entity.Property(e => e.Timestamp)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Changed from GETUTCDATE()

                // Indexes
                entity.HasIndex(e => e.SessionId);
                entity.HasIndex(e => e.Timestamp);
            });

        }
    }
}
