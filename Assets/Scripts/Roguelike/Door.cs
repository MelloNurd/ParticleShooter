using UnityEngine;

public class Door : MonoBehaviour
{
    public enum DoorDirection { North, East, South, West }
    public Door targetDoor;
    public DoorDirection doorDirection;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && targetDoor != null)
        {
            // Move the player to the target door's position
            Vector3 offset = GetOffset(targetDoor.doorDirection);
            other.transform.position = targetDoor.transform.position + offset;
        }
    }

    Vector3 GetOffset(DoorDirection direction)
    {
        switch (direction)
        {
            case DoorDirection.North:
                return new Vector3(0, -1.5f, 0);
            case DoorDirection.East:
                return new Vector3(-1f, 0, 0);
            case DoorDirection.South:
                return new Vector3(0, 1.5f, 0);
            case DoorDirection.West:
                return new Vector3(1f, 0, 0);
            default:

                return Vector3.zero;
        }
    }
}
