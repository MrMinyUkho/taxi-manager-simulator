namespace taxi_manager_simulator;

class PlushButton(int x, int y, int w, int h, Color col, string text)
{
    public int x = x;
    public int y = y;
    public int w = w;
    public int h = h;

    public string font = "Button";

    public Color col = col;
    public string text = text;

    public bool CheckClick(int X, int Y) =>
        X > x && X < x + w &&
        Y > y && Y < y + h;

}

class Color(int a, int r, int g, int b)
{
    public byte a = (byte)a,
                r = (byte)r,
                g = (byte)g,
                b = (byte)b;

    public static Color FromRGB(int r, int g, int b)
        => new(255, r, g, b);

    public static Color FromARGB(int a, int r, int g, int b)
        => new(a, r, g, b);

    public static Color FromHSV(int h, int s, int v)
    {
        double hue = h / 255.0;
        double saturation = s / 255.0;
        double value = v / 255.0;

        double c = value * saturation;
        double x = c * (1 - Math.Abs((hue * 6) % 2 - 1));
        double m = value - c;

        double red, green, blue;
        if (0 <= hue && hue < 1.0 / 6.0)
        {
            red = c;
            green = x;
            blue = 0;
        }
        else if (1.0 / 6.0 <= hue && hue < 2.0 / 6.0)
        {
            red = x;
            green = c;
            blue = 0;
        }
        else if (2.0 / 6.0 <= hue && hue < 3.0 / 6.0)
        {
            red = 0;
            green = c;
            blue = x;
        }
        else if (3.0 / 6.0 <= hue && hue < 4.0 / 6.0)
        {
            red = 0;
            green = x;
            blue = c;
        }
        else if (4.0 / 6.0 <= hue && hue < 5.0 / 6.0)
        {
            red = x;
            green = 0;
            blue = c;
        }
        else
        {
            red = c;
            green = 0;
            blue = x;
        }

        red = (red + m) * 255;
        green = (green + m) * 255;
        blue = (blue + m) * 255;

        return FromRGB((int)red, (int)green, (int)blue);
    }

    public Color Fade(double k) =>
        FromARGB(a, (byte)(r * k), (byte)(g * k), (byte)(b * k));


    public static Color FromAHSV(int a, int h, int s, int v)
    {
        Color c = FromHSV(h, s, v);
        c.a = (byte)a;
        return c;
    }


    public static Color operator +(Color a) => a;
    public static Color operator -(Color a) => new(a.a, -a.r, -a.g, -a.b);
}

class PlushRenderer
{

    readonly nint renderer;
    readonly Dictionary<string, nint> Fonts;

    public static PlushRenderer Empty { get => new(); }
    public PlushRenderer(ref nint renderer)
    {
        Fonts = [];
        this.renderer = renderer;
    }

    PlushRenderer()
    {
        Fonts = [];
        renderer = 0;
    }

    #region Features

    public void DrawButton(PlushButton button)
    {
        SDL_Rect rect = new()
        {
            x = button.x,
            y = button.y,
            h = button.h,
            w = button.w,
        };
        /*SDL_Color Tcol = new()
        {
            a = button.col.a,
            r = button.col.r,
            g = button.col.g,
            b = button.col.b,
        };*/

        DrawRectF(rect.x,
                  rect.y,
                  rect.w,
                  rect.h,
                  button.col);

        if (!Fonts.TryGetValue("Button", out nint value)) throw new ArgumentException($"Button font not found");

        nint surfaceMessage = TTF_RenderText_Solid(value, button.text, new() { a = 255, r = 255, b = 255, g = 255 });

        nint Message = SDL_CreateTextureFromSurface(renderer, surfaceMessage);

        SDL_Rect Message_rect = new();
        _ = SDL_QueryTexture(Message, out _, out _, out Message_rect.w, out Message_rect.h);

        Message_rect.x = rect.x + rect.w / 2 - Message_rect.w / 2;
        Message_rect.y = rect.y + rect.h / 2 - Message_rect.h / 2;

        _ = SDL_RenderCopy(renderer, Message, (nint)null, ref Message_rect);

        SDL_FreeSurface(surfaceMessage);
        SDL_DestroyTexture(Message);
    }

    #endregion

    #region Lines

    private static float[] GetLineNormal(float[] a,
                                         float[] b,
                                         bool invert)
    {
        // Считаем вектор от a до b
        float[] c = [
            b[0] - a[0],
            b[1] - a[1]
        ];

        // Получаем перпендикуляр
        (c[0], c[1]) = (-c[1], c[0]);

        // Нормализуем вектор 
        float d = (float)(1 / Math.Sqrt((c[0] * c[0]) + (c[1] * c[1])));
        (c[0], c[1]) = (c[0] * d, c[1] * d);

        // Если надо меняем направление
        if (invert) (c[0], c[1]) = (-c[0], -c[1]);

        return c;
    }

