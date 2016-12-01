using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography.X509Certificates;

// ReSharper disable once CheckNamespace

public static class Extensions
{
    public static void Times(this int count, Action action)
    {
        for (int i = 0; i < count; i++)
            action();
    }

    public static void Times(this int count, Action<int> action)
    {
        for (int i = 0; i < count; i++)
            action(i);
    }
}

struct Vector2
{
    public int x;
    public int y;

    #region constructors
    public Vector2(int x = -1, int y = -1)
    {
        this.x = x;
        this.y = y;
    }

    public static Vector2 New(int x = -1, int y = -1)
        => new Vector2(x, y);
    #endregion

    #region operators
    public static bool operator ==(Vector2 a, Vector2 b)
    => a.x == b.x && a.y == b.y;

    public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);

    public static Vector2 operator +(Vector2 a, Vector2 b) => New(a.x + b.x, a.y + b.y);
    #endregion

    #region comparings
    public bool Equals(Vector2 other)
    {
        return x == other.x && y == other.y;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is Vector2 && Equals((Vector2)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (x * 397) ^ y;
        }
    }
    #endregion

    #region distance
    static float Square(float x) => x * x;

    public double DistanceTo(Vector2 other)
        => Math.Sqrt(Square(other.x - x) + Square(other.y - y));
    #endregion

    public override string ToString() => $"{x} {y}";

    public bool OnBoard(int width, int height)
        => x >= 0 && y >= 0 && x < width && y < height;

    #region math

    public float DotProduct(Vector2 other)
        => x * other.x + y * other.y;

    public float CrossProduct(Vector2 other)
        => x * other.y - y * other.x;

    public float AngleTo(Vector2 other)
        => (float) Math.Atan2(CrossProduct(other), DotProduct(other));

    public float AbsoluteAngle(Vector2 other)
        => Math.Abs(AngleTo(other));

    public static double DegreesToRadians(short degrees)
        => degrees * Math.PI / 180;


    public double DistanceToLine(Vector2 a, Vector2 b)
    {   
        //distance to line AB
        var up = (b.y - a.y) * x - (b.x - a.x) * y + b.x * a.y - b.y * a.x;
        return Math.Abs(up) / a.DistanceTo(b);
    }

    public bool IsBetweenOnXs(Vector2 a, Vector2 b)
    => (x > a.x && x < b.x) || (x > b.x && x < a.x);

    public bool IsBetweenOnXsInOrder(Vector2 left, Vector2 right)
        => x > left.x && x < right.x;
    #endregion

    #region TYMOTEUSZ
    public Vector2(float x = -1, float y = -1)
    {
        this.x = (int)Math.Round(x);
        this.y = (int)Math.Round(y);
    }
    public Vector2 TouchPointOnLine(Vector2 a, Vector2 b)
    {
        var down = Square(b.y - a.y) + Square(b.x - a.x);
        var aa = (b.y - a.y);
        var bb = (b.x - a.x);
        var cc = b.x * a.y - b.y * a.x;
        return new Vector2((bb * (bb * x - aa * y) - aa * cc) / down, (aa * (-bb * x + aa * y) - bb * cc) / down);
    }

    public bool InCone(Vector2 thin, Vector2 thick, float thickness)
    {
        var dist = DistanceToLine(thin, thick);
        var touch = TouchPointOnLine(thin, thick);
        if (!touch.Between(thin, thick))
            return false;
        var howfar = thin.DistanceTo(touch) / thin.DistanceTo(thick);
        return (dist < thickness * howfar);
    }

    public bool Between(Vector2 a, Vector2 b)
    {
        return (Math.Min(a.x, b.x) <= x && x <= Math.Max(a.x, b.x) &&
                Math.Min(a.y, b.y) <= y && y <= Math.Max(a.y, b.y));
    }
    #endregion
}

class Magicks
{
    public int Mana;
    public void Tick()
    {
        if (Mana < 100) Mana++;

        foreach (var spell in spellsArray)
        {
            if (spell.Timer > 0)
                spell.Timer--;
        }
   
    }

    public void Cast(Spell spell, Entity wizzie, int targetId, out string order)
    {
        Console.Error.WriteLine("Wizard {1} will cast {0}!", spell, wizzie.id);
        order = spell.Cast(targetId);
        Mana -= spell.ManaCost;
    }

    public bool CanAfford(Spell spell)
        => Mana >= spell.ManaCost;

    #region spells

    public static Spell[] spellsArray =
        { Spell.Obliviate, Spell.Accio, Spell.Flipendo, Spell.Petrificus };

