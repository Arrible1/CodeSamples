using System;

public class DebugMenuFloatRange : Attribute
{
    public DebugMenuFloatRange(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    public float min;
    public float max;
}
