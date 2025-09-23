using CMetalsWS.Data;
using CMetalsWS.Models;
using CMetalsWS.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMetalsWS.Components.Pages.Schedule
{
    public class ScheduleItem
    {
        public int Id { get; set; }
        public ScheduleItemType Type { get; set; }
        public int? MachineId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Text { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public object OriginalItem { get; set; } = default!;
    }

    public enum ScheduleItemType
    {
        WorkOrder,
        PickingListItem
    }

    public class AllMachinesScheduleBase : ComponentBase
    {
        [Inject] protected WorkOrderService WorkOrderService { get; set; } = default!;
        [Inject] protected PickingListService PickingListService { get; set; } = default!;
        [Inject] protected MachineService MachineService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;
        [Inject] protected IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;

        private DateTime? _selectedDate = DateTime.Today;
        protected DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                LoadData();
            }
        }

        protected List<Machine> _machines = new();
        protected List<ScheduleItem> _scheduleItems = new();
        protected List<MachineDailyStatusDto> _machineStatuses = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            _machines = await MachineService.GetMachinesAsync();
            _scheduleItems.Clear();

            if (_selectedDate.HasValue)
            {
                _machineStatuses = await MachineService.GetMachineDailyStatusAsync(_selectedDate.Value);

                var workOrders = await WorkOrderService.GetByDateAsync(_selectedDate.Value);
                foreach (var wo in workOrders)
                {
                    _scheduleItems.Add(new ScheduleItem
                    {
                        Id = wo.Id,
                        Type = ScheduleItemType.WorkOrder,
                        MachineId = wo.MachineId,
                        StartDate = wo.ScheduledStartDate,
                        EndDate = wo.ScheduledEndDate,
                        Text = wo.WorkOrderNumber,
                        CustomerName = wo.Items.FirstOrDefault()?.CustomerName ?? "N/A",
                        Status = wo.Status.ToString(),
                        OriginalItem = wo
                    });
                }

                var sheetItems = await PickingListService.GetSheetPullingQueueAsync();
                var coilItems = await PickingListService.GetCoilPullingQueueAsync();
                var pickingListItems = sheetItems.Concat(coilItems)
                    .Where(i => i.ScheduledProcessingDate?.Date == _selectedDate.Value.Date)
                    .ToList();

                var pickingListIds = pickingListItems.Select(i => i.PickingListId).Distinct().ToList();
                using var db = await DbContextFactory.CreateDbContextAsync();
                var pickingLists = await db.PickingLists
                    .Include(p => p.Customer)
                    .Where(p => pickingListIds.Contains(p.Id))
                    .ToDictionaryAsync(p => p.Id);

                foreach (var item in pickingListItems)
                {
                    var pickingList = pickingLists.GetValueOrDefault(item.PickingListId);
                    _scheduleItems.Add(new ScheduleItem
                    {
                        Id = item.Id,
                        Type = ScheduleItemType.PickingListItem,
                        MachineId = item.MachineId,
                        StartDate = item.ScheduledProcessingDate ?? DateTime.MinValue,
                        EndDate = (item.ScheduledProcessingDate ?? DateTime.MinValue).AddHours(1), // Assuming 1 hour duration
                        Text = item.ItemDescription,
                        CustomerName = pickingList?.Customer?.Name ?? "N/A",
                        Status = item.Status.ToString(),
                        OriginalItem = item
                    });
                }
            }
            StateHasChanged();
        }

        protected List<ScheduleItem> GetScheduleItemsForMachine(int machineId)
        {
            return _scheduleItems.Where(item => item.MachineId == machineId).ToList();
        }

        protected string GetScheduleItemStyle(ScheduleItem item)
        {
            var timelineStartHour = 6;
            var timelineEndHour = 18;
            var timelineDurationHours = timelineEndHour - timelineStartHour;

            var start = item.StartDate.TimeOfDay;
            var end = item.EndDate.TimeOfDay;

            // Clamp start and end times to the timeline window
            var clampedStart = Math.Max(timelineStartHour, start.TotalHours);
            var clampedEnd = Math.Min(timelineEndHour, end.TotalHours);

            if (clampedStart >= clampedEnd)
            {
                return "display: none;"; // Hide tasks outside the timeline
            }

            var duration = clampedEnd - clampedStart;
            var left = ((clampedStart - timelineStartHour) / timelineDurationHours) * 100;
            var width = (duration / timelineDurationHours) * 100;

            var color = item.Type == ScheduleItemType.WorkOrder ? "#3f51b5" : "#ff9800"; // Blue for WorkOrder, Orange for PickingListItem

            return $"left: {left.ToString(System.Globalization.CultureInfo.InvariantCulture)}%; width: {width.ToString(System.Globalization.CultureInfo.InvariantCulture)}%; background-color: {color};";
        }

        protected async Task ShowItemDetails(ScheduleItem item)
        {
            if (item.Type == ScheduleItemType.WorkOrder)
            {
                var parameters = new DialogParameters { ["WorkOrderId"] = item.Id };
                await DialogService.ShowAsync<Dialogs.WorkOrderDetailsDialog>("Work Order Details", parameters);
            }
            else
            {
                var parameters = new DialogParameters { ["PickingListItemId"] = item.Id };
                await DialogService.ShowAsync<Dialogs.PickingListItemDetailsDialog>("Picking List Item Details", parameters);
            }
        }

        protected string GetStatusClass(string status)
        {
            return "status-" + status.ToLower();
        }
    }
}
