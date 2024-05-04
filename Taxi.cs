using System.Diagnostics;
using System.Drawing;

namespace taxi_manager_simulator;

class Taxi(Color col, int speed, Point at)
{


    struct CurPos()
    {
        public Point start;
        public Point end;
        public double k = 1;

        public readonly Tuple<int, int> GetPos()
            => new((int)(start.x + (end.x - start.x) * k),
                   (int)(start.y + (end.y - start.y) * k));

        public void Tick(int speed, double t, Point? next)
        {
            var dist = AStar.GetDistance(start, end);
            k += (speed * t) / dist;
            if (k > 1 && next != null)
            {
                k -= 1;
                var nextDist = k * dist;
                start = end;
                end = next;
                dist = AStar.GetDistance(start, end);
                k = nextDist / dist;
            }
            else if (k > 1 && next == null)
            {
                k = 1;
            }
        }
    }



    public readonly Color col = col;
    public int speed = speed;

    public int status = 0;
    // 0 - idle
    // 1 - rideToOrder
    // 2 - InOrder

    public List<Point>? cur_route;
    List<Point>? next_route;

    CurPos pos = new()
    {
        start = at.GetRelationship()[new Random().Next(at.Relationship.Length - 1)],
        end = at
    };

    public void NewOrder(Point start, Point end)
    {
        if (status == 0)
        {
            if (pos.end == start)
            {
                cur_route = AStar.FindPath(pos.end, start);
                status = 2;
            }
            else
            {
                cur_route = AStar.FindPath(pos.end, start);
                next_route = AStar.FindPath(start, end);
                status = 1;
            }
        }
        else if (status == 2)
        {
            next_route = AStar.FindPath(start, end);
        }
    }

    public Tuple<int, int> GetPos() => pos.GetPos();

    public bool ReadyToSetOrder()
    {
        if (status == 0) return true;
        if (status == 2 && next_route == null) return true;
        return false;
    }

    public void StartLoop(int id)
    {
        TaxiControlWindow win = new(this);

        new Thread(() =>
        {
            Thread.CurrentThread.Name = $"Taxi_{id}";

            Stopwatch watch = new();

            while (true)
            {
                Thread.Sleep(15);
                watch.Stop();


                if (status == 1 && cur_route != null)
                {
                    int nextP = cur_route.FindIndex(new Predicate<Point>(a => a.x == pos.end.x && a.y == pos.end.y)) + 1;
                    bool isLastp = nextP == cur_route.Count;

                    pos.Tick(speed,
                                (double)watch.ElapsedMilliseconds / 1000,
                                isLastp ? null : cur_route[nextP]
                    );

                    if (isLastp && next_route is not null)
                    {
                        cur_route = next_route;
                        next_route = null;
                        status = 2;
                    }
                }
                else if (status == 2 && cur_route != null)
                {
                    int nextP = cur_route.FindIndex(new Predicate<Point>(a => a.x == pos.end.x && a.y == pos.end.y)) + 1;
                    bool isLastp = nextP == cur_route.Count;

                    pos.Tick(speed,
                                (double)watch.ElapsedMilliseconds / 1000,
                                isLastp ? null : cur_route[nextP]
                    );

                    if (isLastp && next_route is not null)
                    {
                        cur_route = AStar.FindPath(pos.end, next_route[0]);
                        status = 1;
                    }
                    else if (isLastp && next_route is null && pos.k == 1.0)
                    {
                        status = 0;
                        cur_route = null;
                    }

                }

                watch.Reset();
                watch.Start();
            }
        }).Start();
    }

    public double GetTimeToOrder(Point A, Point B) => status switch
    {
        0 => AStar.CalculatePathLength(AStar.FindPath(pos.end, A)) / speed,
        3 => (AStar.CalculatePathLength(AStar.FindPath(A, B)) +
              AStar.CalculatePathLength(AStar.FindPath(pos.end, A))) / speed,
        _ => -1
    };
}
