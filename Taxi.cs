using System.Diagnostics;

namespace taxi_manager_simulator;

/// <summary>
/// Класс такси с расчётом перемещениямя, маршрута и времени
/// </summary>
/// <param name="col">Цвет такси</param>
/// <param name="speed">Скорость</param>
/// <param name="at">Начальная позиция</param>
class Taxi(Color col, int speed, Point at)
{

    /// <summary>
    /// Вот тута функции расчёта перемещений в структуре с текущей позицей на графе
    /// </summary>
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

    /// <summary>
    /// 0 - В простое<br/>
    /// 1 - Едет на заказ<br/>
    /// 2 - Выполняет заказ<br/>
    /// Другие значения лучше не ставить, не знаю что будет
    /// </summary>
    public int status = 0;
    
    public List<Point>? cur_route;
    List<Point>? next_route;

    CurPos pos = new()
    {
        start = at.GetRelationship()[new Random().Next(at.Relationship.Length - 1)],
        end = at
    };


    /// <summary>
    /// Добавления маршрута, если возможно
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void NewOrder(Point start, Point end)
    {
        if (status == 0) // Если стоим
        {
            if (pos.end == start) // Если мы уже на начльной точке
            {
                cur_route = AStar.FindPath(pos.end, start);
                status = 2;
            }
            else                  // Если нет, то прокладываем маршрут до точки
            {
                cur_route = AStar.FindPath(pos.end, start);
                next_route = AStar.FindPath(start, end);
                status = 1;
            }
        }
        else if (status == 2) // Тут запоминаем следующий маршрут
        {
            next_route = AStar.FindPath(start, end);
        }
    }

    /// <summary>
    /// Получения текущих координат
    /// </summary>
    /// <returns></returns>
    public Tuple<int, int> GetPos() => pos.GetPos();

    /// <summary>
    /// Проверяем может ли такси принимать новые заказы
    /// </summary>
    /// <returns>Думаю очевидно что возвращает эта функция</returns>
    public bool ReadyToSetOrder()
    {
        if (status == 0) return true;
        if (status == 2 && next_route == null) return true;
        return false;
    }

    /// <summary>
    /// Сердце такси, функция которя запускает цикл в котором
    /// происходит расчёт передвижения и контроль заказов
    /// </summary>
    /// <param name="id">Номер такси</param>
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

    /// <summary>
    /// Расчёт времени до следующего заказа + время выполнение заказа
    /// </summary>
    /// <param name="A">Начало заказа</param>
    /// <param name="B">Конец заказа</param>
    /// <returns>Время выполнения</returns>
    public double GetTimeToOrder(Point A, Point B) => status switch
    {
        0 => AStar.CalculatePathLength(AStar.FindPath(pos.end, A)) / speed,
        3 => (AStar.CalculatePathLength(AStar.FindPath(A, B)) +
              AStar.CalculatePathLength(AStar.FindPath(pos.end, A))) / speed,
        _ => -1
    };
}
