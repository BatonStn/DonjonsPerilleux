using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    // Largeur de la map
    [SerializeField]
    private int mapWidth;

    // Longueur de la map
    [SerializeField]
    private int mapHeight;

    [SerializeField]
    private GameObject square;

    private Tile[][] tiles;

    private List<Room> rooms;

    private void Start()
    {
        mapWidth = 30;
        mapHeight = 30;
    }

    private void CreateBSPRoomedMap()
    {
        InitiateTiles();
        CreateEmptyMap();

        MapLeaf map = new(0, 0, mapWidth, mapHeight, 4);
        map.CreateBSPMap();

        CreateRooms(map);

        Mazify();
        HighlightRoomEntrances();
        VisitPathsCoroutine();
        // VisitPaths();
    }

    private void InitiateTiles()
    {
        if (tiles != null) { EmptyMap(); }

        tiles = new Tile[mapWidth + 2][];

        for (int i = 0; i < mapWidth + 2; i++)
        {
            tiles[i] = new Tile[mapHeight + 2];

            for (int j = 0; j < mapHeight + 2; j++)
            {
                tiles[i][j] = new Tile();
            }
        }

        rooms = new();
    }

    private void EmptyMap()
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            for (int j = 0; j < tiles[0].Length; j++)
            {
                Destroy(tiles[i][j].TileGo);
                tiles[i][j] = new Tile();
            }
        }
    }

    private void CreateEmptyMap()
    {
        if (!MapIsEmpty()) EmptyMap();

        for (int i = 0; i < mapWidth + 2; i++)
        {
            for (int j = 0; j < mapHeight + 2; j++)
            {
                GameObject newSquare = Instantiate(square, new Vector3(i + .5f, j +.5f, 0), Quaternion.identity, transform);
                newSquare.name = "Square " + i.ToString() + "," + j.ToString();
                tiles[i][j].TileGo = newSquare;

                if (i == 0 || j == 0 || i == mapWidth + 1 || j == mapHeight + 1) tiles[i][j].ConvertTileToWall();
            }
        }
    }
    private bool MapIsEmpty()
    {
        try { return tiles[0][0].TileGo == null; }
        catch (NullReferenceException) { return true; }
    }

    private void CreateRooms(MapLeaf map)
    {
        if (map.leftChild == null && map.rightChild == null)
        {
            Container container = map.room;

            for (int i = 0; i < container.w; i++)
            {
                for (int j = 0; j < container.h; j++)
                {
                    Tile roomTile = tiles[container.x + i + 1][container.y + j + 1];
                    roomTile.ConvertTileToRoom();
                }
            }

            Room room = new(container);

            RoomSides(room);

            rooms.Add(room);
        }
        else if (map.leftChild != null && map.rightChild != null)
        {
            CreateRooms(map.leftChild);
            CreateRooms(map.rightChild);
        }
    }

    private void Mazify()
    {
        int pos_x = 0;
        int pos_y = 0;
        Tile tile = new()
        {
            TileType = TileType.MazeBegin
        };

        while (tile.TileType != TileType.None)
        {
            pos_x = Random.Range(1, mapWidth + 2);
            pos_y = Random.Range(1, mapHeight + 2);
            tile = tiles[pos_x][pos_y];
        }

        tile.ConvertTileToPath();

        MazeRec(pos_x, pos_y, Direction.None);
    }

    private void MazeRec(int pos_x, int pos_y, Direction previous_dir)
    {
        int first_direction = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            int next_dir = (first_direction + i) % 4;
            if ((next_dir + 2) % 4 != (int) previous_dir || previous_dir == Direction.None)
            {
                switch (next_dir)
                {
                    case 0:
                        Tile eastTile = EastTile(pos_x, pos_y);

                        if (eastTile.TileType != TileType.None) break;
                        if (EastTile(pos_x + 1, pos_y).TileType == TileType.Path) break;
                        if (NorthTile(pos_x, pos_y).TileType == TileType.Path && NorthEastTile(pos_x, pos_y).TileType == TileType.Path ||
                            SouthTile(pos_x, pos_y).TileType == TileType.Path && SouthEastTile(pos_x, pos_y).TileType == TileType.Path)
                        {
                            break;
                        }

                        eastTile.ConvertTileToPath();
                        MazeRec(pos_x + 1, pos_y, Direction.East);
                        
                        break;

                    case 1:
                        Tile southTile = SouthTile(pos_x, pos_y);

                        if (southTile.TileType != TileType.None) break;
                        if (SouthTile(pos_x, pos_y - 1).TileType == TileType.Path) break;
                        if (EastTile(pos_x, pos_y).TileType == TileType.Path && SouthEastTile(pos_x, pos_y).TileType == TileType.Path ||
                            WestTile(pos_x, pos_y).TileType == TileType.Path && SouthWestTile(pos_x, pos_y).TileType == TileType.Path)
                        {
                            break;
                        }

                        southTile.ConvertTileToPath();
                        MazeRec(pos_x, pos_y - 1, Direction.South);
                        
                        break;

                    case 2:
                        Tile westTile = WestTile(pos_x, pos_y);
                        
                        if (westTile.TileType != TileType.None) break;
                        if (WestTile(pos_x - 1, pos_y).TileType == TileType.Path) break;
                        if (SouthTile(pos_x, pos_y).TileType == TileType.Path && SouthWestTile(pos_x, pos_y).TileType == TileType.Path ||
                            NorthTile(pos_x, pos_y).TileType == TileType.Path && NorthWestTile(pos_x, pos_y).TileType == TileType.Path)
                        {
                            break;
                        }

                        westTile.ConvertTileToPath();
                        MazeRec(pos_x - 1, pos_y, Direction.West);
                        
                        break;

                    case 3:
                        //Debug.Log(pos_x.ToString() + " " + pos_y.ToString() + " North");
                        Tile northTile = NorthTile(pos_x, pos_y);

                        if (northTile.TileType != TileType.None) break;
                        if (NorthTile(pos_x, pos_y + 1).TileType == TileType.Path) break;
                        if (WestTile(pos_x, pos_y).TileType == TileType.Path && NorthWestTile(pos_x, pos_y).TileType == TileType.Path ||
                            EastTile(pos_x, pos_y).TileType == TileType.Path && NorthEastTile(pos_x, pos_y).TileType == TileType.Path)
                        {
                            break;
                        }

                        northTile.ConvertTileToPath();
                        MazeRec(pos_x, pos_y + 1, Direction.North);
                        
                        break;
                }
            }
            
        }
    }

    private Tile NorthTile(int i, int j)
    {
        try { return tiles[i][j + 1]; }
        catch (IndexOutOfRangeException) { return new Tile(); }
    }

    private Tile NorthEastTile(int i, int j)
    {
        try { return tiles[i + 1][j + 1]; }
        catch (IndexOutOfRangeException) { return new Tile(); }
    }

    private Tile EastTile(int i, int j)
    {
        try { return tiles[i + 1][j]; }
        catch (IndexOutOfRangeException) { return new Tile(); }
    }
    private Tile SouthEastTile(int i, int j)
    {
        try { return tiles[i + 1][j - 1]; }
        catch (IndexOutOfRangeException) { return new Tile(); }
    }

    private Tile SouthTile(int i, int j)
    {
        try { return tiles[i][j - 1]; }
        catch (IndexOutOfRangeException) { return new Tile(); }
    }
    private Tile SouthWestTile(int i, int j)
    {
        try { return tiles[i - 1][j - 1]; }
        catch (IndexOutOfRangeException) { return new Tile(); }
    }

    private Tile WestTile(int i, int j)
    {
        try { return tiles[i - 1][j]; }
        catch (IndexOutOfRangeException) { return new Tile(); }
    }

    private Tile NorthWestTile(int i, int j)
    {
        try { return tiles[i - 1][j + 1]; }
        catch (IndexOutOfRangeException) { return new Tile(); }
    }

    private void RoomSides(Room r)
    {
        Container c = r.container;

        for (int i = -1; i < c.w + 1; i++)
        {
            Tile northTile = tiles[c.x + i + 1][c.y + c.h + 1];
            if (northTile.TileType != TileType.Wall) northTile.ConvertTileToRoomSide();
            
            Tile southTile = tiles[c.x + i + 1][c.y];
            if (southTile.TileType != TileType.Wall) southTile.ConvertTileToRoomSide();
        }

        for (int j = 0; j < c.h; j++)
        {
            Tile westTile = tiles[c.x][c.y + j + 1];
            if (westTile.TileType != TileType.Wall)  westTile.ConvertTileToRoomSide();
            
            Tile eastTile = tiles[c.x + c.w + 1][c.y + j + 1];
            if (eastTile.TileType != TileType.Wall) eastTile.ConvertTileToRoomSide();
        }
    }

    private void HighlightRoomEntrances()
    {
        foreach (Room r in rooms)
        {
            Container c = r.container;

            for (int i = 0; i < c.w; i++)
            {
                Tile southTile = tiles[c.x + i + 1][c.y];
                if (southTile.TileType == TileType.RoomSide && tiles[c.x + i + 1][c.y - 1].TileType == TileType.Path)
                    r.southBorder.Add(new Vector2(c.x + i + 1, c.y));

                Tile northTile = tiles[c.x + i + 1][c.y + c.h + 1];
                if (northTile.TileType == TileType.RoomSide && tiles[c.x + i + 1][c.y + c.h + 2].TileType == TileType.Path)
                    r.northBorder.Add(new Vector2(c.x + i + 1, c.y + c.h + 1));
            }

            for (int j = 0; j < c.h; j++)
            {
                Tile westTile = tiles[c.x][c.y + j + 1];
                if (westTile.TileType == TileType.RoomSide && tiles[c.x - 1][c.y + j + 1].TileType == TileType.Path)
                    r.westBorder.Add(new Vector2(c.x, c.y + j + 1));

                Tile eastTile = tiles[c.x + c.w + 1][c.y + j + 1];
                if (eastTile.TileType == TileType.RoomSide && tiles[c.x + c.w + 2][c.y + j + 1].TileType == TileType.Path)
                    r.eastBorder.Add(new Vector2(c.x + c.w + 1, c.y + j + 1));
            }

            int[] bordersLengths = { r.eastBorder.Count, r.southBorder.Count, r.westBorder.Count, r.northBorder.Count };
            List<Vector2> chosen_paths = new();

            for (int i = 0; i < 4; i++)
            {
                chosen_paths.Add(new Vector2(i, bordersLengths[i]));
            }

            // On garde seulement les frontières possédant au moins une tile liant la room au path
            for (int i = 3; i >= 0; i--)
            {
                if (chosen_paths[i].y == 0) chosen_paths.RemoveAt(i);
            }

            int bordersLeft = chosen_paths.Count;
            int pathChosen = 0;

            for (int i = 0; i < bordersLeft; i++)
            {
                int rand = Random.Range(0, (int)chosen_paths[i].y);
                bool goodPath = Random.Range(0, 2) == 0;
                switch ((int) chosen_paths[i].x)
                {
                    case 0:
                        if (goodPath || (i == bordersLeft - 1 && pathChosen == 0))
                        {
                            tiles[(int) r.eastBorder[rand].x][(int) r.eastBorder[rand].y].ConvertTileToEntrance();
                            tiles[(int) r.eastBorder[rand].x + 1][(int) r.eastBorder[rand].y].ConvertTileToTruePath();
                            pathChosen++;
                        }
                        break;
                    case 1:
                        if (goodPath || (i == bordersLeft - 1 && pathChosen == 0))
                        {
                            tiles[(int) r.southBorder[rand].x][(int) r.southBorder[rand].y].ConvertTileToEntrance();
                            tiles[(int) r.southBorder[rand].x][(int) r.southBorder[rand].y - 1].ConvertTileToTruePath();
                            pathChosen++;
                        }
                        break;
                    case 2:
                        if (goodPath || (i == bordersLeft - 1 && pathChosen == 0))
                        {
                            tiles[(int) r.westBorder[rand].x][(int) r.westBorder[rand].y].ConvertTileToEntrance();
                            tiles[(int) r.westBorder[rand].x - 1][(int) r.westBorder[rand].y].ConvertTileToTruePath();
                            pathChosen++;
                        }
                        break;
                    case 3:
                        if (goodPath || (i == bordersLeft - 1 && pathChosen == 0))
                        {
                            tiles[(int) r.northBorder[rand].x][(int) r.northBorder[rand].y].ConvertTileToEntrance();
                            tiles[(int) r.northBorder[rand].x][(int) r.northBorder[rand].y + 1].ConvertTileToTruePath();
                            pathChosen++;
                        }
                        break;
                }
            }
        }
    }

    private Vector2 FindRoomEntrance()
    {
        for (int i = 1; i < mapWidth + 2 ; i++)
        {
            for (int j = 1; j < mapHeight + 2 ; j++)
            {
                if (tiles[i][j].TileType == TileType.RoomEntrance)
                {
                    return new Vector2(i, j);
                }
            }
        }
        return Vector2.zero;
    }

    private void VisitPaths()
    {
        Vector2 firstRoomEntrance = FindRoomEntrance();
        int i = (int)firstRoomEntrance.x;
        int j = (int)firstRoomEntrance.y;

        if (IsHorizontalBorder(i, j))
        {
            if (tiles[i][j - 1].TileType == TileType.Room) VisitPathsRec(i, j + 1, Direction.North);
            else VisitPathsRec(i, j - 1, Direction.South);
        }
        else if (IsVerticalBorder(i, j))
        {
            if (tiles[i + 1][j].TileType == TileType.Room) VisitPathsRec(i - 1, j, Direction.West);
            else VisitPathsRec(i + 1, j, Direction.East);
        }
    }

    private bool IsHorizontalBorder(int i, int j)
    {
        return ((tiles[i - 1][j].TileType == TileType.RoomSide && tiles[i + 1][j].TileType == TileType.RoomSide) ||
            (tiles[i - 1][j].TileType == TileType.Wall && tiles[i + 1][j].TileType == TileType.RoomSide) ||
            (tiles[i - 1][j].TileType == TileType.RoomSide && tiles[i + 1][j].TileType == TileType.Wall));
    }

    private bool IsVerticalBorder(int i, int j)
    {
        return ((tiles[i][j - 1].TileType == TileType.RoomSide && tiles[i][j + 1].TileType == TileType.RoomSide) ||
            (tiles[i][j - 1].TileType == TileType.Wall && tiles[i][j + 1].TileType == TileType.RoomSide) ||
            (tiles[i][j - 1].TileType == TileType.RoomSide && tiles[i][j + 1].TileType == TileType.Wall));
    }

    private bool VisitPathsRec(int i, int j, Direction previous_dir)
    {
        int goodPaths = 0;

        if (previous_dir != Direction.West)
        {
            Tile eastTile = EastTile(i, j);
            if (eastTile.TileType != TileType.VisitedPath)
            {
                if (eastTile.TileType == TileType.Path)
                {
                    eastTile.ConvertTileToVisitedPath();
                    if (VisitPathsRec(i + 1, j, Direction.East)) goodPaths++;
                }
                else if (eastTile.TileType == TileType.TruePath)
                {
                    goodPaths++;
                    VisitPathsRec(i + 1, j, Direction.East);
                }
                else if (eastTile.TileType == TileType.RoomEntrance) goodPaths++;
            }
        }

        if (previous_dir != Direction.North)
        {
            Tile southTile = SouthTile(i, j);
            if (southTile.TileType != TileType.VisitedPath)
            {
                if (southTile.TileType == TileType.Path)
                {
                    southTile.ConvertTileToVisitedPath();
                    if (VisitPathsRec(i, j - 1, Direction.South)) goodPaths++;
                }
                else if (southTile.TileType == TileType.TruePath)
                {
                    goodPaths++;
                    VisitPathsRec(i, j - 1, Direction.South);
                }
                else if (southTile.TileType == TileType.RoomEntrance) goodPaths++;
            }
        }

        if (previous_dir != Direction.East)
        {
            Tile westTile = WestTile(i, j);
            if (westTile.TileType != TileType.VisitedPath)
            {
                if (westTile.TileType == TileType.Path)
                {
                    westTile.ConvertTileToVisitedPath();
                    if (VisitPathsRec(i - 1, j, Direction.West)) goodPaths++;
                }
                else if (westTile.TileType == TileType.TruePath)
                {
                    goodPaths++;
                    VisitPathsRec(i - 1, j, Direction.West);
                }
                else if (westTile.TileType == TileType.RoomEntrance) goodPaths++;
            }
        }

        if (previous_dir != Direction.South)
        {
            Tile northTile = NorthTile(i, j);
            if (northTile.TileType != TileType.VisitedPath)
            {
                if (northTile.TileType == TileType.Path)
                {
                    northTile.ConvertTileToVisitedPath();
                    if (VisitPathsRec(i, j + 1, Direction.North)) goodPaths++;
                }
                else if (northTile.TileType == TileType.TruePath)
                {
                    goodPaths++;
                    VisitPathsRec(i, j + 1, Direction.North);
                }
                else if (northTile.TileType == TileType.RoomEntrance) goodPaths++;
            }
        }

        if (goodPaths > 0) return true;
        else
        {
            tiles[i][j].ConvertTileToNone();
            return false;
        }
    }


    void VisitPathsCoroutine()
    {
        Vector2 firstRoomEntrance = FindRoomEntrance();
        int i = (int)firstRoomEntrance.x;
        int j = (int)firstRoomEntrance.y;

        if (IsHorizontalBorder(i, j))
        {
            if (tiles[i][j - 1].TileType == TileType.Room) 
                StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { if (myReturnValue) { Debug.Log("Done"); } }, i, j + 1, Direction.North)); 
            else
                StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { if (myReturnValue) { Debug.Log("Done"); } }, i, j - 1, Direction.South));
        }
        else if (IsVerticalBorder(i, j))
        {
            if (tiles[i + 1][j].TileType == TileType.Room)
                StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { if (myReturnValue) { Debug.Log("Done"); } }, i - 1, j, Direction.West));
            else
                StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { if (myReturnValue) { Debug.Log("Done"); } }, i + 1, j, Direction.East));
        }
    }

    IEnumerator VisitPathsRecCoroutine(Action<bool> callback, int i, int j, Direction previous_dir)
    {
        int goodPaths = 0;

        if (previous_dir != Direction.West)
        {
            Tile eastTile = EastTile(i, j);
            if (eastTile.TileType != TileType.VisitedPath)
            {
                if (eastTile.TileType == TileType.Path)
                {
                    yield return new WaitForSeconds(.05f);
                    eastTile.ConvertTileToVisitedPath();
                    yield return StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { if (myReturnValue) { goodPaths++; } }, i + 1, j, Direction.East));
                }
                else if (eastTile.TileType == TileType.TruePath)
                {
                    goodPaths++;
                    yield return StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { }, i + 1, j, Direction.East));
                }
                else if (eastTile.TileType == TileType.RoomEntrance) goodPaths++;
            }
            //else
            //{
            //    tiles[i][j].ConvertTileToNone();
            //    callback(false);
            //    yield break;
            //}
        }

        if (previous_dir != Direction.North)
        {
            Tile southTile = SouthTile(i, j);
            if (southTile.TileType != TileType.VisitedPath)
            {
                if (southTile.TileType == TileType.Path)
                {
                    yield return new WaitForSeconds(.05f);
                    southTile.ConvertTileToVisitedPath();
                    yield return StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { if (myReturnValue) { goodPaths++; } }, i, j - 1, Direction.South));
                }
                else if (southTile.TileType == TileType.TruePath)
                {
                    goodPaths++;

                    yield return StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { }, i, j - 1, Direction.South));
                }
                else if (southTile.TileType == TileType.RoomEntrance) goodPaths++;
            }
        }

        if (previous_dir != Direction.East)
        {
            Tile westTile = WestTile(i, j);
            if (westTile.TileType != TileType.VisitedPath)
            {
                if (westTile.TileType == TileType.Path)
                {
                    yield return new WaitForSeconds(.05f);
                    westTile.ConvertTileToVisitedPath();
                    yield return StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { if (myReturnValue) { goodPaths++; } }, i - 1, j, Direction.West));

                }
                else if (westTile.TileType == TileType.TruePath)
                {
                    goodPaths++;
                    yield return StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { }, i - 1, j, Direction.West));
                }
                else if (westTile.TileType == TileType.RoomEntrance) goodPaths++;
            }
        }

        if (previous_dir != Direction.South)
        {
            Tile northTile = NorthTile(i, j);
            if (northTile.TileType != TileType.VisitedPath)
            {
                if (northTile.TileType == TileType.Path)
                {
                    yield return new WaitForSeconds(.05f);
                    northTile.ConvertTileToVisitedPath();
                    yield return StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { if (myReturnValue) { goodPaths++; } }, i, j + 1, Direction.North));
                }
                else if (northTile.TileType == TileType.TruePath)
                {
                    goodPaths++;
                    yield return StartCoroutine(VisitPathsRecCoroutine((myReturnValue) => { }, i, j + 1, Direction.North));
                }
                else if (northTile.TileType == TileType.RoomEntrance) goodPaths++;
            }
        }

        // Si on arrive sur un TruePath et qu'il a déjà un Visited à côté alors c'est pas bon

        if (goodPaths > 0) callback(true);
        else
        {
            yield return new WaitForSeconds(.05f);
            tiles[i][j].ConvertTileToNone();
            callback(false);
        }
        yield return null;
        
    }

    /* UI */

    public void CreateBSPMapButton()
    {
        CreateBSPRoomedMap();
    }

    public void ChangeHeight(Slider slider)
    {
        mapHeight = (int) slider.value;
    }

    public void ChangeWidth(Slider slider)
    {
        mapWidth = (int) slider.value;
    }
}

