using System;

namespace CMetalsWS.Data
{
    public interface IClock
    {
        DateTime UtcNow { get; }
    }
}
