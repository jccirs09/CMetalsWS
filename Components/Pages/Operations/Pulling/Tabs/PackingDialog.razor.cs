using CMetalsWS.Data;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CMetalsWS.Components.Pages.Operations.Pulling.Tabs
{
    public partial class PackingDialog
    {
        [CascadingParameter] IMudDialogInstance MudDialog { get; set; }

        [Parameter] public PickingListItem Item { get; set; }

        [Parameter] public decimal PackedQuantity { get; set; }

        private void Submit()
        {
            MudDialog.Close(DialogResult.Ok(PackedQuantity));
        }

        void Cancel() => MudDialog.Cancel();
    }
}
