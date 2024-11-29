using System;

public class DebugMenuIntRange : Attribute
{
    public DebugMenuIntRange(int min, int max)
    {
        this.min = min;
        this.max = max;
    }

    public int min;
    public int max;
}
