using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] int width;
    [SerializeField] int height;
    [SerializeField] int borderSize = 2;
    [SerializeField] GameObject tilePrefabs;
    [SerializeField] GameObject[] gamePiecePrefabs;
    Tile[,] m_allTiles = null;// Tile 스크립트를 2차원 배열로 선언  -> 대괄호 안에 ','가 없으면 1차원 배열
    GamePiece[,] m_allGamePiece;
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
        gamePiece.SetCoord(x, y);
    }

    void FillRandom()
    {
        for(int i = 0; i < width; i++)
        {
            for(int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate(GetRandomGamePiece(),Vector3.zero,Quaternion.identity);
                if(randomPiece != null)
                {
                    randomPiece.name = randomPiece.name.Replace("(Clone)", "");
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(),i,j);
                }
            }
        }
    }
}
