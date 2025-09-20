namespace CMetalsWS.Services
{
    using CMetalsWS.Data;
    using System.Collections.Generic;

    public interface IParsingStateService
    {
        PickingList? ParsedPickingList { get; set; }
        List<PickingListItem>? ParsedItems { get; set; }
        string? UserId { get; set; }
        void Clear();
    }

    public class ParsingStateService : IParsingStateService
    {
        public PickingList? ParsedPickingList { get; set; }
        public List<PickingListItem>? ParsedItems { get; set; }
        public string? UserId { get; set; }

        public void Clear()
        {
            ParsedPickingList = null;
            ParsedItems = null;
            UserId = null;
        }
    }
}
