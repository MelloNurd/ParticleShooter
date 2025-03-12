using UnityEngine;
using System.Collections.Generic;
using Unity.Cinemachine;


public class DungeonGenerator : MonoBehaviour
{
    public int width = 10, height = 10; // Dimensions of the dungeon
    public GameObject roomPrefab; // Prefab for the room
    public GameObject playerPrefab; // Prefab for the player
    public Grid grid; // Reference to the Grid component

    private bool[,] dungeonGrid; // 2D array to keep track of room positions
    private List<Vector2Int> rooms = new List<Vector2Int>(); // List to store room positions
    private Dictionary<Vector2Int, GameObject> roomObjects = new Dictionary<Vector2Int, GameObject>(); // Mapping from room positions to room GameObjects
    private Vector2Int start; // Starting room position

    public CinemachineCamera cinCam; // Reference to the Cinemachine Virtual Camera


    void Start()
    {
        GenerateDungeon(); // Generate the dungeon when the game starts
    }

    void GenerateDungeon()
    {
        dungeonGrid = new bool[width, height]; // Initialize the dungeon grid
        start = new Vector2Int(width / 2, height / 2); // Start in the middle of the grid
        rooms.Add(start); // Add the starting room to the list
        dungeonGrid[start.x, start.y] = true; // Mark the starting room as occupied

        for (int i = 0; i < 10; i++) // Add 10 rooms
        {
            Vector2Int newRoom = GetNewRoom(); // Get a new room position
            if (newRoom != Vector2Int.zero) // If the new room position is valid
            {
                rooms.Add(newRoom); // Add the new room to the list
                dungeonGrid[newRoom.x, newRoom.y] = true; // Mark the new room as occupied
            }
        }

        InstantiateRooms(); // Instantiate the room prefabs in the world
        SetupDoors(); // Set up the doors after rooms are instantiated
        SpawnPlayer(); // Spawn the player in the starting room
    }

    Vector2Int GetNewRoom()
    {
        Vector2Int baseRoom = rooms[Random.Range(0, rooms.Count)]; // Pick a random existing room
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right }; // Possible directions to move
        foreach (Vector2Int dir in directions)
        {
            Vector2Int newPos = baseRoom + dir; // Calculate the new position
            if (IsValidPosition(newPos)) // Check if the new position is valid
                return newPos; // Return the new position if valid
        }
        return Vector2Int.zero; // Return zero if no valid position is found
    }

    bool IsValidPosition(Vector2Int pos)
    {
        // Check if the position is within bounds and not already occupied
        return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height && !dungeonGrid[pos.x, pos.y];
    }

    void InstantiateRooms()
    {
        float cellWidth = grid.cellSize.x + grid.cellGap.x;
        float cellHeight = grid.cellSize.y + grid.cellGap.y;
        Vector3 cellOffset = new Vector3(grid.cellSize.x / 2, grid.cellSize.y / 2, 0); // Offset to center of cell

        foreach (Vector2Int room in rooms)
        {
            Vector3 worldPos = grid.transform.position +
                               new Vector3(room.x * cellWidth, room.y * cellHeight, 0) +
                               cellOffset;
            GameObject roomObj = Instantiate(roomPrefab, worldPos, Quaternion.identity);
            roomObjects.Add(room, roomObj); // Store reference to the room GameObject
        }
    }

    void SetupDoors()
    {
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        string[] doorNames = { "NorthDoor", "EastDoor", "SouthDoor", "WestDoor" };
        string[] oppositeDoorNames = { "SouthDoor", "WestDoor", "NorthDoor", "EastDoor" };

        foreach (Vector2Int roomPos in rooms)
        {
            GameObject roomObj = roomObjects[roomPos];
            Room roomScript = roomObj.GetComponent<Room>();

            for (int i = 0; i < directions.Length; i++)
            {
                Vector2Int adjacentPos = roomPos + directions[i];
                GameObject door = roomScript.GetDoorByName(doorNames[i]);

                if (rooms.Contains(adjacentPos))
                {
                    // Enable the door
                    door.SetActive(true);

                    // Set the target door
                    GameObject adjacentRoomObj = roomObjects[adjacentPos];
                    Room adjacentRoomScript = adjacentRoomObj.GetComponent<Room>();
                    GameObject targetDoor = adjacentRoomScript.GetDoorByName(oppositeDoorNames[i]);

                    Door doorScript = door.GetComponent<Door>();
                    doorScript.targetDoor = targetDoor.GetComponent<Door>();
                    // Remove or comment out the following line
                    // doorScript.doorDirection = (Door.DoorDirection)i;
                }
                else
                {
                    // Disable the door
                    door.SetActive(false);
                }
            }
        }
    }


    void SpawnPlayer()
    {
        GameObject startingRoom = roomObjects[start]; // Get the starting room GameObject
        Vector3 spawnPosition = startingRoom.transform.position; // Position of the starting room
        // Adjust the spawn position if needed (e.g., adjust Y-axis for player height)
        spawnPosition.y += 1f; // Example adjustment

        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        cinCam.Follow = player.transform; // Set the Cinemachine Virtual Camera to follow the player
        cinCam.LookAt = player.transform; // Set the Cinemachine Virtual Camera to look at the player
    }
}
