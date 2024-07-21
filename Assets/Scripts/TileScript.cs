using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class TileScript : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public Color Color
    {
        get => spriteRenderer.color;
        set => spriteRenderer.color = value;
    }

    public int SortingOrder
    {
        get => spriteRenderer.sortingOrder;
        set => spriteRenderer.sortingOrder = value;
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component is missing.");
        }
    }
}