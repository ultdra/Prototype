using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public enum TileType
{
    EMPTY,
    WHITE,
    GREEN,
    RED,
    COUNT
}

public class GridBuildingManager : MonoBehaviour
{
    public static GridBuildingManager Instance;
    
    private GridLayout m_GridLayout;
    public Tilemap m_MainTileMap;
    public Tilemap m_TempTileMap;

    private Dictionary<TileType, TileBase> m_TileBases = new Dictionary<TileType, TileBase>();

    private GridBuilding m_TempBuilding;
    private Vector3 m_PreviousPosition;
    private BoundsInt m_PreviousArea;
    
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        m_GridLayout = GetComponent<GridLayout>();
    }
    
    private void Start()
    {
        // Load tile resources from Resources/Prefabs/Tilemap
        string path = "Prefabs/Tilemap/";
        
        // Try to load the tile prefabs
        m_TileBases.Add(TileType.EMPTY, null);
        m_TileBases.Add(TileType.WHITE, Resources.Load<TileBase>(path + "WHITE"));
        m_TileBases.Add(TileType.GREEN, Resources.Load<TileBase>(path + "GREEN"));
        m_TileBases.Add(TileType.RED, Resources.Load<TileBase>(path + "RED"));
        
    }

    public void InitBuilding(GameObject go)
    {
        m_TempBuilding = Instantiate(go, Vector3.zero,  go.transform.rotation).GetComponent<GridBuilding>();
        FollowTileBuilding();
    }

    private void Update()
    {
        // Return early if we don't have a temporary building
        if(!m_TempBuilding)
            return;
            
        // Don't update position if mouse is over UI elements
        if(EventSystem.current.IsPointerOverGameObject(0))
            return;
            
        // Only update position if the building hasn't been placed yet
        if(!m_TempBuilding.Placed)
        {
            // Get mouse position in screen space
            Vector3 mouseScreenPos = Input.mousePosition;
            
            // Create a ray from the camera through the mouse position
            Ray ray = Camera.main.ScreenPointToRay(mouseScreenPos);
            
            // The grid is rotated 90 degrees on X-axis, so we need to use a horizontal plane 
            // which has its normal pointing up (Y-axis)
            Plane horizontalPlane = new Plane(Vector3.up, Vector3.zero);
            
            float hitDistance;
            if (horizontalPlane.Raycast(ray, out hitDistance))
            {
                // Get the world position where the ray hits the plane
                Vector3 hitPoint = ray.GetPoint(hitDistance);
                
                // Convert the world position to cell coordinates
                Vector3Int cellPos = m_GridLayout.WorldToCell(hitPoint);
                
                // Only update if the cell position has changed
                if (m_PreviousPosition != cellPos)
                {
                    // Use CellToLocal to get the bottom-left of the cell
                    Vector3 localPos = m_GridLayout.CellToLocal(cellPos);
                    
                    // Add half cell size to get the center
                    if (m_GridLayout.cellLayout == GridLayout.CellLayout.Rectangle)
                    {
                        localPos.x += m_GridLayout.cellSize.x / 2;
                        localPos.y += m_GridLayout.cellSize.y / 2;
                    }
                    
                    // Transform from local grid space to world space
                    Vector3 worldPos = m_GridLayout.transform.TransformPoint(localPos);
                    
                    // Set the building position
                    m_TempBuilding.transform.position = worldPos;
                    m_PreviousPosition = cellPos;             

                    // Update highlighted tiles
                    FollowTileBuilding();       
                }
            }
            
            // Handle building placement on mouse click
            if(Input.GetMouseButtonDown(0))
            {
                // Only place if the area is valid
                if(CanPlaceBuilding())
                {
                    PlaceBuilding();
                }
                else
                {
                    Debug.Log("Cannot place building here - area is occupied or invalid!");
                }
            }
        }
    }

    private void ClearArea()
    {
        TileBase[] tileArray = new TileBase[m_PreviousArea.size.x * m_PreviousArea.size.y * m_PreviousArea.size.z];
        FillTiles(tileArray, TileType.EMPTY);
        m_TempTileMap.SetTilesBlock(m_PreviousArea, tileArray);
    }

    private void FollowTileBuilding()
    {
        // Clear the previous highlighted area
        ClearArea();

        // Update the building area position to match grid cells
        m_TempBuilding.Area.position = m_GridLayout.WorldToCell(m_TempBuilding.transform.position);
        BoundsInt buildingArea = m_TempBuilding.Area;

        // Get the tiles from the main tilemap that are in our building area
        TileBase[] baseArea = GetTileBases(buildingArea, m_MainTileMap);

        // Create a new array to store our highlight tiles
        TileBase[] tileArray = new TileBase[baseArea.Length];

        // Flag to track if all tiles are valid for placement
        bool canBuild = true;

        // Check each tile in the area
        for(int i = 0; i < baseArea.Length; ++i)
        {
            // Check if the tile is available (white or empty)
            if(baseArea[i] == m_TileBases[TileType.WHITE] || baseArea[i] == null)
            {
                // Mark as buildable (green)
                tileArray[i] = m_TileBases[TileType.GREEN];
            }   
            else
            {
                // If any tile is occupied, mark the whole area as invalid
                canBuild = false;
                break;
            }
        }

        // If we can't build, fill the entire area with red
        if(!canBuild)
        {
            FillTiles(tileArray, TileType.RED);
        }

        // Apply the highlight tiles to the temporary tilemap
        m_TempTileMap.SetTilesBlock(buildingArea, tileArray);
        m_PreviousArea = buildingArea;

        // Debug info
        Debug.Log($"Building area: {buildingArea.size.x}x{buildingArea.size.y}, Can build: {canBuild}");
    }

    private TileBase[] GetTileBases(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] tileArray = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;

        foreach(var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            tileArray[counter++] = tilemap.GetTile(pos);
        }

        return tileArray;
    }

    private void SetTiles(BoundsInt area, TileType type, Tilemap tilemap)
    {
        TileBase[] tileArray = new TileBase[area.size.x * area.size.y * area.size.z];
        FillTiles(tileArray, type);
        tilemap.SetTilesBlock(area, tileArray);
    }

    private void FillTiles(TileBase[] array, TileType type)
    {
        for(int i = 0; i< array.Length; ++i)
        {
            array[i] = m_TileBases[type];
        }
    }

    // Determines if the current building can be placed at its position
    private bool CanPlaceBuilding()
    {
        if (m_TempBuilding == null)
            return false;

        // Get the current position and area of the building
        BoundsInt buildingArea = m_TempBuilding.Area;
        buildingArea.position = m_GridLayout.WorldToCell(m_TempBuilding.transform.position);

        // Get the tiles in this area from the main tilemap
        TileBase[] baseArea = GetTileBases(buildingArea, m_MainTileMap);

        // Check if all tiles in the area are available (white or empty)
        foreach (var tile in baseArea)
        {
            if (tile != null && tile != m_TileBases[TileType.WHITE])
            {
                // Found an occupied tile
                return false;
            }
        }

        // All tiles are available
        return true;
    }

    // Places the current temporary building permanently
    private void PlaceBuilding()
    {
        if (m_TempBuilding == null)
            return;

        // Get the building area in cell coordinates
        BoundsInt buildingArea = m_TempBuilding.Area;
        buildingArea.position = m_GridLayout.WorldToCell(m_TempBuilding.transform.position);

        // Place the building by modifying the main tilemap
        SetTiles(buildingArea, TileType.WHITE, m_MainTileMap);

        // Clear the temporary highlighting
        ClearArea();

        // Mark the building as placed
        m_TempBuilding.SetBuilding();
        
        // Log the placement
        Debug.Log($"Building placed at {buildingArea.position}");

        // At this point, you might want to reset m_TempBuilding to null, or
        // set up to place another building of the same type
        // m_TempBuilding = null;
    }
}
