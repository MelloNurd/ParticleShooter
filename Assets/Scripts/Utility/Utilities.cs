using UnityEngine;

public static class Utilities
{
    public static Vector3 GetPointInCircle(Vector3 position, float radius)
    {
        return position + ((Vector3)Random.insideUnitCircle * radius);
    }
    public static Vector3 GetPointInCircle(Vector3 position, float minRadius, float maxRadius)
    {
        if(minRadius > maxRadius)
        {
            Debug.LogError("Min radius cannot be greater than max radius.");
            return position;
        }

        int threshold = 0;

        Vector3 newPos;
        do
        {
            newPos = GetPointInCircle(position, maxRadius);
            threshold++;
        }
        while (Vector3.Distance(newPos, position) <= minRadius && threshold < 100);

        // Threshold is in place to prevent any potential infinite loops
        if(threshold >= 100)
        {
            Debug.LogWarning("Threshold reached while trying to find a point in circle.");
            return position;
        }

        return newPos;
    }

    public static Vector3 GetEmptyPointInCircle(Vector3 position, float radius) => GetEmptyPointInCircle(position, 0f, radius);
    public static Vector3 GetEmptyPointInCircle(Vector3 position, float minRadius, float maxRadius)
    {
        if (minRadius > maxRadius)
        {
            Debug.LogError("Min radius cannot be greater than max radius.");
            return position;
        }
        int threshold = 0;
        Vector3 newPos;
        do
        {
            newPos = GetPointInCircle(position, minRadius, maxRadius);
        }
        while (!IsEmptyPosition(newPos) && threshold++ < 100);
        
        if (threshold >= 100)
        {
            Debug.LogWarning("Threshold reached while trying to find an empty point in circle.");
            return position;
        }

        return newPos;
    }

    public static bool IsEmptyPosition(Vector3 pos)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(pos, 0f);
        return colliders.Length == 0;
    }

    public static bool IsOnScreen(Vector3 pos, float margin = 0) => IsOnScreen(Camera.main, pos, margin);
    public static bool IsOnScreen(Camera cam, Vector3 pos, float margin = 0)
    {
        Vector3 viewportPos = cam.WorldToViewportPoint(pos);

        // Check if in front of the camera
        bool isInFront = viewportPos.z > 0;

        // Check if inside screen bounds
        bool isOnScreen = viewportPos.x >= -margin && viewportPos.x <= 1 + margin &&
                          viewportPos.y >= -margin && viewportPos.y <= 1 + margin;

        return isInFront && isOnScreen;
    }

    public static Vector3 GetRandomPointOnScreen() => GetRandomPointOnScreen(Camera.main);
    public static Vector3 GetRandomPointOnScreen(Camera cam)
    {
        Vector3 randomPoint = new(Random.Range(0f, 1f), Random.Range(0f, 1f), cam.nearClipPlane + 1f);

        return cam.ViewportToWorldPoint(randomPoint);
    }

    public static Vector3 GetRandomEmptyPointOnScreen() => GetRandomEmptyPointOnScreen(Camera.main);
    public static Vector3 GetRandomEmptyPointOnScreen(Camera cam)
    {
        Vector3 pos = Vector3.zero;
        int threshhold = 0;
        do
        {
            pos = GetRandomPointOnScreen(cam);
        } 
        while(!IsEmptyPosition(pos) && ++threshhold < 100);

        if (threshhold >= 100)
        {
            Debug.LogWarning("Threshold reached while trying to find an empty point on screen.");
            return pos;
        }

        return pos;
    }

    public static Vector3 GetRandomPointOffScreen(float radius = 1f, float margin = 0) => GetRandomPointOffScreen(Camera.main, radius, margin);
    public static Vector3 GetRandomPointOffScreen(Camera cam, float radius = 1f, float margin = 0)
    {
        // Some manual tweaks to try and prevent infinite loops. Multiply by 0.1f to get it closer to size of a Unity unit.
        radius = Mathf.Max(0.05f, Mathf.Abs(radius * 0.1f)); // Ensure radius is positive and above 0.05f.
        margin = Mathf.Min(radius * 0.5f, Mathf.Abs(margin * 0.1f)); // Ensure margin is posiitve and no more than half of the radius.
        
        Vector3 randomPos;
        int threshold = 0;
        do
        {
            Vector3 temp = new(Random.Range(-radius, 1 + radius), Random.Range(-radius, 1 + radius), cam.nearClipPlane + 1f);
            randomPos = cam.ViewportToWorldPoint(temp);
            threshold++;
        }
        while (IsOnScreen(randomPos, margin) && threshold < 100);

        if (threshold >= 100)
        {
            Debug.LogWarning("Threshold reached while trying to find a point off screen. Check your values and try again.");
            return Vector3.zero;
        }

        return randomPos;
    }

    public static Vector3 GetScreenEdgePosition(Vector3 pos, Vector3 direction)
    {
        Vector3 checkedPos = pos;
        Vector3 travelDir = direction.normalized * 0.01f;

        do
        {
            checkedPos += travelDir;
        }
        while (IsOnScreen(checkedPos));

        return checkedPos;
    }

    public static void ShowCanvasGroup(ref CanvasGroup group)
    {
        group.alpha = 1;
        group.interactable = true;
        group.blocksRaycasts = true;
    }

    public static void HideCanvasGroup(ref CanvasGroup group)
    {
        group.alpha = 0;
        group.interactable = false;
        group.blocksRaycasts = false;
    }
}