    public void DrawLines(List<float[]> points, Color col, float thinkness, bool a = true)
    {
        for (int i = 0; i < points.Count; ++i)
            if (points[i].Length != 2) throw new ArgumentException("Line simplePoints must have only two coordinates");

        if (thinkness < 0) return;

        SDL_FPoint[] SDL_points = new SDL_FPoint[(points.Count * 2) - 2];

        for (int i = 1; i < points.Count; ++i)
        {
            SDL_FPoint pointS = new();
            SDL_FPoint pointE = new();

            float[] dir = GetLineNormal(points[i - 1], points[i], a);

            pointS.x = points[i - 1][0] + (dir[0] * (thinkness - 1) / 2);
            pointS.y = points[i - 1][1] + (dir[1] * (thinkness - 1) / 2);

            pointE.x = points[i][0] + (dir[0] * (thinkness - 1) / 2);
            pointE.y = points[i][1] + (dir[1] * (thinkness - 1) / 2);

            SDL_points[(i * 2) - 2] = pointS;
            SDL_points[(i * 2) - 1] = pointE;
        }

        _ = SDL_SetRenderDrawColor(renderer, col.r, col.g, col.b, col.a);
        _ = SDL_RenderDrawLinesF(renderer, SDL_points, SDL_points.Length);

        DrawLines(points, col, thinkness - (float)0.5, !a);
    }

    public void DrawLine(float[] a, float[] b, Color col, int th)
    {
        DrawLines([a, b], col, th);
    }

    #endregion

    #region NGons

    private void DrawNGon(float x, float y, float r, Color col, int nv, bool mode = false, float defRot = (float)(-Math.PI / 2))
    {
        if (r <= 0) return;

        float dt = (float)(Math.PI * 2) / nv;
        SDL_FPoint[] points = new SDL_FPoint[nv + 1];

        for (int i = 0; i <= nv; ++i)
        {
            points[i] = new SDL_FPoint
            {
                x = x + (r * (float)Math.Cos((dt * i) + defRot)),
                y = y + (r * (float)Math.Sin((dt * i) + defRot))
            };
        }

        _ = SDL_SetRenderDrawColor(renderer, col.r, col.g, col.b, col.a);
        _ = SDL_RenderDrawLinesF(renderer, points, nv + 1);

        if (mode) DrawNGon(x, y, r - (float)0.5, col, nv, true, defRot);
    }

    public void DrawNGonF(float x, float y, float r, Color col, int nv, float defRot = (float)(-Math.PI / 2))
    {
        DrawNGon(x, y, r, col, nv, true, defRot);
    }

    public void DrawNGonW(float x, float y, float r, Color col, int nv, float defRot = (float)(-Math.PI / 2))
    {
        DrawNGon(x, y, r, col, nv, false, defRot);
    }

    public void DrawCircleF(float x, float y, float r, Color col)
    {
        DrawNGon(x, y, r, col, 100, true);
        //filledCircleRGBA(renderer, (short)x, (short)y, (short)r, col.r, col.g, col.b, col.a);
    }

    public void DrawCircleW(float x, float y, float r, Color col)
    {
        DrawNGon(x, y, r, col, 150);
    }

    #endregion

    #region PlushText

    public static void InitText()
    {
        _ = TTF_Init();
    }

    public void LoadFont(string path, int size, string? name = null)
    {

        nint new_font = TTF_OpenFont(path, size);
        string new_name = path.Split('.')[0];
        if (name != null) new_name = name;

        Fonts.Add(new_name, new_font);
    }

    public void DrawText(int x, int y, string s, Color col, string font)
    {
        SDL_Color White = new()
        {
            r = col.r,
            g = col.g,
            b = col.b,
            a = col.a
        };

        if (!Fonts.TryGetValue(font, out nint value)) throw new ArgumentException($"Font {font} not found");

        nint surfaceMessage = TTF_RenderText_Solid(value, s, White);

        nint Message = SDL_CreateTextureFromSurface(renderer, surfaceMessage);

        SDL_Rect Message_rect = new();
        _ = SDL_QueryTexture(Message, out _, out _, out Message_rect.w, out Message_rect.h);

        Message_rect.x = x;
        Message_rect.y = y;

        _ = SDL_RenderCopy(renderer, Message, (nint)null, ref Message_rect);

        SDL_FreeSurface(surfaceMessage);
        SDL_DestroyTexture(Message);
    }

    #endregion

    #region Rects

    private void DrawRect(float x, float y, float w, float h, Color col, bool mode)
    {
        _ = SDL_SetRenderDrawColor(renderer, col.r, col.g, col.b, col.a);
        var rect = new SDL_Rect
        {
            x = (int)x,
            y = (int)y,
            w = (int)w,
            h = (int)h
        };
        if (mode)
            _ = SDL_RenderFillRect(renderer, ref rect);
        else
            _ = SDL_RenderDrawRect(renderer, ref rect);
    }

    public void DrawRectF(float x, float y, float w, float h, Color col)
    {
        DrawRect(x, y, w, h, col, true);
    }

    public void DrawRectW(float x, float y, float w, float h, Color col)
    {
        DrawRect(x, y, w, h, col, false);
    }

    #endregion

    #region SDL_tools

    public void FillWindow(Color col)
    {
        if (SDL_SetRenderDrawColor(renderer, col.r, col.g, col.b, col.a) < 0)
            Console.WriteLine($"There was an issue with setting the render draw color. {SDL_GetError()}");

        if (SDL_RenderClear(renderer) < 0)
            Console.WriteLine($"There was an issue with clearing the render surface. {SDL_GetError()}");
    }

    public void Redraw()
    {
        // Switches out the currently presented render surface with the one we just did work on.
        SDL_RenderPresent(renderer);
    }

    ~PlushRenderer()
    {
        foreach (var el in Fonts.Values) TTF_CloseFont(el);
    }

    #endregion
}
