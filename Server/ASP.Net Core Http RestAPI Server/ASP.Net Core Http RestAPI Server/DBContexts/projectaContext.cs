﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts
{
    public partial class projectaContext : DbContext
    {
        public projectaContext()
        {
        }

        public projectaContext(DbContextOptions<projectaContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AccountInfo> AccountInfos { get; set; }
        public virtual DbSet<UserInfo> UserInfos { get; set; }
        public virtual DbSet<UserLoginLog> UserLoginLogs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySql(WASConfig.GetDBConnectionInfo(),
                    ServerVersion.AutoDetect(WASConfig.GetDBConnectionInfo()), builder =>
                        builder.EnableRetryOnFailure(10));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("latin1")
                .UseCollation("latin1_swedish_ci");

            modelBuilder.Entity<AccountInfo>(entity =>
            {
                entity.HasKey(e => e.AccountUniqueId)
                    .HasName("PRIMARY");

                entity.ToTable("account_info");

                entity.HasIndex(e => e.AccountEmail, "account_info_account_email_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.AccountGuestToken, "account_info_account_guest_token_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.AccountOauthTokenApple, "account_info_account_oauth_token_apple_uindex")
                    .IsUnique();

                entity.HasIndex(e => e.AccountOauthTokenGoogle, "account_info_account_oauth_token_google_uindex")
                    .IsUnique();

                entity.Property(e => e.AccountUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("account_unique_id");

                entity.Property(e => e.AccountAuthLv)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("account_auth_lv");

                entity.Property(e => e.AccountBanExpire)
                    .HasColumnType("datetime")
                    .HasColumnName("account_ban_expire");

                entity.Property(e => e.AccountBanReason)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("account_ban_reason");

                entity.Property(e => e.AccountBanned)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("account_banned");

                entity.Property(e => e.AccountEmail)
                    .HasMaxLength(100)
                    .HasColumnName("account_email");

                entity.Property(e => e.AccountGuestToken)
                    .HasMaxLength(50)
                    .HasColumnName("account_guest_token");

                entity.Property(e => e.AccountOauthTokenApple)
                    .HasMaxLength(50)
                    .HasColumnName("account_oauth_token_apple");

                entity.Property(e => e.AccountOauthTokenGoogle)
                    .HasMaxLength(50)
                    .HasColumnName("account_oauth_token_google");

                entity.Property(e => e.AccountPassword)
                    .HasMaxLength(256)
                    .HasColumnName("account_password");
            });

            modelBuilder.Entity<UserInfo>(entity =>
            {
                entity.HasKey(e => e.UserUniqueId)
                    .HasName("PRIMARY");

                entity.ToTable("user_info");

                entity.HasIndex(e => e.AccountUniqueId, "user_info_account_unique_id_uindex")
                    .IsUnique();

                entity.Property(e => e.UserUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("user_unique_id");

                entity.Property(e => e.AccountUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("account_unique_id");

                entity.Property(e => e.TimestampCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.TimestampLastLogin)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp_last_login");

                entity.Property(e => e.UserExp)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("user_exp")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserLv)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("user_lv")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.UserNickname)
                    .HasMaxLength(15)
                    .HasColumnName("user_nickname");

                entity.Property(e => e.UserStamina)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("user_stamina")
                    .HasDefaultValueSql("'0'");

                entity.HasOne(d => d.AccountUnique)
                    .WithOne(p => p.UserInfo)
                    .HasForeignKey<UserInfo>(d => d.AccountUniqueId)
                    .HasConstraintName("user_info_account_info_account_unique_id_fk");
            });

            modelBuilder.Entity<UserLoginLog>(entity =>
            {
                entity.HasKey(e => e.LogUniqueId)
                    .HasName("PRIMARY");

                entity.ToTable("user_login_log");

                entity.HasIndex(e => e.AccountUniqueId, "user_login_log_account_info_account_unique_id_fk");

                entity.Property(e => e.LogUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("log_unique_id");

                entity.Property(e => e.AccountUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("account_unique_id");

                entity.Property(e => e.TimestampLastLogin)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp_last_login");

                entity.Property(e => e.UserIp)
                    .HasMaxLength(15)
                    .HasColumnName("user_ip");

                entity.Property(e => e.UserNickname)
                    .HasMaxLength(15)
                    .HasColumnName("user_nickname");

                entity.HasOne(d => d.AccountUnique)
                    .WithMany(p => p.UserLoginLogs)
                    .HasForeignKey(d => d.AccountUniqueId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("user_login_log_account_info_account_unique_id_fk");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
