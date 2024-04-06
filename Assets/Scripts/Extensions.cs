using UnityEngine;

public static class Extensions
{
    public static Vector2 Rotate(this Vector2 val, float angle)
    {
        // Had to look up how to rotate a 2d vector
        angle *= Mathf.Deg2Rad;
        return new Vector2(
            val.x * Mathf.Cos(angle) - val.y * Mathf.Sin(angle),
            val.x * Mathf.Sin(angle) + val.y * Mathf.Cos(angle)
        );
    }
}