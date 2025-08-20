using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CMetalsWS.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<Machine> Machines => Set<Machine>();
        public DbSet<Truck> Trucks => Set<Truck>();
        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
        public DbSet<WorkOrderItem> WorkOrderItems => Set<WorkOrderItem>();
        public DbSet<PickingList> PickingLists => Set<PickingList>();
        public DbSet<PickingListItem> PickingListItems => Set<PickingListItem>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Example constraints and relationships
            builder.Entity<Branch>()
                .HasIndex(b => b.Code)
                .IsUnique();

            builder.Entity<Machine>()
                .HasIndex(m => m.Code)
                .IsUnique();

            builder.Entity<PickingList>()
                .HasIndex(p => p.PickingListNumber)
                .IsUnique();

            builder.Entity<WorkOrder>()
                .HasIndex(w => w.WorkOrderNumber)
                .IsUnique();

            // Define one‑to‑many relationships
            builder.Entity<Branch>()
                .HasMany(b => b.Machines)
                .WithOne(m => m.Branch!)
                .HasForeignKey(m => m.BranchId);

            builder.Entity<Branch>()
                .HasMany(b => b.Trucks)
                .WithOne(t => t.Branch!)
                .HasForeignKey(t => t.BranchId);

            builder.Entity<Branch>()
                .HasMany(b => b.WorkOrders)
                .WithOne(w => w.Branch!)
                .HasForeignKey(w => w.BranchId);

            builder.Entity<Branch>()
                .HasMany(b => b.PickingLists)
                .WithOne(p => p.Branch!)
                .HasForeignKey(p => p.BranchId);

            builder.Entity<WorkOrder>()
                .HasMany(w => w.Items)
                .WithOne(i => i.WorkOrder!)
                .HasForeignKey(i => i.WorkOrderId);

            builder.Entity<PickingList>()
                .HasMany(p => p.Items)
                .WithOne(i => i.PickingList!)
                .HasForeignKey(i => i.PickingListId);
            builder.Entity<PickingListItem>()
                .Property(p => p.Quantity)
                .HasColumnType("decimal(18,2)");
            builder.Entity<WorkOrderItem>()
                .Property(w => w.Quantity)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Truck>()
                .Property(t => t.CapacityWeight)
                .HasColumnType("decimal(18,2)");
            builder.Entity<Truck>()
                .Property(t => t.CapacityVolume)
                .HasColumnType("decimal(18,2)");
        }
    }
}
