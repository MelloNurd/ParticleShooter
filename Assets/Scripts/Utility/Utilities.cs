using UnityEngine;

public static class Utilities
{
    public static Vector3 GetPointInCircle(Vector3 position, float radius)
    {
        return position + ((Vector3)Random.insideUnitCircle * radius);
    }
    public static Vector3 GetPointInCircle(Vector3 position, float minRadius, float maxRadius)
    {
        Vector3 newPos;
        do
        {
            newPos = Random.insideUnitCircle * maxRadius;
        }
        while (Vector3.Distance(newPos, position) < minRadius);

        return newPos;
    }

    // Method to get a random point on the screen
    //public Vector3 GetRandomPointOnScreen(bool awayFromPlayer = true)
    //{
    //    Vector3 newPos;

    //    do
    //    {
    //        newPos = new Vector3(
    //            UnityEngine.Random.Range(-HalfScreenSpace.x, HalfScreenSpace.x),
    //            UnityEngine.Random.Range(-HalfScreenSpace.y, HalfScreenSpace.y),
    //        0);
    //    }
    //    while (Vector2.Distance(newPos, player.transform.position) < 6 && awayFromPlayer); // Continue generating new positions if they are too close to the player

    //    return newPos;
    //}
}