//private void CreateRoomedMap()
//{
//    CreateEmptyMap();

//    // On build les rooms à partir du coin en haut à gauche

//    int firstRoomCornerX = Random.Range(0, mapWidth - roomSizeMax + 1);
//    int firstRoomCornerY = Random.Range(0, mapHeight - roomSizeMax + 1);

//    int roomSizeX = Random.Range(roomSizeMin, roomSizeMax + 1);
//    int roomSizeY = Random.Range(roomSizeMin, roomSizeMax + 1);

//    for (int i = 0; i < roomSizeX; i++)
//    {
//        for (int j = 0; j < roomSizeY; j++)
//        {
//            tiles[firstRoomCornerX + i][firstRoomCornerY + j].TileGo.GetComponentInChildren<Renderer>().material.color = Color.gray;
//            tiles[firstRoomCornerX + i][firstRoomCornerY + j].TileType = TileType.Room;
//        }
//    }

//    tiles[firstRoomCornerX][firstRoomCornerY].TileGo.GetComponentInChildren<Renderer>().material.color = Color.black;

//    int spaceOccupied = roomSizeX * roomSizeY;
//    float minSpaceOccupied = .2f;
//    float relativeSpaceOccupied = (float)spaceOccupied / ((float)mapWidth * (float)mapHeight);

