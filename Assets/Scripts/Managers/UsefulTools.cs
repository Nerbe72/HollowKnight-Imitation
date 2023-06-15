using System.Collections;
using System.Collections.Generic;

public enum Tags
{
    Player,
    Attack,
    Ground,
    Save,
    Enemy,
    Boss,
    Trap,
    Obstacle,
    Slidable,
}
public enum Masks
{
    Ground,
    Slidable,
}
public static class GameTagMask
{
    public static string Tag(Tags tag)
    {
        return tag.ToString();
    }

    public static string Mask(Masks mask)
    {
        return mask.ToString();
    }
}