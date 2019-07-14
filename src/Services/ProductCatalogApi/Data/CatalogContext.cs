using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductCatalogApi.Domain;

namespace ProductCatalogApi.Data
{
    public class CatalogContext : DbContext
    {
        public CatalogContext(DbContextOptions options)
            :base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CatalogBrand>(builder =>
            {
                builder.ToTable("CatalogBrand");
                builder.Property(b => b.Id)
                    .ForSqlServerUseSequenceHiLo("catalog_brand_hilo")
                    .IsRequired();
                builder.Property(b => b.Brand)
                    .IsRequired()
                    .HasMaxLength(100);
            });
            modelBuilder.Entity<CatalogType>(builder =>
            {
                builder.ToTable("CatalogType");
                builder.Property(b => b.Id)
                    .ForSqlServerUseSequenceHiLo("catalog_type_hilo")
                    .IsRequired();
                builder.Property(b => b.Type)
                    .IsRequired()
                    .HasMaxLength(100);
            }); 
            modelBuilder.Entity<CatalogItem>(builder =>
            {
                builder.ToTable("Catalog");
                builder
                    .Property(ci => ci.Id).ForSqlServerUseSequenceHiLo("catalog_hilo")
                    .IsRequired(true);
                builder.Property(ci => ci.Name)
                    .IsRequired(true)
                    .HasMaxLength(50);
                builder.Property(ci => ci.Price)
                    .IsRequired(true);
                builder.Property(ci => ci.PictureUrl)
                    .IsRequired(false);
                builder
                    .HasOne(c => c.CatalogBrand)
                    .WithMany()
                    .HasForeignKey(c => c.CatalogBrandId);
                builder
                    .HasOne(c => c.CatalogType)
                    .WithMany()
                    .HasForeignKey(c => c.CatalogTypeId);
            });
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<CatalogType> CatalogTypes { get; set; }

        public DbSet<CatalogBrand> CatalogBrands { get; set; }

        public DbSet<CatalogItem> CatalogItems { get; set; }
    }
}