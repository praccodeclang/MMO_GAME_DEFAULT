using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TestCollision : MonoBehaviour
{
    public Tilemap _tileMap;

    public TileBase _tile;
    // Start is called before the first frame update
    void Start()
    {
        _tileMap.SetTile(new Vector3Int(0,0,0), _tile);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
