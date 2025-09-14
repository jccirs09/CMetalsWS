using CMetalsWS.Data;
using CMetalsWS.Models;
using System;
using System.Linq;

namespace CMetalsWS.Services
{
    public class NowPlayingProjector
    {
        /// <summary>
        /// Projects a WorkOrder object into a NowPlayingDto for UI display.
        /// </summary>
        /// <param name="workOrder">The work order to project.</param>
        /// <returns>A NowPlayingDto instance.</returns>
        public NowPlayingDto Project(WorkOrder workOrder)
        {
            if (workOrder.Machine == null)
            {
                throw new ArgumentNullException(nameof(workOrder.Machine), "WorkOrder must include a Machine to be projected.");
            }

            var now = DateTime.UtcNow;
            TimeSpan runtime = TimeSpan.Zero;
            if (workOrder.ActualStartDate.HasValue)
            {
                // Ensure runtime isn't negative if clock sync is off or data is wrong
                runtime = (now > workOrder.ActualStartDate.Value) ? (now - workOrder.ActualStartDate.Value) : TimeSpan.Zero;
            }

            double progress = 0;
            if (workOrder.ActualStartDate.HasValue && workOrder.EstimatedMinutes > 0)
            {
                var elapsedMinutes = runtime.TotalMinutes;
                progress = Math.Clamp(elapsedMinutes / workOrder.EstimatedMinutes, 0, 1);
            }

            // A simple approach to get a representative customer name from the work order items.
            var customerName = workOrder.Items.FirstOrDefault(i => !string.IsNullOrEmpty(i.CustomerName))?.CustomerName;

            return new NowPlayingDto
            {
                MachineId = workOrder.MachineId.Value,
                MachineName = workOrder.Machine.Name,
                MachineCategory = workOrder.Machine.Category,
                WorkOrderId = workOrder.Id,
                CustomerName = customerName,
                Progress = progress,
                Runtime = $"{(int)runtime.TotalHours}h {runtime.Minutes}m",
                Eta = workOrder.ScheduledEndDate,
                SwapCount = workOrder.CoilUsages?.Count ?? 0,
                Status = workOrder.Status
            };
        }
    }
}
