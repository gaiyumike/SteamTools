// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// 当前应用程序，可能是 UI 程序，也可能是控制台程序
/// </summary>
public partial interface IApplication
{
    static IApplication Instance => Ioc.Get<IApplication>();

    /// <summary>
    /// 当前是否为桌面端，IsWindows or IsMacOS or (IsLinux and !IsAndroid)
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool IsDesktop()
#if WINDOWS || MACOS || LINUX
        => true;
#elif MACCATALYST
        => OperatingSystem.IsMacOS();
#elif IOS || ANDROID
        => false;
#else
        => OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || (OperatingSystem.IsLinux() && !OperatingSystem.IsAndroid());
#endif

    [Obsolete("use IsDesktop", true)]
    static bool IsDesktopPlatform => IsDesktop();

    /// <summary>
    /// 获取当前平台 UI Host
    /// <para>reference to the ViewController (if using Xamarin.iOS), Activity (if using Xamarin.Android) IWin32Window or IntPtr (if using .Net Framework).</para>
    /// </summary>
    object CurrentPlatformUIHost { get; }

    DeploymentMode DeploymentMode => DeploymentMode.SCD;

    /// <inheritdoc cref="IPlatformService.SetBootAutoStart(bool, string)"/>
    static void SetBootAutoStart(bool isAutoStart) => IPlatformService.Instance.SetBootAutoStart(isAutoStart, Constants.HARDCODED_APP_NAME);

    bool IsAvaloniaUI() => false;

    bool IsMAUI() => false;
}
