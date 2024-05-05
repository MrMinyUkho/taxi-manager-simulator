namespace taxi_manager_simulator;

/// <summary>
/// Объект кнопки, ничего сложного, просто прямоугольник, текст и проверка клика
/// </summary>
/// <param name="x">Координата по X</param>
/// <param name="y">Координата по Y</param>
/// <param name="w">Ширина</param>
/// <param name="h">Высота</param>
/// <param name="col">Цвет</param>
/// <param name="text">Текст</param>
class PlushButton(int x, int y, int w, int h, Color col, string text)
{
    public int x = x;
    public int y = y;
    public int w = w;
    public int h = h;

    public string font = "Button";

    public Color col = col;
    public string text = text;

    /// <summary>
    /// Проверяет клик
    /// </summary>
    /// <param name="X">Место клика по X</param>
    /// <param name="Y">Место клика по Y</param>
    /// <returns>Очевидно? - Да.</returns>
    public bool CheckClick(int X, int Y) =>
        X > x && X < x + w &&
        Y > y && Y < y + h;

}


/// <summary>
/// Цвет, не знаю зачем сделал, но в целом полезно
/// </summary>
/// <param name="a">Альфа</param>
/// <param name="r">Красный</param>
/// <param name="g">Зелёный</param>
/// <param name="b">Синиий</param>
class Color(int a, int r, int g, int b)
{
    public byte a = (byte)a,
                r = (byte)r,
                g = (byte)g,
                b = (byte)b;

    /// <summary>
    /// Создание цвета без учёта альфа канала
    /// </summary>
    /// <param name="r">Красный</param>
    /// <param name="g">Зелёный</param>
    /// <param name="b">Синий</param>
    /// <returns>Цвет с альфой 255</returns>
    public static Color FromRGB(int r, int g, int b)
        => new(255, r, g, b);

    /// <summary>
    /// Как конструктор, только статический метот, мб когда-то будет надо
    /// </summary>
    /// <param name="a">Альфа</param>
    /// <param name="r">Красный</param>
    /// <param name="g">Зелёный</param>
    /// <param name="b">Синиий</param>
    /// <returns>Цвет</returns>
    public static Color FromARGB(int a, int r, int g, int b)
        => new(a, r, g, b);

    /// <summary>
    /// Создание цвета из пространства HSV
    /// </summary>
    /// <param name="h">Тон</param>
    /// <param name="s">Насыщенность</param>
    /// <param name="v">Значение</param>
    /// <returns>Цвет</returns>
    public static Color FromHSV(int h, int s, int v)
    {
        // Тут мегаалгоритм с чата гпт
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

    /// <summary>
    /// Затемняет цвет на какой-то процент в значении от 0.0 до 1.0
    /// </summary>
    /// <param name="k">Коэфициент</param>
    /// <returns>Затемнённый цвет</returns>
    public Color Fade(double k) =>
        FromARGB(a, (byte)(r * k), (byte)(g * k), (byte)(b * k));


    /// <summary>
    /// Как FromHSV, только с альфа каналом
    /// </summary>
    /// <param name="a">Альфа</param>
    /// <param name="h">Тон</param>
    /// <param name="s">Насыщенность</param>
    /// <param name="v">Значение</param>
    /// <returns>Цвет</returns>
    public static Color FromAHSV(int a, int h, int s, int v)
    {
        Color c = FromHSV(h, s, v);
        c.a = (byte)a;
        return c;
    }

    /// <summary>
    /// Инвертирпование цвета
    /// </summary>
    /// <param name="a">Цвет</param>
    /// <returns>Инвертированый цвет</returns>
    public static Color operator -(Color a) => new(a.a, -a.r, -a.g, -a.b);
}

/// <summary>
/// Использует возможности SDL для упрощённой отрисовки примитивов 
/// </summary>
class PlushRenderer
{

    readonly nint renderer;
    readonly Dictionary<string, nint> Fonts;

    /// <summary>
    /// Создание пустышки для удовлетворения not-null переменной
    /// </summary>
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

