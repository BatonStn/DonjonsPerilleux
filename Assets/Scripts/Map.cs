using System;
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
        RoomEntrances();
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

        MazeRec(pos_x, pos_y, -1);
    }

    private void MazeRec(int pos_x, int pos_y, int previous_dir)
    {
        int first_direction = Random.Range(0, 4);

        for (int i = 0; i < 4; i++)
        {
            int next_dir = (first_direction + i) % 4;
            if ((next_dir + 2) % 4 != previous_dir || previous_dir == -1)
            {
                switch (next_dir)
                {
                    case 0:
                        //Debug.Log(pos_x.ToString() + " " + pos_y.ToString() + " East");
                        Tile eastTile = EastTile(pos_x, pos_y);

                        if (eastTile.TileType != TileType.None) break;
                        if (EastTile(pos_x + 1, pos_y).TileType == TileType.Path) break;
                        if (NorthTile(pos_x, pos_y).TileType == TileType.Path && NorthEastTile(pos_x, pos_y).TileType == TileType.Path ||
                            SouthTile(pos_x, pos_y).TileType == TileType.Path && SouthEastTile(pos_x, pos_y).TileType == TileType.Path)
                        {
                            break;
                        }

                        eastTile.ConvertTileToPath();
                        MazeRec(pos_x + 1, pos_y, 0);
                        
                        break;

                    case 1:
                        //Debug.Log(pos_x.ToString() + " " + pos_y.ToString() + " South");
                        Tile southTile = SouthTile(pos_x, pos_y);

                        if (southTile.TileType != TileType.None) break;
                        if (SouthTile(pos_x, pos_y - 1).TileType == TileType.Path) break;
                        if (EastTile(pos_x, pos_y).TileType == TileType.Path && SouthEastTile(pos_x, pos_y).TileType == TileType.Path ||
                            WestTile(pos_x, pos_y).TileType == TileType.Path && SouthWestTile(pos_x, pos_y).TileType == TileType.Path)
                        {
                            break;
                        }

                        southTile.ConvertTileToPath();
                        MazeRec(pos_x, pos_y - 1, 1);
                        
                        break;

                    case 2:
                        //Debug.Log(pos_x.ToString() + " " + pos_y.ToString() + " West");
                        Tile westTile = WestTile(pos_x, pos_y);
                        
                        if (westTile.TileType != TileType.None) break;
                        if (WestTile(pos_x - 1, pos_y).TileType == TileType.Path) break;
                        if (SouthTile(pos_x, pos_y).TileType == TileType.Path && SouthWestTile(pos_x, pos_y).TileType == TileType.Path ||
                            NorthTile(pos_x, pos_y).TileType == TileType.Path && NorthWestTile(pos_x, pos_y).TileType == TileType.Path)
                        {
                            break;
                        }

                        westTile.ConvertTileToPath();
                        MazeRec(pos_x - 1, pos_y, 2);
                        
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
                        MazeRec(pos_x, pos_y + 1, 3);
                        
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

        for (int i = 0; i < c.w; i++)
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

    private void RoomEntrances()
    {
        foreach(Room r in rooms)
        {
            Container c = r.container;

            for (int i = 0; i < c.w; i++)
            {
                Tile southTile = tiles[c.x + i + 1][c.y];
                if (southTile.TileType == TileType.RoomSide && tiles[c.x + i + 1][c.y - 1].TileType == TileType.Path) r.southBorder.Add(southTile);
                
                Tile northTile = tiles[c.x + i + 1][c.y + c.h + 1];
                if (northTile.TileType == TileType.RoomSide && tiles[c.x + i + 1][c.y + c.h + 2].TileType == TileType.Path) r.northBorder.Add(northTile);
            }

            for (int j = 0; j < c.h; j++)
            {
                Tile westTile = tiles[c.x][c.y + j + 1];
                if (westTile.TileType == TileType.RoomSide && tiles[c.x - 1][c.y + j + 1].TileType == TileType.Path) r.westBorder.Add(westTile);

                Tile eastTile = tiles[c.x + c.w + 1][c.y + j + 1];
                if (eastTile.TileType == TileType.RoomSide && tiles[c.x + c.w + 2][c.y + j + 1].TileType == TileType.Path) r.eastBorder.Add(eastTile);
            }

            foreach (List<Tile> list in new List<List<Tile>> { r.southBorder, r.northBorder, r.westBorder, r.eastBorder })
            {
                int length = list.Count;
                if (length > 0)
                {
                    int rand = Random.Range(0, length);
                    list[rand].ConvertTileToEntrance();
                }
            }
        }
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

//private void LinkPoints(Point p1, Point p2)
//{
//    // RECURSIF, ON VA DE p1 et on finit à p2
//    if (p1 != p2)
//    {
//        int hallWidth = p2.x - p1.x;
//        int hallLength = p2.y - p1.y;
//        int hallDirection = Math.Abs(hallWidth) - Math.Abs(hallLength);

//        if (hallDirection > 0)
//        {
//            if (hallWidth > 0) LinkHorizontallyToEast(p1, p2);
//            else if (hallWidth < 0) LinkHorizontallyToWest(p1, p2);
//            else
//            {
//                if (Random.Range(0, 2) == 0) LinkHorizontallyToEast(p1, p2);
//                else LinkHorizontallyToWest(p1, p2);
//            }   
//        }
//        else if (hallDirection < 0) {
//            if (hallLength > 0) LinkVerticallyToNorth(p1, p2);
//            else if (hallLength < 0) LinkVerticallyToSouth(p1, p2); 
//            else
//            {
//                if (Random.Range(0, 2) == 0) LinkHorizontallyToEast(p1, p2);
//                else LinkHorizontallyToWest(p1, p2);
//            }
//        }
//    }
//}

//private Tile NorthEastTile(Point p)
//{
//    try { return tiles[p.x + 1][p.y + 1]; }
//    catch (IndexOutOfRangeException) { return null; }
//}

//private void LinkHorizontallyToEast(Point p1, Point p2)
//{
//    // The vertcial link is done
//    if (p1.x == p2.x) LinkPoints(p1, p2);

//    Tile eastTile = tiles[p1.x + 1][p1.y];
//    if (eastTile.TileType == TileType.Room || eastTile.TileType == TileType.Path) LinkHorizontallyToEast(new Point(p1.x + 1, p1.y), p2);


//    Tile northEastTile = NorthEastTile(p1);
//    if (northEastTile != null) 
//    { 
//        if (northEastTile.TileType == TileType.Room )
//        {

//            ConvertTileToPath(tiles[p1.x][p1.y + 1]);
//            LinkHorizontallyToEast(new Point(p1.x + 1, p1.y + 1), p2);
//        }
//        else if (eastTile.TileType == TileType.Path)
//        {

//        }
//    }
//}