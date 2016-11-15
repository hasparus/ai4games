using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;

struct Point
{
    public bool Equals(Point other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Point && Equals((Point)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x * 397) ^ y;
        }
    }

    public int x;
    public int y;

    public Point(int x = -1, int y = -1)
    {
        this.x = x;
        this.y = y;
    }

    public static Point New(int x = -1, int y = -1)
        => new Point(x, y);

    public override string ToString() => $"{x} {y}";

    public static bool operator ==(Point a, Point b)
        => a.x == b.x && a.y == b.y;

    public static bool operator !=(Point a, Point b) => !(a == b);

    static float Square(float x) => x * x;

    public float DistanceTo(Point other)
        => (float)Math.Sqrt(Square(other.x - x) + Square(other.y - y));
}

class Params
{
    public static int Width;
    public static int Height;
    public static int MyPlayerId;

    public static void Load()
    {
        var inputs = Console.ReadLine().Split(' ');
        Width = int.Parse(inputs[0]);
        Height = int.Parse(inputs[1]);
        MyPlayerId = int.Parse(inputs[2]);
    }

    public new static string ToString()
        => $"Board is {Width} x {Height}. I'm {MyPlayerId}.";
}

class Timer
{
    readonly int startTime;
    readonly int length;
    public Timer(int miliseconds)
    {
        startTime = (int) new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds;
        length = miliseconds;
    }

    public bool OutOfTime()
        => startTime - (new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds) > length;
}

class Player
{
    public static void Log(string msg) => Console.Error.WriteLine(msg);

    static void Main(string[] args)
    {
        Params.Load();
        Log("Params loaded: {");

        var timer = new Timer(90);


        string[] inputs;

        // game loop
        while (true)
        {
            for (int i = 0; i < Params.Height; i++)
            {
                string row = Console.ReadLine();
            }
            int entities = int.Parse(Console.ReadLine());
            for (int i = 0; i < entities; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int entityType = int.Parse(inputs[0]);
                int owner = int.Parse(inputs[1]);
                int x = int.Parse(inputs[2]);
                int y = int.Parse(inputs[3]);
                int param1 = int.Parse(inputs[4]);
                int param2 = int.Parse(inputs[5]);
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            Console.WriteLine("BOMB 6 5");
        }
    }
}