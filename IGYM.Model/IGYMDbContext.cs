using IGYM.Model.UserModule.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace IGYM.Model
{
	public class IGYMDbContext : DbContext
	{
		private readonly IConfiguration _configuration;

		// Parameterless constructor is typically used for design-time operations like migrations
		public IGYMDbContext()
		{
		}

		// Corrected the class name in the constructor (was SriKanthContext)
		public IGYMDbContext(DbContextOptions<IGYMDbContext> options, IConfiguration configuration)
			: base(options)
		{
			_configuration = configuration;
		}

		public virtual DbSet<User> Users { get; set; }
		public virtual DbSet<UserRole> UserRole { get; set; }
		public virtual DbSet<LoginTrack> LoginTrack { get; set; }
		public virtual DbSet<UserToken> UserToken { get; set; }
		public virtual DbSet<SendToken> SendToken { get; set; }
		public virtual DbSet<Message> Message { get; set; }
		public virtual DbSet<SentNotification> SentNotification { get; set; }
		public virtual DbSet<UserHistory> UserHistory { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.Entity<UserRole>(entity =>
			{
				entity.HasKey(e => e.UserRoleID);
				entity.Property(e => e.UserRoleName);
				entity.Property(e => e.Description);
				entity.Property(e => e.IsActive);

			});
			modelBuilder.Entity<User>(entity =>
			{
				entity.HasKey(e => e.UserID);
				entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
				entity.Property(e => e.PasswordHash).HasMaxLength(255);
				entity.Property(e => e.UserRoleId);
				entity.Property(e => e.Email).HasMaxLength(255).IsRequired();
				entity.Property(e => e.PhoneNumber).HasMaxLength(50);
				entity.Property(e => e.IsPhoneNumberVerified);
				entity.Property(e => e.IsEmailVerified);
				entity.Property(e => e.IsActive).IsRequired();
				entity.Property(e => e.IsLocked).IsRequired();
				entity.Property(e => e.RememberMe);
				entity.Property(e => e.FailedLoginCount);
				entity.Property(e => e.CreatedAt).IsRequired();
				entity.Property(e => e.LastLoginAt);


				entity.HasOne<UserRole>()
					  .WithMany()
					  .HasForeignKey(e => e.UserRoleId) // Fixed typo: UserRoleId
					  .OnDelete(DeleteBehavior.NoAction);

			});

			modelBuilder.Entity<LoginTrack>(entity =>
			{
				entity.HasKey(e => e.LoginTrackID);
				entity.Property(e => e.UserID);
				entity.Property(e => e.UserID);
				entity.Property(e => e.LoginMethod).HasMaxLength(50).IsRequired();
				entity.Property(e => e.LoginTime).IsRequired();
				entity.Property(e => e.IPAddress).HasMaxLength(50);
				entity.Property(e => e.DeviceType).HasMaxLength(50);
				entity.Property(e => e.OperatingSystem).HasMaxLength(50);
				entity.Property(e => e.Browser).HasMaxLength(100);
				entity.Property(e => e.Country).HasMaxLength(50);
				entity.Property(e => e.City).HasMaxLength(50);
				entity.Property(e => e.IsSuccessful).IsRequired();
				entity.Property(e => e.MFAUsed);
				entity.Property(e => e.MFAMethod).HasMaxLength(50);
				entity.Property(e => e.SessionID).HasMaxLength(255);
				entity.Property(e => e.FailureReason).HasMaxLength(255);


				entity.HasOne<User>()
				  .WithMany()
				  .HasForeignKey(e => e.UserID)
				  .OnDelete(DeleteBehavior.Cascade);

			});
			modelBuilder.Entity<UserToken>(entity =>
			{
				entity.HasKey(e => e.TokenID);
				entity.Property(e => e.UserID);
				entity.Property(e => e.Token).HasMaxLength(1000).IsRequired();
				entity.Property(e => e.TokenType).HasMaxLength(50).IsRequired();
				entity.Property(e => e.CreatedAt);
				entity.Property(e => e.ExpiresAt);
				entity.Property(e => e.IsUsed);
				entity.Property(e => e.IsRevoked);
				entity.Property(e => e.Purpose).HasMaxLength(255);
				entity.Property(e => e.LastUsedAt);

				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserID) // Fixed typo: UserRoleId
					  .OnDelete(DeleteBehavior.NoAction);

			});
			modelBuilder.Entity<SendToken>(entity =>
			{
				entity.HasKey(e => e.SendTokenID);
				entity.Property(e => e.UserID);
				entity.Property(e => e.MFADeviceID).IsRequired();
				entity.Property(e => e.UserTokenID).IsRequired();
				entity.Property(e => e.SendAt);
				entity.Property(e => e.SendSuccessful);

				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserID) // Fixed typo: UserRoleId
					  .OnDelete(DeleteBehavior.NoAction);

			});
			modelBuilder.Entity<Message>(entity =>
			{
				entity.HasKey(e => e.MessageId);
				entity.Property(e => e.MessageName).IsRequired();
				entity.Property(e => e.MessageBody).IsRequired();

			});
			modelBuilder.Entity<SentNotification>(entity =>
			{
				entity.HasKey(e => e.NotificationId);
				entity.Property(e => e.Recipient);
				entity.Property(e => e.NotificationType).IsRequired();
				entity.Property(e => e.Subject).IsRequired();
				entity.Property(e => e.Message);
				entity.Property(e => e.SentAt);
				entity.Property(e => e.IsSuccess);

			});
			modelBuilder.Entity<UserHistory>(entity =>
			{
				entity.HasKey(e => e.UserHistoryId);
				entity.Property(e => e.UserId).IsRequired();
				entity.Property(e => e.ActionType).IsRequired();
				entity.Property(e => e.EntityType);
				entity.Property(e => e.Endpoint);
				entity.Property(e => e.Timestamp).IsRequired();
				entity.Property(e => e.IPAddress);

				entity.HasOne<User>()
					  .WithMany()
					  .HasForeignKey(e => e.UserId)
					  .OnDelete(DeleteBehavior.NoAction);


			});
		}
	}
}
