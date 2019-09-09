using NinjaDomain.Classes;
using NinjaDomain.Classes.Interfaces;
using System;
using System.Data.Entity;
using System.Linq;

namespace NinjaDomain.DataModel
{
    public class NinjaContext : DbContext
    {
        public DbSet<Ninja> Ninjas { get; set; }
        public DbSet<Clan> Clans { get; set; }
        public DbSet<NinjaEquipment> Equipment { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // When creating a migration, adding this line will tell the
            // EF to not update by adding a column with IsDirty.
            // This is a field that we want only on the client side
            // and dont want to persist it in the DB.
            modelBuilder.Types().Configure(c => c.Ignore("IsDirty"));
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            // Give me everything its tracking that implements 
            // IModificationHistory and is known to be modified or added.
            foreach (IModificationHistory history in ChangeTracker.Entries()
                .Where(e => e.Entity is IModificationHistory && (e.State == EntityState.Added ||
                e.State == EntityState.Modified))
                .Select(e => e.Entity as IModificationHistory))
            {
                history.DateModified = DateTime.Now;
                if (history.DateCreated == DateTime.MinValue)
                {
                    history.DateCreated = DateTime.Now;
                }
            }

            int result = base.SaveChanges();

            foreach (IModificationHistory history in ChangeTracker.Entries()
                .Where(e => e.Entity is IModificationHistory)
                .Select(e => e.Entity as IModificationHistory))
            {
                history.IsDirty = false;
            }
            return result;
        }
    }
}
