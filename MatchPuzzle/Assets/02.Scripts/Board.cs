using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int borderSize = 2;
    [SerializeField] GameObject tilePrefabs;
    [SerializeField] GameObject[] gamePiecePrefabs;
    Tile[,] m_allTiles = null;// Tile 스크립트를 2차원 배열로 선언  -> 대괄호 안에 ','가 없으면 1차원 배열
    GamePiece[,] m_allGamePiece;

    private float swapTime = 0.5f;

    Tile m_clickedTile;
    Tile m_targetTile;
    private void Awake()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePiece = new GamePiece[width, height];
    }

    private void Start()
    {
        SetupTiles();
        SetupCamera();
        FillRandom();
        HighlightMatches();
    }

    public void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate(tilePrefabs, new Vector2(i, j), Quaternion.identity);
                tile.name = "Tile(" + i + "," + j + ")";
                m_allTiles[i, j] = tile.GetComponent<Tile>();
                tile.transform.parent = transform;
                m_allTiles[i, j].Init(i, j, this);
            }
        }
    }

    private void SetupCamera()
    {
        Camera.main.transform.position = new Vector3((float)(width - 1) / 2f, (float)(height - 1) / 2f, -10);
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)height / 2f + (float)borderSize;
        float horizontalSize = ((float)width / 2f + (float)borderSize) / aspectRatio;
        Camera.main.orthographicSize = (verticalSize > horizontalSize ? verticalSize : horizontalSize);
    }

    private GameObject GetRandomGamePiece()
    {
        int ramdomIndex = Random.Range(0, gamePiecePrefabs.Length);
        if (gamePiecePrefabs[ramdomIndex] == null)
            Debug.LogWarning("Board : " + ramdomIndex + "does not contain a valid GamePiece prefab!");
        return gamePiecePrefabs[ramdomIndex];
    }

    public void PlaceGamePiece(GamePiece gamePiece, int x, int y)
    {
        if (gamePiece == null)
        {
            Debug.LogWarning("Board : Invalid FamePieces!");
            return;
        }
        gamePiece.transform.position = new Vector3(x, y);
        gamePiece.transform.rotation = Quaternion.identity;
        if (isWithinBounds(x, y))
            m_allGamePiece[x, y] = gamePiece;
        gamePiece.SetCoord(x, y);
    }
    bool isWithinBounds(int x, int y)
    {
        return (x >= 0 && y >= 0 && x < width && y < height);
    }

    void FillRandom()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);
                if (randomPiece != null)
                {
                    randomPiece.name = randomPiece.name.Replace("(Clone)", "");
                    randomPiece.GetComponent<GamePiece>().Init(this);
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);
                    randomPiece.transform.parent = transform;
                }
            }
        }
    }

    public void ClickTile(Tile tile)
    {
        if (m_clickedTile == null)
        {
            m_clickedTile = tile;

            //Debug.Log("clicked tile: " + tile.name);
        }

    }

    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && InNextTo(m_clickedTile, tile))
        {
            m_targetTile = tile;
            //Debug.Log("Target tile: " + tile.name);
        }
    }
    public void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }

        m_clickedTile = null;
        m_targetTile = null;
    }

    private void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        //두개 gamepiece를 교체
        GamePiece clickedPiece = m_allGamePiece[clickedTile.xIndex, clickedTile.yIndex];
        GamePiece targetPiece = m_allGamePiece[targetTile.xIndex, targetTile.yIndex];

        clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
        targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

    }

    bool InNextTo(Tile start, Tile end)
    {
        //if (Vector3.Distance(start.transform.position, end.transform.position) <= 1)
        //    return true;

        if (start.xIndex == end.xIndex && Mathf.Abs(end.yIndex - start.yIndex) <= 1)
            return true;
        else if (Mathf.Abs(end.xIndex - start.xIndex) <= 1 && start.yIndex == end.yIndex)
            return true;

        return false;
    }

    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;
        if (isWithinBounds(startX, startY))
        {
            startPiece = m_allGamePiece[startX, startY];
        }
        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }
        int nextX;
        int nextY;

        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!isWithinBounds(nextX, nextY))
            {
                break;
            }
            GamePiece nextPiece = m_allGamePiece[nextX, nextY];

            if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }
        }
        if (matches.Count >= minLength)
            return matches;

        return null;
    }

    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> upwardMatches = FindMatches(startX, startY, Vector2.up, 2);
        List<GamePiece> downwardMatches = FindMatches(startX, startY, Vector2.down, 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }
        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }
        //var combineMatches = upwardMatches.Union(downwardMatches).ToList();
        //return (combineMatches.Count >= minLength) ? combineMatches : null;

        foreach (GamePiece piece in downwardMatches)
        {
            if (!upwardMatches.Contains(piece))
            {
                upwardMatches.Add(piece);
            }
        }

        return (upwardMatches.Count >= minLength) ? upwardMatches : null;
    }
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(startX, startY, Vector2.left, 2);
        List<GamePiece> rightMatches = FindMatches(startX, startY, Vector2.right, 2);

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }
        //var combineMatches = leftMatches.Union(rightMatches).ToList();
        //return (combineMatches.Count >= minLength) ? combineMatches : null;

        foreach (GamePiece piece in rightMatches)
        {
            if (!leftMatches.Contains(piece))
            {
                leftMatches.Add(piece);
            }
        }

        return (leftMatches.Count >= minLength) ? leftMatches : null;
    }

    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                SpriteRenderer spriteRenderer = m_allTiles[i, j].GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);

                List<GamePiece> horizontalMatches = FindHorizontalMatches(i, j, 3);
                List<GamePiece> verticalMatches = FindVerticalMatches(i, j, 3);

                if(horizontalMatches == null)
                {
                    horizontalMatches = new List<GamePiece>();
                }
                if (verticalMatches == null)
                {
                    verticalMatches = new List<GamePiece>();
                }

                var combineMatches = horizontalMatches.Union(verticalMatches).ToList();

                if(combineMatches.Count > 0)
                {
                    foreach(GamePiece piece in combineMatches)
                    {
                        spriteRenderer = m_allTiles[piece.xIndex, piece.yIndex].GetComponent<SpriteRenderer>();
                        spriteRenderer.color = piece.GetComponent<SpriteRenderer>().color;  
                    }
                }
            }
        }
    }
}
