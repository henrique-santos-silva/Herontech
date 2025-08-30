using Herontech.Contracts.Interfaces;
using Herontech.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Herontech.Infrastructure.Persistence;

public sealed class AppDbContext(
    DbContextOptions<AppDbContext> options,
    ICurrentUserService currentUserService

    ) : DbContext(options)
{
    
    public override int SaveChanges()
    {
        UpdateBaseModelInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateBaseModelInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder b)
    {
        
        b.Entity<RefreshToken>(e => {
            e.HasKey(x => x.Id);
            e.Property(x => x.TokenHash).HasMaxLength(32).IsFixedLength().IsRequired();
            e.Property(x => x.CreatedByIp).HasMaxLength(64);
            e.Property(x => x.UserAgent).HasMaxLength(256);
            e.HasIndex(x => new { x.UserId, x.ExpiresAt });
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        b.Entity<User>(e => {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).IsRequired().HasDefaultValue(RoleEnum.None);            
            e.Property(x => x.Email).HasMaxLength(50).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(32).IsFixedLength();
            e.Property(x => x.PassWordSalt).HasMaxLength(16).IsFixedLength();
        });

        b.Entity<Client>(e => {
            e.HasKey(x => x.Id);

            e.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            e.Property(x => x.Register)
                .HasMaxLength(14)
                .IsRequired();

            e.Property(x => x.Name)
                .HasMaxLength(50)
                .IsRequired();

            e.Property(x => x.LegalName)
                .HasMaxLength(100);

            e.Property(x => x.Email)
                .HasMaxLength(120);

            e.Property(x => x.Phone)
                .HasMaxLength(40);

            e.HasIndex(x => x.Register).IsUnique();

            // Se o seu provider permitir índice único com nulos, mantém.
            // Caso contrário e você precisar permitir vários NULLs, remova esta linha.
            e.HasIndex(x => x.Email).IsUnique();

            // HQ ↔ Branches: autorrelacionamento
            e.HasOne(x => x.CompanyHeadQuarters)
                .WithMany(x => x.CompanyBranches)
                .HasForeignKey(x => x.CompanyHeadQuartersId)
                .OnDelete(DeleteBehavior.Restrict);

            // Client ↔ ContactRelationship
            e.HasMany(x => x.ClientContactRelationships)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        b.Entity<Contact>(e => {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.PersonalEmail).IsUnique();
            e.Property(x => x.PersonalEmail).HasMaxLength(50);
            e.HasIndex(x => x.Register).IsUnique();
            e.Property(x => x.Register).HasMaxLength(11).IsFixedLength();
            e.HasMany(x => x.ClientContactRelationships)
                .WithOne(x => x.Contact)
                .HasForeignKey(x => x.ContactId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<ProductCategory>(e => {
            e.HasKey(x => x.Id);
            
            e
                .HasOne(x => x.ParentProductCategory)
                .WithMany(x => x.ChildProductCategories)
                .HasForeignKey(x => x.ParentProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        b.Entity<Product>(e => {
            e.HasKey(x => x.Id);
            
            e
                .HasOne(x => x.ParentProductCategory)
                .WithMany(x => x.Products)
                .HasForeignKey(x => x.ParentProductCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.MeasurementUnit)
                .WithMany()
                .HasForeignKey(x => x.MeasurementUnitId)
                .IsRequired();
        });

        b.Entity<Quote>(e => {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.QuoteNumber).IsUnique();
            
            e.HasOne(x => x.Client)
                .WithMany(x => x.Quotes)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            
            e.HasOne(x => x.Contact)
                .WithMany()
                .HasForeignKey(x => x.ContactId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        b.Entity<QuoteRevision>(e => {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Quote)
                .WithMany(x => x.QuoteRevisions)
                .HasForeignKey(x => x.QuoteId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.PaymentTerm).WithMany()
                .HasForeignKey(x => x.PaymentTermId)
                .OnDelete(DeleteBehavior.Restrict);

        });

        b.Entity<QuoteItem>(e => {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.QuoteRevision)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.QuoteRevisionId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        b.Entity<QuoteProduct>(e => {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Product)
                .WithMany()
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.QuoteItem)
                .WithMany(x => x.QuoteProducts)
                .HasForeignKey(x => x.QuoteItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        
        // RELAÇÃO CREATOR PARA TODAS AS ENTIDADES QUE HERDAM BaseModel
        foreach (IMutableEntityType et in b.Model.GetEntityTypes()
                     .Where(t => typeof(BaseEntity).IsAssignableFrom(t.ClrType) && t.ClrType != typeof(User)))
        {
            EntityTypeBuilder e = b.Entity(et.ClrType);

            e.HasOne(typeof(User), nameof(BaseEntity.Creator)).WithMany()                                
                .HasForeignKey(nameof(BaseEntity.CreatorId))
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            
            e.HasOne(typeof(User), nameof(BaseEntity.LastUpdater)).WithMany()                                
                .HasForeignKey(nameof(BaseEntity.LastUpdaterId))
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
            

        }

        b.UseSnakeCaseNames();
    }
    
    private void UpdateBaseModelInfo()
    {
        IEnumerable<EntityEntry> entries = ChangeTracker
            .Entries()
            .Where(e => e is
            {
                Entity: BaseEntity, 
                State: EntityState.Modified or EntityState.Added or EntityState.Deleted
            });

        foreach (EntityEntry entry in entries)
        {
            BaseEntity entity = (BaseEntity)entry.Entity;
            Guid? userId = currentUserService.UserId;
            
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = DateTimeOffset.UtcNow;
                if (userId is not null)
                    entity.CreatorId = userId.Value;
            }
            
            if (entry.State is EntityState.Modified or EntityState.Added )
            {
                entity.LastUpdatedAt = DateTimeOffset.UtcNow;
                if (userId is not null) entity.LastUpdaterId = userId.Value;
            }
        }
    }
    
}