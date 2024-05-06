namespace taxi_manager_simulator;

/// <summary>
/// Окошко для контроля такси, отображения статуса такси и управление скоростью
/// </summary>
/// <param name="taxi">Такси которое мы будем контроллировать</param>
class TaxiControlWindow(Taxi taxi) : BaseWindow(200, 100, "TaxiControl")
{

    readonly Taxi taxi = taxi;

    protected override void Run()
    {
        Thread.Sleep(100);

        PlushButton spDown = new(20, 20, 30, 30, -taxi.col, "<");
        PlushButton spUp = new(150, 20, 30, 30, -taxi.col, ">");

        plushRenderer.LoadFont("RobotoMono-Regular.ttf", 20, "Button");

        while (running)
        {
            while (SDL_PollEvent(out SDL_Event e) == 1)
            {
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        running = false;
                        break;
                    case SDL_EventType.SDL_MOUSEBUTTONUP:
                        int x, y;
                        _ = SDL_GetMouseState(out x, out y);
                        if (spDown.CheckClick(x, y)) taxi.speed--; // Регулируем скорость
                        if (spUp.CheckClick(x, y)) taxi.speed++;
                        break;
                }
            }
            plushRenderer.FillWindow(taxi.col.Fade(0.3));

            // Рисуем
            plushRenderer.DrawText(80, 23, $"{taxi.speed}", -taxi.col.Fade(0.3), "Button");
            switch (taxi.status)
            {
                case 0: plushRenderer.DrawText(5, 60, "Idle", -taxi.col.Fade(0.3), "Button"); break;
                case 1: plushRenderer.DrawText(5, 60, "Go to order", -taxi.col.Fade(0.3), "Button"); break;
                case 2: plushRenderer.DrawText(5, 60, "On order", -taxi.col.Fade(0.3), "Button"); break;
                default: plushRenderer.DrawText(5, 60, "Unknown", -taxi.col.Fade(0.3), "Button"); break;
            };

            plushRenderer.DrawButton(spDown);
            plushRenderer.DrawButton(spUp);
            plushRenderer.Redraw();
        }
    }
}
