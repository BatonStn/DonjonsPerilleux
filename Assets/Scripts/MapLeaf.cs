using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class MapLeaf
{
    const int MIN_LEAF_SIZE = 5;
    const int MAX_ROOM_SIZE = 7;

    // the position and size of this Leaf	
    public int leafX, leafY, leafWidth, leafHeight, leafPadding;

    // the Leaf's left child Leaf	   
    public MapLeaf leftChild;

    // the Leaf's right child Leaf	
    public MapLeaf rightChild;

    // the room that is inside this Leaf
    public Container room;

    public List<Point> points;

    public MapLeaf(int x, int y, int width, int height, int padding)
    {
        leafX = x;
        leafY = y;
        leafWidth = width;
        leafHeight = height;
        leafPadding = padding;
    }

    public void CreateBSPMap()
    {
        //GameObject lineRd1 = new()
        //{
        //    name = "Surround Line"
        //};

        //LineRenderer lineRenderer = lineRd1.AddComponent<LineRenderer>();
        //lineRenderer.material = new Material(Shader.Find("Sprites/Default"))
        //{
        //    color = Color.green
        //};
        //lineRenderer.widthMultiplier = 0.4f;
        //lineRenderer.positionCount = 5;
        //lineRenderer.SetPositions(new Vector3[] { new(1, 1, 0), new(1, leafHeight + 1, 0), new(leafWidth + 1, leafHeight + 1, 0), new(leafWidth + 1, 1, 0), new(1, 1, 0)});

        if (Split(0)) CreateBSPMapRec(1);

        CreateRooms();
    }

    private void CreateBSPMapRec(int iterator)
    {
        if (iterator < 4)
        {
            if (leftChild.Split(iterator)) leftChild.CreateBSPMapRec(iterator + 1);
            if (rightChild.Split(iterator)) rightChild.CreateBSPMapRec(iterator + 1);
        }
    }

    public bool Split(int i)
    {
        // we're already split! Abort!
        if (leftChild != null || rightChild != null) return false;

        // determine direction of split	
        bool splitH;
        if (leafWidth > leafHeight) splitH = false;
        else if (leafHeight > leafWidth) splitH = true;
        else splitH = Random.Range(0, 2) == 0;

        // determines the maximum height or width
        int area = (splitH ? leafHeight : leafWidth) - leafPadding - 2 * MIN_LEAF_SIZE;

        // the area is too small to split any more
        if (area < 0) return false;

        // determine where we're going to split
        int split = MIN_LEAF_SIZE + leafPadding / 2 + Random.Range(0, area + 1);

        //GameObject line = new()
        //{
        //    name = i.ToString() + " " + "Split Line"
        //};

        //LineRenderer lineRenderer = line.AddComponent<LineRenderer>();
        //lineRenderer.material = new(Shader.Find("Sprites/Default"));
        //if (i == 0) lineRenderer.material.color = Color.green;
        //else if (i == 1) lineRenderer.material.color = Color.cyan;
        //else if (i == 2) lineRenderer.material.color = Color.blue;
        //else lineRenderer.material.color = Color.magenta;
        //lineRenderer.widthMultiplier = 0.2f;
        //lineRenderer.positionCount = 2;

        //GameObject line2 = new()
        //{
        //    name = i.ToString() + " " + "Split Line 2"
        //};

        //LineRenderer lineRenderer2 = line2.AddComponent<LineRenderer>();
        //lineRenderer2.material = new(Shader.Find("Sprites/Default"));
        //if (i == 0) lineRenderer2.material.color = Color.green;
        //else if (i == 1) lineRenderer2.material.color = Color.cyan;
        //else if (i == 2) lineRenderer2.material.color = Color.blue;
        //else lineRenderer2.material.color = Color.magenta;
        //lineRenderer2.widthMultiplier = 0.2f;
        //lineRenderer2.positionCount = 2;
        //lineRenderer2.SetPositions(new Vector3[] { new(1, 1, 0), new(1, leafHeight + 1, 0) });

        // create our left and right children based on the direction of the split
        // split Horizontal
        if (splitH)
        {
            leftChild = new MapLeaf(leafX, leafY, leafWidth, split - leafPadding / 2, leafPadding);
            rightChild = new MapLeaf(leafX, leafY + split + leafPadding / 2, leafWidth, leafHeight - split - leafPadding / 2, leafPadding);

            //lineRenderer.SetPositions(new Vector3[] { new(leafX + 1, split + 1 + leafPadding/2, 0), new(leafX + leafWidth + 1, split + 1 + leafPadding / 2, 0) });
            //lineRenderer2.SetPositions(new Vector3[] { new(leafX + 1, split + 1 - leafPadding / 2, 0), new(leafX + leafWidth + 1, split + 1 - leafPadding / 2, 0) });
        }
        // split Vertical
        else
        {
            leftChild = new MapLeaf(leafX, leafY, split - leafPadding / 2, leafHeight, leafPadding);
            rightChild = new MapLeaf(leafX + split + leafPadding / 2, leafY, leafWidth - split - leafPadding / 2, leafHeight, leafPadding);

            //lineRenderer.SetPositions(new Vector3[] { new(split + 1 + leafPadding / 2, leafY + 1, 0), new(split + 1 + leafPadding / 2, leafY + leafHeight + 1, 0) });
            //lineRenderer2.SetPositions(new Vector3[] { new(split + 1 - leafPadding / 2, leafY + 1, 0), new(split + 1 - leafPadding / 2, leafY + leafHeight + 1, 0) });
        }

        return true;
    }

    public void CreateRooms()
    {
        // We're at the end of a leaf, we can create a room
        if (leftChild == null && rightChild == null)
        {
            Point roomSize;
            Point roomPos;

            int maxWidth = Math.Min(MAX_ROOM_SIZE, leafWidth + 1);
            int maxHeight = Math.Min(MAX_ROOM_SIZE, leafHeight + 1);

            // the room can be between 4 x 4 tiles to the size of the leaf
            roomSize = new Point(Random.Range(4, maxWidth), Random.Range(4, maxHeight));
            //roomSize = new Point(leafWidth, leafHeight);

            // place the room within the leaf
            roomPos = new Point(Random.Range(0, leafWidth - roomSize.x + 1), Random.Range(0, leafHeight - roomSize.y + 1));
            room = new Container(leafX + roomPos.x, leafY + roomPos.y, roomSize.x, roomSize.y);
        }
        else if (leftChild != null && rightChild != null)
        {
            // Crée les deux rooms
            leftChild.CreateRooms();
            rightChild.CreateRooms();

            // Crée un Path entre les 2
            //halls = CreateHall(leftChild.GetRoom(), rightChild.GetRoom());

            //points = CreateHallPoints(leftChild.GetRoom(), rightChild.GetRoom());
        }
    }

    public Container GetRoom()
    {
        if (leftChild != null && rightChild != null)
        {
            if (Random.Range(0, 2) == 0) return leftChild.GetRoom();
            else return rightChild.GetRoom();
        }
        else return room;
    }

    private List<Point> CreateHallPoints(Container l, Container r)
    {
        List<Point> points = new();

        // Two random points in each room
        Point point1 = new(Random.Range(l.left + 1, l.right - 2), Random.Range(l.top + 1, l.bottom - 2));
        Point point2 = new(Random.Range(r.left + 1, r.right - 2), Random.Range(r.top + 1, r.bottom - 2));

        points.Add(point1);
        points.Add(point2);

        return points;
    }

    //private List<Container> CreateHall(Container l, Container r)
    //{
    //    List<Container> halls = new();

    //    // Two random points in each room
    //    Point point1 = new(Random.Range(l.left + 1, l.right - 2), Random.Range(l.top + 1, l.bottom - 2));
    //    Point point2 = new(Random.Range(r.left + 1, r.right - 2), Random.Range(r.top + 1, r.bottom - 2));

    //    int hallWidth = point2.x - point1.x;
    //    int hallHeight = point2.y - point1.y;

    //    if (hallWidth < 0)
    //    {
    //        if (hallHeight < 0)
    //        {
    //            if (Random.Range(0, 2) > 0)
    //            {
    //                halls.Add(new Container(point2.x, point1.y, Math.Abs(hallWidth), 1));
    //                halls.Add(new Container(point2.x, point2.y, 1, Math.Abs(hallHeight)));
    //            }
    //            else
    //            {
    //                halls.Add(new Container(point2.x, point2.y, Math.Abs(hallWidth), 1));
    //                halls.Add(new Container(point1.x, point2.y, 1, Math.Abs(hallHeight)));
    //            }
    //        }
    //        else if (hallHeight > 0)
    //        {
    //            if (Random.Range(0, 2) > 0)
    //            {
    //                halls.Add(new Container(point2.x, point1.y, Math.Abs(hallWidth), 1));
    //                halls.Add(new Container(point2.x, point1.y, 1, Math.Abs(hallHeight)));
    //            }
    //            else
    //            {
    //                halls.Add(new Container(point2.x, point2.y, Math.Abs(hallWidth), 1));
    //                halls.Add(new Container(point1.x, point1.y, 1, Math.Abs(hallHeight)));
    //            }
    //        }
    //        else halls.Add(new Container(point2.x, point2.y, Math.Abs(hallWidth), 1));
    //    }
    //    else if (hallWidth > 0)
    //    {
    //        if (hallHeight < 0)
    //        {
    //            if (Random.Range(0, 2) > 0)
    //            {
    //                halls.Add(new Container(point1.x, point2.y, Math.Abs(hallWidth), 1));
    //                halls.Add(new Container(point1.x, point2.y, 1, Math.Abs(hallHeight)));
    //            }
    //            else
    //            {
    //                halls.Add(new Container(point1.x, point1.y, Math.Abs(hallWidth), 1));
    //                halls.Add(new Container(point2.x, point2.y, 1, Math.Abs(hallHeight)));
    //            }
    //        }
    //        else if (hallHeight > 0)
    //        {
    //            if (Random.Range(0, 2) > 0)
    //            {
    //                halls.Add(new Container(point1.x, point1.y, Math.Abs(hallWidth), 1));
    //                halls.Add(new Container(point2.x, point1.y, 1, Math.Abs(hallHeight)));
    //            }
    //            else
    //            {
    //                halls.Add(new Container(point1.x, point2.y, Math.Abs(hallWidth), 1));
    //                halls.Add(new Container(point1.x, point1.y, 1, Math.Abs(hallHeight)));
    //            }
    //        }
    //        else halls.Add(new Container(point1.x, point1.y, Math.Abs(hallWidth), 1));
    //    }
    //    else
    //    {
    //        if (hallHeight < 0)
    //        {
    //            halls.Add(new Container(point2.x, point2.y, 1, Math.Abs(hallHeight)));
    //        }
    //        else if (hallHeight > 0)
    //        {
    //            halls.Add(new Container(point1.x, point1.y, 1, Math.Abs(hallHeight)));
    //        }
    //    }

    //    return halls;
    //}
}




