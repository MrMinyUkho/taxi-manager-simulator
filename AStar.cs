namespace taxi_manager_simulator;

/// <summary>
/// Оно как-то работает, написал чатгпт, за его ошибки не ручаюсь
/// </summary>
class AStar
{
    /// <summary>
    /// Нахождения самого короткого пути, могут быть неоптимальные результаты из-за неоптимального эврестического 
    /// алгоритма. В данном случае используется евклидово расстояние. Универсальная, но не всегда эффективная функция.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="goal"></param>
    /// <returns></returns>
    public static List<Point> FindPath(Point start, Point goal)
    {
        List<Point> path = [];

        // Список открытых вершин, которые ещё не были рассмотрены
        var openSet = new List<Point> { start };

        // Словарь для хранения родителей вершин
        var cameFrom = new Dictionary<Point, Point>();

        // Словарь для хранения затрат на путь от старта до каждой вершины
        var gScore = new Dictionary<Point, int>();

        foreach (var point in start.GetRelationship())
            gScore[point] = int.MaxValue;

        gScore[start] = 0;

        // Словарь для хранения затрат на путь от старта до каждой вершины, используя A*
        var fScore = new Dictionary<Point, int>();

        foreach (var point in start.GetRelationship())
            fScore[point] = int.MaxValue;

        fScore[start] = (int)HeuristicCostEstimate(start, goal);

        while (openSet.Count != 0)
        {
            var current = openSet.OrderBy(p => fScore[p]).First();

            if (current == goal)
            {
                path = ReconstructPath(cameFrom, current);
                break;
            }

            openSet.Remove(current);

            foreach (var neighbor in current.GetRelationship())
            {
                var tentativeGScore = gScore[current] + GetDistance(current, neighbor);

                // Проверка, были ли затраты на путь от старта до соседа уже установлены
                if (!gScore.TryGetValue(neighbor, out int value))
                {
                    value = int.MaxValue;
                    gScore[neighbor] = value;
                }

                if (tentativeGScore < value)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeGScore;
                    fScore[neighbor] = gScore[neighbor] + (int)HeuristicCostEstimate(neighbor, goal);

                    if (!openSet.Contains(neighbor)) openSet.Add(neighbor);
                }
            }
        }

        return path;
    }


    private static List<Point> ReconstructPath(Dictionary<Point, Point> cameFrom, Point current)
    {
        var totalPath = new List<Point> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            totalPath.Insert(0, current);
        }
        return totalPath;
    }

    private static double HeuristicCostEstimate(Point start, Point goal) =>
        Math.Sqrt(Math.Pow(goal.x - start.x, 2) + Math.Pow(goal.y - start.y, 2));

    /// <summary>
    /// Нахождение растояния между двумя точками которых связывает одно ребро
    /// </summary>
    /// <param name="start">Начальная точка</param>
    /// <param name="goal">Конечная точка</param>
    /// <returns></returns>
    public static int GetDistance(Point start, Point goal)
    {
        // Здесь вы можете определить стоимость перемещения между точками.
        // Например, если у вас есть связанный список весов для каждой связи, 
        // вы можете использовать его для вычисления расстояния.
        foreach (var tuple in start.Relationship)
            if (tuple.Item1 == goal)
                return tuple.Item2;

        return int.MaxValue; // Возврат максимального значения, если нет связи
    }

    /// <summary>
    /// Находит длинну пути
    /// </summary>
    /// <param name="path">Путь</param>
    /// <returns>Длинна пути</returns>
    public static int CalculatePathLength(List<Point> path)
    {
        int length = 0;

        // Проходим по всем вершинам пути и суммируем длины ребер между ними
        for (int i = 0; i < path.Count - 1; i++)
        {
            Point current = path[i];
            Point next = path[i + 1];

            // Ищем длину ребра между текущей и следующей вершиной
            int edgeLength = GetDistance(current, next);

            // Если длина ребра между вершинами равна int.MaxValue, значит связи между ними нет
            // и мы не можем вычислить длину пути
            if (edgeLength == int.MaxValue)
            {
                Console.WriteLine("Path contains unconnected vertices.");
                return -1; // Возвращаем NaN (Not a Number) как признак ошибки
            }

            length += edgeLength;
        }

        return length;
    }
}
