namespace taxi_manager_simulator;

class ManagerWindow : BaseWindow
{
    readonly Dictionary<string, List<int>> points;
    readonly List<List<string>> edges;

    public Dictionary<Taxi, int> taxies;

    TaxiManager mngr;

    public ManagerWindow(int w,
                         int h,
                         string name,
                         Dictionary<string, List<int>>? points,
                         List<List<string>>? edges,
                         Dictionary<Taxi, int> taxies,
                         TaxiManager mngr
    ) : base(w, h, name)
    {
        ArgumentNullException.ThrowIfNull(points);
        ArgumentNullException.ThrowIfNull(edges);
        this.points = points;
        this.edges = edges;
        this.taxies = taxies;
        this.mngr = mngr;
    }

    public int ordersInQueue = 0;

    protected override void Run()
    {
        PlushRenderer.InitText();
        plushRenderer.LoadFont("RobotoMono-Regular.ttf", 15, "Roboto");
        plushRenderer.LoadFont("RobotoMono-Regular.ttf", 20, "Button");

        Color mapEdges = new(255, 255, 100, 50);
        Color mapPoints = new(255, 50, 100, 255);
        Color mapRoute = new(255, 0, 255, 0);

        bool showPointNumber = false;
        PlushButton button = new(646, 20, 250, 50, -new Color(255, 50, 255, 75), "Show X-roads numbers");
        PlushButton genord = new(376, 20, 250, 50, new(255, 50, 255, 75), "Generate new order");
        PlushButton ordinq = new(646, 90, 250, 50, new(255, 25, 25, 25), $"Orders in queue: {ordersInQueue}");

        while (running)
        {
            while (SDL_PollEvent(out SDL_Event e) == 1)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        running = false;
                        break;
                    case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                        int x, y;
                        _ = SDL_GetMouseState(out x, out y);
                        if (button.CheckClick(x, y))
                        {
                            button.col = -button.col;
                            showPointNumber = !showPointNumber;
                        }
                        if (genord.CheckClick(x, y))
                        {
                            genord.col = -genord.col;
                            mngr.generateNewOrder = !mngr.generateNewOrder;
                        }
                        break;
                }
            }

            ordinq.text = $"Orders in queue: {ordersInQueue}";
            plushRenderer.FillWindow(new(255, 0, 0, 0));

            foreach (var el in edges)
            {
                plushRenderer.DrawLine(
                    points[el[0]].Select((a) =>
                    {
                        return (float)a;
                    }).ToArray(),
                    points[el[1]].Select((a) =>
                    {
                        return (float)a;
                    }).ToArray(),
                    mapEdges,
                    5
                );
            }

            if (showPointNumber)
                foreach (var el in points.Keys.ToArray())
                {
                    plushRenderer.DrawCircleF(points[el][0], points[el][1], 5, mapPoints);
                    plushRenderer.DrawText(points[el][0], points[el][1] - 20, $"{el}", new(255, 255, 255, 255), "Roboto");
                }

            foreach (var el in taxies)
            {
                var t = el.Key;
                var r = t.cur_route;

                if (r == null) continue;

                for (int i = 1; i < r.Count; i++)
                {
                    plushRenderer.DrawLine(
                        [r[i - 1].x, r[i - 1].y],
                        [r[i].x, r[i].y],
                        t.col.Fade(0.5),
                        7
                    );
                    plushRenderer.DrawCircleF(r[i - 1].x, r[i - 1].y, 7, t.col.Fade(0.8));
                    plushRenderer.DrawCircleF(r[i].x, r[i].y, 7, t.col.Fade(0.8));
                }

            }

            foreach (var el in taxies)
            {
                var t = el.Key;
                plushRenderer.DrawCircleF(t.GetPos().Item1, t.GetPos().Item2, 10, t.col);
            }

            plushRenderer.DrawButton(button);
            plushRenderer.DrawButton(ordinq);
            plushRenderer.DrawButton(genord);

            plushRenderer.Redraw();
            Thread.Sleep(10);
        }
    }
}
