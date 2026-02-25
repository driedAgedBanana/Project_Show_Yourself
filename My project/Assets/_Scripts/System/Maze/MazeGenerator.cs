using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MazeGenerator : MonoBehaviour
{
    [Header("Maze Settings")]
    // The bigger the number, the more complex the maze, thus longer loading time
    [Range(5, 200)]
    // Dimension of the maze
    public int mazeWidth = 5, mazeHeight = 5;
    // The position of the maze
    public int startX = 0, startY = 0;
    MazeCell[,] maze;

    [Header("Player")]
    [SerializeField] private GameObject _playerPrefab;

    [Header("Enemies")]
    [SerializeField] private List<GameObject> _enemyPrefabs;
    private List<GameObject> _activeEnemies = new List<GameObject>();
    [SerializeField] private int _minTotalEnemies = 5; // Minimum total in the maze
    [SerializeField] private int _maxTotalEnemies = 15; // Maximum total in the maze
    private Transform _playerTransform; // Store this when you spawn the player
    [SerializeField] private float _minSpawnDistance = 10f; // Don't spawn closer than 10 units

    private Vector2Int _currentCell; // Represent the cell the player is currently looking at


    public MazeCell[,] GetMaze()
    {
        maze = new MazeCell[mazeWidth, mazeHeight];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = new MazeCell(x, y);
            }
        }

        CarvePath(startX, startY);
        return maze;
    }

    List<Direction> directions = new List<Direction>
    {
        Direction.Up, Direction.Down, Direction.Left, Direction.Right
    };

    List<Direction> GetRandomDirections()
    {
        // Generate a copy of the direction list the we can modify without affecting the original list
        List<Direction> dir = new List<Direction>(directions);

        // This list will hold the randomly shuffled directions
        List<Direction> rndDir = new List<Direction>();

        // Randomly shuffle the direction list using the Fisher-Yates algorithm
        while (dir.Count > 0) // While there are still directions to shuffle
        {
            int index = Random.Range(0, dir.Count); // Get random index in the list
            rndDir.Add(dir[index]); // Add the direction at that index to the shuffled list
            dir.RemoveAt(index); // Remove the direction from the original list to avoid duplicates
        }

        // Return the shuffled list of directions
        return rndDir;
    }

    private bool IsCellValid(int x, int y)
    {
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1 || maze[x, y].visited) return false;
        else return true;
    }

    private Vector2Int CheckNextCell()
    {
        List<Direction> rndDir = GetRandomDirections();

        for (int i = 0; i < rndDir.Count; i++)
        {
            Vector2Int nextCell = _currentCell;

            switch (rndDir[i])
            {
                case Direction.Up:
                    nextCell.y++;
                    break;
                case Direction.Down:
                    nextCell.y--;
                    break;
                case Direction.Right:
                    nextCell.x++;
                    break;
                case Direction.Left:
                    nextCell.x--;
                    break;
            }

            // Check if the next cell is valid (within bounds and not visited), if not, try again
            if (IsCellValid(nextCell.x, nextCell.y)) return nextCell;
        }

        return _currentCell; // If no valid cell is found, return the current cell
    }

    private void BreakingWalls(Vector2Int primaryCell, Vector2Int secondaryCell)
    {
        if (primaryCell.x > secondaryCell.x)
        {
            // Primary cell's left wall
            maze[primaryCell.x, primaryCell.y].leftWall = false;
        }
        else if (primaryCell.x < secondaryCell.x)
        {
            // Secondary cell's left wall
            maze[secondaryCell.x, secondaryCell.y].leftWall = false;
        }
        else if (primaryCell.y < secondaryCell.y)
        {
            // Primary cell's top wall
            maze[primaryCell.x, primaryCell.y].topWall = false;
        }
        else if (primaryCell.y > secondaryCell.y)
        {
            // Secondary cell's top wall
            maze[secondaryCell.x, secondaryCell.y].topWall = false;
        }
    }

    // Starting at the x and Y passed in, cave a path through the maze by breaking walls between cells until there are no more valid cells to move to
    // (a dead end is a cell with no valid neighboring cells to move to)
    private void CarvePath(int x, int y)
    {
        // Perform safety check to ensure the starting cell is within bounds and not already visited
        // if not, throw in a small warning and return to prevent errors
        if (x < 0 || y < 0 || x > mazeWidth - 1 || y > mazeHeight - 1)
        {
            Debug.LogWarning("Starting cell is out of bounds. Please provide valid coordinates.");
            return;
        }

        _currentCell = new Vector2Int(x, y);

        // List to keep track of the current path
        List<Vector2Int> paths = new List<Vector2Int>();

        // Loop until there are no more valid cells to move to (i.e., we hit a dead end)
        bool deadEnd = false;
        while (!deadEnd)
        {
            // Get the cell we want to move to next
            Vector2Int nextCell = CheckNextCell();

            // If the next cell is the same as the current cell, it means there are no valid cells to move to, so we have hit a dead end, thus break the loop
            if (nextCell == _currentCell)
            {
                for (int i = paths.Count - 1; i >= 0; i--)
                {
                    _currentCell = paths[i]; // Backtrack to the previous cell in the path
                    paths.RemoveAt(i); // Remove the cell from the path list
                    nextCell = CheckNextCell(); // Check for valid neighboring cells from the backtracked cell

                    // If we find a valid neighboring cell, break out of the backtracking loop and continue carving the path from there
                    if (nextCell != _currentCell)
                    {
                        break;
                    }
                }

                if (nextCell == _currentCell)
                {
                    // If we have backtracked all the way to the starting cell and still find no valid cells, then we are truly at a dead end, thus break the main loop
                    deadEnd = true;
                }
            }
            else
            {
                BreakingWalls(_currentCell, nextCell); // Break the wall between the current cell and the next cell to create a path
                maze[nextCell.x, nextCell.y].visited = true; // Mark the next cell as visited
                _currentCell = nextCell; // Move to the next cell
                paths.Add(_currentCell); // Add the current cell to the path list
            }
        }
    }

    public void SpawnPlayer()
    {
        if (_playerPrefab != null)
        {
            Vector3 spawnPos = new Vector3(startX, 0f, startY);
            GameObject p = Instantiate(_playerPrefab, spawnPos, Quaternion.identity);
            _playerTransform = p.transform; // Save the reference!
        }
    }

    public void SpawnEnemies()
    {
        if (_enemyPrefabs == null || _enemyPrefabs.Count == 0) return;

        // 1. Clean list of dead enemies
        _activeEnemies.RemoveAll(item => item == null);

        // 2. Decide how many to spawn to reach your max
        int targetCount = Random.Range(_minTotalEnemies, _maxTotalEnemies + 1);
        int amountToSpawn = targetCount - _activeEnemies.Count;

        if (amountToSpawn <= 0) return;

        List<Vector2Int> walkableCells = GetWalkableCells();

        for (int i = 0; i < amountToSpawn; i++)
        {
            if (walkableCells.Count == 0) break;

            int randomIndex = Random.Range(0, walkableCells.Count);
            Vector2Int coords = walkableCells[randomIndex];
            Vector3 worldPos = new Vector3(coords.x, 0f, coords.y);

            // 3. SAFETY CHECK: Is this spot too close to the player?
            if (_playerTransform != null)
            {
                if (Vector3.Distance(worldPos, _playerTransform.position) < _minSpawnDistance)
                {
                    // Skip this coordinate and try again
                    walkableCells.RemoveAt(randomIndex);
                    i--; // Decrement i so we don't lose a spawn count
                    continue;
                }
            }

            // 4. Actual Spawning
            NavMeshHit hit;
            if (NavMesh.SamplePosition(worldPos, out hit, 2.0f, NavMesh.AllAreas))
            {
                GameObject enemy = Instantiate(_enemyPrefabs[Random.Range(0, _enemyPrefabs.Count)], hit.position, Quaternion.identity);
                _activeEnemies.Add(enemy);
            }

            walkableCells.RemoveAt(randomIndex);
        }
    }

    private List<Vector2Int> GetWalkableCells()
    {
        List<Vector2Int> walkable = new List<Vector2Int>();
        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                if (maze[x, y].visited)
                {
                    walkable.Add(new Vector2Int(x, y));
                }
            }
        }
        return walkable;
    }

    // Change the subscriber to call a "Delay" version
    private void OnEnable()
    {
        EnemyController.OnEnemyKilled += HandleEnemyDeath;
    }

    private void HandleEnemyDeath()
    {
        // Wait 3 seconds, then replenish
        StartCoroutine(WaitAndReplenish(3f));
    }

    private IEnumerator WaitAndReplenish(float delay)
    {
        yield return new WaitForSeconds(delay);
        SpawnEnemies();
    }

    public void OnDisable()
    {
        EnemyController.OnEnemyKilled -= SpawnEnemies; // Unsubscribe from the event when the object is disabled to prevent memory leaks
    }
}

public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public class MazeCell
{
    public bool visited;
    public int x, y;

    public bool topWall;
    public bool leftWall;

    // Return x and y as a vector2int for convenience
    public Vector2Int Position
    {
        get { return new Vector2Int(x, y); }
    }

    public MazeCell(int x, int y)
    {
        // The coordinates of the cell in the maze grid
        this.x = x;
        this.y = y;

        // Whether the cell has been visited during maze generation
        visited = false;

        // All the walls are initially present
        topWall = leftWall = true;
    }
}
