using UnityEngine;
using Unity.AI.Navigation;

public class MazeRenderer : MonoBehaviour
{
    [SerializeField] private MazeGenerator _mazeGenerator;
    [SerializeField] private GameObject _mazeCellPrefab;
    [SerializeField] private NavMeshSurface navMeshSurface;

    // Physical size of each cell in the maze, used to position the cells correctly in the world
    public float CellSize = 1f;

    private void Awake()
    {
        // 0. Spawn in global volume first
        _mazeGenerator.SpawnGlobalVolume();
    }

    private void Start()
    {
        // 1. Generate the maze and instantiate the cell prefabs
        MazeCell[,] maze = _mazeGenerator.GetMaze();
        Vector2Int exitCoords = _mazeGenerator.GetExitCoordinates();

        for (int x = 0; x < maze.GetLength(0); x++)
        {
            for (int y = 0; y < maze.GetLength(1); y++)
            {
                // Instantiate a new maze cell prefab at the correct position based on its coordinates in the maze array
                GameObject newCell = Instantiate(_mazeCellPrefab, new Vector3((float)x * CellSize, 0f, (float)y * CellSize), Quaternion.identity, transform);

                // Get a reference to the MazeCellObject
                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();

                // Decide which walls to activate based on the properties of the maze cell
                bool topWall = maze[x, y].topWall;
                bool leftWall = maze[x, y].leftWall;

                // Bottom and right walls are deactivated by default
                bool rightWall = false;
                bool bottomWall = false;

                if (x == _mazeGenerator.mazeWidth - 1) rightWall = true;
                if(y == 0) bottomWall = true;

                // Check if this specific cell is the exit
                bool isThisTheExit = (x == exitCoords.x && y == exitCoords.y);

                mazeCell.Initialize(topWall, bottomWall, rightWall, leftWall, isThisTheExit);
            }
        }

        // 2. Spawn Moving Walls (Do this BEFORE baking so NavMesh sees them)
        // Note: If they have NavMeshObstacle + Carve, they can be spawned after, 
        // but it's safer to have them ready now.
        _mazeGenerator.SpawnMovingWalls();

        // 3. BAKE THE NAVMESH
        // This creates the floor that SamplePosition needs to work!
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
        }

        // 4. NOW spawn the entities
        _mazeGenerator.SpawnPlayer();
        _mazeGenerator.SpawnEnemies();
    }
}
