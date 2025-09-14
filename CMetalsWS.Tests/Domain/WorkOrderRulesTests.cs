using System;
using CMetalsWS.Data;
using CMetalsWS.Domain;
using CMetalsWS.Tests.TestInfrastructure;
using Xunit;

namespace CMetalsWS.Tests.Domain
{
    public class WorkOrderRulesTests
    {
        private readonly IClock _clock = new FakeClock(new DateTime(2025, 1, 1, 12, 0, 0, DateTimeKind.Utc));
        private readonly ApplicationUser _user = new ApplicationUser { Id = "test-user", UserName = "tester" };

        [Fact]
        public void ApplyCoilSnapshot_Copies_Correct_Properties()
        {
            // Arrange
            var workOrder = new WorkOrder();
            var coil = new InventoryItem
            {
                ItemId = "COIL-123",
                Description = "Test Coil",
                Snapshot = 1234.5m,
                Location = "A1"
            };

            // Act
            WorkOrderRules.ApplyCoilSnapshot(workOrder, coil, _clock);

            // Assert
            Assert.Equal("COIL-123", workOrder.CoilItemId);
            Assert.Equal("Test Coil", workOrder.CoilDescription);
            Assert.Equal(1234.5m, workOrder.CoilWeightAtWOStartLbs);
            Assert.Equal("A1", workOrder.CoilLocationAtWOStart);
            Assert.Equal(_clock.UtcNow, workOrder.CoilSnapshotAt);
        }

        [Theory]
        [InlineData(WorkOrderStatus.Pending)]
        [InlineData(WorkOrderStatus.Paused)]
        public void ValidateCanStart_Allows_Valid_Statuses(WorkOrderStatus status)
        {
            // Arrange
            var workOrder = new WorkOrder { Status = status };

            // Act
            var exception = Record.Exception(() => WorkOrderRules.ValidateCanStart(workOrder));

            // Assert
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(WorkOrderStatus.Draft)]
        [InlineData(WorkOrderStatus.InProgress)]
        [InlineData(WorkOrderStatus.Completed)]
        [InlineData(WorkOrderStatus.Canceled)]
        public void ValidateCanStart_Throws_For_Invalid_Statuses(WorkOrderStatus status)
        {
            // Arrange
            var workOrder = new WorkOrder { Status = status };

            // Act & Assert
            Assert.Throws<DomainException>(() => WorkOrderRules.ValidateCanStart(workOrder));
        }

        [Fact]
        public void ApplyStart_Sets_Status_And_Timestamps()
        {
            // Arrange
            var workOrder = new WorkOrder { Status = WorkOrderStatus.Pending };

            // Act
            WorkOrderRules.ApplyStart(workOrder, _user, null, _clock);

            // Assert
            Assert.Equal(WorkOrderStatus.InProgress, workOrder.Status);
            Assert.Equal(_clock.UtcNow, workOrder.ActualStart);
            Assert.Equal(_user.UserName, workOrder.LastUpdatedBy);
            Assert.Equal(_clock.UtcNow, workOrder.LastUpdatedDate);
        }
    }
}
