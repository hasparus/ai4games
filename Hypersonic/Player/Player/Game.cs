using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Xml;

struct Point
{
    public bool Equals(Point other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Point && Equals((Point) obj);
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

    public static Point operator +(Point a, Point b) => Point.New(a.x + b.x, a.y + b.y);

    static float Square(float x) => x * x;

    public float DistanceTo(Point other)
        => (float) Math.Sqrt(Square(other.x - x) + Square(other.y - y));
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
        => startTime - new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds > length;
}

/*
    sbyte type; // player -- 0, bomb -- 1
    sbyte id; // player -- id, bomb -- owner's id
    sbyte param1; // 
*/

struct Bomb
{
    public byte owner;
    public byte turnsUntilExplosion;
    public byte range;
    public Point p;

    public Bomb(byte owner, byte turns, byte range, Point point)
    {
        this.owner = owner;
        turnsUntilExplosion = turns;
        this.range = range;
        p = point;
    }
}

struct Player
{
    public byte owner;
    public byte bombs; //number of bombs robot can place
    public byte bombRange;
    public Point p;

    public Player(byte owner, byte bombs, byte bombRange, Point point)
    {
        this.owner = owner;
        this.bombs = bombs;
        this.bombRange = bombRange;
        p = point;
    }
}

struct State
{
    public List<Point> boxes;
    public List<Bomb> bombs;
    public List<Player> robots;

    public bool IsTerminal()
    {
        return boxes.Count == 0;
    }
}

struct Play
{
    public enum Type { Move, Bomb }

    public readonly Type type;
    public readonly Point pos;

    public Play(Type t, Point p)
    {
        type = t;
        pos = p;
    }

    public static Play New(Type t, Point p)
        => new Play(t, p);

    public IEnumerable<Play> NextPlays()
    {
        var ths = this;
        var neighbors = new[] {Point.New(1, 0), Point.New(-1, 0), Point.New(0, -1), Point.New(0, 1)};
        return neighbors.Select(
            x => new[] {new Play(Type.Move, x + ths.pos), new Play(Type.Bomb, x + ths.pos)})
            .SelectMany(x => x);
    }

    public IEnumerable<Play> NextLegalPlays(State s)
    {
        var forbiddenPoints = s.boxes.Concat(s.bombs.Select(x => x.p));
        return NextPlays().Where(k => !forbiddenPoints.Contains(k.pos));
        // not optimal, should have modified NextPlays instead, but there's 8 possible moves so...
    }

    public void Do(string msg = "") 
        => Console.WriteLine($"{type.ToString().ToUpper()} {pos} {msg}");
}

class Info
{
    public static int PlaysTotal = 0;
}


static class Mcts
{
    public static Random Rand = new Random();

    public class Node
    {
        public Node parent;
        public byte index;

        public static double ExplorationParameter = Math.Sqrt(2);

        public int wins;
        public int plays;
        public State state;


        public Play[] legalPlays;
        public bool[] PlayExpanded;
        public Node kids;


        double UCB()
            => wins / (float)plays * ExplorationParameter *
               Math.Sqrt(Math.Log(Info.PlaysTotal) / plays);

        public Node(Node parent, byte index, int w = 0, int p = 0, State s = new State())
        {
            this.parent = parent;
            this.index = index;
            wins = w;
            plays = p;
            state = s;
        }
    }

    public static Play Search(Timer timer)
    {
        var root = new Node(null, 0);


        // main loop
        while (!timer.OutOfTime())
        {
            
        }


        return new Play();
    }

    public static Node TreePolicy(Node v)
    {
        while (!v.state.IsTerminal())
        {
            if (true /* v is not fully expanded */)
            {
                /* return Expand(v)*/
            }
            else
            {
                return null; /*BestChild(v, Cp)*/
            }
        }
    }

    public static Node Expand(Node v)
    {
        var firstNotExpanded = v.PlayExpanded.TakeWhile(x => x == false).Count();
        var action = v.legalPlays[firstNotExpanded];

        //todo use action to generate new Node

    }
}



class Game
{
    public static void Log(string msg) => Console.Error.WriteLine(msg);

    static void Main(string[] args)
    {
        Params.Load();
        Log($"Params loaded: {Params.ToString()}");

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

                if (entityType == 0)
                {
                    var robot = new Player(
                        byte.Parse(inputs[1]),
                        byte.Parse(inputs[4]),
                        byte.Parse(inputs[5]),
                        Point.New(int.Parse(inputs[2]), int.Parse(inputs[3]))
                        );
                }
                else
                {
                    //ugly
                    var bomb = new Bomb(
                        byte.Parse(inputs[1]),
                        byte.Parse(inputs[4]),
                        byte.Parse(inputs[5]),
                        Point.New(int.Parse(inputs[2]), int.Parse(inputs[3]))
                        );
                }
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");
            Console.WriteLine("BOMB 6 5");
        }
    }
}