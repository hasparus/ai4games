using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class PriorityQueue<T> : IEnumerable<T>
{
    readonly IComparer<T> comparer;
    T[] heap;

    public PriorityQueue() : this(null)
    {
    }

    public PriorityQueue(int capacity) : this(capacity, null)
    {
    }

    public PriorityQueue(IComparer<T> comparer) : this(16, comparer)
    {
    }

    public PriorityQueue(int capacity, IComparer<T> comparer)
    {
        this.comparer = comparer ?? Comparer<T>.Default;
        heap = new T[capacity];
    }

    public int Count { get; private set; }

    public IEnumerator<T> GetEnumerator()
    {
        return ((IEnumerable<T>) heap).GetEnumerator();
    }

    /// <summary>
    ///     Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    ///     An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Push(T v)
    {
        if (Count >= heap.Length) Array.Resize(ref heap, Count * 2);
        heap[Count] = v;
        SiftUp(Count++);
    }

    public T Pop()
    {
        var v = Top();
        heap[0] = heap[--Count];
        if (Count > 0) SiftDown(0);
        return v;
    }

    public T Top()
    {
        if (Count > 0) return heap[0];
        throw new InvalidOperationException("Oops. Count == 0.");
    }

    void SiftUp(int n)
    {
        var v = heap[n];
        for (var n2 = n / 2; n > 0 && comparer.Compare(v, heap[n2]) > 0; n = n2, n2 /= 2) heap[n] = heap[n2];
        heap[n] = v;
    }

    void SiftDown(int n)
    {
        var v = heap[n];
        for (var n2 = n * 2; n2 < Count; n = n2, n2 *= 2)
        {
            if (n2 + 1 < Count && comparer.Compare(heap[n2 + 1], heap[n2]) > 0) n2++;
            if (comparer.Compare(v, heap[n2]) >= 0) break;
            heap[n] = heap[n2];
        }
        heap[n] = v;
    }
}

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

    public static Point operator +(Point a, Point b) => New(a.x + b.x, a.y + b.y);

    static float Square(float x) => x * x;

    public float DistanceTo(Point other)
        => (float) Math.Sqrt(Square(other.x - x) + Square(other.y - y));

    public static List<Point> Versors()
        => new List<Point>(4)
        {
            New(1, 0),
            New(-1, 0),
            New(0, -1),
            New(0, 1)
        };

    public bool OnBoard(int width, int height)
        => x >= 0 && y >= 0 && x < width && y < height;
}

class Boomer
{
    public const int EntityType = 0;
    public int bombsAvailable;
    public int explosionRange;
    public int id;
    public Point position = new Point(0, 0);

    public override string ToString()
        => $"Boomer [{position}, {bombsAvailable}, {explosionRange}]";

    public Bomb CreateBomb(Point position)
    {
        var b = new Bomb();
        b.position = position;
        b.timer = Bomb.Countdown;
        b.explosionRange = explosionRange;
        return b;
    }
}

class Bomb
{
    public const int ExplodeNextTurn = 2;
    public const int AlreadyExploded = 2;
    public const int NoExplosion = 0;

    public const int EntityType = 1;
    public const int Countdown = 8;
    public int explosionRange;

    public Point position = new Point(0, 0);
    public int timer;

    public override string ToString()
        => $"Bomb [{position}, {timer}, {explosionRange}]";
}

class Item
{
    public enum Type
    {
        ExtraRange = 1,
        ExtraBomb = 2
    }

    public const int EntityCode = 2;

    public Point position = new Point(0, 0);
    public Type type;
}

class Cell
{
    public enum Type
    {
        Floor = '.',
        Box = '0',
        BoxWithExtraRange = '1',
        BoxWithExtraBomb = '2',
        ExtraRange = 'R',
        ExtraBomb = 'B',
        Wall = 'X',
        Bomb = '*'
    }

    public Point position;

    public static bool IsBox(Type t)
        => t == Type.Box || t == Type.BoxWithExtraBomb || t == Type.BoxWithExtraRange;

    public static bool IsBonus(Type t)
        => t == Type.ExtraRange || t == Type.ExtraBomb;

    public static bool IsPassable(Type t)
        => t == Type.Floor || IsBonus(t);

    public static bool IsNonPassable(Type t)
        => t == Type.Wall || t == Type.Bomb || IsBox(t);

    /* => !IsPassable(t); */

    public static bool StopsExplosions(Type t)
        => IsNonPassable(t) || IsBonus(t);

    public static bool IsDestructible(Type t)
        => IsBox(t) || IsBonus(t);

    public override string ToString() => $"{position}";

    public override int GetHashCode() => position.GetHashCode();

    public override bool Equals(object obj)
    {
        var cell = (Cell) obj;
        return cell != null && cell.position == position;
    }
}

class IntegerParameter
{
    public int Value;
}

class BoolParameter
{
    public bool value;
}

class PathParameter
{
    public int distance = int.MaxValue;
    public Cell PreviousCell;
}

class TypeParameter
{
    public Cell.Type value = Cell.Type.Floor;
}

class IntegerMap
{
    public readonly List<IntegerParameter> AsList;
    public readonly IntegerParameter[,] Values;

