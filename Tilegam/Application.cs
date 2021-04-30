using System;
using System.Diagnostics;
using System.Threading;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace Tilegam.Client
{
    public abstract class Application
    {
        private Sdl2Window _window;
        private GraphicsDevice _graphicsDevice;

        private Action _enableScreensaver;
        private Action _disableScreensaver;

        private bool _shouldExit;
        private string _windowTitle = "";

        public TimeAverager TimeAverager { get; private set; }

        public bool IsActive { get; private set; }

        public bool AlwaysRecreateWindow { get; set; } = true;
        public bool SrgbSwapchain { get; set; } = false;
        public bool DrawWhenUnfocused { get; set; } = true;
        public bool DrawWhenMinimized { get; set; } = false;
        public TimeSpan? TargetFrameTime { get; set; } = null;
        public TimeSpan UnfocusedFrameTime { get; set; } = TimeSpan.FromSeconds(1 / 20.0);
        public TimeSpan MinimizedFrameTime { get; set; } = TimeSpan.FromSeconds(1 / 20.0);

        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                _windowTitle = value ?? "";
                _window.Title = _windowTitle;
            }
        }

        public GraphicsDevice GraphicsDevice
        {
            get => _graphicsDevice;
            private set => _graphicsDevice = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Sdl2Window Window
        {
            get => _window;
            private set
            {
                _window = value ?? throw new ArgumentNullException(nameof(value));
                _window.Resized += () => WindowResized();
                _window.FocusGained += Window_FocusGained;
                _window.FocusLost += Window_FocusLost;
            }
        }

        public ShaderCache ShaderCache { get; } = new();
        public GraphicsResourceCache GraphicsResourceCache { get; } = new();

        public Application(GraphicsBackend? preferredBackend = null)
        {
            var windowCI = new WindowCreateInfo
            {
                X = 50,
                Y = 50,
                WindowWidth = 960,
                WindowHeight = 540,
                WindowInitialState = WindowState.Hidden,
                WindowTitle = _windowTitle
            };

            var gdOptions = new GraphicsDeviceOptions(
                ShouldEnableGraphicsDeviceDebug(), null, false, ResourceBindingModel.Improved, true, true, SrgbSwapchain);

            GraphicsDevice gd;
            Sdl2Window window;
            try
            {
                VeldridStartup.CreateWindowAndGraphicsDevice(
                    windowCI,
                    gdOptions,
                    preferredBackend ?? VeldridStartup.GetPlatformDefaultBackend(),
                    out window,
                    out gd);
            }
            catch
            {
                VeldridStartup.CreateWindowAndGraphicsDevice(
                    windowCI,
                    gdOptions,
                    VeldridStartup.GetPlatformDefaultBackend(),
                    out window,
                    out gd);
            }
            _enableScreensaver = Sdl2Native.LoadFunction<Action>("SDL_EnableScreenSaver");
            _disableScreensaver = Sdl2Native.LoadFunction<Action>("SDL_DisableScreenSaver");

            TimeAverager = new TimeAverager(4, TimeSpan.FromSeconds(0.5));
            GraphicsDevice = gd;
            Window = window;
        }

        private void Window_FocusGained()
        {
            IsActive = true;
            _disableScreensaver.Invoke();
            WindowGainedFocus();
        }

        private void Window_FocusLost()
        {
            IsActive = false;
            _enableScreensaver.Invoke();
            WindowLostFocus();
        }

        public void Run()
        {
            long totalTicks = 0;
            long previousTicks = Stopwatch.GetTimestamp();

            if (RunBody(ref totalTicks, ref previousTicks))
            {
                Window.Visible = true;

                do
                {
                    if (!RunBody(ref totalTicks, ref previousTicks))
                        break;

                    TimeAverager.Tick();
                }
                while (Window.Exists);
            }

            DisposeGraphicsDeviceObjects();
            GraphicsDevice.Dispose();
        }

        private bool RunBody(ref long totalTicks, ref long previousTicks)
        {
            long currentTicks = Stopwatch.GetTimestamp();
            long deltaTicks = currentTicks - previousTicks;
            previousTicks = currentTicks;

            // TODO: update more based on overtime

            TimeSpan deltaTime = TargetFrameTime.HasValue
                ? TargetFrameTime.GetValueOrDefault()
                : TimeSpan.FromSeconds(deltaTicks * TimeAverager.SecondsPerTick);

            totalTicks += deltaTime.Ticks;

            var time = new FrameTime(
                TimeSpan.FromTicks(totalTicks),
                deltaTime,
                IsActive);

            TimeAverager.BeginUpdate();
            PumpSdlEvents();
            Update(time);
            TimeAverager.EndUpdate();

            if (_shouldExit)
            {
                Window.Close();
                _shouldExit = false;
                return false;
            }

            if (!Window.Exists)
                return false;

            double spentMillis = (Stopwatch.GetTimestamp() - currentTicks) * TimeAverager.MillisPerTick;
            int sleepMillis;

            if (time.IsActive)
            {
                DrawAndPresent();

                if (TargetFrameTime.HasValue)
                {
                    sleepMillis = (int)(TargetFrameTime.GetValueOrDefault().TotalSeconds - spentMillis);
                }
                else
                {
                    sleepMillis = 0;
                }
            }
            else
            {
                WindowState windowState = Window.WindowState;
                if (DrawWhenUnfocused)
                {
                    if (windowState == WindowState.Minimized)
                    {
                        if (DrawWhenMinimized)
                            DrawAndPresent();
                    }
                    else
                    {
                        DrawAndPresent();
                    }
                }

                if (windowState == WindowState.Minimized)
                {
                    sleepMillis = (int)(MinimizedFrameTime.TotalMilliseconds - spentMillis);
                }
                else
                {
                    sleepMillis = (int)(UnfocusedFrameTime.TotalMilliseconds - spentMillis);
                }
            }

            if (sleepMillis > 0)
            {
                Thread.Sleep(sleepMillis);
            }
            return true;
        }

        public void Exit()
        {
            _shouldExit = true;
        }

        private void PumpSdlEvents()
        {
            Sdl2Events.ProcessEvents();
            InputSnapshot snapshot = Window.PumpEvents();
            InputTracker.UpdateFrameInput(snapshot, Window);
        }

        public virtual void Update(in FrameTime time)
        {
        }

        public virtual void Draw()
        {
        }

        public virtual void Present()
        {
            GraphicsDevice.SwapBuffers();
        }

        private void DrawAndPresent()
        {
            // TODO: try to revive other backends
            try
            {
                TimeAverager.BeginDraw();
                Draw();
                TimeAverager.EndDraw();
            }
            catch (SharpGen.Runtime.SharpGenException ex) when
                (ex.Descriptor == Vortice.DXGI.ResultCode.DeviceRemoved ||
                ex.Descriptor == Vortice.DXGI.ResultCode.DeviceReset)
            {
                Console.WriteLine(ex); // TODO: log proper error

                ChangeGraphicsBackend(true, null);
                return;
            }

            TimeAverager.BeginPresent();
            Present();
            TimeAverager.EndPresent();
        }

        public void ChangeGraphicsBackend(bool forceRecreateWindow, GraphicsBackend? preferredBackend = null)
        {
            GraphicsBackend previousBackend = GraphicsDevice.BackendType;
            bool syncToVBlank = GraphicsDevice.SyncToVerticalBlank;

            DisposeGraphicsDeviceObjects();
            GraphicsDevice.Dispose();

            if (AlwaysRecreateWindow || forceRecreateWindow)
            {
                var windowCI = new WindowCreateInfo
                {
                    X = Window.X,
                    Y = Window.Y,
                    WindowWidth = Window.Width,
                    WindowHeight = Window.Height,
                    WindowInitialState = Window.WindowState,
                    WindowTitle = _windowTitle
                };

                Window.Close();
                Window = VeldridStartup.CreateWindow(windowCI);
            }

            var gdOptions = new GraphicsDeviceOptions(
                ShouldEnableGraphicsDeviceDebug(), null, syncToVBlank, ResourceBindingModel.Improved, true, true, SrgbSwapchain);

            try
            {
                if (preferredBackend == null)
                    preferredBackend = previousBackend;

                GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, gdOptions, preferredBackend.Value);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // TODO: log proper error

                GraphicsDevice = VeldridStartup.CreateGraphicsDevice(Window, gdOptions);
            }

            CreateGraphicsDeviceObjects();
        }

        protected virtual bool ShouldEnableGraphicsDeviceDebug()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }

        protected virtual void WindowResized()
        {
        }

        protected virtual void WindowGainedFocus()
        {
        }

        protected virtual void WindowLostFocus()
        {
        }

        protected void ChangeGraphicsBackend(GraphicsBackend? preferredBackend = null)
        {
            ChangeGraphicsBackend(false, preferredBackend);
        }

        protected virtual void CreateGraphicsDeviceObjects()
        {
        }

        protected virtual void DisposeGraphicsDeviceObjects()
        {
        }
    }
}