    public class Spell
    {
        public string Name;
        public int ManaCost;
        public int Duration;

        public int Timer;
        //public string[] target_types;

        public string ToString(bool debug)
            => debug ? Name : $"{Name}, {Duration}, {ManaCost}";

        public override string ToString()
            => Name;

        public string Cast(int id)
        {
            Timer = Duration;
            return $"{Name} {id}";
        }

        public Spell(string name, int manaCost, int duration)
        {
            Name = name;
            ManaCost = manaCost;
            Duration = duration;
            Timer = 0;
        }

        public static Spell Obliviate = new Spell("OBLIVIATE", 5, 3);
        public static Spell Petrificus = new Spell("PETRIFICUS", 10, 1);
        public static Spell Accio = new Spell("ACCIO", 20, 6);
        public static Spell Flipendo = new Spell(name: "FLIPENDO", manaCost: 20, duration: 3);

    }

    #endregion
}

class Act
{
    public static string Move(Vector2 destination, short thrust)
    => $"MOVE {destination} {thrust}";

    public static string Move(Vector2 destination, short thrust, string msg)
        => $"MOVE {destination} {thrust} {msg}";

    public static string Throw(Vector2 destination, short power)
    => $"THROW {destination} {power}";

    public static string Throw(Vector2 destination, short power, string msg)
        => $"THROW {destination} {power} {msg}";
}

class Entity
{
    public int id;
    public string type;
    public Vector2 position;
    public Vector2 velocity;
    public int state;

    public Entity chaser;

    public override string ToString()
        => $"id: {id}, type: {type}, p: {position}, v: {velocity}, state: {state}";

    public Vector2 NextPositionExpected()
        => position + velocity;

    public double DistanceTo(Entity other)
        => position.DistanceTo(other.position);

    public double DistanceTo(Vector2 otherPos)
    => position.DistanceTo(otherPos);
}

class Game
{
    public int MyTeamId;

    public class Team
    {
        public Entity[] Wizards = new Entity[2];
        public Vector2 Goal;
        // Pole radius == 300
        // +2000 i -2000 w wysokości są słupki, czyli na +1800 i -1800 można strzelać
        // rykoszety?
    }

    public readonly Team[] Teams =
    {
        new Team {Goal = Vector2.New(0, 3750)},
        new Team {Goal = Vector2.New(16000, 3750)}
    };

    public Team MyTeam => Teams[MyTeamId];
    public Team EnemyTeam => Teams[1 ^ MyTeamId];
}

//todo detect who was hit by a bludger
//todo passing
//todo rewrite to behaviour tree

class Player
{
    static Game game;
    static Magicks magicks;

    static bool IsInsideGoal(Vector2 position, Vector2 goalCenter)
        => position.x == goalCenter.x && 
           (Math.Abs(position.y - goalCenter.y) <= 1850);

    static bool IsInsideGoalY(int y, int goalY)
        => (Math.Abs(y - goalY) <= 1850);

    static int IdIfWorthToShoot(
        Entity wizzie,
        IEnumerable<Entity> snuffles,
        IEnumerable<Entity> enemyWizards,
        IEnumerable<Entity> bludgers,
        short minDistance = 500,
        short minDistS2G = 1000,
        short ownershipRange = 1000)
    {
        var enemyGoal = game.EnemyTeam.Goal;

        Entity coolSnaffle = null;
        foreach (var snuffle in snuffles)
        {
            var w = wizzie.position;
            var s = snuffle.position;

            var distToGoal = enemyGoal.DistanceTo(w);
            var distToSnaffle = s.DistanceTo(w);

            // If ball is already mine and nobody is close to steal it.
            // let's say that if i'm at least ownershipRange closer to snaffle than enemy
            // i can say it's mine.
            // THEN, when distance from snaffle to goal is less than minDistS2G
            // i'll probably just carry it to the goal

            var closestEnemyDistance = enemyWizards.Select(x => x.DistanceTo(snuffle)).Min();
            if (distToSnaffle < closestEnemyDistance + ownershipRange && // to można trochę inaczej
                s.DistanceTo(enemyGoal) < minDistS2G + closestEnemyDistance)
                continue;


            if (distToSnaffle < minDistance ||
                !snuffle.position.IsBetweenOnXs(enemyGoal, w)
                ) continue;

            var y = s.y + (w.y - s.y) * (enemyGoal.x - s.x) / (w.x - s.x * 1d);
            if (IsInsideGoalY((int) y, enemyGoal.y))
            {
                //check for defender // enemies closer to their goal than me
                var defenderExists = enemyWizards.Concat(bludgers).
                    Any(x => x.position.IsBetweenOnXs(w, enemyGoal) &&
                             x.DistanceTo(enemyGoal) < distToGoal &&
                             x.position.DistanceToLine(w, s) <= 900);
                if (!defenderExists)
                {
                    coolSnaffle = snuffle;
                    //todo FIND IF IT WILL REALLY DELIVER [use info about power]
                    break;
                }
            }
        }
        return coolSnaffle?.id ?? -1;
    }

