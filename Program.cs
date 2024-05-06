namespace taxi_manager_simulator;

internal class Programm
{
    static void Main()
    {
        if (SDL_Init(SDL_INIT_VIDEO) < 0)
        {
            Console.WriteLine($"There was an issue initilizing SDL. {SDL_GetError()}");
            return;
        }

        _ = new TaxiManager();
    }
}