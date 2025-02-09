#if WINDOWS || NETFRAMEWORK || APP_HOST
#if NETFRAMEWORK
global using WPFMessageBox = System.Windows.MessageBox;
global using WPFMessageBoxButton = System.Windows.MessageBoxButton;
global using WPFMessageBoxImage = System.Windows.MessageBoxImage;
global using WPFMessageBoxResult = System.Windows.MessageBoxResult;
#endif
using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

static partial class Program
{
    /// <summary>
    /// 兼容性检查
    /// <para>此应用程序仅兼容 Windows 11 与 Windows 10 版本 1809（OS 内部版本 17763）或更高版本</para>
    /// <para>不能在临时文件夹中运行此程序，请将所有文件复制或解压到其他路径后再启动程序</para>
    /// </summary>
    /// <returns></returns>
#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    static bool CompatibilityCheck()
    {
        int major = 10, minor = 0, build = 17763;
#if NETFRAMEWORK
        if (Environment.OSVersion.Version < new Version(major, minor, build))
#else
        if (!OperatingSystem.IsWindowsVersionAtLeast(major, minor, build))
#endif
        {
            ShowErrMessageBox(AppResources.Error_IncompatibleOS);
            return false;
        }
        var baseDirectory =
#if NET46_OR_GREATER || NETCOREAPP
        AppContext.BaseDirectory;
#else
        AppDomain.CurrentDomain.BaseDirectory;
#endif
        if (baseDirectory.StartsWith(Path.GetTempPath(), StringComparison.OrdinalIgnoreCase))
        {
            // 检测当前目录 Temp\Rar$ 这类目录，可能是在压缩包中直接启动程序导致的，还有一堆 文件找不到/加载失败的异常
            //  System.IO.DirectoryNotFoundException: Could not find a part of the path 'C:\Users\USER\AppData\Local\Temp\Rar$EXa15528.13350\Cache\switchproxy.reg'.
            //  System.IO.FileLoadException ...
            //  System.IO.FileNotFoundException: Could not load file or assembly ...
            ShowErrMessageBox(AppResources.Error_BaseDir_StartsWith_Temp);
            return false;
        }
        return true;
    }

#if NET35 || NET40
    [MethodImpl((MethodImplOptions)0x100)]
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    internal static WPFMessageBoxResult ShowErrMessageBox(string error, WPFMessageBoxButton button = WPFMessageBoxButton.OK)
    {
        try
        {
            if (Environment.UserInteractive)
            {
                return WPFMessageBox.Show(error, AppResources.Error, button, WPFMessageBoxImage.Error);
            }
        }
        catch
        {

        }

        Console.WriteLine(AppResources.Error);
        Console.WriteLine(error);
        switch (button)
        {
            case WPFMessageBoxButton.OKCancel:
                {
                    Console.WriteLine("OK/Cancel");
                    var line = Console.ReadLine();
                    if (string.Equals(line, nameof(WPFMessageBoxResult.OK)) || string.Equals(line, nameof(WPFMessageBoxResult.Yes)) || string.Equals(line, "o") || string.Equals(line, "y"))
                        return WPFMessageBoxResult.OK;
                    if (string.Equals(line, nameof(WPFMessageBoxResult.Cancel)) || string.Equals(line, nameof(WPFMessageBoxResult.No)) || string.Equals(line, "c") || string.Equals(line, "n"))
                        return WPFMessageBoxResult.Cancel;
                }
                break;
            case WPFMessageBoxButton.YesNoCancel:
                {
                    Console.WriteLine("Yes/No/Cancel");
                    var line = Console.ReadLine();
                    if (string.Equals(line, nameof(WPFMessageBoxResult.OK)) || string.Equals(line, nameof(WPFMessageBoxResult.Yes)) || string.Equals(line, "o") || string.Equals(line, "y"))
                        return WPFMessageBoxResult.Yes;
                    if (string.Equals(line, nameof(WPFMessageBoxResult.No)) || string.Equals(line, "n"))
                        return WPFMessageBoxResult.No;
                    if (string.Equals(line, nameof(WPFMessageBoxResult.Cancel)) || string.Equals(line, "c"))
                        return WPFMessageBoxResult.Cancel;
                }
                break;
            case WPFMessageBoxButton.YesNo:
                {
                    Console.WriteLine("Yes/No");
                    var line = Console.ReadLine();
                    if (string.Equals(line, nameof(WPFMessageBoxResult.OK)) || string.Equals(line, nameof(WPFMessageBoxResult.Yes)) || string.Equals(line, "o") || string.Equals(line, "y"))
                        return WPFMessageBoxResult.Yes;
                    if (string.Equals(line, nameof(WPFMessageBoxResult.Cancel)) || string.Equals(line, nameof(WPFMessageBoxResult.No)) || string.Equals(line, "c") || string.Equals(line, "n"))
                        return WPFMessageBoxResult.No;
                }
                break;
        }
        return WPFMessageBoxResult.OK;
    }
}
#endif