//    while (relativeSpaceOccupied < minSpaceOccupied)
//    {
//        int newRoomCornerX = Random.Range(0, mapWidth - roomSizeMax + 1);
//        int newRoomCornerY = Random.Range(0, mapHeight - roomSizeMax + 1);

//        roomSizeX = Random.Range(roomSizeMin, roomSizeMax + 1);
//        roomSizeY = Random.Range(roomSizeMin, roomSizeMax + 1);

//        // On vérifie si on peut construire la salle

//        bool roomIsBuildable = true;

//        Tile[] goodTiles = new Tile[roomSizeX * roomSizeY];

//        // On vérifie que la salle qu'on veut créer ne se superpose pas avec une autre
//        for (int i = 0; i < roomSizeX; i++)
//        {
//            for (int j = 0; j < roomSizeY; j++)
//            {
//                if (tiles[newRoomCornerX + i][newRoomCornerY + j].TileType != TileType.Room)
//                {
//                    goodTiles[i * roomSizeY + j] = tiles[newRoomCornerX + i][newRoomCornerY + j];
//                }
//                else roomIsBuildable = false;
//            }
//            if (!roomIsBuildable) break;
//        }

//        // On vérifie que la salle qu'on veut créer n'est pas collée à une autre
//        if (roomIsBuildable)
//        {
//            for (int i = 0; i < roomSizeX; i++)
//            {
//                if ((SouthTile(newRoomCornerX + i, newRoomCornerY).TileType == TileType.Room) ||
//                    (NorthTile(newRoomCornerX + i, newRoomCornerY + roomSizeY - 1).TileType == TileType.Room))
//                {
//                    roomIsBuildable = false;
//                    break;
//                }
//            }
//        }

