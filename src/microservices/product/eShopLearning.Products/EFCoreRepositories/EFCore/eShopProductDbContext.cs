using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using eShopLearning.Products.EFCoreRepositories.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using eShopLearning.Products.EFCoreRepositories.Base;
using System.Threading;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace eShopLearning.Products.EFCoreRepositories.EFCore
{
    public class eShopProductDbContext : DbContext
    {
        public eShopProductDbContext(DbContextOptions<eShopProductDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {           
            base.OnModelCreating(modelBuilder);        
        }

        public override int SaveChanges()
        {
            // get entries that are being Added or Updated
            var modifiedEntries = ChangeTracker.Entries()
                    .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entry in modifiedEntries)
                SetTimeArgs(entry);

            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // get entries that are being Added or Updated
            var modifiedEntries = ChangeTracker.Entries()
                    .Where(x => (x.State == EntityState.Added || x.State == EntityState.Modified));

            foreach (var entry in modifiedEntries)
                SetTimeArgs(entry);

            return base.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// 对 addtime 和 updatetime 进行赋值
        /// </summary>
        /// <param name="entry"></param>
        private void SetTimeArgs(EntityEntry entry)
        {
            var type = entry.Entity.GetType();

            if (entry.State == EntityState.Added)
            {
                var addTimeProp = type.GetProperty(nameof(Entity<dynamic>.AddTime));
                if (addTimeProp != null)
                    addTimeProp.SetValue(entry.Entity, DateTime.Now);
            }

            var updateTimeProp = type.GetProperty(nameof(Entity<dynamic>.UpdateTime));
            if (updateTimeProp != null)
                updateTimeProp.SetValue(entry.Entity, DateTime.Now);
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Spu> Spus { get; set; }
        public DbSet<Sku> Skus { get; set; }
        public DbSet<SkuAttr> SkuAttrs { get; set; }
    }
}
