using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    [SerializeField] private int _edgeCount = 1; // Default of 1 for circle
	public int EdgeCount => _edgeCount;

    public Vector2Int GridPosition { get; set; }
}
