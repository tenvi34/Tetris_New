using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class StageScript : MonoBehaviour
{
    [Header("Editor Objects")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject ghostTilePrefab;

    [SerializeField] private Transform backgroundNode;
    [SerializeField] private Transform boardNode;
    [SerializeField] private Transform tetrominoNode;
    private Transform ghostTetrominoNode;

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

    void Start()
    {
        halfWidth = Mathf.RoundToInt(boardWidth * 0.5f);
        halfHeight = Mathf.RoundToInt(boardHeight * 0.5f);

        nextFallTime = Time.time + fallCycle;

        CreateBackground();

        for (int i = 0; i < boardHeight; ++i)
        {
            var col = new GameObject((boardHeight - i - 1).ToString());
            col.transform.position = new Vector3(0, halfHeight - i, 0);
            col.transform.parent = boardNode;
        }

        CreateTetromino();
        CreateGhostTetromino();
    }

    void Update()
    {
        Vector3 moveDir = Vector3.zero;
        bool isRotate = false;

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            moveDir.x = -1;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            moveDir.x = 1;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isRotate = true;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            moveDir.y = -1;
        }

        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            while (MoveTetromino(Vector3.down, false)) { }
        }

        if (Time.time > nextFallTime)
        {
            nextFallTime = Time.time + fallCycle;
            moveDir = Vector3.down;
            isRotate = false;
        }

        if (moveDir != Vector3.zero || isRotate)
        {
            MoveTetromino(moveDir, isRotate);
        }

        UpdateGhostTetromino();
    }

    bool MoveTetromino(Vector3 moveDir, bool isRotate)
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

            if ((int)moveDir.y == -1 && (int)moveDir.x == 0 && !isRotate)
            {
                AddToBoard(tetrominoNode);
                CheckBoardColumn();
                CreateTetromino();
                CreateGhostTetromino();
            }

            return false;
        }

        return true;
    }

    bool CanMoveTo(Transform root)
    {
        for (int i = 0; i < root.childCount; ++i)
        {
            var node = root.GetChild(i);
            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            if (x < 0 || x >= boardWidth || y < 0) return false;

            var column = boardNode.Find(y.ToString());
            if (column != null && column.Find(x.ToString()) != null) return false;
        }
        return true;
    }

    void CreateBackground()
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

    TileScript CreateTile(Transform parent, Vector2 position, Color color, int order = 1)
    {
        var newTile = Instantiate(tilePrefab);
        newTile.transform.parent = parent;
        newTile.transform.localPosition = position;

        var tile = newTile.GetComponent<TileScript>();
        tile.Color = color;
        tile.SortingOrder = order;

        return tile;
    }

    void CreateTetromino()
    {
        int index = Random.Range(0, 7);
        Color32 color = Color.white;

        tetrominoNode.rotation = Quaternion.identity;
        tetrominoNode.position = new Vector2(0, halfHeight);

        switch (index)
        {
            case 0:
                color = new Color32(115, 251, 253, 255);
                CreateTile(tetrominoNode, new Vector2(-2f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                break;

            case 1:
                color = new Color32(0, 33, 245, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(-1f, 1.0f), color);
                break;

            case 2:
                color = new Color32(243, 168, 59, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0.0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 1.0f), color);
                break;

            case 3:
                color = new Color32(255, 253, 84, 255);
                CreateTile(tetrominoNode, new Vector2(0f, 0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 1f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 1f), color);
                break;

            case 4:
                color = new Color32(117, 250, 76, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, -1f), color);
                CreateTile(tetrominoNode, new Vector2(0f, -1f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0f), color);
                break;

            case 5:
                color = new Color32(155, 47, 246, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 1f), color);
                break;

            case 6:
                color = new Color32(235, 51, 35, 255);
                CreateTile(tetrominoNode, new Vector2(-1f, 1f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 1f), color);
                CreateTile(tetrominoNode, new Vector2(0f, 0f), color);
                CreateTile(tetrominoNode, new Vector2(1f, 0f), color);
                break;
        }
    }

    void CreateGhostTetromino()
    {
        if (ghostTetrominoNode != null)
        {
            Destroy(ghostTetrominoNode.gameObject);
        }

        ghostTetrominoNode = new GameObject("GhostTetromino").transform;
        ghostTetrominoNode.parent = transform;

        foreach (Transform child in tetrominoNode)
        {
            var ghostTile = Instantiate(ghostTilePrefab, child.position, child.rotation, ghostTetrominoNode);
            var tileScript = ghostTile.GetComponent<TileScript>();
            tileScript.Color = new Color(1, 1, 1, 0.3f); // 반투명 색상
        }
        MoveGhostTetrominoToBottom();
    }

    void UpdateGhostTetromino()
    {
        // 고스트 블럭의 자식들만 업데이트
        for (int i = 0; i < ghostTetrominoNode.childCount; i++)
        {
            Transform ghostChild = ghostTetrominoNode.GetChild(i);
            Transform tetrominoChild = tetrominoNode.GetChild(i);

            ghostChild.position = tetrominoChild.position;
            ghostChild.rotation = tetrominoChild.rotation;
        }
        MoveGhostTetrominoToBottom();
    }

    void MoveGhostTetrominoToBottom()
    {
        while (CanMoveTo(ghostTetrominoNode))
        {
            ghostTetrominoNode.position += Vector3.down;
        }
        ghostTetrominoNode.position += Vector3.up; // 한 칸 올라가서 최종 위치 설정
    }

    void AddToBoard(Transform root)
    {
        while (root.childCount > 0)
        {
            var node = root.GetChild(0);

            int x = Mathf.RoundToInt(node.transform.position.x + halfWidth);
            int y = Mathf.RoundToInt(node.transform.position.y + halfHeight - 1);

            node.parent = boardNode.Find(y.ToString());
            node.name = x.ToString();
        }
    }

    void CheckBoardColumn()
    {
        bool isCleared = false;

        foreach (Transform column in boardNode)
        {
            if (column.childCount == boardWidth)
            {
                foreach (Transform tile in column)
                {
                    Destroy(tile.gameObject);
                }

                column.DetachChildren();
                isCleared = true;
            }
        }

        if (isCleared)
        {
            for (int i = 1; i < boardNode.childCount; ++i)
            {
                var column = boardNode.Find(i.ToString());

                if (column.childCount == 0)
                    continue;

                int emptyCol = 0;
                int j = i - 1;
                while (j >= 0)
                {
                    if (boardNode.Find(j.ToString()).childCount == 0)
                    {
                        emptyCol++;
                    }

                    j--;
                }

                if (emptyCol > 0)
                {
                    var targetColumn = boardNode.Find((i - emptyCol).ToString());

                    while (column.childCount > 0)
                    {
                        Transform tile = column.GetChild(0);
                        tile.parent = targetColumn;
                        tile.transform.position += new Vector3(0, -emptyCol, 0);
                    }

                    column.DetachChildren();
                }
            }
        }
    }
}
