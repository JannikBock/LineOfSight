using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer spriteRenderer;

    [SerializeField]
    private Color[] playerColors;

    public Vector2Int Pos
    {
        get;
        private set;
    }

    public int PlayerNumber
    {
        get;
        private set;
    }

    public void Init(Vector2Int inPos, int inPlayerNumber)
    {
        Pos = inPos;
        PlayerNumber = inPlayerNumber;
        spriteRenderer.color = playerColors[inPlayerNumber];
    }
}
