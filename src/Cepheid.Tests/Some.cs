using System;

namespace Codestellation.Cepheid.Tests
{
    public static class Some
    {
        public static string String()
        {
            return Guid.NewGuid().ToString("N");
        }
    }
}