//        if (roomIsBuildable)
//        {
//            for (int j = 0; j < roomSizeY; j++)
//            {
//                if ((WestTile(newRoomCornerX, newRoomCornerY + j).TileType == TileType.Room) ||
//                    (EastTile(newRoomCornerX + roomSizeX - 1, newRoomCornerY).TileType == TileType.Room))
//                {
//                    roomIsBuildable = false;
//                    break;
//                }
//            }
//        }

//        if (roomIsBuildable)
//        {
//            foreach (Tile tile in goodTiles)
//            {
//                tile.TileGo.GetComponentInChildren<Renderer>().material.color = Color.gray;
//                tile.TileType = TileType.Room;
//            }

//            Tile southTile = SouthTile(newRoomCornerX + Random.Range(0, roomSizeX), newRoomCornerY);
//            southTile.TileGo.GetComponentInChildren<Renderer>().material.color = Color.red;
//            southTile.TileType = TileType.RoomEntrance;

//            Tile northTile = NorthTile(newRoomCornerX + Random.Range(0, roomSizeX), newRoomCornerY + roomSizeY - 1);
//            northTile.TileGo.GetComponentInChildren<Renderer>().material.color = Color.blue;
//            northTile.TileType = TileType.RoomEntrance;

//            Tile eastTile = EastTile(newRoomCornerX + roomSizeX - 1, newRoomCornerY + Random.Range(0, roomSizeY));
//            eastTile.TileGo.GetComponentInChildren<Renderer>().material.color = Color.green;
//            eastTile.TileType = TileType.RoomEntrance;

