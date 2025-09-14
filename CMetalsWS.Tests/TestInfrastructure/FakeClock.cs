using System;
using CMetalsWS.Data;

namespace CMetalsWS.Tests.TestInfrastructure
{
    public class FakeClock : IClock
    {
        private readonly DateTime _now;
        public FakeClock(DateTime now) => _now = now;
        public DateTime UtcNow => _now;
    }
}
