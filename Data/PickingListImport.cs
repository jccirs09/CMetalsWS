using System.ComponentModel.DataAnnotations;

namespace CMetalsWS.Data
{
    public class PickingListImport
    {
        public int Id { get; set; }
        public int? PickingListId { get; set; }
        public virtual PickingList? PickingList { get; set; }
        public int BranchId { get; set; }
        public virtual Branch Branch { get; set; } = null!;
        public string? SalesOrderNumber { get; set; }
        public string SourcePdfPath { get; set; } = string.Empty;
        public string ImagesPath { get; set; } = string.Empty;
        public string ModelUsed { get; set; } = string.Empty;
        public DateTime StartedUtc { get; set; }
        public DateTime? CompletedUtc { get; set; }
        public ImportStatus Status { get; set; }
        public string? Error { get; set; }
        public string? RawJson { get; set; }
        public virtual ICollection<PickingListPageImage> PageImages { get; set; } = new List<PickingListPageImage>();
    }

    public class PickingListPageImage
    {
        public int Id { get; set; }
        public int PickingListImportId { get; set; }
        public virtual PickingListImport Import { get; set; } = null!;
        public int PageNumber { get; set; }
        public string ImagePath { get; set; } = string.Empty;
    }

    public enum ImportStatus
    {
        Queued,
        Processing,
        Success,
        Failed
    }
}
