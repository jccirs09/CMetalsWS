using System;
using System.Threading.Tasks;
using CMetalsWS.Data;
using Microsoft.Extensions.Caching.Memory;
using System.IO;

namespace CMetalsWS.Services
{
    public class MemoryPickingListDraftStore : IPickingListDraftStore
    {
        private readonly IMemoryCache _cache;
        private readonly PdfParsingService _parser;
        private readonly MemoryCacheEntryOptions _opts = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(2)
        };

        public MemoryPickingListDraftStore(IMemoryCache cache, PdfParsingService parser)
        {
            _cache = cache;
            _parser = parser;
        }

        private static string Key(Guid id) => $"pickdraft:{id}";

        public async Task<Guid> CreateAsync(Stream pdf, string fileName, int branchId)
        {
            var draft = await _parser.ParseAsync(pdf, fileName);
            draft.BranchId = branchId;
            var id = Guid.NewGuid();
            _cache.Set(Key(id), draft, _opts);
            return id;
        }

        public Task<PickingList?> GetAsync(Guid id)
            => Task.FromResult(_cache.TryGetValue(Key(id), out PickingList? pl) ? pl : null);

        public Task<bool> ReplaceAsync(Guid id, PickingList draft)
        {
            if (!_cache.TryGetValue(Key(id), out _)) return Task.FromResult(false);
            _cache.Set(Key(id), draft, _opts);
            return Task.FromResult(true);
        }

        public void Remove(Guid id) => _cache.Remove(Key(id));
    }
}
