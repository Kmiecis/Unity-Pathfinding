using System;

namespace Custom.Pathfinding
{
    [Flags]
    public enum PF_ENodeState : byte
    {
        Idle = 0,
        Added = 1,
        Checked = 2
    }
}
