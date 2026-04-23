namespace tetris_fight.Features.Audio.Infrastructure;

using System.Runtime.InteropServices;

public sealed class GameMusicService : IDisposable
{
    private const string MusicFile = "assets/sounds/theme.mp3";
    private const float BaseRate = 1.0f;
    private const float RateStep = 0.005f;
    private const float MaxRate = 1.40f;

    private readonly IAudioPlayer _player;
    private bool _started;
    private bool _disposed;

    public GameMusicService()
    {
        _player = CreatePlayer();
    }

    public void Start()
    {
        if (_disposed)
            return;

        _started = true;
        SetSpeedLevel(0);
        _player.Play();
    }

    public void Pause()
    {
        if (!_started || _disposed)
            return;

        _player.Pause();
    }

    public void Resume()
    {
        if (!_started || _disposed)
            return;

        _player.Play();
    }

    public void Stop()
    {
        if (_disposed)
            return;

        _started = false;
        _player.Stop();
    }

    public void SetSpeedLevel(int speedLevel)
    {
        if (_disposed)
            return;

        float rate = Math.Min(MaxRate, BaseRate + Math.Max(0, speedLevel) * RateStep);
        _player.SetRate(rate);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        _player.Dispose();
    }

    private static IAudioPlayer CreatePlayer()
    {
        string path = ResolveMusicPath();
        if (!File.Exists(path))
            return NoOpAudioPlayer.Instance;

        if (OperatingSystem.IsMacOS())
            return MacOsAudioPlayer.TryCreate(path) is { } player ? player : NoOpAudioPlayer.Instance;

        return NoOpAudioPlayer.Instance;
    }

    private static string ResolveMusicPath()
    {
        string outputPath = Path.Combine(AppContext.BaseDirectory, MusicFile);
        if (File.Exists(outputPath))
            return outputPath;

        return Path.Combine(Environment.CurrentDirectory, MusicFile);
    }

    private interface IAudioPlayer : IDisposable
    {
        void Play();
        void Pause();
        void Stop();
        void SetRate(float rate);
    }

    private sealed class NoOpAudioPlayer : IAudioPlayer
    {
        public static readonly NoOpAudioPlayer Instance = new();

        private NoOpAudioPlayer() { }

        public void Play() { }
        public void Pause() { }
        public void Stop() { }
        public void SetRate(float rate) { }
        public void Dispose() { }
    }

    private sealed class MacOsAudioPlayer : IAudioPlayer
    {
        private const string ObjCLibrary = "/usr/lib/libobjc.A.dylib";
        private const string LibSystem = "/usr/lib/libSystem.dylib";
        private const int RtldNow = 2;

        private readonly IntPtr _player;
        private bool _disposed;

        private MacOsAudioPlayer(IntPtr player)
        {
            _player = player;
        }

        public static MacOsAudioPlayer? TryCreate(string path)
        {
            try
            {
                dlopen("/System/Library/Frameworks/AVFoundation.framework/AVFoundation", RtldNow);

                IntPtr nsStringClass = objc_getClass("NSString");
                IntPtr nsUrlClass = objc_getClass("NSURL");
                IntPtr playerClass = objc_getClass("AVAudioPlayer");

                if (nsStringClass == IntPtr.Zero || nsUrlClass == IntPtr.Zero || playerClass == IntPtr.Zero)
                    return null;

                IntPtr nsPath = IntPtr_objc_msgSend_String(
                    nsStringClass,
                    sel_registerName("stringWithUTF8String:"),
                    path);
                IntPtr url = IntPtr_objc_msgSend_IntPtr(
                    nsUrlClass,
                    sel_registerName("fileURLWithPath:"),
                    nsPath);
                IntPtr allocatedPlayer = IntPtr_objc_msgSend(
                    playerClass,
                    sel_registerName("alloc"));
                IntPtr player = IntPtr_objc_msgSend_IntPtr_IntPtr(
                    allocatedPlayer,
                    sel_registerName("initWithContentsOfURL:error:"),
                    url,
                    IntPtr.Zero);

                if (player == IntPtr.Zero)
                    return null;

                Void_objc_msgSend_NInt(player, sel_registerName("setNumberOfLoops:"), -1);
                Void_objc_msgSend_Byte(player, sel_registerName("setEnableRate:"), 1);
                Void_objc_msgSend_Float(player, sel_registerName("setVolume:"), 0.45f);
                Void_objc_msgSend_Float(player, sel_registerName("setRate:"), BaseRate);
                Bool_objc_msgSend(player, sel_registerName("prepareToPlay"));

                return new MacOsAudioPlayer(player);
            }
            catch
            {
                return null;
            }
        }

        public void Play()
        {
            if (_disposed)
                return;

            Bool_objc_msgSend(_player, sel_registerName("play"));
        }

        public void Pause()
        {
            if (_disposed)
                return;

            Void_objc_msgSend(_player, sel_registerName("pause"));
        }

        public void Stop()
        {
            if (_disposed)
                return;

            Void_objc_msgSend(_player, sel_registerName("stop"));
            Void_objc_msgSend_Double(_player, sel_registerName("setCurrentTime:"), 0);
        }

        public void SetRate(float rate)
        {
            if (_disposed)
                return;

            Void_objc_msgSend_Float(_player, sel_registerName("setRate:"), rate);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _disposed = true;
            Void_objc_msgSend(_player, sel_registerName("release"));
        }

        [DllImport(LibSystem)]
        private static extern IntPtr dlopen(string path, int mode);

        [DllImport(ObjCLibrary)]
        private static extern IntPtr objc_getClass(string name);

        [DllImport(ObjCLibrary)]
        private static extern IntPtr sel_registerName(string name);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr IntPtr_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr IntPtr_objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern IntPtr IntPtr_objc_msgSend_String(
            IntPtr receiver,
            IntPtr selector,
            [MarshalAs(UnmanagedType.LPUTF8Str)] string arg1);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern void Void_objc_msgSend(IntPtr receiver, IntPtr selector);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern void Void_objc_msgSend_NInt(IntPtr receiver, IntPtr selector, nint arg1);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern void Void_objc_msgSend_Byte(IntPtr receiver, IntPtr selector, byte arg1);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern void Void_objc_msgSend_Float(IntPtr receiver, IntPtr selector, float arg1);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        private static extern void Void_objc_msgSend_Double(IntPtr receiver, IntPtr selector, double arg1);

        [DllImport(ObjCLibrary, EntryPoint = "objc_msgSend")]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool Bool_objc_msgSend(IntPtr receiver, IntPtr selector);
    }
}
