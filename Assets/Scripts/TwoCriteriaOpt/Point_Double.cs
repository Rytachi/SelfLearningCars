using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Point_Double : IComparable<Point_Double>
{
    public double x, y, eucDis, timeElapsed, fitness_Ratio;
    public int number;
    public bool finished = false;
    public Point_Double(double x, double y, double timeElapsed)
    {
        this.x = x;
        this.y = y;
        this.timeElapsed = timeElapsed;
    }

    public int CompareTo(Point_Double comparePart)
    {
        // A null value means that this object is greater.
        if (comparePart == null)
            return 1;

        else
            return comparePart.eucDis.CompareTo(this.eucDis);
    }
    public int CompareTo(double x)
    { 
        return this.x.CompareTo(x);
    }
}
