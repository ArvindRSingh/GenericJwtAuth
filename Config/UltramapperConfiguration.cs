using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GenericJwtAuth.Config
{
    internal static class UltramapperConfiguration
    {
        private const UltraMapper.Configuration configuration = null;

        public static UltraMapper.Configuration Get()
        {
            return configuration;
        }
    }
}
