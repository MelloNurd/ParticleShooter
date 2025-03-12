using UnityEngine;
using System.Collections.Generic;

public class Room : MonoBehaviour
{
    // References to door GameObjects
    public GameObject NorthDoor;
    public GameObject SouthDoor;
    public GameObject EastDoor;
    public GameObject WestDoor;

    private Dictionary<string, GameObject> doors;

    void Awake()
    {
        // Initialize the dictionary with door references
        doors = new Dictionary<string, GameObject>
        {
            { "NorthDoor", NorthDoor },
            { "SouthDoor", SouthDoor },
            { "EastDoor", EastDoor },
            { "WestDoor", WestDoor }
        };
    }

    // Method to retrieve a door by name
    public GameObject GetDoorByName(string doorName)
    {
        if (doors.ContainsKey(doorName))
            return doors[doorName];
        else
            return null;
    }
}