    IntegerMap(int width, int height, int defaultValue)
    {
        Values = new IntegerParameter[width, height];
        AsList = new List<IntegerParameter>(width * height);
        for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y)
            {
                var p = new IntegerParameter();
                p.Value = defaultValue;
                Values[x, y] = p;
                AsList.Add(p);
            }
    }

    public IntegerParameter At(Point pos)
        => Values[pos.x, pos.y];

    public static IntegerMap CreateUtilityMap(int width, int height)
        => new IntegerMap(width, height, 0);

    public static IntegerMap CreateExplosionMap(int width, int height)
        => new IntegerMap(width, height, 0);

    public static IntegerMap CreateSafetyMap(int width, int height)
        => new IntegerMap(width, height, Bomb.AlreadyExploded);
}

class BoolMap
{
    public readonly List<BoolParameter> AsList;
    public readonly BoolParameter[,] Values;

    BoolMap(int width, int height, bool defaultValue)
    {
        Values = new BoolParameter[width, height];
        AsList = new List<BoolParameter>(width * height);
        for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y)
            {
                var p = new BoolParameter();
                p.value = defaultValue;
                Values[x, y] = p;
                AsList.Add(p);
            }
    }

    public BoolParameter At(Point pos)
        => Values[pos.x, pos.y];

    public static BoolMap CreateTrueMap(int width, int height)
        => new BoolMap(width, height, true);

    public static BoolMap CreateFalseMap(int width, int height)
        => new BoolMap(width, height, false);
}

class PathMap
{
    public readonly List<PathParameter> AsList;
    public readonly PathParameter[,] Values;

    PathMap(int width, int height)
    {
        Values = new PathParameter[width, height];
        AsList = new List<PathParameter>(width * height);
        for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y)
            {
                var p = new PathParameter();
                Values[x, y] = p;
                AsList.Add(p);
            }
    }

    public PathParameter At(Point pos)
        => Values[pos.x, pos.y];

    public static PathMap CreatePathMap(int width, int height)
        => new PathMap(width, height);
}

class TypeMap
{
    public readonly List<TypeParameter> AsList;
    public readonly TypeParameter[,] Values;

    TypeMap(int width, int height)
    {
        Values = new TypeParameter[width, height];
        AsList = new List<TypeParameter>(width * height);
        for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y)
            {
                var p = new TypeParameter();
                Values[x, y] = p;
                AsList.Add(p);
            }
    }

    TypeMap(int width, int height, TypeMap other)
    {
        Values = new TypeParameter[width, height];
        AsList = new List<TypeParameter>(width * height);
        for (var x = 0; x < width; ++x)
            for (var y = 0; y < height; ++y)
            {
                var p = new TypeParameter();
                p.value = other.Values[x, y].value;
                Values[x, y] = p;
                AsList.Add(p);
            }
    }

    public TypeParameter At(Point pos)
        => Values[pos.x, pos.y];

    public TypeMap DeepCopy(int width, int height)
        => new TypeMap(width, height, this);

    public static TypeMap CreateTypeMap(int width, int height)
        => new TypeMap(width, height);
}

class Grid
{
    public List<Cell> AsList;
    public Cell[,] Cells;
    public int Height;
    public int Width;

    public void Clear()
    {
        Cells = new Cell[Width, Height];
        AsList = new List<Cell>(Width * Height);
    }

