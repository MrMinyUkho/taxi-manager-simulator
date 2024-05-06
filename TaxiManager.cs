using Newtonsoft.Json;

namespace taxi_manager_simulator;

/// <summary>
/// Вроде точка, но по умному - вершина графа
/// </summary>
/// <param name="x">Координата по X</param>
/// <param name="y">Координата по Y</param>
class Point(int x, int y)
{
    public int x = x;
    public int y = y;

    /// <summary>
    /// Массив вершин с весами рёбер
    /// </summary>
    public Tuple<Point, int>[] Relationship { get; set; } = [];

    /// <summary>
    /// Массив вершин без весов
    /// </summary>
    /// <returns>Очевидно?</returns>
    public Point[] GetRelationship()
        => Relationship.Select(a => a.Item1).ToArray();
}

/// <summary>
/// Мозги менеджера такси, здесь генерируються новые заказы,
/// загружаеться и обрабатываеться карта и создаются такси 
/// </summary>
class TaxiManager
{
    readonly Dictionary<string, Point> pointsDictObj;

    readonly ManagerWindow managerWindow;

    Dictionary<string, List<int>> simplePoints;
    List<List<string>> simpleEdges;

    readonly Queue<Tuple<string, string>> ordersQueue = [];

    readonly Dictionary<Taxi, int> taxies = [];

    public bool generateNewOrder = true;

    /// <summary>
    /// Считывает два файла points.json и edges.json<br/>
    /// points.json - точки, edges.json - связи между ними
    /// </summary>
    /// <remarks>
    /// Формат points.json { "Номер точки":  [ Координата по X, Координата по Y ], ... }<br/>
    /// Формат edges.json  [ ["Номер первой точки", "Номер второй точки"], ... ]
    /// </remarks>
    /// <exception cref="NullReferenceException">В случае если файлы считались криво и где-то проскачил null</exception>
    void LoadJson()
    {
        using StreamReader fileEdges  = new("edges.json"),
                           filePoints = new("points.json");

        var points_temp = JsonConvert.DeserializeObject<Dictionary<string, List<int>>>(filePoints.ReadToEnd());
        var edges_temp  = JsonConvert.DeserializeObject<List<List<string>>>(fileEdges.ReadToEnd());

        if (points_temp == null || edges_temp == null)
            throw new NullReferenceException();

        simplePoints = points_temp;
        simpleEdges = edges_temp;
    }

    /// <summary>
    /// Тут создаются объекты точек, формируются связи и создаются такси
    /// </summary>
    /// <exception cref="NullReferenceException">Такая же проверка как и в <see cref="LoadJson">загрузке json'ов</see></exception>
    public TaxiManager()
    {
        // Грузим Jсоновов Стетхемов
        LoadJson();

        // Та самамя проверка на null
        if (simplePoints == null || simpleEdges == null)
            throw new NullReferenceException();

        // Формирование словоря кнопок
        pointsDictObj = simplePoints
            .ToArray()
            .Select((a) => new KeyValuePair<string, Point>(a.Key, new Point(a.Value[0], a.Value[1]))
            ).ToDictionary();

        // Запись рёбер в вершины 
        foreach (var n in simpleEdges)
        {
            Point a1 = pointsDictObj[n[0]], b1 = pointsDictObj[n[1]];
            int dist = (int)Math.Sqrt(Math.Abs((b1.x - a1.x) * (b1.y - a1.y)));

            a1.Relationship = [.. a1.Relationship, new(b1, dist)];
            b1.Relationship = [.. b1.Relationship, new(a1, dist)];
        }

        // Создания окна, загрузка в него рёбер и вершин, массива такси(пока пустой, потом заполним)
        managerWindow = new(909, 606, "Manager Window", simplePoints, simpleEdges, taxies, this);
        _ = new CreateOrderWindow(this, pointsDictObj);


        // Создание такси
        for (int i = 0; i < 5; i++)
        {
            taxies.Add(new(Color.FromHSV((int)(255 / 5.0 * i), 255, 255), new Random().Next(5, 15), pointsDictObj.Values.ElementAt(new Random().Next(pointsDictObj.Count - 1))), i);

            taxies.Keys.ToArray()[i].NewOrder(
                pointsDictObj.Values.ElementAt(new Random().Next(pointsDictObj.Count - 1)),
                pointsDictObj.Values.ElementAt(new Random().Next(pointsDictObj.Count - 1))
            );

            taxies.Keys.ToArray()[i].StartLoop(i);

        }

        // Запуск цикла работы с заказами
        new Thread(() => { Run(); }).Start();
    }

    void Run()
    {
        while (true)
        {
            Thread.Sleep(100); // Задержка чтобы постоянно не дёргать такси, в этом смылса нет
            List<Tuple<int, Taxi>> timings = [];

            managerWindow.ordersInQueue = ordersQueue.Count; // Количевство заказов отображаемое в окне

            if (ordersQueue.Count < 10 && generateNewOrder) // Если надо генерировать заказы и их меньше 10
            {
                ordersQueue.Enqueue(new(
                    pointsDictObj.Keys.ToArray().ElementAt(new Random().Next(pointsDictObj.Count - 1)),
                    pointsDictObj.Keys.ToArray().ElementAt(new Random().Next(pointsDictObj.Count - 1))
                ));
            }

            // Растыкивание заказов по такси
            if (ordersQueue.Count > 0)
            {
                // Вытягивание заказа из очереди
                var nextOrder = ordersQueue.Dequeue();

                // Проверка такси на вохможность принимать заказы
                foreach (var taxi in taxies)
                    if (taxi.Key.ReadyToSetOrder()) timings.Add(new((int)taxi.Key.GetTimeToOrder(pointsDictObj[nextOrder.Item1], pointsDictObj[nextOrder.Item2]), taxi.Key));

                // Сортируем такси по времени выполнения заказа и создаём новый заказ для такси
                if (timings.Count > 0)
                {
                    timings.Sort((a, b) => a.Item1 - b.Item1);

                    timings[0].Item2.NewOrder(pointsDictObj[nextOrder.Item1], pointsDictObj[nextOrder.Item2]);
                }
                else // Если такси свободных нет - возвращаем в очередь
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
