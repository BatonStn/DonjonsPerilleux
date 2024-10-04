using System.Collections.Generic;
using UnityEngine;

public class Room
{
    public Container container;

    public List<Vector2> northBorder;
    public List<Vector2> eastBorder;
    public List<Vector2> southBorder;
    public List<Vector2> westBorder;

    public Room()
    {
        container = new Container();
        northBorder = new List<Vector2>();
        eastBorder = new List<Vector2>();
        southBorder = new List<Vector2>();
        westBorder = new List<Vector2>();
    }

    public Room(Container c)
    {
        container = c;

        northBorder = new List<Vector2>();
        eastBorder = new List<Vector2>();
        southBorder = new List<Vector2>();
        westBorder = new List<Vector2>();
    }

    public Room(Container c, List<Vector2> nB, List<Vector2> eB, List<Vector2> sB, List<Vector2> wB)
    {
        container = c;
        northBorder = nB;
        eastBorder = eB;
        southBorder = sB;
        westBorder = wB;
    }
}
