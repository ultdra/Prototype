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
        
        this.gameObject.SetActive(false);
    }

    public void InitBuilding(GameObject go)
    {
        // Instantiate the building, maintaining the original rotation
        GameObject buildingObject = Instantiate(go, Vector3.zero, go.transform.rotation);
        m_TempBuilding = buildingObject.GetComponent<GridBuilding>();
        
        this.gameObject.SetActive(true);
        // Log the building's properties
        Debug.Log($"Initialized building with rotation: {m_TempBuilding.transform.rotation.eulerAngles} and area size: {m_TempBuilding.Area.size}");
        
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
                    // Calculate the world position for the center of the cell
                    // CellToWorld gives us the world position of the cell (including the grid's rotation and position)
                    Vector3 cellWorldPos = m_GridLayout.CellToWorld(cellPos);
                    
                    // Calculate the center of the cell (add half cell size)
                    Vector3 cellCenter = cellWorldPos;
                    if (m_GridLayout.cellLayout == GridLayout.CellLayout.Rectangle)
                    {
                        // Account for the rotated grid (90 degrees on X axis means Y coord becomes Z)
                        cellCenter += new Vector3(m_GridLayout.cellSize.x / 2, 0, m_GridLayout.cellSize.y / 2);
                    }
                    
                    // Calculate the offset needed for multi-cell buildings
                    Vector3 centerOffset = CalculateBuildingCenterOffset(m_TempBuilding.Area.size);
                    
                    // Apply the multi-cell offset (respecting both grid and building rotation)
                    // We need to rotate the offset vector based on the building's rotation
                    Quaternion buildingRotation = m_TempBuilding.transform.rotation;
                    Vector3 rotatedOffset = RotateOffsetForBuilding(centerOffset, buildingRotation);
                    
                    // Apply the rotated offset to get the final position
                    Vector3 worldPos = cellCenter + rotatedOffset;
                    
                    // Set the building position, maintaining its original rotation
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
                    this.gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log("Cannot place building here - area is occupied or invalid!");
                }
            }
        }
    }

    // Rotate the offset vector based on the building's rotation
    private Vector3 RotateOffsetForBuilding(Vector3 offset, Quaternion buildingRotation)
    {
        // For 30-degree X rotation on BaseBuilding:
        // When a building is rotated 30 degrees on X-axis, the Z component of the offset 
        // should be adjusted to account for the perspective change
        
        // Get the rotation angle on X axis in degrees
        float xRotation = buildingRotation.eulerAngles.x;
        
        // We don't want to adjust X offset as the rotation is on X-axis
        // But we need to adjust the Z offset based on the X rotation
        // This is a simplified calculation for common building angles
        if (Mathf.Approximately(xRotation, 30f) || Mathf.Approximately(xRotation, 330f))
        {
            // For 30-degree rotation, adjust Z offset to maintain visual center
            // The factor 0.866 is approximately cos(30°)
            // This helps account for the visual foreshortening due to the angle
            return new Vector3(offset.x, offset.y, offset.z * 0.866f);
        }
        
        // For other rotations, apply the full rotation to the offset
        return buildingRotation * offset;
    }

    // Calculate the offset needed to center a multi-cell building
    private Vector3 CalculateBuildingCenterOffset(Vector3Int size)
    {
        // No offset needed for 1x1 buildings
        if (size.x <= 1 && size.y <= 1)
            return Vector3.zero;
            
        // Get the cell gap from the grid layout
        Vector3 cellGap = m_GridLayout.cellGap;
        Vector3 cellSize = m_GridLayout.cellSize;
        
        // For buildings larger than 1x1, calculate the center offset
        // These offsets are calculated in grid space (before rotation)
        float offsetX = 0;
        float offsetZ = 0; // Using Z instead of Y because grid is rotated 90° on X-axis
        
        // If width > 1, adjust X offset
        if (size.x > 1)
        {
            // Offset by (width-1)/2 cells to center (including gaps)
            offsetX = (size.x - 1) * (cellSize.x + cellGap.x) / 2;
        }
        
        // If height (z in world space) > 1, adjust Z offset
        if (size.y > 1)
        {
            // Offset by (height-1)/2 cells to center (including gaps)
            offsetZ = (size.y - 1) * (cellSize.y + cellGap.y) / 2;
        }
        
        // Create the offset vector - since grid is rotated 90° on X-axis,
        // Y in grid space becomes Z in world space
        Vector3 offset = new Vector3(offsetX, 0, offsetZ);
        
        return offset;
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

        // Get cell position from world position, accounting for building center position and rotation
        Vector3 adjustedPosition = m_TempBuilding.transform.position;
        
        // For rotated buildings, we need to adjust how we calculate the area origin
        // Since the building model is rotated but the area calculation expects a grid-aligned area
        Vector3Int cellOrigin = GetCellOriginForRotatedBuilding(adjustedPosition, m_TempBuilding.transform.rotation, m_TempBuilding.Area.size);
        
        // Set the area position to this cell origin
        m_TempBuilding.Area.position = cellOrigin;
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
    }

    // Calculate the cell origin for a rotated building
    private Vector3Int GetCellOriginForRotatedBuilding(Vector3 position, Quaternion rotation, Vector3Int size)
    {
        // Get the base cell position from the world position
        Vector3Int baseCell = m_GridLayout.WorldToCell(position);
        
        // For a 30-degree rotation on X axis, we need to adjust the cell origin
        // This ensures the highlighted area correctly represents where the building will be placed
        float xRotation = rotation.eulerAngles.x;
        
        // 30-degree rotation specific adjustment
        if (Mathf.Approximately(xRotation, 30f) || Mathf.Approximately(xRotation, 330f))
        {
            // For multi-cell buildings, adjust the cell origin to account for the rotation
            if (size.x > 1 || size.y > 1)
            {
                // We need to offset the grid origin to align with the visually correct position
                // For the 30-degree rotation, we need to adjust the origin based on building size
                int adjustZ = 0;
                
                // For taller buildings (y > 1), we typically need to adjust the Z origin
                if (size.y > 1)
                {
                    // Shift origin back by floor(size.y/2) cells to align with visual center
                    adjustZ = -((size.y - 1) / 2);
                }
                
                // Apply the adjustment (y coordinate in grid space becomes z in world space)
                baseCell.z += adjustZ;
            }
        }
        
        return baseCell;
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

        // The area position is already correctly calculated in FollowTileBuilding
        BoundsInt buildingArea = m_TempBuilding.Area;

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

        // The area position is already correctly calculated in FollowTileBuilding
        BoundsInt buildingArea = m_TempBuilding.Area;

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
