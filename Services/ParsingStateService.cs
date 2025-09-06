using CMetalsWS.Data;
using System.Collections.Generic;

namespace CMetalsWS.Services
{
    public class ParsingStateService : IParsingStateService
    {
        public PickingList? ParsedPickingList { get; set; }
        public List<PickingListItem>? ParsedItems { get; set; }

        public void Clear()
        {
            ParsedPickingList = null;
            ParsedItems = null;
        }
    }
}
