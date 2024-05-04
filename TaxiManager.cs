using Newtonsoft.Json;

namespace taxi_manager_simulator;

class Point(int x, int y)
{
    public int x = x;
    public int y = y;

    public Tuple<Point, int>[] Relationship { get; set; } = [];

    public Point[] GetRelationship()
        => Relationship.Select(a => a.Item1).ToArray();
}

class TaxiManager
{
    readonly Dictionary<string, Point> pointsDictObj;

    readonly ManagerWindow managerWindow;

    Dictionary<string, List<int>> simplePoints;
    List<List<string>> simpleEdges;

    readonly Queue<Tuple<string, string>> ordersQueue = [];

    readonly Dictionary<Taxi, int> taxies = [];

    public bool generateNewOrder = true;

    public void LoadJson()
    {
        using StreamReader fileEdges = new("edges.json"),
                           filePoints = new("points.json");

        var points_temp = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(filePoints.ReadToEnd());
        var edges_temp = JsonConvert.DeserializeObject<List<List<string>>>(fileEdges.ReadToEnd());

        if (points_temp == null || edges_temp == null)
            throw new NullReferenceException();

        simplePoints = points_temp;
        simpleEdges = edges_temp;
    }
    public TaxiManager()
    {
        LoadJson();

        if (simplePoints == null || simpleEdges == null)
            throw new NullReferenceException();

        pointsDictObj = simplePoints
            .ToArray()
            .Select((a) => new KeyValuePair<string, Point>(a.Key, new Point(a.Value[0], a.Value[1]))
            ).ToDictionary();


        foreach (var n in simpleEdges)
        {
            Point a1 = pointsDictObj[n[0]], b1 = pointsDictObj[n[1]];
            int dist = (int)Math.Sqrt(Math.Abs((b1.x - a1.x) * (b1.y - a1.y)));

            a1.Relationship = [.. a1.Relationship, new(b1, dist)];
            b1.Relationship = [.. b1.Relationship, new(a1, dist)];
        }

        managerWindow = new(909, 606, "Manager Window", simplePoints, simpleEdges, taxies, this);
        _ = new CreateOrderWindow(this, pointsDictObj);

        for (int i = 0; i < 5; i++)
        {
            taxies.Add(new(Color.FromHSV((int)(255 / 5.0 * i), 255, 255), new Random().Next(5, 15), pointsDictObj.Values.ElementAt(new Random().Next(pointsDictObj.Count - 1))), i);

            taxies.Keys.ToArray()[i].NewOrder(
                pointsDictObj.Values.ElementAt(new Random().Next(pointsDictObj.Count - 1)),
                pointsDictObj.Values.ElementAt(new Random().Next(pointsDictObj.Count - 1))
            );

            taxies.Keys.ToArray()[i].StartLoop(i);

        }

        new Thread(() => { Run(); }).Start();
    }

    void Run()
    {
        while (true)
        {
            Thread.Sleep(100);
            List<Tuple<int, Taxi>> timings = [];

            managerWindow.ordersInQueue = ordersQueue.Count;

            if (ordersQueue.Count < 10 && generateNewOrder)
            {
                ordersQueue.Enqueue(new(
                    pointsDictObj.Keys.ToArray().ElementAt(new Random().Next(pointsDictObj.Count - 1)),
                    pointsDictObj.Keys.ToArray().ElementAt(new Random().Next(pointsDictObj.Count - 1))
                ));
            }


            if (ordersQueue.Count > 0)
            {
                var nextOrder = ordersQueue.Dequeue();

                foreach (var taxi in taxies)
                    if (taxi.Key.ReadyToSetOrder()) timings.Add(new((int)taxi.Key.GetTimeToOrder(pointsDictObj[nextOrder.Item1], pointsDictObj[nextOrder.Item2]), taxi.Key));

                if (timings.Count > 0)
                {
                    timings.Sort((a, b) => a.Item1 - b.Item1);

                    timings[0].Item2.NewOrder(pointsDictObj[nextOrder.Item1], pointsDictObj[nextOrder.Item2]);
                }
                else
                {
                    ordersQueue.Enqueue(nextOrder);
                }
            }
        }
    }

    public void AddOrderInQueue(string StartPoint, string EndPoint)
    {
        ordersQueue.Enqueue(new(StartPoint, EndPoint));
    }
}
