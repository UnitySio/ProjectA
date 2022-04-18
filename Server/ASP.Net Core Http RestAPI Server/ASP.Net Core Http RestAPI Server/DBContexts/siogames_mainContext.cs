using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace ASP.Net_Core_Http_RestAPI_Server.DBContexts
{
    public partial class siogames_mainContext : DbContext
    {
        public siogames_mainContext()
        {
        }

        public siogames_mainContext(DbContextOptions<siogames_mainContext> options)
            : base(options)
        {
        }

        public virtual DbSet<AccountInfo> AccountInfos { get; set; }
        public virtual DbSet<CharacterInfo> CharacterInfos { get; set; }
        public virtual DbSet<PlayerInfo> PlayerInfos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
                optionsBuilder.UseMySql(WAS_Config.getDBConnInfo(),
                    ServerVersion.AutoDetect(WAS_Config.getDBConnInfo()), builder => builder.EnableRetryOnFailure(10));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasCharSet("utf8")
                .UseCollation("utf8_general_ci");

            modelBuilder.Entity<AccountInfo>(entity =>
            {
                entity.HasKey(e => e.AccountUniqueId)
                    .HasName("PRIMARY");

                entity.ToTable("account_info");

                entity.HasIndex(e => e.AccountEmail, "account_email")
                    .IsUnique();

                entity.HasIndex(e => e.AccountGuestToken, "account_guest_token")
                    .IsUnique();

                entity.HasIndex(e => e.AccountOauthTokenApple, "account_oauth_token_apple")
                    .IsUnique();

                entity.HasIndex(e => e.AccountOauthTokenGoogle, "account_oauth_token_google")
                    .IsUnique();

                entity.Property(e => e.AccountUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("account_unique_id");

                entity.Property(e => e.AccountAuthLv)
                    .HasColumnType("tinyint(3) unsigned")
                    .HasColumnName("account_authLv");

                entity.Property(e => e.AccountBanExpire)
                    .HasColumnType("datetime")
                    .HasColumnName("account_ban_expire");

                entity.Property(e => e.AccountBanReason)
                    .HasColumnType("tinyint(1) unsigned")
                    .HasColumnName("account_ban_reason")
                    .HasDefaultValueSql("'0'");

                entity.Property(e => e.AccountBanned)
                    .HasColumnType("tinyint(1) unsigned")
                    .HasColumnName("account_banned")
                    .HasDefaultValueSql("'0'");

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

            modelBuilder.Entity<CharacterInfo>(entity =>
            {
                entity.HasKey(e => e.CharacterUniqueId)
                    .HasName("PRIMARY");

                entity.ToTable("character_info");

                entity.HasIndex(e => e.AccountUniqueId, "account_unique_id_character");

                entity.Property(e => e.CharacterUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("character_unique_id");

                entity.Property(e => e.AccountUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("account_unique_id");

                entity.Property(e => e.CharacterAtk)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("character_atk");

                entity.Property(e => e.CharacterDef)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("character_def");

                entity.Property(e => e.CharacterExp)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("character_exp");

                entity.Property(e => e.CharacterHp)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("character_hp");

                entity.Property(e => e.CharacterLv)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("character_Lv");

                entity.Property(e => e.CharacterName)
                    .HasMaxLength(15)
                    .HasColumnName("character_name");

                entity.Property(e => e.CharacterSpecStat)
                    .HasMaxLength(50)
                    .HasColumnName("character_spec_stat");

                entity.HasOne(d => d.AccountUnique)
                    .WithMany(p => p.CharacterInfos)
                    .HasForeignKey(d => d.AccountUniqueId)
                    .HasConstraintName("account_unique_id_character");
            });

            modelBuilder.Entity<PlayerInfo>(entity =>
            {
                entity.HasKey(e => e.PlayerUniqueId)
                    .HasName("PRIMARY");

                entity.ToTable("player_info");

                entity.HasIndex(e => e.AccountUniqueId, "account_unique_id")
                    .IsUnique();

                entity.Property(e => e.PlayerUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("player_unique_id");

                entity.Property(e => e.AccountUniqueId)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("account_unique_id");

                entity.Property(e => e.PlayerBirthdate)
                    .HasColumnType("date")
                    .HasColumnName("player_birthdate");

                entity.Property(e => e.PlayerCashPoint)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("player_cash_point");

                entity.Property(e => e.PlayerExp)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("player_exp");

                entity.Property(e => e.PlayerGender)
                    .HasMaxLength(6)
                    .HasColumnName("player_gender");

                entity.Property(e => e.PlayerIngamePoint)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("player_ingame_point");

                entity.Property(e => e.PlayerLv)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("player_Lv");

                entity.Property(e => e.PlayerNickname)
                    .HasMaxLength(15)
                    .HasColumnName("player_nickname");

                entity.Property(e => e.PlayerProfileImageUrl)
                    .HasMaxLength(256)
                    .HasColumnName("player_profile_image_url");

                entity.Property(e => e.PlayerStamina)
                    .HasColumnType("int(10) unsigned")
                    .HasColumnName("player_stamina");

                entity.Property(e => e.TimestampCreated)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp_created")
                    .HasDefaultValueSql("current_timestamp()");

                entity.Property(e => e.TimestampLastSignin)
                    .HasColumnType("datetime")
                    .HasColumnName("timestamp_last_signin");

                entity.HasOne(d => d.AccountUnique)
                    .WithOne(p => p.PlayerInfo)
                    .HasForeignKey<PlayerInfo>(d => d.AccountUniqueId)
                    .HasConstraintName("account_unique_id_player");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
