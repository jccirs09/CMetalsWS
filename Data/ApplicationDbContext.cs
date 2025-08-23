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
        public DbSet<PickingList> PickingLists => Set<PickingList>();
        public DbSet<PickingListItem> PickingListItems => Set<PickingListItem>();
        public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
        public DbSet<WorkOrderItem> WorkOrderItems => Set<WorkOrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Machine -> Branch
            modelBuilder.Entity<Machine>()
                .HasOne(m => m.Branch)
                .WithMany(b => b.Machines) // point to Branch.Machines
                .HasForeignKey(m => m.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // Truck -> Branch
            modelBuilder.Entity<Truck>()
                .HasOne(t => t.Branch)
                .WithMany(b => b.Trucks) // point to Branch.Trucks
                .HasForeignKey(t => t.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

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
                .WithMany(b => b.PickingLists) // point to Branch.PickingLists
                .HasForeignKey(p => p.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            // PickingList -> Truck
            modelBuilder.Entity<PickingList>()
                .HasOne(p => p.Truck)
                .WithMany()
                .HasForeignKey(p => p.TruckId)
                .OnDelete(DeleteBehavior.SetNull);

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

            // Numeric precisions
            modelBuilder.Entity<PickingListItem>()
                .Property(i => i.Quantity).HasPrecision(18, 3);
            modelBuilder.Entity<PickingListItem>()
                .Property(i => i.Width).HasPrecision(18, 3);
            modelBuilder.Entity<PickingListItem>()
                .Property(i => i.Length).HasPrecision(18, 3);
            modelBuilder.Entity<PickingListItem>()
                .Property(i => i.Weight).HasPrecision(18, 3);

            modelBuilder.Entity<Truck>()
                .Property(t => t.CapacityWeight).HasPrecision(18, 2);
            modelBuilder.Entity<Truck>()
                .Property(t => t.CapacityVolume).HasPrecision(18, 2);

            // WorkOrder numeric precision (to remove EF warning)
            modelBuilder.Entity<WorkOrderItem>()
                .Property(w => w.Quantity).HasPrecision(18, 3);
        }
    }
}