    static void Log(string format, params object[] @params)
        => Console.Error.WriteLine(format, @params);

    static void Say(string format, params object[] @params)
        => Console.WriteLine(format, @params);


    static void Main(string[] args)
    {
        game = new Game {MyTeamId = int.Parse(Console.ReadLine())};
        magicks = new Magicks();

        while (true)
        {
            var snaffles = new List<Entity>();
            var bludgers = new List<Entity>();
            var allWizards = new List<Entity>();
            var bludgersTargets = new Entity[2];
            var orders = new string[2];
 
            int entities = int.Parse(Console.ReadLine()); // number of entities still in game
            

            Log("Mana: {0}", magicks.Mana);

            for (int i = 0; i < entities; i++)
            {
                string[] inputs = Console.ReadLine().Split(' ');
                var entity = new Entity
                {
                    id = int.Parse(inputs[0]), // entity identifier
                    type = inputs[1],  // "WIZARD", "OPPONENT_WIZARD" or "SNAFFLE" (or "BLUDGER" after first league)
                    position = Vector2.New(int.Parse(inputs[2]), int.Parse(inputs[3])),
                    velocity = Vector2.New(int.Parse(inputs[4]), int.Parse(inputs[5])),
                    state = int.Parse(inputs[6]),
                    chaser = null
                };

                switch (entity.type)
                {
                    case "OPPONENT_WIZARD":
                    case "WIZARD":
                        game.Teams[entity.id >> 1].Wizards[entity.id % 2] = entity;
                        Log("t[{0}].w[{1}] is {2}", entity.id >> 1, entity.id % 2, entity);
                        allWizards.Add(entity);
                        break;

                    case "SNAFFLE":
                        snaffles.Add(entity);
                        break;
                        
                    case "BLUDGER":
                        bludgers.Add(entity);
                        break;

                    default:
                        throw new Exception("Wtf?");
                }
            }

            //detect who is chased by bludger
            for (int _bludger = 0; _bludger < 2; ++_bludger)
            {
                var poorWizard = allWizards.OrderBy(x => x.position.DistanceTo(bludgers[_bludger].position)).First();
                poorWizard.chaser = bludgers[_bludger];
                bludgersTargets[_bludger] = poorWizard;
            }


            // pick targets
            var bestSnaffles = new Entity[2];
            if (snaffles.Count > 1)
            {
                var sortedSnaffles = new List<Entity>[2];
                2.Times(j => sortedSnaffles[j] = snaffles
                    .OrderBy(x => x.position.DistanceTo(game.MyTeam.Wizards[j].position))
                    .ToList());


                2.Times(j => bestSnaffles[j] = sortedSnaffles[j][0]);
                if (bestSnaffles[1] == bestSnaffles[0])
                {  
                    double[] distances = sortedSnaffles.
                        Select((x, i) => x[1].DistanceTo(game.MyTeam.Wizards[i])).ToArray();
                    var toChange = distances[0] > distances[1] ? 1 : 0;
                    bestSnaffles[toChange] = sortedSnaffles[toChange][1];
                }
            }
            else
            {
                bestSnaffles[1] = bestSnaffles[0] = snaffles[0];
            }

            //for each wizard
            for (int i = 0; i < 2; i++)
            {
                var curWizzie = game.MyTeam.Wizards[i];
                var closestSnaffle = bestSnaffles[i];


                if (curWizzie.state == 1)
                {
                    Log("I AM {0} AND I THROW! STATE IS {1}", curWizzie.id, curWizzie.state);
                    orders[i] = Act.Throw(ThrowTarget(i, game.EnemyTeam.Goal), 500);
                }
                else if (curWizzie.state == 0)
                {

                    if (magicks.CanAfford(Magicks.Spell.Flipendo))
                    {
                        var ball = IdIfWorthToShoot
                            (curWizzie, snaffles, game.EnemyTeam.Wizards, bludgers, 500);
                        if (ball != -1)
                        {
                            magicks.Cast(Magicks.Spell.Flipendo, curWizzie, ball, out orders[i]);
                            continue;
                        }
                    }
                    // If I'm being chased by bludger:
                    if (curWizzie.chaser?.type == "BLUDGER")
                    {
                        var distie = curWizzie.position.DistanceTo(curWizzie.chaser.position);
                        Log("Wizard {0} is closest wizard for bludger {1}. Distance {2}.",
                            curWizzie.id, curWizzie.chaser.id, distie);

                        // If it's dangerously close:
                        var dangerous = 2500;
                        var tooClose = 1500;
                        if (tooClose < distie && distie < dangerous)
                        {
                            Log("Wizard {0} is dangerously close to a bludger!", curWizzie.id);
                            // And I can cast Obliviate:
                            if (magicks.CanAfford(Magicks.Spell.Obliviate)
                                && Magicks.Spell.Obliviate.Timer == 0   
                                && false) // hack!!
                            {
                                magicks.Cast(Magicks.Spell.Obliviate, curWizzie,
                                    curWizzie.chaser.id, out orders[i]);
                                continue;
                            }
                        }

                        if (distie < tooClose)
                        {
                            Log("Wizard {0} is super close to a bludger!", curWizzie.id);
                            if (magicks.CanAfford(Magicks.Spell.Petrificus)
                                && Magicks.Spell.Petrificus.Timer != 0)
                            {
                                Log("Wizard {0} will cast PETRIFICUS!", curWizzie.id);
                                orders[i] =
                                    Magicks.Spell.Petrificus.Cast(curWizzie.chaser.id);
                                magicks.Mana -= Magicks.Spell.Petrificus.ManaCost;
                                continue;
                            }
                        }

                        var dx = closestSnaffle.position.x - curWizzie.position.x;

                        Log($"Considering Accio, current mana is {magicks.Mana}.");
                        Log($"Accio mana cost is {Magicks.Spell.Accio.ManaCost}");
                        if (magicks.CanAfford(Magicks.Spell.Accio) &&
                            (game.MyTeamId == 0
                            ? dx < 0
                            : dx > 0))
                        {
                            Log("magicks.CanAfford(Magicks.Spell.Accio) == " +
                                $"{magicks.CanAfford(Magicks.Spell.Accio)}");
                            Log("ACCIO IS GOOD IDEA.");
                            orders[i] =
                                Magicks.Spell.Accio.Cast(closestSnaffle.id);
                            magicks.Mana -= Magicks.Spell.Accio.ManaCost;
                            continue;
                        }
                    }

                   
                    orders[i] = Act.Move(closestSnaffle.position, 150);
                }
            }

            PrintOrders(orders);
            magicks.Tick();
        }
    }

