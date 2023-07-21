using Microsoft.EntityFrameworkCore;
using Collectify.Domain.Entities.Items.ItemComments;
using Collectify.Domain.Entities.Items.Basics;
using Collectify.Domain.Entities.Users;
using Collectify.Domain.Entities.Others;

namespace Collectify.Data.DbContexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    DbSet<User> Users { get; set; }
    DbSet<Item> Items { get; set; }
    DbSet<Photo> Photos { get; set; }
    DbSet<ItemLike> ItemLikes { get; set; }
    DbSet<ItemField> ItemFields { get; set; }
    DbSet<Collection> Collections { get; set; }
    DbSet<ItemComment> ItemComments { get; set; }
    DbSet<ItemCommentLike> ItemCommentLikes { get; set; }
    DbSet<UserToken> UserVerificationTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
}