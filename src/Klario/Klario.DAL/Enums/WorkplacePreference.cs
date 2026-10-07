using System;
using System.Collections.Generic;
using System.Text;

namespace Klario.DAL.Enums
{
    [Flags]
    public enum WorkplacePreference
    {
        None = 0,
        Remote = 1 << 0,
        Hybrid = 1 << 1,
        Onsite = 1 << 2
    }
}
