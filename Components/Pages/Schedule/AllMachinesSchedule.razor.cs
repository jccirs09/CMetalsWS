using CMetalsWS.Data;
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

        private DateTime? _selectedDate = DateTime.Today.AddDays(1);
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
                        OriginalItem = wo
                    });
                }

                var sheetItems = await PickingListService.GetSheetPullingQueueAsync();
                var coilItems = await PickingListService.GetCoilPullingQueueAsync();
                var pickingListItems = sheetItems.Concat(coilItems).ToList();

                foreach (var item in pickingListItems.Where(i => i.ScheduledProcessingDate?.Date == _selectedDate.Value.Date))
                {
                    _scheduleItems.Add(new ScheduleItem
                    {
                        Id = item.Id,
                        Type = ScheduleItemType.PickingListItem,
                        MachineId = item.MachineId,
                        StartDate = item.ScheduledProcessingDate ?? DateTime.MinValue,
                        EndDate = (item.ScheduledProcessingDate ?? DateTime.MinValue).AddHours(1), // Assuming 1 hour duration
                        Text = item.ItemDescription,
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
            var start = item.StartDate.TimeOfDay;
            var end = item.EndDate.TimeOfDay;
            var duration = end - start;
            var left = (start.TotalHours / 24) * 100;
            var width = (duration.TotalHours / 24) * 100;

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
                // TODO: Create a dialog for PickingListItem details
                var parameters = new DialogParameters { ["PickingListItemId"] = item.Id };
                // await DialogService.ShowAsync<Dialogs.PickingListItemDetailsDialog>("Picking List Item Details", parameters);
            }
        }
    }
}
