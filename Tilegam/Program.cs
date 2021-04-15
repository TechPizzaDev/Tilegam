using Veldrid.Sdl2;

namespace Tilegam.Client
{
    class Program
    {
        static unsafe void Main(string[] args)
        {
            SDL_version version;
            Sdl2Native.SDL_GetVersion(&version);

            var app = new Tilegam();
            app.Run();
        }
    }
}
