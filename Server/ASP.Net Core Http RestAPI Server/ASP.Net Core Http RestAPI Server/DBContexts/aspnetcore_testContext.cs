using System;
using ASP.Net_Core_Http_RestAPI_Server;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace ASP.NET_Core_RestfulAPI_TestServer.DBContexts
{
    public partial class aspnetcore_testContext : DbContext
    {
        public aspnetcore_testContext()
        {
        }

        public aspnetcore_testContext(DbContextOptions<aspnetcore_testContext> options)
            : base(options)
        {
        }

        public virtual DbSet<TestLogin> TestLogins { get; set; }
        public virtual DbSet<TestTable> TestTables { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseMySql(WAS_Config.getDBConnInfo(),
                    ServerVersion.AutoDetect(WAS_Config.getDBConnInfo()), builder =>
                    {
                        builder.EnableRetryOnFailure(10);
                    });
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            modelBuilder.Entity<TestLogin>(entity =>
            {
                entity.HasKey(e => e.AccountUniqueId)
                    .HasName("PRIMARY");

                entity.ToTable("testLogin");

                entity.HasIndex(e => new { e.AccountId, e.UserName }, "Unique_UserName")
                    .IsUnique();

                entity.Property(e => e.AccountUniqueId)
                    .HasColumnType("int(11) unsigned zerofill")
                    .HasColumnName("account_unique_id")
                    .HasComment("유저 고유id");

                entity.Property(e => e.AccountId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("account_id");

                entity.Property(e => e.AccountPassword)
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnName("account_password");

                entity.Property(e => e.UserEmail)
                    .HasMaxLength(100)
                    .HasColumnName("user_email");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(25)
                    .HasColumnName("user_name");
            });

            modelBuilder.Entity<TestTable>(entity =>
            {
                entity.HasKey(e => e.UniqueId)
                    .HasName("PRIMARY");

                entity.ToTable("testTable");

                entity.Property(e => e.UniqueId)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("unique_id");

                entity.Property(e => e.Exp)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("exp")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.Level)
                    .HasColumnType("bigint(20)")
                    .HasColumnName("level")
                    .HasDefaultValueSql("'1'");

                entity.Property(e => e.TestString)
                    .HasColumnType("text")
                    .HasColumnName("testString");

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(20)
                    .HasColumnName("userName");

                entity.Property(e => e.UserPassword)
                    .IsRequired()
                    .HasMaxLength(200)
                    .HasColumnName("userPassword");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
