using UnityEngine;

public class Container
{
    public int x, y, w, h;
    readonly public int bottom, top, left, right;

    public Container()
    {
        x = 0;
        y = 0;
        w = 0;
        h = 0;
    }

    public Vector2 Center() { return new Vector2(x + ((float)w / 2), y + ((float)h / 2)); }

    public Container(int x, int y, int w, int h)
    {
        this.x = x;
        this.y = y;
        this.w = w;
        this.h = h;
        top = y;
        bottom = y + h;
        left = x;
        right = x + w;
    }

    public bool IsEmpty()
    {
        return (w == 0 && h == 0);
    }
}