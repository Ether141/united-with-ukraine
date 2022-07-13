using UnityEngine;

public static class TransformUtil
{
    /// <summary>
    /// Returns child in given <see cref="Transform"/> with given name.
    /// </summary>
    public static Transform GetChildWithName(this Transform t, string s)
    {
        Transform child = null;

        for (int i = 0; i < t.childCount; i++)
        {
            if (t.GetChild(i).name == s)
            {
                child = t.GetChild(i);
                break;
            }
        }

        return child;
    }
}
