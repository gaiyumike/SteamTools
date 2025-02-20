using static BD.WTTS.CommandLineHost;

namespace BD.WTTS;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicMethods)]
static partial class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    internal static int Main(string[] args)
    {
#if WINDOWS
        if (!IsCustomEntryPoint && !CompatibilityCheck()) return 0;
#endif

#if DEBUG
        Console.WriteLine($"Environment.Version: {Environment.Version}");
        Console.WriteLine($"args: {string.Join(' ', args)}");
        Console.WriteLine($"AppContext.BaseDirectory: {AppContext.BaseDirectory}");
        //Console.WriteLine($"AppDomain.CurrentDomain.BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
        Console.WriteLine($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");
        Console.WriteLine($"Environment.ProcessPath: {Environment.ProcessPath}");
        Console.WriteLine($"Environment.UserInteractive: {Environment.UserInteractive}");
        Console.WriteLine($"Assembly.GetEntryAssembly()?.Location: {Assembly.GetEntryAssembly()?.Location}");
        Console.WriteLine($"Directory.GetCurrentDirectory: {Directory.GetCurrentDirectory()}");
        Console.WriteLine($"CurrentThread.ManagedThreadId: {Environment.CurrentManagedThreadId}");
        Console.WriteLine($"CurrentThread.ApartmentState: {Thread.CurrentThread.GetApartmentState()}");
        // 调试时移动本机库到 native，通常指定了单个 RID(RuntimeIdentifier) 后本机库将位于程序根目录上否则将位于 runtimes 文件夹中
        GlobalDllImportResolver.MoveFiles();
#endif

        // 监听当前应用程序域的程序集加载
        AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
        static void CurrentDomain_AssemblyLoad(object? sender, AssemblyLoadEventArgs args)
        {
#if DEBUG
            Console.WriteLine($"loadasm: {args.LoadedAssembly}, location: {args.LoadedAssembly.Location}");
#endif
            // 使用 native 文件夹导入解析本机库
            try
            {
                NativeLibrary.SetDllImportResolver(args.LoadedAssembly, GlobalDllImportResolver.Delegate);
            }
            catch
            {
                // ArgumentNullException assembly 或 resolver 为 null。
                // ArgumentException 已为此程序集设置解析程序。
                // 此每程序集解析程序是第一次尝试解析此程序集启动的本机库加载。
                // 此方法的调用方应仅为自己的程序集注册解析程序。
                // 每个程序集只能注册一个解析程序。 尝试注册第二个解析程序失败并出现 InvalidOperationException。
                // https://learn.microsoft.com/zh-cn/dotnet/api/system.runtime.interopservices.nativelibrary.setdllimportresolver
            }
        }

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        static Assembly? CurrentDomain_AssemblyResolve(object? sender, ResolveEventArgs args)
        {
            try
            {
                var fileNameWithoutEx = args.Name.Split(',', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                if (!string.IsNullOrEmpty(fileNameWithoutEx))
                {
                    var isResources = fileNameWithoutEx.EndsWith(".resources");
                    if (isResources)
                    {
                        // System.Composition.Convention.resources
                        // 已包含默认资源，通过反射调用已验证成功
                        // typeof(ConventionBuilder).Assembly.GetType("System.SR").GetProperty("ArgumentOutOfRange_InvalidEnumInSet", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null)
                        return null;
                    }
                    // 当前根目录下搜索程序集
                    var filePath = Path.Combine(AppContext.BaseDirectory, $"{fileNameWithoutEx}.dll");
                    if (File.Exists(filePath)) return Assembly.LoadFile(filePath);
                    // 当前根目录下独立框架运行时中搜索程序集
                    filePath = Path.Combine(AppContext.BaseDirectory, "..", "dotnet", "shared", "Microsoft.AspNetCore.App", Environment.Version.ToString(), $"{fileNameWithoutEx}.dll");
                    if (File.Exists(filePath)) return Assembly.LoadFile(filePath);
                    // 当前已安装的运行时
                    filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet", "shared", "Microsoft.AspNetCore.App", Environment.Version.ToString(), $"{fileNameWithoutEx}.dll");
                    if (File.Exists(filePath)) return Assembly.LoadFile(filePath);
                    var dotnet_root = Environment.GetEnvironmentVariable("DOTNET_ROOT");
                    if (!string.IsNullOrWhiteSpace(dotnet_root) && Directory.Exists(dotnet_root))
                    {
                        filePath = Path.Combine(dotnet_root, "shared", "Microsoft.AspNetCore.App", Environment.Version.ToString(), $"{fileNameWithoutEx}.dll");
                        if (File.Exists(filePath)) return Assembly.LoadFile(filePath);
                    }

                }
            }
            catch
            {

            }
#if DEBUG
            Console.WriteLine($"asm-resolve fail, name: {args.Name}");
#endif
            return null;
        }

        // 注册 MemoryPack 某些自定义类型的格式化，如 Cookie, IPAddress, RSAParameters
        MemoryPackFormatterProvider.Register<MemoryPackFormatters>();

        // 添加 .NET Framework 中可用的代码页提供对编码提供程序
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // fix The request was aborted: Could not create SSL/TLS secure channel
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;

#if WINDOWS
        // 在 Windows 上还原 .NET Framework 中网络请求跟随系统网络代理变化而动态切换代理行为
        HttpClient.DefaultProxy = DynamicHttpWindowsProxy.Instance;
#endif
#if WINDOWS_DESKTOP_BRIDGE
        if (!DesktopBridgeHelper.Init()) return 0;
        InitWithDesktopBridge(ref args);
#elif IOS || MACOS || MACCATALYST
        UIApplication.Main(args, null, typeof(AppDelegate));
#endif

        var host = Host.Instance;
        host.IsMainProcess = args.Length == 0;
        host.IsCLTProcess = !host.IsMainProcess && args.FirstOrDefault() == "-clt";

        OnCreateAppExecuting();

        try
        {
            string[] args_clt;
            if (host.IsCLTProcess) // 命令行模式
            {
                args_clt = args.Skip(1).ToArray();
                if (args_clt.Length == 1 && args_clt[0].Equals(command_main, StringComparison.OrdinalIgnoreCase)) return default;
            }
            else
            {
                args_clt = new[] { command_main };
            }
            return host.Run(args_clt);
        }
        catch (Exception ex)
        {
            GlobalExceptionHelpers.Handler(ex, nameof(Main));
            throw;
        }
        finally
        {
            // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
            PlatformApp?.Dispose();
            Ioc.Dispose();
            LogManager.Shutdown();
        }
    }

    public static bool IsCustomEntryPoint { get; internal set; }

    /// <summary>
    /// 自定义 .NET Host 入口点
    /// <para>https://github.com/dotnet/runtime/blob/v7.0.3/docs/design/features/native-hosting.md#loading-and-calling-managed-components</para>
    /// </summary>
    /// <param name="args"></param>
    /// <param name="sizeBytes"></param>
    /// <returns></returns>
#pragma warning disable IDE0060 // 删除未使用的参数
    public static int CustomEntryPoint(nint args, int sizeBytes)
#pragma warning restore IDE0060 // 删除未使用的参数
    {
        IsCustomEntryPoint = true;
        AppContext.SetData("APP_CONTEXT_BASE_DIRECTORY", Environment.CurrentDirectory);
        return Main(Environment.GetCommandLineArgs().Skip(1).ToArray());
    }

#if WINDOWS_DESKTOP_BRIDGE
    static void InitWithDesktopBridge(ref string[] args)
    {
        DesktopBridgeHelper.OnActivated(ref args);
    }
#endif

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        // 设计器模式不会执行 Main 函数所以以此区分来初始化文件系统
        if (Design.IsDesignMode)
        {
            OnCreateAppExecuting();
        }

        var builder = AppBuilder.Configure(() => Host.Instance.App)
               .UsePlatformDetect2()
               .LogToTrace()
               .UseReactiveUI();

        var useGpu = !IApplication.DisableGPU && GeneralSettings.UseGPURendering.Value;
#if MACOS
        builder.With(new AvaloniaNativePlatformOptions
        {
            UseGpu = useGpu,
        });
#elif LINUX
        builder.With(new X11PlatformOptions
        {
            UseGpu = useGpu,
        });
#elif WINDOWS
        var useWgl = IApplication.UseWgl || GeneralSettings.UseWgl.Value;
        var options = new Win32PlatformOptions
        {
            UseWgl = useWgl,
            AllowEglInitialization = useGpu,
        };
        builder.With(options);

        var skiaOptions = new SkiaOptions
        {
            MaxGpuResourceSizeBytes = 1024000000,
        };

        builder.With(skiaOptions);
#else
        throw new PlatformNotSupportedException();
#endif
        return builder;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static void StartAvaloniaApp(string[] args,
        ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
    {
        if (!Environment.UserInteractive) return;
        var builder = BuildAvaloniaApp();
        builder.StartWithClassicDesktopLifetime2(args, shutdownMode);
    }
}
