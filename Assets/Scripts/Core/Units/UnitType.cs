using System;

[Flags]
public enum UnitType
{
    None = 0,
    Unmounted = 1,
    Mounted = 2,

    Ground = Unmounted | Mounted,
    Air = 4,

    All = Ground | Air
}