    /// <summary>
    /// Отрисока кнопки
    /// </summary>
    /// <remarks> Не забудьте загрузить шрифт в рендерер с именем "Button"</remarks>
    /// <param name="button">Кнопка</param>
    /// <exception cref="ArgumentException">В слуаче отсутсвия шрифта</exception>
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

        // Рисуем прямоугольник кнопки
        DrawRectF(rect.x,
                  rect.y,
                  rect.w,
                  rect.h,
                  button.col);

        // Тут делаем всё так же как и в отрисовке текста, 
        if (!Fonts.TryGetValue("Button", out nint value)) throw new ArgumentException($"Button fontName not found");

        nint surfaceMessage = TTF_RenderText_Solid(value, button.text, new() { a = 255, r = 255, b = 255, g = 255 });

        nint Message = SDL_CreateTextureFromSurface(renderer, surfaceMessage);

        SDL_Rect Message_rect = new();
        _ = SDL_QueryTexture(Message, out _, out _, out Message_rect.w, out Message_rect.h);

        // но координаты считаем как отступ от центра кнопки 
        Message_rect.x = rect.x + rect.w / 2 - Message_rect.w / 2;
        Message_rect.y = rect.y + rect.h / 2 - Message_rect.h / 2;

        _ = SDL_RenderCopy(renderer, Message, (nint)null, ref Message_rect);

        SDL_FreeSurface(surfaceMessage);
        SDL_DestroyTexture(Message);
    }

    #endregion

    #region Lines

    /// <summary>
    /// Нахождение нормали к вектору по двум точкам
    /// </summary>
    /// <param name="a">Первая точка</param>
    /// <param name="b">Вторая точка</param>
    /// <param name="invert">Менять ли направление</param>
    /// <returns></returns>
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

    /// <summary>
    /// Отрисовывает линии из массива пар чисел определённой толщины
    /// </summary>
    /// <remarks>
    /// Толщина создаётся с помощью тупейшего костыля в виде 
    /// множества линий с отступом в полпикселя в стороны
    /// </remarks>
    /// <param name="points">Точки</param>
    /// <param name="col">Цвет</param>
    /// <param name="thinkness">Толщина</param>
    /// <param name="a">Для рекурсивного вызова, без разницы чё туда передать</param>
    /// <exception cref="ArgumentException">С массивом точек явно что-то не то</exception>
    public void DrawLines(List<float[]> points, Color col, float thinkness, bool a = true)
    {
        // Смотрим попал ли какой-то мусор в список
        for (int i = 0; i < points.Count; ++i)
            if (points[i].Length != 2) throw new ArgumentException("Line simplePoints must have only two coordinates");

        // Базовое условие
        if (thinkness < 0) return;

        // Массив точек
        SDL_FPoint[] SDL_points = new SDL_FPoint[(points.Count * 2) - 2];

        for (int i = 1; i < points.Count; ++i)
        {
            SDL_FPoint pointS = new();
            SDL_FPoint pointE = new();

            float[] dir = GetLineNormal(points[i - 1], points[i], a);

            // Линнии с отступами в стороны для создания толщины
            pointS.x = points[i - 1][0] + (dir[0] * (thinkness - 1) / 2);
            pointS.y = points[i - 1][1] + (dir[1] * (thinkness - 1) / 2);

            pointE.x = points[i][0] + (dir[0] * (thinkness - 1) / 2);
            pointE.y = points[i][1] + (dir[1] * (thinkness - 1) / 2);

            SDL_points[(i * 2) - 2] = pointS;
            SDL_points[(i * 2) - 1] = pointE;
        }

        // Рендерим всё это месиво
        _ = SDL_SetRenderDrawColor(renderer, col.r, col.g, col.b, col.a);
        _ = SDL_RenderDrawLinesF(renderer, SDL_points, SDL_points.Length);

        DrawLines(points, col, thinkness - (float)0.5, !a); // Рекурсивный вызов
    }

