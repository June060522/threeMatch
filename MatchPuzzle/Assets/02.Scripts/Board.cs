using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int borderSize = 2;
    [SerializeField] GameObject tileNormalPrefabs;
    [SerializeField] GameObject tileObstaclePrefabs;
    [SerializeField] GameObject[] gamePiecePrefabs;
    Tile[,] m_allTiles = null;
    GamePiece[,] m_allGamePiece;

    private float swapTime = 0.5f;

    private bool m_playerInputEnable = true;

    Tile m_clickedTile;
    Tile m_targetTile;

    public StartingObject[] startingTiles;
    public StartingObject[] startingGamePieces;

    ParticleManager m_particleManager;
    [System.Serializable]
    public class StartingObject
    {
        public GameObject Prefab;
        public int x;
        public int y;
        public int z;
    }

    private void Awake()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePiece = new GamePiece[width, height];
        m_particleManager = FindObjectOfType<ParticleManager>();
    }

    private void Start()
    {
        m_allTiles = new Tile[width, height];
        m_allGamePiece = new GamePiece[width, height];
        SetupTiles();
        SetupGamePiece();
        SetupCamera();
        FillBoard(10, 0.5f);
        //HighlightMatches();
    }

    public void SetupTiles()
    {
        foreach (StartingObject sTile in startingTiles)
        {
            if (sTile != null)
                MakeTile(sTile.Prefab, sTile.x, sTile.y, sTile.z);
        }
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allTiles[i, j] == null)
                    MakeTile(tileNormalPrefabs, i, j);
            }
        }
    }

    private void SetupGamePiece()
    {
        foreach (StartingObject sPiece in startingGamePieces)
        {
            if (sPiece != null)
            {
                GameObject piece = Instantiate(sPiece.Prefab, new Vector3(sPiece.x, sPiece.y), Quaternion.identity);
                MakeGamePiece(piece, sPiece.x, sPiece.y, 10, 0.1f);
            }
        }
    }

    private void MakeTile(GameObject prefab, int x, int y, int z = 0)
    {
        if (prefab != null)
        {
            GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
            tile.name = "Tile(" + x + "," + y + ")";
            m_allTiles[x, y] = tile.GetComponent<Tile>();
            tile.transform.parent = transform;
            m_allTiles[x, y].Init(x, y, this);
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
                FillRandomAt(i, j);
            }
        }
    }

    void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
    {
        int maxInterations = 100;
        int iterations = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePiece[i, j] == null && m_allTiles[i, j].tileType != TileType.Obstacle)
                {
                    GamePiece piece = FillRandomAt(i, j, falseYOffset, moveTime);
                    while (HasMatchOnFill(i, j))
                    {
                        ClearPieceAt(i, j);
                        piece = FillRandomAt(i, j, falseYOffset, moveTime);
                        iterations++;
                        if (iterations >= maxInterations)
                        {
                            Debug.Log($"iterations Count : {iterations} is over maxInterations : {maxInterations}");
                            break;
                        }
                    }
                }
            }
        }
    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, Vector2.left, minLength);
        List<GamePiece> downMatches = FindMatches(x, y, Vector2.down, minLength);

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        if (downMatches == null)
        {
            downMatches = new List<GamePiece>();
        }

        return (leftMatches.Count > 0 || downMatches.Count > 0);
    }

    private GamePiece FillRandomAt(int x, int y, int falseOffset = 0, float moveTime = 0.1f)
    {
        if (isWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);
            MakeGamePiece(randomPiece, x, y, falseOffset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    private void MakeGamePiece(GameObject prefab, int x, int y, int falseOffset = 0, float moveTime = 0.1f)
    {
        if (prefab != null)
            prefab.name = prefab.name.Replace("(Clone)", "");
        prefab.GetComponent<GamePiece>().Init(this);
        PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);

        if (falseOffset != 0)
        {
            prefab.transform.position = new Vector3(x, y + falseOffset, 0);
            prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
        }
        prefab.transform.parent = transform;
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
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {
        if (m_playerInputEnable)
        {

            GamePiece clickedPiece = m_allGamePiece[clickedTile.xIndex, clickedTile.yIndex];
            GamePiece targetPiece = m_allGamePiece[targetTile.xIndex, targetTile.yIndex];

            if (clickedPiece != null && targetPiece != null)
            {

                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

                if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);

                    yield return new WaitForSeconds(swapTime);
                }
                else
                {
                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
                    // ClearPieceAt(clickedPieceMatches);
                    // ClearPieceAt(targetPieceMatches);

                    // CollapseColumn(clickedPieceMatches);
                    // CollapseColumn(targetPieceMatches);
                }
                //HighlightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                //HighlightMatchesAt(targetTile.xIndex, targetTile.yIndex);
            }
        }

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
            if (nextPiece == null)
            {
                break;
            }
            else
            {
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
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

    List<GamePiece> FindAllMatches()
    {
        List<GamePiece> combinedMatches = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                List<GamePiece> matches = FindMatchesAt(i, j);
                combinedMatches = combinedMatches.Union(matches).ToList();
            }
        }
        return combinedMatches;
    }

    void HighlightTileOff(int x, int y)
    {
        if (m_allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
        }
    }

    void HighlightTileOn(int x, int y, Color color)
    {
        if (m_allTiles[x, y].tileType != TileType.Breakable)
        {
            SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
            spriteRenderer.color = color;
        }
    }

    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);
            }
        }
    }

    void HighlightMatchesAt(int i, int j)
    {
        HighlightTileOff(i, j);
        List<GamePiece> combineMatches = FindMatchesAt(i, j);

        if (combineMatches.Count > 0)
        {
            foreach (GamePiece piece in combineMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    void HighlightPieces(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    private List<GamePiece> FindMatchesAt(int i, int j, int minLength = 3)
    {
        List<GamePiece> horizontalMatches = FindHorizontalMatches(i, j, minLength);
        List<GamePiece> verticalMatches = FindVerticalMatches(i, j, minLength);

        if (horizontalMatches == null)
        {
            horizontalMatches = new List<GamePiece>();
        }
        if (verticalMatches == null)
        {
            verticalMatches = new List<GamePiece>();
        }

        var combineMatches = horizontalMatches.Union(verticalMatches).ToList();
        return combineMatches;
    }

    private List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        foreach (GamePiece piece in gamePieces)
        {
            matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
        }
        return matches;
    }

    void ClearPieceAt(int x, int y)
    {
        GamePiece pieceToClear = m_allGamePiece[x, y];
        if (pieceToClear != null)
        {
            m_allGamePiece[x, y] = null;
            Destroy(pieceToClear.gameObject);
        }

        //HighlightTileOff(x, y);
    }

    void ClearPieceAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);

                if (m_particleManager != null)
                    m_particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
            }

        }
    }

    private void BreakTileAt(int x, int y)
    {
        Tile tileToBreak = m_allTiles[x, y];
        if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
        {
            if (m_particleManager != null)
                m_particleManager.BreakTileFXAt(tileToBreak.breakableValue, x, y);
            tileToBreak.BreakTile();
        }
    }

    void BreakTileAt(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                BreakTileAt(piece.xIndex, piece.yIndex);
            }
        }
    }

    void ClearBoard()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                ClearPieceAt(i, j);
            }
        }
    }

    List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
    {
        List<GamePiece> movingPiece = new List<GamePiece>();
        for (int i = 0; i < height - 1; i++)
        {
            if (m_allGamePiece[column, i] == null && m_allTiles[column, i].tileType != TileType.Obstacle)
            {
                for (int j = i + 1; j < height; j++)
                {
                    if (m_allGamePiece[column, j] != null)
                    {
                        m_allGamePiece[column, j].Move(column, i, collapseTime * (j - i));
                        m_allGamePiece[column, i] = m_allGamePiece[column, j];
                        m_allGamePiece[column, i].SetCoord(column, i);

                        if (!movingPiece.Contains(m_allGamePiece[column, i]))
                        {
                            movingPiece.Add(m_allGamePiece[column, i]);
                        }
                        m_allGamePiece[column, j] = null;
                        break;
                    }
                }
            }
        }

        return movingPiece;
    }

    List<GamePiece> CollapseColumn(List<GamePiece> gamePiece)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<int> colmnsToCollapse = GetColumns(gamePiece);

        foreach (int column in colmnsToCollapse)
        {
            movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
        }
        return movingPieces;
    }

    List<int> GetColumns(List<GamePiece> gamePiece)
    {
        List<int> columns = new List<int>();
        foreach (GamePiece piece in gamePiece)
        {
            if (!columns.Contains(piece.xIndex))
            {
                columns.Add(piece.xIndex);
            }
        }
        return columns;
    }

    void ClearAndRefillBoard(List<GamePiece> gamePiece)
    {
        StartCoroutine(ClearAndRefillBoardRoutine(gamePiece));
    }

    IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePiece)
    {
        m_playerInputEnable = false;
        List<GamePiece> matches = gamePiece;

        do
        {
            yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            yield return null;
            yield return StartCoroutine(RefillRoutine());

            matches = FindAllMatches();
            yield return new WaitForSeconds(0.5f);
        } while (matches.Count != 0);
        m_playerInputEnable = true;
    }

    IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePiece)
    {
        List<GamePiece> movingPieces = new List<GamePiece>();
        List<GamePiece> matches = new List<GamePiece>();

        //HighlightPieces(gamePiece);
        yield return new WaitForSeconds(0.2f);
        bool isFinished = false;

        while (!isFinished)
        {
            ClearPieceAt(gamePiece);
            BreakTileAt(gamePiece);
            yield return new WaitForSeconds(0.25f);
            movingPieces = CollapseColumn(gamePiece);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            matches = FindMatchesAt(movingPieces);

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
        }
        yield return null;
    }

    IEnumerator RefillRoutine()
    {
        FillBoard(10, 0.5f);
        yield return null;
    }

    bool IsCollapsed(List<GamePiece> gamePieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
                if (piece.transform.position.y - (float)piece.yIndex > 0.01f)
                    return false;
        }
        return true;
    }
}
