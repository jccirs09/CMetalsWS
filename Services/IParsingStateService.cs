using CMetalsWS.Data;
using System.Collections.Generic;

namespace CMetalsWS.Services
{
    public interface IParsingStateService
    {
        PickingList? ParsedPickingList { get; set; }
        List<PickingListItem>? ParsedItems { get; set; }
        void Clear();
    }
}
