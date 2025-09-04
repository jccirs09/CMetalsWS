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
        public DbSet<Load> Loads => Set<Load>();
        public DbSet<PickingList> PickingLists => Set<PickingList>();
        public DbSet<PickingListItem> PickingListItems => Set<PickingListItem>();
        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
        public DbSet<WorkOrderItem> WorkOrderItems => Set<WorkOrderItem>();
        public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
        public DbSet<ItemRelationship> ItemRelationships => Set<ItemRelationship>();
        public DbSet<LoadItem> LoadItems => Set<LoadItem>();
        public DbSet<TruckRoute> TruckRoutes => Set<TruckRoute>();
        public DbSet<Customer> Customers => Set<Customer>();
        public DbSet<TruckRouteStop> TruckRouteStops => Set<TruckRouteStop>();
        public DbSet<TaskAuditEvent> TaskAuditEvents => Set<TaskAuditEvent>();
        public DbSet<TransferItem> TransferItems => Set<TransferItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Machine -> Branch
            modelBuilder.Entity<Machine>()
                .HasOne(m => m.Branch)
                .WithMany(b => b.Machines)
                .HasForeignKey(m => m.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Machine>()
               .Property(m => m.Category)
               .HasConversion<string>()
               .HasMaxLength(32);

            // Truck -> Branch
            modelBuilder.Entity<Truck>()
                .HasOne(t => t.Branch)
                .WithMany(b => b.Trucks)
                .HasForeignKey(t => t.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customer (map to existing table name and columns/indexes)
            modelBuilder.Entity<Customer>(entity =>
            {
                entity.ToTable("Customer"); // important: fixes “Invalid object name 'Customers'”
                entity.HasIndex(c => c.CustomerCode).IsUnique();
                entity.Property(c => c.CustomerCode).HasMaxLength(16).IsRequired();
                entity.Property(c => c.CustomerName).HasMaxLength(200).IsRequired();
                entity.Property(c => c.LocationCode).HasMaxLength(32);
                entity.Property(c => c.Address).HasMaxLength(256);
                entity.HasIndex(c => c.LocationCode);
            });

            // TruckRoute + stops
            modelBuilder.Entity<TruckRoute>(e =>
            {
                e.Property(r => r.RegionCode).HasMaxLength(32).IsRequired();
                e.HasOne(r => r.Truck).WithMany().HasForeignKey(r => r.TruckId).OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(r => new { r.BranchId, r.RouteDate, r.RegionCode });
            });

            modelBuilder.Entity<TruckRouteStop>(e =>
            {
                e.HasOne(s => s.Route).WithMany(r => r.Stops).HasForeignKey(s => s.RouteId).OnDelete(DeleteBehavior.Cascade);
                e.HasOne(s => s.Load).WithMany().HasForeignKey(s => s.LoadId).OnDelete(DeleteBehavior.Restrict);
                e.HasIndex(s => new { s.RouteId, s.StopOrder }).IsUnique();
            });

            // Truck -> Driver (AspNetUsers)
            modelBuilder.Entity<Truck>()
                .HasOne(t => t.Driver)
                .WithMany()
                .HasForeignKey(t => t.DriverId)
                .OnDelete(DeleteBehavior.SetNull);

            // PickingList unique SO number
            modelBuilder.Entity<PickingList>()
                .HasIndex(p => p.SalesOrderNumber)
                .IsUnique();

            // PickingList -> Branch
            modelBuilder.Entity<PickingList>()
                .HasOne(p => p.Branch)
                .WithMany(b => b.PickingLists)
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // PickingList -> Truck
            modelBuilder.Entity<PickingList>()
                .HasOne(p => p.Truck)
                .WithMany()
                .HasForeignKey(p => p.TruckId)
                .OnDelete(DeleteBehavior.SetNull);

            // Load relationships (NEW)
            modelBuilder.Entity<Load>(e =>
            {
                e.HasMany(l => l.Items)
                    .WithOne(i => i.Load)
                    .HasForeignKey(i => i.LoadId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(l => l.OriginBranch)
                    .WithMany()
                    .HasForeignKey(l => l.OriginBranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(l => l.DestinationBranch)
                    .WithMany()
                    .HasForeignKey(l => l.DestinationBranchId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // LoadItem relationships (NEW)
            modelBuilder.Entity<LoadItem>(e =>
            {
                e.HasOne(i => i.PickingList)
                    .WithMany()
                    .HasForeignKey(i => i.PickingListId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(li => li.TransferItem)
                    .WithMany()
                    .HasForeignKey(li => li.TransferItemId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Date types
            modelBuilder.Entity<PickingList>()
                .Property(p => p.OrderDate)
                .HasColumnType("datetime2");
            modelBuilder.Entity<PickingList>()
                .Property(p => p.ShipDate)
                .HasColumnType("datetime2");

            // PickingListItem -> PickingList
            modelBuilder.Entity<PickingListItem>()
                .HasOne(i => i.PickingList)
                .WithMany(p => p.Items)
                .HasForeignKey(i => i.PickingListId)
                .OnDelete(DeleteBehavior.Cascade);

            // PickingListItem -> Machine
            modelBuilder.Entity<PickingListItem>()
                .HasOne(i => i.Machine)
                .WithMany()
                .HasForeignKey(i => i.MachineId)
                .OnDelete(DeleteBehavior.SetNull);

            // PickingListItem status enum as int with default
            modelBuilder.Entity<PickingListItem>()
                .Property(i => i.Status)
                .HasConversion<int>()
                .HasDefaultValue(PickingLineStatus.Pending);

            // ItemRelationship
            modelBuilder.Entity<ItemRelationship>(e =>
            {
                e.ToTable("ItemRelationship");
                e.Property(x => x.ParentItemId).HasMaxLength(128).IsRequired();
                e.Property(x => x.ChildItemId).HasMaxLength(128).IsRequired();
                e.Property(x => x.Relation).HasMaxLength(32).HasDefaultValue("CoilToSheet");
                e.HasIndex(x => new { x.ParentItemId, x.ChildItemId, x.Relation }).IsUnique();
            });

            // Numeric precisions (fix EF warnings)
            modelBuilder.Entity<PickingListItem>().Property(i => i.Quantity).HasPrecision(18, 3);
            modelBuilder.Entity<PickingListItem>().Property(i => i.Width).HasPrecision(18, 3);
            modelBuilder.Entity<PickingListItem>().Property(i => i.Length).HasPrecision(18, 3);
            modelBuilder.Entity<PickingListItem>().Property(i => i.Weight).HasPrecision(18, 3);
            modelBuilder.Entity<PickingList>().Property(pl => pl.TotalWeight).HasPrecision(18, 3);
            modelBuilder.Entity<PickingList>().Property(pl => pl.RemainingWeight).HasPrecision(18, 3);

            modelBuilder.Entity<Truck>().Property(t => t.CapacityWeight).HasPrecision(18, 2);
            modelBuilder.Entity<Truck>().Property(t => t.CapacityVolume).HasPrecision(18, 2);

            modelBuilder.Entity<InventoryItem>().Property(p => p.Width).HasPrecision(18, 3);
            modelBuilder.Entity<InventoryItem>().Property(p => p.Length).HasPrecision(18, 3);
            modelBuilder.Entity<InventoryItem>().Property(p => p.Snapshot).HasPrecision(18, 3);

            // LoadItem
            modelBuilder.Entity<Load>().Property(l => l.TotalWeight).HasPrecision(18, 3);
            modelBuilder.Entity<LoadItem>().Property(li => li.ShippedWeight).HasPrecision(18, 3);
            modelBuilder.Entity<TransferItem>().Property(ti => ti.Weight).HasPrecision(18, 3);

            // WorkOrderItem numeric fields
            modelBuilder.Entity<WorkOrderItem>().Property(w => w.Length).HasPrecision(18, 3);
            modelBuilder.Entity<WorkOrderItem>().Property(w => w.OrderQuantity).HasPrecision(18, 3);
            modelBuilder.Entity<WorkOrderItem>().Property(w => w.OrderWeight).HasPrecision(18, 3);
            modelBuilder.Entity<WorkOrderItem>().Property(w => w.ProducedQuantity).HasPrecision(18, 3);
            modelBuilder.Entity<WorkOrderItem>().Property(w => w.ProducedWeight).HasPrecision(18, 3);
            modelBuilder.Entity<WorkOrderItem>().Property(w => w.Width).HasPrecision(18, 3);

            // TaskAuditEvent
            modelBuilder.Entity<TaskAuditEvent>()
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}