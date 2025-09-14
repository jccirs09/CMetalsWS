using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMetalsWS.Data;
using CMetalsWS.Domain;
using CMetalsWS.Hubs;
using CMetalsWS.Services;
using CMetalsWS.Tests.TestInfrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace CMetalsWS.Tests
{
    public class WorkOrderServiceTests : IClassFixture<EfFixture>
    {
        private readonly EfFixture _fixture;
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<IHubContext<ScheduleHub>> _mockHubContext;
        private readonly Mock<IPickingListStatusUpdater> _mockPickingListUpdater;

        public WorkOrderServiceTests(EfFixture fixture)
        {
            _fixture = fixture;

            // Mock UserManager
            var store = new Mock<IUserStore<ApplicationUser>>();
            var options = new Mock<IOptions<IdentityOptions>>();
            var passwordHasher = new Mock<IPasswordHasher<ApplicationUser>>();
            var userValidators = new List<IUserValidator<ApplicationUser>>();
            var passwordValidators = new List<IPasswordValidator<ApplicationUser>>();
            var normalizer = new Mock<ILookupNormalizer>();
            var errors = new Mock<IdentityErrorDescriber>();
            var services = new Mock<IServiceProvider>();
            var logger = new Mock<ILogger<UserManager<ApplicationUser>>>();
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                store.Object, options.Object, passwordHasher.Object, userValidators, passwordValidators,
                normalizer.Object, errors.Object, services.Object, logger.Object);

            // Mock HubContext
            _mockHubContext = new Mock<IHubContext<ScheduleHub>>();
            var mockClients = new Mock<IHubClients>();
            var mockClientProxy = new Mock<IClientProxy>();
            _mockHubContext.Setup(h => h.Clients).Returns(mockClients.Object);
            mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);

            // Mock IPickingListStatusUpdater
            _mockPickingListUpdater = new Mock<IPickingListStatusUpdater>();
            _mockPickingListUpdater
                .Setup(u => u.UpdatePickingListStatusAsync(It.IsAny<int>(), It.IsAny<PickingListStatus>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private async Task<(int, int)> SeedDatabase()
        {
            using var context = _fixture.Factory.CreateDbContext();

            // Clear database for test isolation
            var workOrders = await context.WorkOrders.ToListAsync();
            foreach (var wo in workOrders)
            {
                wo.ActiveCoilUsageId = null;
            }
            await context.SaveChangesAsync();

            context.WorkOrderCoilUsages.RemoveRange(context.WorkOrderCoilUsages);
            context.WorkOrders.RemoveRange(context.WorkOrders);
            context.InventoryItems.RemoveRange(context.InventoryItems);
            context.Branches.RemoveRange(context.Branches);
            context.Users.RemoveRange(context.Users);
            await context.SaveChangesAsync();

            var user = new ApplicationUser { Id = "test-user", UserName = "tester" };
            context.Users.Add(user);
            _mockUserManager.Setup(x => x.FindByIdAsync("test-user")).ReturnsAsync(user);

            var branch = new Branch { Id = 1, Name = "Test Branch", Code = "TB" };
            context.Branches.Add(branch);

            var coil1 = new InventoryItem { Id = 1, TagNumber = "COIL-001", ItemId = "ITEM-A", Snapshot = 1000, BranchId = 1 };
            var coil2 = new InventoryItem { Id = 2, TagNumber = "COIL-002", ItemId = "ITEM-B", Snapshot = 2000, BranchId = 1 };
            context.InventoryItems.AddRange(coil1, coil2);

            var workOrder = new WorkOrder
            {
                WorkOrderNumber = "WO-TEST-001",
                TagNumber = "TAG-TEST-001",
                Status = WorkOrderStatus.Pending,
                CoilInventoryId = coil1.Id,
                BranchId = branch.Id
            };
            context.WorkOrders.Add(workOrder);

            await context.SaveChangesAsync();
            return (workOrder.Id, coil2.Id);
        }

        [Fact]
        public async Task StartWorkOrderAsync_ChangesStatus_And_SetsActualStart()
        {
            // Arrange
            var (workOrderId, _) = await SeedDatabase();
            var clock = new FakeClock(new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc));
            var service = new WorkOrderService(_fixture.Factory, _mockUserManager.Object, _mockHubContext.Object, _mockPickingListUpdater.Object, clock);

            // Act
            await service.StartWorkOrderAsync(workOrderId, "test-user");

            // Assert
            using var assertDb = _fixture.Factory.CreateDbContext();
            var workOrder = await assertDb.WorkOrders.FindAsync(workOrderId);
            Assert.NotNull(workOrder);
            Assert.Equal(WorkOrderStatus.InProgress, workOrder!.Status);
            Assert.Equal(clock.UtcNow, workOrder.ActualStart);
        }

        [Fact]
        public async Task StartWorkOrderAsync_CreatesInitialCoilUsage()
        {
            // Arrange
            var (workOrderId, _) = await SeedDatabase();
            var clock = new FakeClock(new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc));
            var service = new WorkOrderService(_fixture.Factory, _mockUserManager.Object, _mockHubContext.Object, _mockPickingListUpdater.Object, clock);

            // Act
            await service.StartWorkOrderAsync(workOrderId, "test-user");

            // Assert
            using var assertDb = _fixture.Factory.CreateDbContext();
            var workOrder = await assertDb.WorkOrders.Include(wo => wo.CoilUsages).FirstAsync(wo => wo.Id == workOrderId);
            Assert.NotNull(workOrder.ActiveCoilUsageId);
            Assert.Single(workOrder.CoilUsages);
        }

        [Fact]
        public async Task PauseWorkOrderAsync_ChangesStatusToPaused()
        {
            // Arrange
            var (workOrderId, _) = await SeedDatabase();
            var clock = new FakeClock(new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc));
            var service = new WorkOrderService(_fixture.Factory, _mockUserManager.Object, _mockHubContext.Object, _mockPickingListUpdater.Object, clock);
            await service.StartWorkOrderAsync(workOrderId, "test-user");

            // Act
            await service.PauseWorkOrderAsync(workOrderId, "test-user");

            // Assert
            using var assertDb = _fixture.Factory.CreateDbContext();
            var workOrder = await assertDb.WorkOrders.FindAsync(workOrderId);
            Assert.NotNull(workOrder);
            Assert.Equal(WorkOrderStatus.Paused, workOrder!.Status);
        }

        [Fact]
        public async Task ResumeWorkOrderAsync_ChangesStatusToInProgress()
        {
            // Arrange
            var (workOrderId, _) = await SeedDatabase();
            var clock = new FakeClock(new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc));
            var service = new WorkOrderService(_fixture.Factory, _mockUserManager.Object, _mockHubContext.Object, _mockPickingListUpdater.Object, clock);
            await service.StartWorkOrderAsync(workOrderId, "test-user");
            await service.PauseWorkOrderAsync(workOrderId, "test-user");

            // Act
            await service.ResumeWorkOrderAsync(workOrderId, "test-user");

            // Assert
            using var assertDb = _fixture.Factory.CreateDbContext();
            var workOrder = await assertDb.WorkOrders.FindAsync(workOrderId);
            Assert.NotNull(workOrder);
            Assert.Equal(WorkOrderStatus.InProgress, workOrder!.Status);
        }

        [Fact]
        public async Task CompleteWorkOrderAsync_ChangesStatus_And_SetsActualEnd()
        {
            // Arrange
            var (workOrderId, _) = await SeedDatabase();
            var clock = new FakeClock(new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc));
            var service = new WorkOrderService(_fixture.Factory, _mockUserManager.Object, _mockHubContext.Object, _mockPickingListUpdater.Object, clock);
            await service.StartWorkOrderAsync(workOrderId, "test-user");

            // Act
            await service.CompleteWorkOrderAsync(workOrderId, "test-user");

            // Assert
            using var assertDb = _fixture.Factory.CreateDbContext();
            var workOrder = await assertDb.WorkOrders.FindAsync(workOrderId);
            Assert.NotNull(workOrder);
            Assert.Equal(WorkOrderStatus.Completed, workOrder!.Status);
            Assert.NotNull(workOrder.ActualEnd);
        }

        [Fact]
        public async Task CompleteWorkOrderAsync_ClosesActiveCoilUsage()
        {
            // Arrange
            var (workOrderId, _) = await SeedDatabase();
            var clock = new FakeClock(new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc));
            var service = new WorkOrderService(_fixture.Factory, _mockUserManager.Object, _mockHubContext.Object, _mockPickingListUpdater.Object, clock);
            await service.StartWorkOrderAsync(workOrderId, "test-user");

            int? initialUsageId;
            using (var db = _fixture.Factory.CreateDbContext())
            {
                var workOrder = await db.WorkOrders.FindAsync(workOrderId);
                initialUsageId = workOrder!.ActiveCoilUsageId;
            }
            Assert.NotNull(initialUsageId);

            // Act
            await service.CompleteWorkOrderAsync(workOrderId, "test-user");

            // Assert
            using var assertDb = _fixture.Factory.CreateDbContext();
            var updatedWorkOrder = await assertDb.WorkOrders.FindAsync(workOrderId);
            var usage = await assertDb.WorkOrderCoilUsages.FindAsync(initialUsageId.Value);
            Assert.NotNull(usage);
            Assert.Null(updatedWorkOrder!.ActiveCoilUsageId);
            Assert.NotNull(usage!.EndedAt);
        }

        [Fact]
        public async Task SwapCoilAsync_ClosesOldUsage_And_OpensNewOne()
        {
            // Arrange
            var (workOrderId, newCoilId) = await SeedDatabase();
            var clock = new FakeClock(new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc));
            var service = new WorkOrderService(_fixture.Factory, _mockUserManager.Object, _mockHubContext.Object, _mockPickingListUpdater.Object, clock);
            await service.StartWorkOrderAsync(workOrderId, "test-user");

            int? initialUsageId;
            int initialUsageCount;
            using (var db = _fixture.Factory.CreateDbContext())
            {
                var workOrderBeforeSwap = await db.WorkOrders.AsNoTracking().Include(wo => wo.CoilUsages).FirstAsync(wo => wo.Id == workOrderId);
                initialUsageId = workOrderBeforeSwap.ActiveCoilUsageId;
                initialUsageCount = workOrderBeforeSwap.CoilUsages.Count;
            }
            Assert.NotNull(initialUsageId);

            // Act
            await service.SwapCoilAsync(workOrderId, newCoilId, CoilSwapReason.SwapEmpty, "Test swap", "test-user");

            // Assert
            using var assertDb = _fixture.Factory.CreateDbContext();
            var workOrderAfterSwap = await assertDb.WorkOrders.Include(wo => wo.CoilUsages).Include(wo => wo.ActiveCoilUsage).FirstAsync(wo => wo.Id == workOrderId);
            var oldUsage = await assertDb.WorkOrderCoilUsages.FindAsync(initialUsageId.Value);
            var newUsage = workOrderAfterSwap.ActiveCoilUsage;

            Assert.NotNull(oldUsage);
            Assert.Equal(initialUsageCount + 1, workOrderAfterSwap.CoilUsages.Count);
            Assert.NotNull(oldUsage!.EndedAt);
            Assert.NotEqual(initialUsageId, workOrderAfterSwap.ActiveCoilUsageId);
            Assert.NotNull(newUsage);
            Assert.Null(newUsage!.EndedAt);
            Assert.Equal(2, newUsage.Sequence);
            Assert.Equal(newCoilId, newUsage.CoilInventoryId);
            Assert.Equal(CoilSwapReason.SwapEmpty, newUsage.Reason);
        }

        [Fact]
        public async Task SwapCoilAsync_Throws_When_WorkOrderNotInProgressOrPaused()
        {
            // Arrange
            var (workOrderId, newCoilId) = await SeedDatabase();
            var clock = new FakeClock(new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc));
            var service = new WorkOrderService(_fixture.Factory, _mockUserManager.Object, _mockHubContext.Object, _mockPickingListUpdater.Object, clock);
            // Note: WO is in 'Pending' status

            // Act & Assert
            var ex = await Assert.ThrowsAsync<DomainException>(() =>
                service.SwapCoilAsync(workOrderId, newCoilId, CoilSwapReason.SwapEmpty, null, "test-user"));
            Assert.Contains("Cannot swap coils", ex.Message);
        }
    }
}
