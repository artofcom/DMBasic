using UnityEngine;
using System.Collections;

using NOVNINE.Diagnostics;

// Point Class to handle Coordinate Functions
[System.Serializable]
public struct Point {
    public static Point Empty = new Point(0, 0);
    // List of cardinal directions useful in offset and Coordinate calculations
    public enum CardinalDirection { None = -1, N = 0, NE = 1, E = 2, SE = 3, S = 4, SW = 5, W = 6, NW = 7 };
	
    // Private data members
    public int x;
    public int y;

    public int X
    {
        get {
            return x;
        } set {
            x = value;
        }
    }
    public int Y
    {
        get {
            return y;
        } set {
            y = value;
        }
    }

    // Standard Constructor
    public Point(int X, int Y)
    {
        // Set the internal values
        this.x = X;
        this.y = Y;
    }

    // Limit the values of the point to an additional supplied arbitrary
    // bounds - useful when working with calculated values
    public Point(int X, int Y, int Min, int Max)
    {
        // Set the internal values
        this.x = X;
        this.y = Y;

        // Limit the x value if necessary
        if (this.x > Max) {
            this.x = Max;
        } else if (this.x < Min) {
            this.x = Min;
        }

        // Limit the y value if necessary
        if (this.y > Max) {
            this.y = Max;
        } else if (this.y < Min) {
            this.y = Min;
        }
    }

    // Copy Constructor - we need to use this because unlike C++ we cannot
    // overload the assignment operator
    public Point(Point P2)
    {
        this.x = 0;
        this.y = 0;

        // If we have a valid reference
        if ((object)P2 != null) {
            // Set the internal values
            this.x = P2.x;
            this.y = P2.y;
        }
    }

    // Override the ToString method
    public override string ToString()
    {
        return string.Format("[{0},{1}]", this.x, this.y);
    }

    // Overload the equality operator
    public static bool operator ==(Point P1, Point P2)
    {
        // If we have a valid reference
        if ((object)P1 == null) {
            return false;
        }
        if ((object)P2 == null) {
            return false;
        }

        // Check for full equality on both values
        return (P1.x == P2.x && P1.y == P2.y);
    }

    // Overload the non-equality operator
    public static bool operator !=(Point P1, Point P2)
    {
        // If we have a valid reference
        if ((object)P1 == null) {
            return false;
        }
        if ((object)P2 == null) {
            return false;
        }

        // Check for inequality on either values
        return (P1.x != P2.x || P1.y != P2.y);
    }

    // Overload the equality operator
    public static Point operator -(Point P1, Point P2)
    {
        return new Point(P1.X - P2.X, P1.Y - P2.Y);
    }

    // Overload the equals operator
    public override bool Equals(System.Object P2)
    {
        Debugger.Assert(P2 != null);

        // If we have a valid reference
        if ((object)P2 == null) {
            return false;
        }

        // Check we can cast the incoming object to a Point
        //Point p = P2 as Point;
        Point p = (Point)P2;
        if ((System.Object)p == null) {
            return false;
        }

        // Check for full equality on both values
        return (this.x == p.x && this.y == p.y);
    }

    // Provide a custom GetHashCode function (needed when the Equals operator is
    // overridden
    public override int GetHashCode()
    {
        // Use XOR
        return this.x ^ this.y;
    }

    // Provide an equivalent of an assignment operator
    public void Set(Point P2)
    {
        // If we have a valid reference
        if ((object)P2 != null) {
            // Set the internal values
            this.x = P2.x;
            this.y = P2.y;
        }
    }

    // Provide another equivalent of an assignment operator
    public void Set(int X, int Y)
    {
        // Set the internal values
        this.x = X;
        this.y = Y;
    }

    // Return the euclidean distance between two points
    public float DistanceTo(Point P2)
    {
        // If we have a valid reference
        if ((object)P2 == null) {
            return -1;
        }

        // Return the distance (as an int, rounded down)

        return Mathf.Sqrt((this.x - P2.x) * (this.x - P2.x) +
                          (this.y - P2.y) * (this.y - P2.y));
    }

    // get the difference between two points
    public void SetFromOffset(Point P1, Point P2)
    {
        // Set the default values
        this.x = 0;
        this.y = 0;

        // If we have a valid reference
        if ((object)P1 == null) {
            return;
        }
        if ((object)P2 == null) {
            return;
        }

        // Get the offsets
        this.x = (P1.x - P2.x);
        this.y = (P1.y - P2.y);
    }

    // Return the difference between two points optionally limiting the
    // values returned
    public void SetFromOffset(Point P1, Point P2, int Min, int Max)
    {
        // Set the default values
        this.x = 0;
        this.y = 0;

        // If we have a valid reference
        if ((object)P1 == null) {
            return;
        }
        if ((object)P2 == null) {
            return;
        }

        // Get the offsets
        this.x = (P1.x - P2.x);
        this.y = (P1.y - P2.y);

        // Limit the x value if necessary
        if (this.x > Max) {
            this.x = Max;
        } else if (this.x < Min) {
            this.x = Min;
        }

        // Limit the y value if necessary
        if (this.y > Max) {
            this.y = Max;
        } else if (this.y < Min) {
            this.y = Min;
        }
    }

    // Get the direction of one point from another as an enum
    public CardinalDirection DirectionTo(Point P2)
    {
        // If we have a valid reference
        if ((object)P2 == null) {
            return CardinalDirection.None;
        }

        // Set up an offset array to convert the offsets of the two points
        // into a direction
        Point[] Directions = new Point[8];
        Directions[(int)CardinalDirection.N] = new Point(0, -1);
        Directions[(int)CardinalDirection.NE] = new Point(1, -1);
        Directions[(int)CardinalDirection.E] = new Point(1, 0);
        Directions[(int)CardinalDirection.SE] = new Point(1, 1);
        Directions[(int)CardinalDirection.S] = new Point(0, 1);
        Directions[(int)CardinalDirection.SW] = new Point(-1, 1);
        Directions[(int)CardinalDirection.W] = new Point(-1, 0);
        Directions[(int)CardinalDirection.NW] = new Point(-1, -1);

        // Get the offset from one point to another
        Point P = new Point();
        P.SetFromOffset(this, P2, -1, 1);

        // Find the matching direction
        int Index = 0;
        foreach (Point Item in Directions) {
            if (Item == P) {
                return (CardinalDirection)Index;
            } else {
                Index++;
            }
        }

        // Return the null value just in case
        return CardinalDirection.None;
    }

    // Check if two points are adjacent to each other
    public bool Adjacent(Point P2)
    {
        // Test if the points are 1 square apart
        return (this.DistanceTo(P2) == 1);
    }

}

