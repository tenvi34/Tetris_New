using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    [Header("Editor Objects")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private Transform backgroundNode;
    [SerializeField] private Transform boardNode;
    [SerializeField] private Transform tetrominoNode;

    [Header("Game Settings")]
    [SerializeField, Range(4, 40)] private int boardWidth = 10;
    [SerializeField, Range(5, 20)] private int boardHeight = 20;
    [SerializeField] private float fallCycle = 1.0f;

    public GameObject TilePrefab => tilePrefab;
    public Transform BackgroundNode => backgroundNode;
    public Transform BoardNode => boardNode;
    public Transform TetrominoNode => tetrominoNode;

    public int BoardWidth => boardWidth;
    public int BoardHeight => boardHeight;
    public float FallCycle => fallCycle;

    private int halfWidth;
    private int halfHeight;
    private float nextFallTime;
    private Transform[,] boardState;

    void Start()
    {
        halfWidth = Mathf.RoundToInt(boardWidth * 0.5f);
        halfHeight = Mathf.RoundToInt(boardHeight * 0.5f);
        nextFallTime = Time.time + fallCycle;

        boardState = new Transform[boardWidth, boardHeight];
        CreateBackground();
        CreateTetromino();
    }

    void Update()
    {
        HandleInput();
        HandleFall();
    }

    private void HandleInput()
    {
        Vector3 moveDir = Vector3.zero;
        bool isRotate = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) moveDir.x = -1;
        else if (Input.GetKeyDown(KeyCode.RightArrow)) moveDir.x = 1;
        if (Input.GetKeyDown(KeyCode.DownArrow)) moveDir.y = -1;
        if (Input.GetKeyDown(KeyCode.Space)) isRotate = true;

        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            while (MoveTetromino(Vector3.down, false)) { }
        }

        if (moveDir != Vector3.zero || isRotate)
        {
            MoveTetromino(moveDir, isRotate);
        }
    }

    private void HandleFall()
    {
        if (Time.time > nextFallTime)
        {
            nextFallTime = Time.time + fallCycle;
            MoveTetromino(Vector3.down, false);
        }
    }

    private bool MoveTetromino(Vector3 moveDir, bool isRotate)
    {
        Vector3 oldPos = tetrominoNode.transform.position;
        Quaternion oldRot = tetrominoNode.transform.rotation;

        tetrominoNode.transform.position += moveDir;
        if (isRotate)
        {
            tetrominoNode.transform.rotation *= Quaternion.Euler(0, 0, 90);
        }

        if (!CanMoveTo(tetrominoNode))
        {
            tetrominoNode.transform.position = oldPos;
            tetrominoNode.transform.rotation = oldRot;

            if (moveDir == Vector3.down && !isRotate)
            {
                AddToBoard(tetrominoNode);
                CheckBoard();
                CreateTetromino();
            }

            return false;
        }

        return true;
    }

    private bool CanMoveTo(Transform root)
    {
        foreach (Transform node in root)
        {
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            if (x < 0 || x >= boardWidth || y < 0) return false;
            if (y < boardHeight && boardState[x, y] != null) return false;
        }

        return true;
    }

    private void CreateBackground()
    {
        Color color = Color.gray;
        color.a = 0.5f;

        for (int x = -halfWidth; x < halfWidth; ++x)
        {
            for (int y = halfHeight; y > -halfHeight; --y)
            {
                CreateTile(backgroundNode, new Vector2(x, y), color, 0);
            }
        }

        color.a = 1.0f;
        for (int y = halfHeight; y > -halfHeight; --y)
        {
            CreateTile(backgroundNode, new Vector2(-halfWidth - 1, y), color, 0);
            CreateTile(backgroundNode, new Vector2(halfWidth, y), color, 0);
        }

        for (int x = -halfWidth - 1; x <= halfWidth; ++x)
        {
            CreateTile(backgroundNode, new Vector2(x, -halfHeight), color, 0);
        }
    }

    private TileScript CreateTile(Transform parent, Vector2 position, Color color, int order = 1)
    {
        var newTile = Instantiate(tilePrefab, parent);
        newTile.transform.localPosition = position;

        var tile = newTile.GetComponent<TileScript>();
        tile.Color = color;
        tile.SortingOrder = order;

        return tile;
    }

    private void CreateTetromino()
    {
        int index = Random.Range(0, 7);
        Color32 color = Color.white;
        tetrominoNode.rotation = Quaternion.identity;
        tetrominoNode.position = new Vector2(0, halfHeight);

        Vector2[] positions = new Vector2[4];
        switch (index)
        {
            case 0: color = new Color32(115, 251, 253, 255); positions = new[] { new Vector2(-2f, 0f), new Vector2(-1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f) }; break;
            case 1: color = new Color32(0, 33, 245, 255); positions = new[] { new Vector2(-1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(-1f, 1f) }; break;
            case 2: color = new Color32(243, 168, 59, 255); positions = new[] { new Vector2(-1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f) }; break;
            case 3: color = new Color32(255, 253, 84, 255); positions = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) }; break;
            case 4: color = new Color32(117, 250, 76, 255); positions = new[] { new Vector2(-1f, -1f), new Vector2(0f, -1f), new Vector2(0f, 0f), new Vector2(1f, 0f) }; break;
            case 5: color = new Color32(155, 47, 246, 255); positions = new[] { new Vector2(-1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f) }; break;
            case 6: color = new Color32(235, 51, 35, 255); positions = new[] { new Vector2(-1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f) }; break;
        }

        foreach (var pos in positions)
        {
            CreateTile(tetrominoNode, pos, color);
        }
    }

    private void AddToBoard(Transform root)
    {
        foreach (Transform node in root)
        {
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            boardState[x, y] = node;
            node.parent = boardNode;
            node.name = x.ToString();
        }
    }

    private void CheckBoard()
    {
        for (int y = 0; y < boardHeight; ++y)
        {
            if (IsRowComplete(y))
            {
                ClearRow(y);
                ShiftDown(y);
                --y;
            }
        }
    }

    private bool IsRowComplete(int y)
    {
        for (int x = 0; x < boardWidth; ++x)
        {
            if (boardState[x, y] == null) return false;
        }
        return true;
    }

    private void ClearRow(int y)
    {
        for (int x = 0; x < boardWidth; ++x)
        {
            Destroy(boardState[x, y].gameObject);
            boardState[x, y] = null;
        }
    }

    private void ShiftDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < boardHeight; ++y)
        {
            for (int x = 0; x < boardWidth; ++x)
            {
                if (boardState[x, y] != null)
                {
                    boardState[x, y - 1] = boardState[x, y];
                    boardState[x, y] = null;
                    boardState[x, y - 1].position += Vector3.down;
                }
            }
        }
    }
}