    /// <summary>
    /// Отрисовка только одной линии
    /// </summary>
    /// <param name="a">Начало</param>
    /// <param name="b">Конец</param>
    /// <param name="col">Цвет</param>
    /// <param name="th">Толщина</param>
    public void DrawLine(float[] a, float[] b, Color col, int th)
    {
        DrawLines([a, b], col, th);
    }

    #endregion

    #region NGons

    private void DrawNGon(float x, float y, float r, Color col, int numVerticies, bool mode = false, float defRot = (float)(-Math.PI / 2))
    {
        if (r <= 0) return;
        // базовый случай рекурсии, ну или для особо тупых фармошлёпов, которые решили что радиус может меньше нуля

        float dt = (float)(Math.PI * 2) / numVerticies;            // угловой размер линии
        SDL_FPoint[] points = new SDL_FPoint[numVerticies + 1];

        for (int i = 0; i <= numVerticies; ++i)
        {
            points[i] = new SDL_FPoint
            {
                x = x + (r * (float)Math.Cos((dt * i) + defRot)),  // вращаем точки вокруг центра на шаг
                y = y + (r * (float)Math.Sin((dt * i) + defRot))
            };
        }

        _ = SDL_SetRenderDrawColor(renderer, col.r, col.g, col.b, col.a);  // рисуем
        _ = SDL_RenderDrawLinesF(renderer, points, numVerticies + 1);

        if (mode) DrawNGon(x, y, r - (float)0.5, col, numVerticies, true, defRot); 
        // В случае заливки делаем рекрсивный вызов с теми же аргументами, только радиус немного уменьшаем
    }

    /// <summary>
    /// Отрисовка правильного многоугольника с заливкой
    /// </summary>
    /// <param name="x">Координата по X</param>
    /// <param name="y">Координата по Y</param>
    /// <param name="r">Радиус</param>
    /// <param name="col">Цвет</param>
    /// <param name="numVerticies">Количество вершин</param>
    /// <param name="defRot">Поворот вокруг центра в радианах</param>
    public void DrawNGonF(float x, float y, float r, Color col, int numVerticies, float defRot = (float)(-Math.PI / 2))
    {
        DrawNGon(x, y, r, col, numVerticies, true, defRot);
    }

    /// <summary>
    /// Отрисовка контура правильного многоугольника
    /// </summary>
    /// <param name="x">Координата по X</param>
    /// <param name="y">Координата по Y</param>
    /// <param name="r">Радиус</param>
    /// <param name="col">Цвет</param>
    /// <param name="numVerticies">Количество вершин</param>
    /// <param name="defRot">Поворот вокруг центра в радианах</param>
    public void DrawNGonW(float x, float y, float r, Color col, int numVerticies, float defRot = (float)(-Math.PI / 2))
    {
        DrawNGon(x, y, r, col, numVerticies, false, defRot);
    }

    /// <summary>
    /// Отрисовка круга
    /// </summary>
    /// <param name="x">Координаты по X</param>
    /// <param name="y">Координаты по Y</param>
    /// <param name="r">Радиус</param>
    /// <param name="col">Цвет</param>
    public void DrawCircleF(float x, float y, float r, Color col)
    {
        DrawNGon(x, y, r, col, 100, true);
    }

    /// <summary>
    /// Отрисовка окружности
    /// </summary>
    /// <param name="x">Координаты по X</param>
    /// <param name="y">Координаты по Y</param>
    /// <param name="r">Радиус</param>
    /// <param name="col">Цвет</param>
    public void DrawCircleW(float x, float y, float r, Color col)
    {
        DrawNGon(x, y, r, col, 150);
    }

    #endregion

    #region PlushText

    /// <summary>
    /// Инициализация SDL TTF
    /// </summary>
    public static void InitText()
    {
        _ = TTF_Init();
    }

