using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

public class ChunkManager : MonoBehaviour
{
    [Header("Chunk Settings")]
    [SerializeField] private GameObject chunkPrefab; // Assign your chunk prefab in the Inspector
    [SerializeField] private Transform target; // Assign the target transform in the Inspector
    [SerializeField] private Vector3 chunkSize = new Vector3(5, 5, 5); // Set the chunk size in the Inspector

    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugLogging = false; // Toggle for debug logging
    [SerializeField] private bool visualizeChunkBorders = true; // Toggle for visualizing chunk borders
    [SerializeField, Range(0, 5)] private int extraChunkBorders = 1; // Slider for extra chunk borders to visualize
    [SerializeField, Range(0f, 1f)] private float borderOpacity = 0.5f; // Slider for chunk border opacity

    private readonly Dictionary<Vector3Int, GameObject> activeChunks = new Dictionary<Vector3Int, GameObject>();
    private Vector3Int previousTargetChunk;

    private void Start()
    {
        UpdateChunks();
    }

    private void Update()
    {

        if (GetTargetChunk() != previousTargetChunk)
        {
            UpdateChunks();
            previousTargetChunk = GetTargetChunk();
        }
    }

    private Vector3Int GetTargetChunk()
    {
        Vector3 targetPosition = target.position;
        return new Vector3Int(
            Mathf.FloorToInt(targetPosition.x / chunkSize.x),
            Mathf.FloorToInt(targetPosition.y / chunkSize.y),
            Mathf.FloorToInt(targetPosition.z / chunkSize.z)
        );
    }

    private void UpdateChunks()
    {
        Vector3Int targetChunk = GetTargetChunk();
        HashSet<Vector3Int> newChunkCoords = GetSurroundingChunks(targetChunk);
        List<Vector3Int> coordsToMove = new List<Vector3Int>(activeChunks.Keys);

        foreach (Vector3Int coord in newChunkCoords)
        {
            if (!activeChunks.ContainsKey(coord))
            {
                if (coordsToMove.Count > 0)
                {
                    Vector3Int oldCoord = coordsToMove[0];
                    coordsToMove.RemoveAt(0);
                    MoveChunk(oldCoord, coord);
                }
                else
                {
                    CreateChunk(coord);
                }
            }
            else
            {
                coordsToMove.Remove(coord);
            }
        }
    }

    private HashSet<Vector3Int> GetSurroundingChunks(Vector3Int centerChunk)
    {
        HashSet<Vector3Int> chunkCoords = new HashSet<Vector3Int>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    chunkCoords.Add(centerChunk + new Vector3Int(x, y, z));
                }
            }
        }
        return chunkCoords;
    }

    private void CreateChunk(Vector3Int coord)
    {
        GameObject chunkObj = Instantiate(chunkPrefab, Vector3.Scale(coord, chunkSize) + chunkSize / 2, Quaternion.identity, transform);
        chunkObj.name = $"Chunk({coord.x},{coord.y},{coord.z})";
        activeChunks[coord] = chunkObj;
        LogDebug($"Created Chunk at {coord}");
    }

    private void MoveChunk(Vector3Int oldCoord, Vector3Int newCoord)
    {
        if (activeChunks.TryGetValue(oldCoord, out GameObject chunk))
        {
            chunk.transform.position = Vector3.Scale(newCoord, chunkSize) + chunkSize / 2;
            chunk.name = $"Chunk({newCoord.x},{newCoord.y},{newCoord.z})";
            activeChunks.Remove(oldCoord);
            activeChunks[newCoord] = chunk;
            LogDebug($"Moved Chunk from {oldCoord} to {newCoord}");
        }
        else
        {
            LogDebug($"Chunk at {oldCoord} not found.");
        }
    }

    private void LogDebug(string message)
    {
        if (enableDebugLogging)
        {
            UnityEngine.Debug.Log(message);
        }
    }

    private void OnDrawGizmos()
    {
        if (visualizeChunkBorders)
        {
            Vector3Int targetChunk = GetTargetChunk();
            DrawChunkBorders(Color.green, activeChunks.Keys);
            DrawChunkBorders(Color.red, GetPotentialChunkBorders(targetChunk));
        }
    }

    private void DrawChunkBorders(Color color, IEnumerable<Vector3Int> chunkCoords)
    {
        Gizmos.color = new Color(color.r, color.g, color.b, borderOpacity);
        foreach (Vector3Int coord in chunkCoords)
        {
            Vector3 chunkCenter = Vector3.Scale(coord, chunkSize) + chunkSize / 2;
            Gizmos.DrawWireCube(chunkCenter, chunkSize);
        }
    }

    private HashSet<Vector3Int> GetPotentialChunkBorders(Vector3Int targetChunk)
    {
        HashSet<Vector3Int> potentialBorders = new HashSet<Vector3Int>();
        for (int x = -extraChunkBorders; x <= extraChunkBorders; x++)
        {
            for (int y = -extraChunkBorders; y <= extraChunkBorders; y++)
            {
                for (int z = -extraChunkBorders; z <= extraChunkBorders; z++)
                {
                    Vector3Int coord = targetChunk + new Vector3Int(x, y, z);
                    if (!activeChunks.ContainsKey(coord))
                    {
                        potentialBorders.Add(coord);
                    }
                }
            }
        }
        return potentialBorders;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 130, 300, 20), $"Current pos: {target.position}");
        GUI.Label(new Rect(10, 150, 300, 20), $"Current Chunk: {GetTargetChunk()}");
    }
}
