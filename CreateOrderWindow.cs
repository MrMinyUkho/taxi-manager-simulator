namespace taxi_manager_simulator;

/// <summary>
/// Окно создания заказа
/// </summary>
/// <param name="manager">Менеджер такси</param>
/// <param name="points">Массив точек</param>
class CreateOrderWindow(TaxiManager manager, Dictionary<string, Point> points) : BaseWindow(200, 170, "TaxiControl")
{
    readonly TaxiManager mngr = manager;
    readonly string[] nums = [.. points.Keys];

    protected override void Run()
    {
        Thread.Sleep(200);

        // Кнопочки для выбора точек
        PlushButton SP_pl = new(20,  20,  30,  30, new(255,  40, 230, 150), "<");
        PlushButton SP_mn = new(150, 20,  30,  30, new(255,  40, 230, 150), ">");
        PlushButton EP_pl = new(20,  70,  30,  30, new(255,  40, 230, 150), "<");
        PlushButton EP_mn = new(150, 70,  30,  30, new(255,  40, 230, 150), ">");
        PlushButton NW_od = new(20, 120, 160,  30, new(255,  40, 230, 150), "Create order");

        // Это как бы кнопки, но не кнопки, используются для отслеживания прокрутки
        PlushButton SP_WS = new(20,  20, 160,  30, new(255,  40, 230, 150), "");
        PlushButton EP_WS = new(20,  70, 160,  30, new(255,  40, 230, 150), "");

        int spp = 0;
        int epp = 0;

        plushRenderer.LoadFont("RobotoMono-Regular.ttf", 20, "Button");

        while (running)
        {
            while (SDL_PollEvent(out SDL_Event e) == 1)
            {
                _ = SDL_GetMouseState(out int x, out int y);
                switch (e.type)
                {
                    case SDL_EventType.SDL_QUIT:
                        running = false;
                        break;
                    case SDL_EventType.SDL_MOUSEBUTTONUP: // Тут кнопочки
                        if (NW_od.CheckClick(x, y)) mngr.AddOrderInQueue(nums[spp], nums[epp]);
                        if (SP_pl.CheckClick(x, y)) spp = spp - 1 < 0 ? spp : spp - 1;
                        if (EP_pl.CheckClick(x, y)) epp = epp - 1 < 0 ? epp : epp - 1;
                        if (SP_mn.CheckClick(x, y)) spp = spp + 1 >= nums.Length ? spp : spp + 1;
                        if (EP_mn.CheckClick(x, y)) epp = epp + 1 >= nums.Length ? epp : epp + 1;
                        break;
                    case SDL_EventType.SDL_MOUSEWHEEL: // А здесь колёсико
                        if (e.wheel.y == -1)
                        {
                            if (SP_WS.CheckClick(x, y)) spp = spp - 1 < 0 ? spp : spp - 1;
                            if (EP_WS.CheckClick(x, y)) epp = epp - 1 < 0 ? epp : epp - 1;
                        }
                        if (e.wheel.y == 1)
                        {
                            if (SP_WS.CheckClick(x, y)) spp = spp + 1 >= nums.Length ? spp : spp + 1;
                            if (EP_WS.CheckClick(x, y)) epp = epp + 1 >= nums.Length ? epp : epp + 1;
                        }
                        break;
                }
            }

            plushRenderer.FillWindow(new(255, 20, 20, 20));

            // Тут всё рисуем
            plushRenderer.DrawText(70, 20, nums[spp], new(255, 255, 255, 255), "Button");
            plushRenderer.DrawText(70, 70, nums[epp], new(255, 255, 255, 255), "Button");

            plushRenderer.DrawButton(EP_mn);
            plushRenderer.DrawButton(SP_mn);
            plushRenderer.DrawButton(EP_pl);
            plushRenderer.DrawButton(SP_pl);
            plushRenderer.DrawButton(NW_od);

            plushRenderer.Redraw();
        }
    }
}
