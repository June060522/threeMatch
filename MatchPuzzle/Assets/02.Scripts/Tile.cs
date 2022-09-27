using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TileType
{
    Normal,
    Obstacle,
    Breakable
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    [SerializeField] public int xIndex;
    [SerializeField] public int yIndex;
    Board m_board;

    public TileType tileType = TileType.Normal;

    SpriteRenderer spriteRenderer;

    public int breakableValue = 0;
    public Sprite[] breakableSprite;

    public Color normalColor;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
        if(tileType ==TileType.Breakable)
        {
            spriteRenderer.sprite = breakableSprite[breakableValue];
        }
    }

    private void OnMouseDown()
    {
        if (m_board != null)
            m_board.ClickTile(this);
    }

    private void OnMouseEnter()
    {
        if (m_board != null)
            m_board.DragToTile(this);
    }

    private void OnMouseUp()
    {
        m_board.ReleaseTile();
    }

    public void BreakTile()
    {
        if (tileType != TileType.Breakable) return;
        StartCoroutine(BreakTileRoutine());
    }

    IEnumerator BreakTileRoutine()
    {
        breakableValue--;
        breakableValue = Mathf.Max(breakableValue, 0, breakableValue);

        yield return new WaitForSeconds(0.25f);
        if (breakableSprite[breakableValue] != null)
        {
            spriteRenderer.sprite = breakableSprite[breakableValue];
        }
        if (breakableValue == 0)
        {
            tileType = TileType.Normal;
            spriteRenderer.color = normalColor;
        }
    }
}
