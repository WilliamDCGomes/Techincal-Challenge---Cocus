using Microsoft.Maui.Handlers;
#if ANDROID
using Android.Content.Res;
#endif

namespace WeatherApp.Handlers;

internal static class EntryHandlerCustomizations
{
    public static void Apply()
    {
        EntryHandler.Mapper.AppendToMapping("WeatherApp.NoUnderline", (handler, view) =>
        {
#if ANDROID
            handler.PlatformView.BackgroundTintList =
                ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#elif IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
        });
    }
}
