using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace CMetalsWS.Components.FigmaDesign
{
    public partial class MessagingHeader
    {
        [Parameter]
        public EventCallback OnClose { get; set; }

        private async Task HandleClose()
        {
            if (OnClose.HasDelegate)
            {
                await OnClose.InvokeAsync();
            }
        }
    }
}