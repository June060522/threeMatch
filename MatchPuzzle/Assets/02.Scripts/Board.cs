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

    [SerializeField] GameObject adjacentBombPrefab;
    [SerializeField] GameObject rowBombPrefab;
    [SerializeField] GameObject columnBombPrefab;
    [SerializeField] GameObject colorBombPrefab;

    public int maxCollectibles = 3;
    public int collectibleCount = 0;

    [Range(0, 1)]
    public float changeForCollectible = 0.1f;
    public GameObject[] collectiblePrefabs;

    Tile[,] m_allTiles = null;
    GamePiece[,] m_allGamePiece;

    GameObject m_clickedTileBomb;
    GameObject m_targetTileBomb;

    public float swapTime = 0.5f;
    public int falseYOffset = 10;
    public float moveTime = 0.5f;

    Tile m_clickedTile;
    Tile m_targetTile;

    public StartingObject[] startingTiles;
    public StartingObject[] startingGamePieces;

    private bool m_playerInputEnable = true;
    private int m_scoreMultiplierm = 1;

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
        SetupGamePiece();
        SetupTiles();

        List<GamePiece> startingCollectibles = FindAllCollectibles();
        collectibleCount = startingCollectibles.Count;

        SetupCamera();
        FillBoard(falseYOffset, moveTime);
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

    GameObject GetRandomObject(GameObject[] objectArray)
    {
        int randomIndex = Random.Range(0, objectArray.Length);
        if (objectArray[randomIndex] == null)
        {
            Debug.LogWarning("Error : Board.GetRandomObject at index " + randomIndex + "does not contain a valid GamePiece prefab!");
        }
        return objectArray[randomIndex];
    }

    GameObject GetRandomGamePiece()
    {
        return GetRandomObject(gamePiecePrefabs);
    }

    GameObject GetRandomCollectible()
    {
        return GetRandomObject(collectiblePrefabs);
    }

    private GameObject GetRandomPiece()
    {
        int randomIndex = Random.Range(0, gamePiecePrefabs.Length);
        if (gamePiecePrefabs[randomIndex] == null)
            Debug.LogWarning("Board : " + randomIndex + "does not contain a valid GamePiece prefab!");
        return gamePiecePrefabs[randomIndex];
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
                FillRandomGamePieceAt(i, j);
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
                    GamePiece piece = null;
                    if (j == height - 1 && CanAddCollectible())
                    {
                        piece = FillRandomCollectibleAt(i, j, falseYOffset, moveTime);
                        collectibleCount++;
                    }
                    else
                    {
                        piece = FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
                        while (HasMatchOnFill(i, j))
                        {
                            ClearPieceAt(i, j);
                            piece = FillRandomGamePieceAt(i, j, falseYOffset, moveTime);
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
    }

    bool CanAddCollectible()
    {
        return (collectibleCount < maxCollectibles && collectiblePrefabs.Length > 0 && Random.Range(0f, 1f) <= changeForCollectible);
    }

    private bool HasMatchOnFill(int x, int y, int minLength = 3)
    {
        List<GamePiece> leftMatches = FindMatches(x, y, Vector2.left, minLength);
        List<GamePiece> downMatches = FindMatches(x, y, Vector2.down, minLength);
        List<GamePiece> rightMatches = FindMatches(x, y, Vector2.right, minLength);
        List<GamePiece> upMatches = FindMatches(x, y, Vector2.up, minLength);

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }
        if (downMatches == null)
        {
            downMatches = new List<GamePiece>();
        }
        if (upMatches == null)
        {
            upMatches = new List<GamePiece>();
        }
        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        return (leftMatches.Count > 0 || downMatches.Count > 0 || rightMatches.Count > 0 || upMatches.Count > 0);
    }

    private GamePiece FillRandomGamePieceAt(int x, int y, int falseOffset = 0, float moveTime = 0.1f)
    {
        if (isWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity);
            MakeGamePiece(randomPiece, x, y, falseOffset, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    GamePiece FillRandomCollectibleAt(int x, int y, int falseOffSet = 0, float moveTime = 0.1f)
    {
        if (isWithinBounds(x, y))
        {
            GameObject randomPiece = Instantiate(GetRandomCollectible(), Vector3.zero, Quaternion.identity);
            MakeGamePiece(randomPiece, x, y, falseOffSet, moveTime);
            return randomPiece.GetComponent<GamePiece>();
        }
        return null;
    }

    GameObject MakeBomb(GameObject prefab, int x, int y)
    {
        if (prefab != null && isWithinBounds(x, y))
        {
            GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity);
            bomb.GetComponent<Bomb>().Init(this);
            bomb.GetComponent<Bomb>().SetCoord(x, y);
            bomb.transform.parent = transform;
            return bomb;
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

            if (clickedPiece != null && targetPiece != null && clickedPiece != targetPiece)
            {

                clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

                yield return new WaitForSeconds(swapTime);

                List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
                List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);
                List<GamePiece> colorMatches = new List<GamePiece>();

                if (IsColorBomb(clickedPiece) && !IsColorBomb(targetPiece))
                {
                    clickedPiece.matchValue = targetPiece.matchValue;
                    colorMatches = FindAllMatchValue(clickedPiece.matchValue);
                }
                else if (!IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                {
                    targetPiece.matchValue = clickedPiece.matchValue;
                    colorMatches = FindAllMatchValue(targetPiece.matchValue);
                }
                else if (IsColorBomb(clickedPiece) && IsColorBomb(targetPiece))
                {
                    foreach (GamePiece gamePiece in m_allGamePiece)
                    {
                        if (!colorMatches.Contains(gamePiece))
                        {
                            colorMatches.Add(gamePiece);
                        }
                    }
                }

                if (clickedPieceMatches.Count == 0 && targetPieceMatches.Count == 0 && colorMatches.Count == 0)
                {
                    clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                    targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);

                    yield return new WaitForSeconds(swapTime);
                }
                else
                {
                    Vector2 swapDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex, targetTile.yIndex - clickedTile.yIndex);
                    m_clickedTileBomb = DropBomb(clickedTile.xIndex, clickedTile.yIndex, swapDirection, clickedPieceMatches);
                    m_targetTileBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swapDirection, targetPieceMatches);

                    if (m_clickedTileBomb != null && targetPiece != null)
                    {
                        GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(clickedBombPiece))
                        {
                            clickedBombPiece.ChangeColor(targetPiece);
                        }
                    }
                    if (m_targetTileBomb != null && clickedPiece != null)
                    {
                        GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();
                        if (!IsColorBomb(targetBombPiece))
                        {
                            targetBombPiece.ChangeColor(clickedPiece);
                        }
                    }


                    ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList().Union(colorMatches).ToList());
                }
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
                if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece) && nextPiece.matchValue != MatchValue.None)
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

    void ClearPieceAt(List<GamePiece> gamePieces, List<GamePiece> bombedPieces)
    {
        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                ClearPieceAt(piece.xIndex, piece.yIndex);
                
                int bonus = 0;
                if(gamePieces.Count >= 4)
                    bonus = 20;

                piece.ScorePoints(m_scoreMultiplierm,bonus);
                if (m_particleManager != null)
                {
                    if (bombedPieces.Contains(piece))
                    {
                        m_particleManager.BombFXAt(piece.xIndex, piece.yIndex);
                    }
                    else
                    {
                        m_particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
                    }
                }
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

        m_scoreMultiplierm = 0;
        do
        {
            m_scoreMultiplierm++;
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
            List<GamePiece> bombedPieces = GetBombedPieces(gamePiece);
            gamePiece = gamePiece.Union(bombedPieces).ToList();

            bombedPieces = GetBombedPieces(gamePiece);
            gamePiece = gamePiece.Union(bombedPieces).ToList();

            List<GamePiece> collectedPieces = FindCollectiblesAt(0, true);

            List<GamePiece> allCollectibles = FindAllCollectibles();
            List<GamePiece> blockers = gamePiece.Intersect(allCollectibles).ToList();
            collectedPieces = collectedPieces.Union(blockers).ToList();
            collectibleCount -= collectedPieces.Count;

            gamePiece = gamePiece.Union(collectedPieces).ToList();

            ClearPieceAt(gamePiece, bombedPieces);
            BreakTileAt(gamePiece);

            if (m_clickedTileBomb != null)
            {
                ActiveBomb(m_clickedTileBomb);
                m_clickedTileBomb = null;
            }
            if (m_targetTileBomb != null)
            {
                ActiveBomb(m_targetTileBomb);
                m_targetTile = null;
            }
            yield return new WaitForSeconds(0.25f);

            movingPieces = CollapseColumn(gamePiece);

            while (!IsCollapsed(movingPieces))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            matches = FindMatchesAt(movingPieces);
            collectedPieces = FindCollectiblesAt(0,true);
            matches = matches.Union(collectedPieces).ToList();

            if (matches.Count == 0)
            {
                isFinished = true;
                break;
            }
            else
            {
                m_scoreMultiplierm++;
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
            }
        }
        yield return null;
    }

    IEnumerator RefillRoutine()
    {
        FillBoard(falseYOffset, moveTime);
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

    List<GamePiece> GetRowPieces(int row)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            if (m_allGamePiece[i, row] != null)
            {
                gamePieces.Add(m_allGamePiece[i, row]);
            }
        }

        return gamePieces;
    }

    List<GamePiece> GetColumnPieces(int column)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            if (m_allGamePiece[column, i] != null)
            {
                gamePieces.Add(m_allGamePiece[column, i]);
            }
        }

        return gamePieces;
    }

    List<GamePiece> GetAdjacentPieces(int x, int y, int offset = 1)
    {
        List<GamePiece> gamePieces = new List<GamePiece>();

        for (int i = x - offset; i <= x + offset; i++)
        {
            for (int j = y - offset; j <= y + offset; j++)
            {
                if (isWithinBounds(i, j))
                {
                    gamePieces.Add(m_allGamePiece[i, j]);
                }
            }
        }
        return gamePieces;
    }

    List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
    {
        List<GamePiece> allPiecedToClear = new List<GamePiece>();

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                List<GamePiece> piecesToClear = new List<GamePiece>();

                Bomb bomb = piece.GetComponent<Bomb>();
                if (bomb != null)
                {
                    switch (bomb.bombType)
                    {
                        case BombType.Column:
                            piecesToClear = GetColumnPieces(bomb.xIndex);
                            break;
                        case BombType.Row:
                            piecesToClear = GetRowPieces(bomb.yIndex);
                            break;
                        case BombType.Adjacent:
                            piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
                            break;
                        case BombType.Color:
                            break;
                    }
                    allPiecedToClear = allPiecedToClear.Union(piecesToClear).ToList();
                    allPiecedToClear = RemoveCollectibles(allPiecedToClear);
                }
            }
        }
        return allPiecedToClear;
    }

    bool IsCornerMatch(List<GamePiece> gamePieces)
    {
        bool vertical = false;
        bool horizontal = false;
        int xStart = -1;
        int yStart = -1;

        foreach (GamePiece piece in gamePieces)
        {
            if (piece != null)
            {
                if (xStart == -1 || yStart == -1)
                {
                    xStart = piece.xIndex;
                    yStart = piece.yIndex;
                    continue;
                }

                if (piece.xIndex != xStart && piece.yIndex == yStart)
                {
                    horizontal = true;
                }
                if (piece.xIndex == xStart && piece.yIndex != yStart)
                {
                    vertical = true;
                }
            }
        }
        return (horizontal && vertical);
    }

    GameObject DropBomb(int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
    {
        GameObject bomb = null;

        if (gamePieces.Count >= 4)
        {
            if (IsCornerMatch(gamePieces))
            {
                if (adjacentBombPrefab != null)
                {
                    bomb = MakeBomb(adjacentBombPrefab, x, y);
                }
            }
            else
            {
                if (gamePieces.Count >= 5)
                {
                    bomb = MakeBomb(colorBombPrefab, x, y);
                }
                //columnBombPrefab
                else
                {
                    if (swapDirection.x != 0)
                    {
                        if (rowBombPrefab != null)
                        {
                            bomb = MakeBomb(columnBombPrefab, x, y);
                        }
                    }
                    //rowBombPrefab
                    else
                    {
                        if (columnBombPrefab != null)
                        {
                            bomb = MakeBomb(rowBombPrefab, x, y);
                        }
                    }
                }
            }
        }

        return bomb;
    }

    void ActiveBomb(GameObject bomb)
    {
        int x = (int)bomb.transform.position.x;
        int y = (int)bomb.transform.position.y;

        if (isWithinBounds(x, y))
        {
            m_allGamePiece[x, y] = bomb.GetComponent<GamePiece>();
        }
    }

    List<GamePiece> FindAllMatchValue(MatchValue mValue)
    {
        List<GamePiece> foundPieces = new List<GamePiece>();

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (m_allGamePiece[i, j] != null)
                {
                    if (m_allGamePiece[i, j].matchValue == mValue)
                    {
                        foundPieces.Add(m_allGamePiece[i, j]);
                    }
                }
            }
        }
        return foundPieces;
    }

    bool IsColorBomb(GamePiece gamePiece)
    {
        Bomb bomb = gamePiece.GetComponent<Bomb>();
        if (bomb != null)
        {
            return (bomb.bombType == BombType.Color);
        }
        return false;
    }

    List<GamePiece> FindCollectiblesAt(int row, bool clearAtBottomOnly = false)
    {
        List<GamePiece> foundCollectible = new List<GamePiece>();
        for (int i = 0; i < width; i++)
        {
            if (m_allGamePiece[i, row] != null)
            {
                Collectable collectibleComponent = m_allGamePiece[i, row].GetComponent<Collectable>();

                if (collectibleComponent)
                {
                    if(!clearAtBottomOnly || (clearAtBottomOnly && collectibleComponent.clearedAtBottom))
                        foundCollectible.Add(m_allGamePiece[i, row]);
                }
            }
        }
        return foundCollectible;
    }

    List<GamePiece> FindAllCollectibles()
    {
        List<GamePiece> foundCollectibles = new List<GamePiece>();

        for (int i = 0; i < height; i++)
        {
            List<GamePiece> collectibleRow = FindCollectiblesAt(i);
            foundCollectibles = foundCollectibles.Union(collectibleRow).ToList();
        }
        return foundCollectibles;
    }

    List<GamePiece> RemoveCollectibles(List<GamePiece> bombedPieces)
    {
        List<GamePiece> collectiblePieces = FindAllCollectibles();
        List<GamePiece> piecesToRemove = new List<GamePiece>();

        foreach(GamePiece piece in collectiblePieces)
        {
            Collectable collectibleComponent = piece.GetComponent<Collectable>();
            if(collectibleComponent != null)
            {
                if(!collectibleComponent.clearedByBomb)
                {
                    piecesToRemove.Add(piece);
                }
            }
        }
        return bombedPieces.Except(piecesToRemove).ToList();
    }
}
