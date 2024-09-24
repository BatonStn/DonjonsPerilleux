using System.Collections.Generic;

public class Room
{
    public Container container;

    public List<Tile> northBorder;
    public List<Tile> eastBorder;
    public List<Tile> southBorder;
    public List<Tile> westBorder;

    public Room()
    {
        container = new Container();
        northBorder = new List<Tile>();
        eastBorder = new List<Tile>();
        southBorder = new List<Tile>();
        westBorder = new List<Tile>();
    }

    public Room(Container c)
    {
        container = c;

        northBorder = new List<Tile>();
        eastBorder = new List<Tile>();
        southBorder = new List<Tile>();
        westBorder = new List<Tile>();
    }

    public Room(Container c, List<Tile> nB, List<Tile> eB, List<Tile> sB, List<Tile> wB)
    {
        container = c;
        northBorder = nB;
        eastBorder = eB;
        southBorder = sB;
        westBorder = wB;
    }
}