    static void PrintOrders(string[] orders)
    {
        Log("--------------------\n" +
            "Orders for this turn: ");
        foreach (var order in orders)
        {
            Console.Error.WriteLine(order);
            Console.WriteLine(order);
        }
    }

    #region TYMOTEUSZ
    static Vector2 ThrowTarget(int who, Vector2 enemyGoal)
    {
        var myWizards = game.MyTeam.Wizards;
        var enemyWizards = game.EnemyTeam.Wizards;

        if (myWizards[1 - who].position.DistanceTo(myWizards[who].position) < 600 &&
            myWizards[1 - who].position.InCone(myWizards[who].position, enemyGoal, 3000))
            return myWizards[1 - who].position;

        int[] ys = Enumerable.Range(1, 9)
            .Select(m => 3750 + 4000 / 10 * (m - 5)).ToArray();

        float bestMinEnemyDistance = 0;
        int bestShotY = 3750;
        foreach (var y in ys)
        {
            enemyGoal.y = y;
            float minenemydistance = float.MaxValue;
            foreach (Entity ew in enemyWizards)
            {
                float enemydistance = (float)ew.position.DistanceToLine(myWizards[who].position, enemyGoal);
                if (enemydistance < minenemydistance && ew.position.Between(myWizards[who].position, enemyGoal))
                {
                    minenemydistance = enemydistance;
                }
            }
            if (Math.Abs(minenemydistance - bestMinEnemyDistance) < 100
                 && Math.Abs(bestShotY - 3750) > Math.Abs(y - 3750)
                 || minenemydistance > bestMinEnemyDistance)
            {
                bestMinEnemyDistance = minenemydistance;
                bestShotY = y;
            }
        }

        enemyGoal.y = bestShotY;
        return enemyGoal;
    }
    #endregion
}