//            Tile westTile = WestTile(newRoomCornerX, newRoomCornerY + Random.Range(0, roomSizeY));
//            westTile.TileGo.GetComponentInChildren<Renderer>().material.color = Color.yellow;
//            westTile.TileType = TileType.RoomEntrance;

//            spaceOccupied += roomSizeX * roomSizeY;
//            relativeSpaceOccupied = spaceOccupied / (mapWidth * (float)mapHeight);
//        }
//    }
//}

//private void CreateHalls(Point p1, Point p2)
//{
//    if (p1.x < p2.x)
//    {
//        if (p1.y < p2.y)
//        {
//            for (int i = p1.x; i < p2.x; i++) tiles[i][p1.y].ConvertTileToPath();
//            for (int i = p1.y; i < p2.y + 1; i++) tiles[p2.x][i].ConvertTileToPath();
//        }
//        else if (p1.y > p2.y)
//        {
//            for (int i = p2.y; i < p1.y; i++) tiles[p1.x][i].ConvertTileToPath();
//            for (int i = p1.x; i < p2.x + 1; i++) tiles[i][p2.y].ConvertTileToPath();
//        }
//        else
//            for (int i = p1.x; i < p2.x + 1; i++) tiles[i][p1.y].ConvertTileToPath();
//    }
//    else if (p1.x > p2.x)
//    {
//        if (p1.y < p2.y)
//        {
//            for (int i = p2.x; i < p1.x; i++) tiles[i][p1.y].ConvertTileToPath();
//            for (int i = p1.y; i < p2.y + 1; i++) tiles[p2.x][i].ConvertTileToPath();
//        }
//        else if (p1.y > p2.y)
//        {
//            for (int i = p2.y; i < p1.y; i++) tiles[p1.x][i].ConvertTileToPath();
//            for (int i = p2.x; i < p1.x + 1; i++) tiles[i][p2.y].ConvertTileToPath();
//        }
//        else
//            for (int i = p2.x; i < p1.x; i++) tiles[i][p1.y].ConvertTileToPath();
//    }
//    else
//    {
//        if (p1.y < p2.y)
//            for (int i = p1.y; i < p2.y; i++) tiles[p1.x][i].ConvertTileToPath();
//        else
//            for (int i = p2.y; i < p1.y; i++) tiles[p1.x][i].ConvertTileToPath();
//    }
//}