    /// <summary>
    /// Добавление шрифта в "библиотеку" шрифтов отдельно взятого рендерера
    /// </summary>
    /// <remarks>
    /// Если не указано имя, то берётся название файла без расширения
    /// </remarks>
    /// <param name="path">Где лежит шрифт</param>
    /// <param name="size">Размер</param>
    /// <param name="name">Имя шрифта которое будет использовано дальше в коде</param>
    public void LoadFont(string path, int size, string? name = null)
    {
        nint new_font = TTF_OpenFont(path, size);
        string new_name = path.Split('.').First().Split("/").Last(); // Берём либо имя, либо название файла
        if (name != null) new_name = name;

        Fonts.Add(new_name, new_font);
    }

    /// <summary>
    /// Рисует шрифт
    /// </summary>
    /// <param name="x">Координата по X</param>
    /// <param name="y">Координаты по Y</param>
    /// <param name="s">Текст</param>
    /// <param name="col">Цвет</param>
    /// <param name="fontName">Имя шрифта указаное в <c>LoadFont</c></param>
    /// <exception cref="ArgumentException">Если шрифт не найден</exception>
    public void DrawText(int x, int y, string s, Color col, string fontName)
    {
        SDL_Color White = new()
        {
            r = col.r,
            g = col.g,
            b = col.b,
            a = col.a
        };

        // Проверяем наличие шрифта
        if (!Fonts.TryGetValue(fontName, out nint font)) throw new ArgumentException($"Font {fontName} not found");

        // Рендерим текст
        nint surfaceMessage = TTF_RenderText_Solid(font, s, White);

        // Фигачим всё в текстуру
        nint Message = SDL_CreateTextureFromSurface(renderer, surfaceMessage);

        SDL_Rect Message_rect = new();
        // Получаем размеры текстуры
        _ = SDL_QueryTexture(Message, out _, out _, out Message_rect.w, out Message_rect.h);

        // Координаты
        Message_rect.x = x;
        Message_rect.y = y;

        // Рендерим текстуру в кадре
        _ = SDL_RenderCopy(renderer, Message, (nint)null, ref Message_rect);

        SDL_FreeSurface(surfaceMessage);
        SDL_DestroyTexture(Message);
    }

    #endregion

    #region Rects

    /// <summary>
    /// Отрисовывает прямоугольник(ого какая неожиданность)
    /// </summary>
    /// <param name="x">Координата по X</param>
    /// <param name="y">Координата по Y</param>
    /// <param name="w">Ширина</param>
    /// <param name="h">Высота</param>
    /// <param name="col">Цвет</param>
    /// <param name="mode">С заливкой или нет</param>
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

    /// <summary>
    /// Отрисовывает прямоугольник с заливкой
    /// </summary>
    /// <param name="x">Координата по X</param>
    /// <param name="y">Координата по Y</param>
    /// <param name="w">Ширина</param>
    /// <param name="h">Высота</param>
    /// <param name="col">Цвет</param>
    public void DrawRectF(float x, float y, float w, float h, Color col)
    {
        DrawRect(x, y, w, h, col, true);
    }

    /// <summary>
    /// Отрисовывает контур прямоугольника
    /// </summary>
    /// <param name="x">Координата по X</param>
    /// <param name="y">Координата по Y</param>
    /// <param name="w">Ширина</param>
    /// <param name="h">Высота</param>
    /// <param name="col">Цвет</param>
    public void DrawRectW(float x, float y, float w, float h, Color col)
    {
        DrawRect(x, y, w, h, col, false);
    }

    #endregion

    #region SDL_tools

    /// <summary>
    /// Заливает окно сплошным цветом
    /// </summary>
    /// <param name="col">Цвет</param>
    public void FillWindow(Color col)
    {
        if (SDL_SetRenderDrawColor(renderer, col.r, col.g, col.b, col.a) < 0)
            Console.WriteLine($"There was an issue with setting the render draw color. {SDL_GetError()}");

        if (SDL_RenderClear(renderer) < 0)
            Console.WriteLine($"There was an issue with clearing the render surface. {SDL_GetError()}");
    }

    /// <summary>
    /// Отрисовывает новый кадр на экране. Вызывать надо после других вызовов отрисовки
    /// </summary>
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
