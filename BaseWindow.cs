namespace taxi_manager_simulator;

abstract class BaseWindow
{
    protected nint renderer, window;
    protected bool running = true;
    protected PlushRenderer plushRenderer = PlushRenderer.Empty;

    public BaseWindow(int w, int h, string name)
    {
        new Thread(() =>
        {
            window = SDL_CreateWindow(name, SDL_WINDOWPOS_UNDEFINED, SDL_WINDOWPOS_UNDEFINED, w, h, SDL_WindowFlags.SDL_WINDOW_SHOWN);

            if (window == IntPtr.Zero)
                Console.WriteLine($"There was an issue creating the window. {SDL_GetError()}");

            renderer = SDL_CreateRenderer(window,
                                          -1,
                                          SDL_RendererFlags.SDL_RENDERER_ACCELERATED |
                                          SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

            if (renderer == IntPtr.Zero)
                Console.WriteLine($"There was an issue creating the renderer. {SDL_GetError()}");

            plushRenderer = new(ref renderer);
            Thread.CurrentThread.Name = name;
            Run();
        }).Start();
    }

    abstract protected void Run();

    ~BaseWindow()
    {
        SDL_DestroyRenderer(renderer);
        SDL_DestroyWindow(window);
        SDL_Quit();
    }
}
