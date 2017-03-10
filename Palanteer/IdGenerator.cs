using System;
using System.Linq;

namespace Palanteer
{
    public static class IdGenerator
    {
        public static string Generate()
            => Guid.NewGuid().ToByteArray().Select(b => b.ToString("x2")).Aggregate((l, r) => l + r);
    }
}