using UnityEngine;

public enum TileType
{
    None,
    RoomSide,
    Path,
    Room,
    MazeBegin,
    Wall
}

public class Tile
{
    public TileType TileType { get; set; }
    public GameObject TileGo { get; set; }

    public Tile()
    {
        TileType = TileType.None;
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

    public void ConvertTileToRoomSide()
    {
        TileGo.GetComponent<Renderer>().material.color = Color.grey;
        TileType = TileType.RoomSide;
    }

    public void ConvertTileToEntrance()
    {
        TileGo.GetComponent<Renderer>().material.color = Color.magenta;
    }
}
