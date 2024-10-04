using UnityEngine;

public enum TileType
{
    None,
    RoomSide,
    Path,
    VisitedPath,
    TruePath,
    Room,
    RoomEntrance,
    MazeBegin,
    Wall
}

public enum Direction
{
    East = 0,
    South = 1,
    West = 2,
    North = 3,
    None
}

public class Tile
{
    public TileType TileType { get; set; }
    public GameObject TileGo { get; set; }

    public Tile()
    {
        TileType = TileType.None;
    }

    public void ConvertTileToNone()
    {
        TileType = TileType.None;
        TileGo.GetComponent<Renderer>().material.color = Color.black;
    }

    public void ConvertTileToRoom()
    {
        TileType = TileType.Room;
        TileGo.GetComponent<Renderer>().material.color = Color.red;
    }

    public void ConvertTileToWall()
    {
        TileType = TileType.Wall;
        TileGo.GetComponent<Renderer>().material.color = Color.black;
    }

    public void ConvertTileToPath()
    {
        TileType = TileType.Path;
        TileGo.GetComponent<Renderer>().material.color = Color.yellow;
    }

    public void ConvertTileToVisitedPath()
    {
        TileType = TileType.VisitedPath;
        TileGo.GetComponent<Renderer>().material.color = new Color(1f, .5f, 0);
    }

    public void ConvertTileToTruePath()
    {
        TileType = TileType.TruePath;
        TileGo.GetComponent<Renderer>().material.color = Color.green;
    }

    public void ConvertTileToRoomSide()
    {
        TileGo.GetComponent<Renderer>().material.color = Color.black;
        TileType = TileType.RoomSide;
    }

    public void ConvertTileToEntrance()
    {
        TileGo.GetComponent<Renderer>().material.color = Color.magenta;
        TileType = TileType.RoomEntrance;
    }
}