    public string ShowTypes(TypeMap typeMap)
    {
        var sb = new StringBuilder(Height * Width);
        for (var rowIndex = 0; rowIndex < Height; ++rowIndex)
        {
            for (var columnIndex = 0; columnIndex < Width; ++columnIndex)
            {
                sb.Append(typeMap.Values[columnIndex, rowIndex].value); // (char)?
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    public string ShowUtility(IntegerMap utilityMap)
    {
        var sb = new StringBuilder(Height * Width);
        sb.Append("Utility\n");
        for (var rowIndex = 0; rowIndex < Height; ++rowIndex)
        {
            for (var columnIndex = 0; columnIndex < Width; ++columnIndex)
            {
                sb.Append(utilityMap.Values[columnIndex, rowIndex].Value); // (char)?
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    public string ShowDistanceFromPlayer(PathMap pathMap)
    {
        var sb = new StringBuilder(Height * Width);
        sb.Append("Distance\n");
        for (var rowIndex = 0; rowIndex < Height; ++rowIndex)
        {
            for (var columnIndex = 0; columnIndex < Width; ++columnIndex)
            {
                var distance = pathMap.Values[columnIndex, rowIndex].distance;
                sb.Append(
                     (distance == int.MaxValue) ? "." : Math.Min(9, distance).ToString());
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    public string ShowExplosionmap(IntegerMap explosionMap, TypeMap typeMap)
    {
        var sb = new StringBuilder(Height * Width);
        sb.Append("Explosions\n");
        for (var rowIndex = 0; rowIndex < Height; ++rowIndex)
        {
            for (var columnIndex = 0; columnIndex < Width; ++columnIndex)
            {
                var timeToExplosion = explosionMap.Values[columnIndex, rowIndex].Value;
                if (timeToExplosion == Bomb.NoExplosion)
                    sb.Append(Cell.IsNonPassable(typeMap.Values[columnIndex, rowIndex].value) ? 'X' : '.');
                else
                    sb.Append(timeToExplosion);
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }

    public string ShowSafetyMap(IntegerMap safetyMap)
    {
        var sb = new StringBuilder(Height * Width);
        sb.Append("Safety\n");
        for (var rowIndex = 0; rowIndex < Height; ++rowIndex)
        {
            for (var columnIndex = 0; columnIndex < Width; ++columnIndex)
            {
                var p = safetyMap.Values[columnIndex, rowIndex];
                sb.Append(p.Value);
            }
            sb.Append('\n');
        }
        return sb.ToString();
    }
}

class World
{
    public readonly Dictionary<int, Boomer> Enemies = new Dictionary<int, Boomer>(3);
    public readonly List<Bomb> EnemyBombs = new List<Bomb>();
    public readonly Grid Grid = new Grid();
    public readonly Boomer Player = new Boomer();
    public readonly List<Bomb> PlayerBombs = new List<Bomb>();
    public int BombCount;
    public int BonusCount;

    public int BoxCount;
    public bool Changed;
    public bool PlayersBombCountChanged;

    public List<Bomb> AllBombs => PlayerBombs.Concat(EnemyBombs).ToList();

    public void Initialize()
    {
        var inputs = Console.ReadLine().Split(' ');
        Grid.Width = int.Parse(inputs[0]);
        Grid.Height = int.Parse(inputs[1]);
        Player.id = int.Parse(inputs[2]);
    }
}

abstract class Action : IComparable<Action>
{
    public int order;

    public int priority = Priority.Normal;

    public int CompareTo(Action other)
    {
        var priorityDiff = priority - other.priority;
        if (priorityDiff != 0) return priorityDiff;
        return order - other.order;
    }

    public abstract void Execute();

    public abstract bool CheckPreconditions();

    public abstract bool CheckPostconditions();

    public static class Priority
    {
        public static readonly int Low = 3;
        public static readonly int Normal = 2;
        public static readonly int High = 1;
    }
}

class Move : Action
{
    readonly Boomer player;
    readonly Point targetPoint;

    public Move(Point targetPoint, Boomer player)
    {
        this.player = player;
        this.targetPoint = targetPoint;
    }


    public override bool CheckPreconditions() => !(player.position == targetPoint);

    public override bool CheckPostconditions() => player.position == targetPoint;

    public override string ToString() => $"MOVE {targetPoint}";

    public override void Execute() => Console.WriteLine(this);
}

class SkipTurn : Action
{
    readonly bool done = false;
    readonly Boomer player;

    public SkipTurn(Boomer player)
    {
        this.player = player;
    }

    public override bool CheckPreconditions() => true;

    public override bool CheckPostconditions() => done;

    public override string ToString() => $"MOVE {player.position}";

    public override void Execute() => Console.WriteLine(this);
}

class PlaceBomb : Action
{
    readonly Boomer player;
    readonly Point targetPoint;
    bool bombPlaced;

    public PlaceBomb(Point targetPoint, Boomer player)
    {
        this.player = player;
        this.targetPoint = targetPoint;
    }

    public override bool CheckPreconditions() => !bombPlaced;

    public override bool CheckPostconditions() => bombPlaced;

    public override string ToString() => $"Place bomb at {player.position}";

    public override void Execute()
    {
        if (player.position == targetPoint)
        {
            Console.WriteLine($"BOMB {targetPoint}");
            bombPlaced = true;
        }
        else
            Console.WriteLine($"MOVE {targetPoint}");
    }
}

class PlaceBombAndGoTo : Action
{
    readonly Boomer player;
    readonly Point target;
    bool done;

    public PlaceBombAndGoTo(Point target, Boomer player)
    {
        this.target = target;
        this.player = player;
    }

    public override bool CheckPreconditions() => true;

    public override bool CheckPostconditions() => done;

    public override string ToString() => $"Place bomb and run to {player.position}";

    public override void Execute()
    {
        if (player.bombsAvailable > 0)
        {
            Console.WriteLine($"BOMB {target}");
            done = true;
        }
        else
            Console.WriteLine($"MOVE {target} Waitin'");
    }
}

class Planner
{
    PriorityQueue<Action> actions = new PriorityQueue<Action>(10);
    int orderCounter;

    public void executeNext()
    {
        var actionsToRemove = new HashSet<Action>();
        foreach (var action in actions)
        {
            if (action.CheckPreconditions())
            {
                Console.Error.WriteLine($"Executing action {action}");
                action.Execute();
                if (action.CheckPostconditions())
                {
                    actionsToRemove.Add(action);
                    Console.Error.WriteLine($"Removing action {action}");
                }
                break;
            }
        }
        var newActions = new PriorityQueue<Action>(actions.Count - actionsToRemove.Count);
        var nuu = actions.Where(x => !actionsToRemove.Contains(x));
        foreach (var elem in nuu) newActions.Push(elem);
        actions = newActions;
    }

    public void ClearFinished()
    {
        var actionsToRemove = new HashSet<Action>();
        foreach (var action in actions)
        {
            Console.Error.WriteLine($"||||||||| {action}");
            if (action.CheckPostconditions())
            {
                actionsToRemove.Add(action);
                Console.Error.WriteLine($"clearFinished: Removing action {action}");
            }
            else break;
        }
        var newActions = new PriorityQueue<Action>(actions.Count - actionsToRemove.Count);
        var nuu = actions.Where(x => !actionsToRemove.Contains(x));
        foreach (var elem in nuu) newActions.Push(elem);
        actions = newActions;
    }

    public void Clear()
    {
        actions = new PriorityQueue<Action>(10);
    }

    public void Add(Action action)
    {
        action.order = orderCounter++;
        actions.Push(action);
        Console.Error.WriteLine($"Action added {action}");
    }

    public bool IsEmpty()
        => actions.Count == 0;

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var x in actions)
            sb.Append($"{x}\n");
        return sb.ToString();
    }
}

class TimeCalculator
{
    public bool enabled = true;
    double value;

    double MilisecondsNow
        => new TimeSpan(DateTime.Now.Ticks).TotalMilliseconds;

    public void Start()
    {
        value = MilisecondsNow;
    }

    public double GetTimeAndRestart()
    {
        var now = MilisecondsNow;
        var miliseconds = now - value;
        value = now;
        return miliseconds;
    }

    public void ShowTime(string msg)
    {
        if (enabled)
        {
            Console.Error.WriteLine(msg + GetTimeAndRestart() + " ms");
        }
    }
}

class Player
{
    readonly Planner planner = new Planner();
    readonly World world = new World();

    static void Main(string[] args)
    {
        var game = new Player();
        Console.Error.WriteLine("Loaded player.");

        game.world.Initialize();
        game.Run();
    }

    void UpdateWorldState(TypeMap typeMap)
    {
        world.PlayerBombs.Clear();
        world.EnemyBombs.Clear();
        world.AllBombs.Clear();
        world.Grid.Clear();
        for (var rowIndex = 0; rowIndex < world.Grid.Height; rowIndex++)
        {
            var row = Console.ReadLine();
            for (var columnIndex = 0; columnIndex < world.Grid.Width; ++columnIndex)
            {
                var cell = new Cell();
                cell.position = new Point(columnIndex, rowIndex);
                var typeSymbol = row[columnIndex];
                var ctype = (Cell.Type) typeSymbol;
                if (Cell.IsBox(ctype) || ctype == Cell.Type.Wall)
                {
                    typeMap.Values[columnIndex, rowIndex].value =
                        ctype;
                }
                world.Grid.Cells[columnIndex, rowIndex] = cell;
                world.Grid.AsList.Add(cell);
            }
        }

        var entities = int.Parse(Console.ReadLine());
        for (var i = 0; i < entities; i++)
        {
            var inputs = Console.ReadLine().Split(' ');

            var entityType = int.Parse(inputs[0]);
            var owner = int.Parse(inputs[1]);
            var x = int.Parse(inputs[2]);
            var y = int.Parse(inputs[3]);
            var param1 = int.Parse(inputs[4]);
            var param2 = int.Parse(inputs[5]);

            switch (entityType)
            {
                case Boomer.EntityType:
                    if (owner == world.Player.id)
                    {
                        world.Player.position.x = x;
                        world.Player.position.y = y;
                        world.PlayersBombCountChanged = world.Player.bombsAvailable != param1;
                        world.Player.bombsAvailable = param1;
                        world.Player.explosionRange = param2;
                    }
                    else
                    {
                        if (!world.Enemies.ContainsKey(owner))
                            world.Enemies.Add(owner, new Boomer());
                        var enemigo = world.Enemies[owner];
                        enemigo.position.x = x;
                        enemigo.position.y = y;
                        enemigo.bombsAvailable = param1;
                        enemigo.explosionRange = param2;
                    }
                    break;

                case Bomb.EntityType:
                    var bomb = new Bomb
                    {
                        position =
                        {
                            x = x,
                            y = y
                        },
                        timer = param1,
                        explosionRange = param2
                    };
                    if (owner == world.Player.id)
                        world.PlayerBombs.Add(bomb);
                    else world.EnemyBombs.Add(bomb);
                    break;

                case Item.EntityCode:
                    var item = new Item
                    {
                        position =
                        {
                            x = x,
                            y = y
                        },
                        type = (Item.Type) param1
                    };
                    if (item.type == Item.Type.ExtraRange)
                        typeMap.Values[x, y].value = Cell.Type.ExtraRange;
                    if (item.type == Item.Type.ExtraBomb)
                        typeMap.Values[x, y].value = Cell.Type.ExtraBomb;
                    break;

                default:
                    break;
            }
        }

        world.AllBombs.AddRange(world.PlayerBombs);
        world.AllBombs.AddRange(world.EnemyBombs);
        world.AllBombs.ForEach(x => typeMap.At(x.position).value = Cell.Type.Bomb);
    }

    void UpdateObjectCounters(TypeMap typeMap)
    {
        var bombCounter = 0;
        var boxCounter = 0;
        var bonusCounter = 0;
        foreach (var cell in world.Grid.AsList)
        {
            switch (typeMap.At(cell.position).value)
            {
                case Cell.Type.Bomb:
                    ++bombCounter;
                    break;
                case Cell.Type.Box:
                case Cell.Type.BoxWithExtraRange:
                case Cell.Type.BoxWithExtraBomb:
                    ++boxCounter;
                    break;
                case Cell.Type.ExtraBomb:
                case Cell.Type.ExtraRange:
                    ++bonusCounter;
                    break;
                case Cell.Type.Floor:
                    break;
                case Cell.Type.Wall:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        world.Changed = false;
        if (bombCounter != world.BombCount)
        {
            world.BombCount = bombCounter;
            world.Changed = true;
        }
        if (boxCounter != world.BoxCount)
        {
            world.BoxCount = boxCounter;
            world.Changed = true;
        }
        if (bonusCounter == world.BonusCount) return;
        world.BonusCount = bonusCounter;
        world.Changed = true;
    }

    void CheckExplosionWaveFromBomb(Bomb bomb, Point position, IntegerMap explosionMap)
    {
        var stateAtPosition = explosionMap.At(bomb.position).Value;
        var timer =
            stateAtPosition == Bomb.NoExplosion
                ? bomb.timer
                : stateAtPosition;
        var previous = explosionMap.At(position).Value;
        if (previous == Bomb.NoExplosion || previous > timer)
            explosionMap.At(position).Value = timer;
    }

    HashSet<Cell> CalculateDestroyedObjects(Point bombPosition, int explosionRange,
        TypeMap typeMap, Func<Cell.Type, bool> filter, HashSet<Cell> killList)
    {
        var width = world.Grid.Width;
        var height = world.Grid.Height;

        var destroyedObjects = new HashSet<Cell>();
        var directions = Point.Versors();
        directions.ForEach(x =>
        {
            var exploRadius = explosionRange - 1;
            var pos = bombPosition;
            while (exploRadius > 0)
            {
                --exploRadius;
                pos += x;
                if (pos.x < 0 || pos.y < 0 || pos.x >= width || pos.y >= height)
                    break;
                var cell = world.Grid.Cells[pos.x, pos.y];
                var cellType = typeMap.At(pos).value;
                if (filter(cellType) && !killList.Contains(cell))
                    destroyedObjects.Add(cell);
                if (Cell.StopsExplosions(cellType))
                    break;
            }
        });
        return destroyedObjects;
    }

    HashSet<Cell> CalculateDestroyedObjects(Point bombPosition, int explosionRange,
        TypeMap typeMap, HashSet<Cell.Type> filter, HashSet<Cell> killList)
    {
        var width = world.Grid.Width;
        var height = world.Grid.Height;

        var destroyedObjects = new HashSet<Cell>();
        var directions = Point.Versors();
        directions.ForEach(x =>
        {
            var exploRadius = explosionRange - 1;
            var pos = bombPosition;
            while (exploRadius > 0)
            {
                --exploRadius;
                pos += x;
                if (pos.x < 0 || pos.y < 0 || pos.x >= width || pos.y >= height)
                    break;
                var cell = world.Grid.Cells[pos.x, pos.y];
                var cellType = typeMap.At(pos).value;
                if (filter.Contains(cellType) && !killList.Contains(cell))
                    destroyedObjects.Add(cell);
                if (Cell.StopsExplosions(cellType))
                    break;
            }
        });
        return destroyedObjects;
    }

    void CalculateExplosionMapForBomb(Bomb bomb, HashSet<Cell> destroyedObjects,
        TypeMap typeMap, IntegerMap explosionMap)
    {
        var width = world.Grid.Width;
        var height = world.Grid.Height;

        CheckExplosionWaveFromBomb(bomb, bomb.position, explosionMap);
        var directions = Point.Versors();
        directions.ForEach(x =>
        {
            var exploRadius = bomb.explosionRange - 1;
            var pos = bomb.position;
            while (exploRadius > 0)
            {
                --exploRadius;
                pos += x;
                if (!pos.OnBoard(width, height))
                    break;
                var cell = world.Grid.Cells[pos.x, pos.y];
                var cellType = typeMap.At(pos).value;
                CheckExplosionWaveFromBomb(bomb, cell.position, explosionMap);
                if (Cell.StopsExplosions(cellType))
                {
                    if (Cell.IsDestructible(cellType) && !destroyedObjects.Contains(cell))
                        destroyedObjects.Add(cell);
                    break;
                }
            }
        });
    }

    Cell FindNearestCellWithHighestUtility(int scanRange, IntegerMap utilityMap, PathMap pathMap,
        bool ignoreZeroUtility)
    {
        var maxUtility = 0;
        maxUtility = world.Grid.AsList
            .Where(x => pathMap.At(x.position).distance <= scanRange)
            .Select(x => utilityMap.At(x.position).Value)
            .Max();
        if (ignoreZeroUtility && maxUtility == 0)
            return null;
        return world.Grid.AsList
            .Where(x => pathMap.At(x.position).distance <= scanRange
                        && utilityMap.At(x.position).Value == maxUtility)
            .OrderByDescending(x => pathMap.At(x.position).distance)
            .FirstOrDefault();
    }


    List<Point> GenerateAdjacentPoints(Point mid, HashSet<Cell.Type> filter, TypeMap typeMap)
    {
        var adjacent = Point.Versors()
            .Select(x => x + mid)
            .Where(x => x.OnBoard(world.Grid.Width, world.Grid.Height));

        if (filter != null)
            adjacent = adjacent.Where(x => filter.Contains(typeMap.At(x).value));
        return adjacent.ToList();
    }

    List<Point> GenerateAdjacentPoints(Point mid, Func<Cell.Type, bool> filter, TypeMap typeMap)
    {
        var adjacent = Point.Versors()
            .Select(x => x + mid)
            .Where(x => x.OnBoard(world.Grid.Width, world.Grid.Height));

        if (filter != null)
            adjacent = adjacent.Where(x => filter(typeMap.At(x).value));
        return adjacent.ToList();
    }

    List<Cell> GetPathTo(Cell targetCell, PathMap pathMap)
    {
        var targetPathParameter = pathMap.At(targetCell.position);
        var path = new List<Cell>(targetPathParameter.distance);
        if (targetPathParameter.PreviousCell == null)
            return path;
        var nextCell = targetCell;
        var nextPathParam = targetPathParameter;
        do
        {
            path.Add(nextCell);
            nextCell = nextPathParam.PreviousCell;
            nextPathParam = pathMap.At(nextCell.position);
        } while (nextPathParam.PreviousCell != null);
        path.Reverse();
        return path;
    }

    int GetSafetyCellCount(IntegerMap safetyMap)
        => safetyMap.AsList.Count(x => x.Value == Bomb.NoExplosion);

    Cell FindNearestSafetyPoint(IntegerMap safetyMap, PathMap pathMap)
    {
        return world.Grid.AsList
            .Where(x => safetyMap.At(x.position).Value == Bomb.NoExplosion)
            .OrderBy(x => pathMap.At(x.position).distance).FirstOrDefault();
    }

    void CalculateLotsOfStuff(Point startPoint, HashSet<Cell> killList,
        TypeMap typeMap, IntegerMap explosionMap,
        IntegerMap utilityMap, PathMap pathMap,
        IntegerMap safetyMap)
    {
        var queue = new PriorityQueue<Cell>(world.Grid.Width * world.Grid.Height,
            Comparer<Cell>.Create((cell, cell1) =>
                pathMap.At(cell.position).distance - pathMap.At(cell1.position).distance));

        var utilityCalculated = BoolMap.CreateFalseMap(world.Grid.Width, world.Grid.Height);
        var pathCalculated = BoolMap.CreateFalseMap(world.Grid.Width, world.Grid.Height);
        var cells = world.Grid.Cells;
        var startCell = cells[startPoint.x, startPoint.y];
        pathMap.At(startPoint).distance = 0;
        safetyMap.At(startPoint).Value = explosionMap.At(startPoint).Value;
        queue.Push(startCell);
        while (queue.Count != 0)
        {
            var cur = queue.Pop();
            Console.Error.WriteLine("Current cell: " + cur);
            var curPathParam = pathMap.At(cur.position);
            var curCellUtilCalculated = utilityCalculated.At(cur.position);
            if (!curCellUtilCalculated.value)
            {
                CalculateUtilityForCell(cur, typeMap, killList, utilityMap, pathMap);
                Console.Error.WriteLine("Utility = " + utilityMap.At(cur.position).Value);
            }

            curCellUtilCalculated.value = true;
            pathCalculated.At(cur.position).value = true;

            var adjacent = GenerateAdjacentPoints(cur.position, (HashSet<Cell.Type>) null, typeMap);

            foreach (var p in adjacent)
            {
                var adjacentCell = cells[p.x, p.y];
                if (pathCalculated.At(p).value) return;
                var adjacentPathParam = pathMap.At(p);
                var explosionTime = explosionMap.At(p).Value;
                if (Cell.IsPassable(typeMap.At(p).value))
                {
                    var newDistance = curPathParam.distance + 1;
                    var adjacentSafety = safetyMap.At(p);
                    if (explosionTime != Bomb.NoExplosion &&
                        explosionTime - newDistance == Bomb.AlreadyExploded)
                    {
                        adjacentSafety.Value = Bomb.AlreadyExploded;
                        return;
                    }
                    if (newDistance < adjacentPathParam.distance)
                    {
                        adjacentPathParam.distance = newDistance;
                        adjacentPathParam.PreviousCell = cur;
                        if (explosionTime == Bomb.NoExplosion)
                            adjacentSafety.Value = Bomb.NoExplosion;
                        else adjacentSafety.Value = Math.Max(0, explosionTime - newDistance);
                    }
                    //else
                    //{
                    //}
                    if (queue.Contains(adjacentCell))
                    {
                        var nuitems = queue.ToList();
                        nuitems.Remove(adjacentCell);
                        queue = new PriorityQueue<Cell>();
                        foreach (var x in nuitems) queue.Push(x);
                    }
                    queue.Push(adjacentCell);
                }
            }
        }
    }

    void ModelNewBomb(List<Bomb> newBombs, Point playerPoint, int turns,
        TypeMap typeMap, IntegerMap utilityMap, PathMap pathMap, IntegerMap explosionMap, IntegerMap safetyMap)
    {
        var bombs = new List<Bomb>(world.AllBombs.Count + 1);
        newBombs.ForEach(x =>
        {
            x.timer += turns;
            bombs.Add(x);
            typeMap.At(x.position).value = Cell.Type.Bomb;
        });

        bombs.AddRange(world.AllBombs);
        CalculateExplosionMap(bombs, typeMap, explosionMap);
        explosionMap.AsList.ForEach(x => x.Value = Math.Max(0, x.Value - turns));
        var killList = new HashSet<Cell>();
        foreach (var cell in world.Grid.AsList.Where(cell =>
            Cell.IsDestructible(typeMap.At(cell.position).value) &&
            explosionMap.At(cell.position).Value >= Bomb.AlreadyExploded))
        {
            killList.Add(cell);
        }
        // << 1305
    }

    Cell FindTarget(int initialScanRange, int scanDepth, bool ignoreZeroUtility,
        IntegerMap utilityMap, PathMap pathMap)
    {
        Cell targetCell = null;
        var scanCount = scanDepth;
        var scanRange = initialScanRange;

        while (targetCell == null && scanCount > 0)
        {
            --scanCount;
            Console.Error.WriteLine(
                "Search target, scan range = {0} {1}", scanRange,
                ignoreZeroUtility ? "ignore zero utility" : "check zero utility");
            targetCell = FindNearestCellWithHighestUtility(scanRange, utilityMap, pathMap, ignoreZeroUtility);
            scanRange *= 2;
        }
        return targetCell;
    }

    Cell FindCellToRetreat(Cell bombTarget, TypeMap typeMap, PathMap pathMap)
    {
        var width = world.Grid.Width;
        var height = world.Grid.Height;

        var targetPos = bombTarget.position;
        var distanceToTarget = pathMap.At(targetPos).distance;
        var adjacentPoints = GenerateAdjacentPoints(targetPos, Cell.IsPassable, typeMap);
        var maxSafetyCellCount = 0;
        Cell cellToRetreat = null;
        foreach (var adjacentPoint in adjacentPoints)
        {
            Console.Error.WriteLine($"Checkin adjacent point {adjacentPoint}");
            var typeMapModel = typeMap.DeepCopy(width, height);
            var utilMapModel = IntegerMap.CreateUtilityMap(width, height);
            var pathMapModel = PathMap.CreatePathMap(width, height);
            var explosionMapModel = IntegerMap.CreateExplosionMap(width, height);
            var safetyMapModel = IntegerMap.CreateSafetyMap(width, height);
            var newBombs = new List<Bomb>(4) {world.Player.CreateBomb(targetPos)};
            ModelNewBomb(newBombs, adjacentPoint, distanceToTarget + 1,
                typeMapModel, utilMapModel, pathMapModel, explosionMapModel, safetyMapModel);
            if (safetyMapModel.At(adjacentPoint).Value == Bomb.AlreadyExploded)
                continue;
            var safetyCellCount = GetSafetyCellCount(safetyMapModel);
            if (safetyCellCount > maxSafetyCellCount)
            {
                maxSafetyCellCount = safetyCellCount;
                cellToRetreat = world.Grid.Cells[adjacentPoint.x, adjacentPoint.y];
            }
        }
        return cellToRetreat;
    }

    void CheckExplosionsAndDodge(Point playerPoint, TypeMap typeMap, PathMap pathMap,
        IntegerMap safetyMap)
    {
        var adjacent = GenerateAdjacentPoints(playerPoint, Cell.IsPassable, typeMap);
        var cells = world.Grid.Cells;
        var playersCell = cells[playerPoint.x, playerPoint.y];
        var playerSafety = safetyMap.At(playerPoint);
        if (playerSafety.Value == Bomb.ExplodeNextTurn)
        {
            Console.Error.WriteLine("Fuck, we're exploding next turn.");
            var haven = adjacent.Where(x => pathMap.At(x).distance == 1)
                .OrderByDescending(x => safetyMap.At(x).Value, //todo: Descending or ascending?
                    Comparer<int>.Create((i, i1) =>
                    {
                        if (i == Bomb.NoExplosion) i = 100500;
                        if (i1 == Bomb.NoExplosion) i1 = 100500;
                        return i - i1;
                    }))
                .Select(x => cells[x.x, x.y]).First() ?? playersCell;

            Console.Error.WriteLine($"Cell to run to {haven}");
            var dodge = new Move(haven.position, world.Player) {priority = Action.Priority.High};
            planner.Add(dodge);
        }
    }

    void CalculateExplosionMap(IEnumerable<Bomb> bombs, TypeMap typeMap, IntegerMap explosionMap)
    {
        var destroyedObjects = new HashSet<Cell>();
        foreach (var bomb in bombs.OrderByDescending(x => x.timer))
        {
            CalculateExplosionMapForBomb(bomb, destroyedObjects, typeMap, explosionMap);
        }
    }

    void CalculateUtilityForCell(Cell cell, TypeMap typeMap,
        HashSet<Cell> killList, IntegerMap utilitymap,
        PathMap pathMap)
    {
        Console.Error.WriteLine($"||Calculating utility for {cell}");

        var util = utilitymap.At(cell.position);
        var cellType = typeMap.At(cell.position).value;
        if (Cell.IsPassable(cellType))
        {
            var boxes = CalculateDestroyedObjects(cell.position,
                world.Player.explosionRange, typeMap, Cell.IsBox, killList);
            util.Value = 0;
            foreach (var box in boxes)
            {
                Console.Error.WriteLine($"$$ {box.position} {typeMap.At(box.position).value}");
                switch (typeMap.At(box.position).value)
                {
                    case Cell.Type.Floor:
                        break;
                    case Cell.Type.Box:
                        util.Value++;
                        break;
                    case Cell.Type.BoxWithExtraRange:
                        if (world.Player.explosionRange > 5)
                            util.Value++;
                        else util.Value += 2;
                        break;
                    case Cell.Type.BoxWithExtraBomb:
                        if (world.Player.bombsAvailable + world.PlayerBombs.Count > 5)
                            util.Value++;
                        else if (world.Player.bombsAvailable + world.PlayerBombs.Count > 3)
                            util.Value += 2;
                        else
                            util.Value += 3;
                        break;
                    case Cell.Type.ExtraRange:
                        break;
                    case Cell.Type.ExtraBomb:
                        break;
                    case Cell.Type.Wall:
                        break;
                    case Cell.Type.Bomb:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        if (Cell.IsBonus(cellType))
        {
            util.Value += 2;
            if (cellType == Cell.Type.ExtraBomb) util.Value += 1;
            if (world.Player.bombsAvailable == 0) util.Value += 1;
        }

        util.Value -= pathMap.At(cell.position).distance / 2;
        util.Value = Math.Max(0, util.Value);
    }

    void Run()
    {
        var timeCalculator = new TimeCalculator();
        timeCalculator.enabled = false;


        while (true)
        {
            timeCalculator.Start();


            var width = world.Grid.Width;
            var height = world.Grid.Height;
            var typeMap = TypeMap.CreateTypeMap(width, height);
            var utilityMap = IntegerMap.CreateUtilityMap(width, height);
            var pathMap = PathMap.CreatePathMap(width, height);
            var explosionMap = IntegerMap.CreateExplosionMap(width, height);
            var safetyMap = IntegerMap.CreateSafetyMap(width, height);
            timeCalculator.ShowTime("Maps allocation.");

            UpdateWorldState(typeMap);
            timeCalculator.ShowTime("Update world");

            Console.Error.WriteLine("Player position: " + world.Player.position);
            UpdateObjectCounters(typeMap);
            timeCalculator.ShowTime("World counters");

            //in nextline

            if (world.Changed || world.PlayersBombCountChanged)
            {
                planner.Clear();
            }
            else
            {
                planner.ClearFinished();
            }

            CalculateExplosionMap(world.AllBombs, typeMap, explosionMap);
            timeCalculator.ShowTime("Explosion map");

            var killList = new HashSet<Cell>();
            foreach (var cell in world.Grid.AsList.Where(x => Cell.IsDestructible(typeMap.At(x.position).value))
                .Where(x => explosionMap.At(x.position).Value >= Bomb.AlreadyExploded))
            {
                killList.Add(cell);
            }

            timeCalculator.ShowTime("Model destroyed objects");

            CalculateLotsOfStuff(
                world.Player.position,
                killList,
                typeMap,
                explosionMap,
                utilityMap,
                pathMap,
                safetyMap);

            timeCalculator.ShowTime("Utils, paths and safety.");

            Console.Error.WriteLine("Original");
            Console.Error.WriteLine(world.Grid.ShowUtility(utilityMap));
            Console.Error.WriteLine(world.Grid.ShowDistanceFromPlayer(pathMap));
            Console.Error.WriteLine(world.Grid.ShowExplosionmap(explosionMap, typeMap));
            Console.Error.WriteLine(world.Grid.ShowSafetyMap(safetyMap));


            var ignoreZeroUtility = true;

            if (planner.IsEmpty())
            {
                var modelIterationCount = 5;
                while (modelIterationCount-- > 0)
                {
                    Cell targetCell = null;
                    int initialScanRange;
                    if (world.Player.bombsAvailable == 0)
                        initialScanRange = 8;
                    else if (world.Player.bombsAvailable == 1)
                        initialScanRange = 4;
                    else initialScanRange = 2;

                    targetCell = FindTarget(initialScanRange, 5, ignoreZeroUtility,
                        utilityMap, pathMap);
                    if (targetCell != null)
                    {
                        var cellToRetreat = FindCellToRetreat(targetCell, typeMap, pathMap);
                        if (cellToRetreat != null)
                        {
                            var path = GetPathTo(targetCell, pathMap);
                            path.ForEach(x => planner.Add(new Move(x.position, world.Player)));
                            planner.Add(new PlaceBombAndGoTo(cellToRetreat.position, world.Player));
                            break;
                        }
                        utilityMap.At(targetCell.position).Value = 0;
                    }
                    else
                    {
                        var nearestSafetyPoint = FindNearestSafetyPoint(safetyMap, pathMap);
                        if (nearestSafetyPoint != null)
                        {
                            var path = GetPathTo(nearestSafetyPoint, pathMap);
                            if (path == null || path.Count == 0)
                            {
                                planner.Add(new SkipTurn(world.Player));
                            }
                            else
                            {
                                path.ForEach(x => planner.Add(new Move(x.position, world.Player)));
                            }
                        }
                        else
                        {
                            planner.Add(new SkipTurn(world.Player));
                        }
                        break;
                    }
                }
            }
            if (planner.IsEmpty())
            {
                planner.Add(new SkipTurn(world.Player));
            }

            CheckExplosionsAndDodge(world.Player.position, typeMap, pathMap, safetyMap);

            Console.Error.WriteLine("final:");
            Console.Error.WriteLine(world.Grid.ShowUtility(utilityMap));
            Console.Error.WriteLine(planner);

            planner.executeNext();
        }
    }
}