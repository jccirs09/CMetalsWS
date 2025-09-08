using CMetalsWS.Data;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Threading.Tasks;

namespace CMetalsWS.Components.Pages.Customers
{
    public partial class EditCustomerDialog
    {
        [CascadingParameter]
        IMudDialogInstance MudDialog { get; set; } = default!;
        [Parameter] public Customer Customer { get; set; } = new();
        [Parameter] public bool IsNew { get; set; }

        private MudForm _form = default!;

        private async Task Save()
        {
            await _form.Validate();
            if (_form.IsValid)
            {
                MudDialog.Close(DialogResult.Ok(Customer));
            }
        }

        void Cancel() => MudDialog.Cancel();
    }
}
