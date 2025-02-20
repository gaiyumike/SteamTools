namespace BD.WTTS.Enums;

/// <summary>
/// 图片下载渠道类型
/// </summary>
public enum ImageChannelType : byte
{
    /// <summary>
    /// Steam 头像图片
    /// </summary>
    SteamAvatars,

    /// <summary>
    /// Steam 游戏图片
    /// </summary>
    SteamGames,

    /// <summary>
    /// Steam 市场物品图片
    /// </summary>
    SteamEconomys,

    /// <summary>
    /// Steam 成就图标
    /// </summary>
    SteamAchievementIcon,

    /// <summary>
    /// Steam 加速项目图标
    /// </summary>
    AccelerateGroup,

    /// <summary>
    /// 脚本图标
    /// </summary>
    ScriptIcon,

    /// <summary>
    /// 验证码图片
    /// </summary>
    CodeImage,

    /// <summary>
    /// 通知公告封面图
    /// </summary>
    NoticePicture,

    /// <summary>
    /// 广告图片
    /// </summary>
    Advertisement,
}

/// <summary>
/// Enum 扩展 <see cref="ImageChannelType"/>
/// </summary>
public static class ImageChannelTypeEnumExtensions
{
    /// <inheritdoc cref="IImageHttpClientService.GetImageAsync(string, string, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<string?> GetImageAsync(this IImageHttpClientService imageHttpClientService,
        string? requestUri,
        ImageChannelType channelType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestUri)) return Task.FromResult((string?)null);
        var channelType_ = channelType.ToString();
        return imageHttpClientService.GetImageAsync(requestUri, channelType_, cancellationToken);
    }

    /// <inheritdoc cref="IImageHttpClientService.GetImageAsync(string, string, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<string?> GetImageAsync(this ImageChannelType channelType,
        string? requestUri,
        CancellationToken cancellationToken = default)
    {
        var imageHttpClientService = Ioc.Get<IImageHttpClientService>();
        return imageHttpClientService.GetImageAsync(requestUri, channelType, cancellationToken);
    }

    /// <inheritdoc cref="IImageHttpClientService.GetImageStreamAsync(string, string, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Stream?> GetImageStreamAsync(this IImageHttpClientService imageHttpClientService,
        string? requestUri,
        ImageChannelType channelType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(requestUri)) return Task.FromResult((Stream?)null);
        var channelType_ = channelType.ToString();
        return imageHttpClientService.GetImageStreamAsync(requestUri, channelType_, cancellationToken);
    }

    /// <inheritdoc cref="IImageHttpClientService.GetImageStreamAsync(string, string, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<Stream?> GetImageStreamAsync(this ImageChannelType channelType,
        string? requestUri,
        CancellationToken cancellationToken = default)
    {
        var imageHttpClientService = Ioc.Get<IImageHttpClientService>();
        return imageHttpClientService.GetImageStreamAsync(requestUri, channelType, cancellationToken);
    }
}