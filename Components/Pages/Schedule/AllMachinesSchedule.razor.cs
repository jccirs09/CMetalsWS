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
    public class AllMachinesScheduleBase : ComponentBase
    {
        [Inject] protected WorkOrderService WorkOrderService { get; set; } = default!;
        [Inject] protected MachineService MachineService { get; set; } = default!;
        [Inject] protected IDialogService DialogService { get; set; } = default!;

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
        protected List<WorkOrder> _workOrders = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadData();
        }

        protected async Task LoadData()
        {
            _machines = await MachineService.GetMachinesAsync();
            if (_selectedDate.HasValue)
            {
                _workOrders = await WorkOrderService.GetByDateAsync(_selectedDate.Value);
            }
            StateHasChanged();
        }

        protected List<WorkOrder> GetWorkOrdersForMachine(int machineId)
        {
            return _workOrders.Where(wo => wo.MachineId == machineId).ToList();
        }

        protected string GetWorkOrderStyle(WorkOrder workOrder)
        {
            var start = workOrder.ScheduledStartDate.TimeOfDay;
            var end = workOrder.ScheduledEndDate.TimeOfDay;
            var duration = end - start;
            var left = (start.TotalHours / 24) * 100;
            var width = (duration.TotalHours / 24) * 100;

            return $"left: {left.ToString(System.Globalization.CultureInfo.InvariantCulture)}%; width: {width.ToString(System.Globalization.CultureInfo.InvariantCulture)}%;";
        }

        protected async Task ShowWorkOrderDetails(WorkOrder workOrder)
        {
            var parameters = new DialogParameters { ["WorkOrderId"] = workOrder.Id };
            await DialogService.ShowAsync<Dialogs.WorkOrderDetailsDialog>("Work Order Details", parameters);
        }
    }
}
