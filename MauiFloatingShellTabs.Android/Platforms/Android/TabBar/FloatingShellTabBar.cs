#if ANDROID
using Android.Content;
using Android.Content.Res;
using Android.Graphics.Drawables;
using Android.Views;
using AndroidX.Core.View;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;
using Microsoft.Maui.Hosting;
using System;
using AColor = Android.Graphics.Color;
using AOutline = Android.Graphics.Outline;
using AView = Android.Views.View;
using AViewGroup = Android.Views.ViewGroup;

namespace MauiFloatingShellTabs.Android;

public static class FloatingShellTabBarExtensions
{
    private static FloatingShellTabBarOptions _options = new();

    public static MauiAppBuilder UseFloatingShellTabBar(
        this MauiAppBuilder builder,
        Action<FloatingShellTabBarOptions>? configure = null)
    {
        _options = new FloatingShellTabBarOptions();
        configure?.Invoke(_options);

        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler(typeof(Shell), typeof(FloatingShellRenderer));
        });

        return builder;
    }

    internal static FloatingShellTabBarOptions Options => _options;
}

public sealed class FloatingShellTabBarOptions
{
    public string BackgroundColorHex { get; set; } = "#FFFFFF";
    public string SelectedItemBackgroundColorHex { get; set; } = "#F2F4F7";
    public string SelectedColorHex { get; set; } = "#111827";
    public string UnselectedColorHex { get; set; } = "#98A2B3";
    public float BarCornerRadiusDp { get; set; } = 28f;
    public float ItemCornerRadiusDp { get; set; } = 24f;
    public float ElevationDp { get; set; } = 4f;
    public int IconSizeDp { get; set; } = 22;
    public int BottomMarginDp { get; set; } = 24;
    public int HorizontalMarginDp { get; set; } = 16;
    public int VerticalPaddingDp { get; set; } = 6;
    public int HorizontalPaddingDp { get; set; } = 8;
    public int TransparentParentDepth { get; set; } = 5;
}

public sealed class FloatingShellRenderer : ShellRenderer
{
    public FloatingShellRenderer(Context context) : base(context)
    {
    }

    protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
    {
        return new FloatingBottomNavViewAppearanceTracker(this, shellItem, FloatingShellTabBarExtensions.Options);
    }
}

internal sealed class FloatingBottomNavViewAppearanceTracker : ShellBottomNavViewAppearanceTracker
{
    private readonly FloatingShellTabBarOptions _options;
    private bool _initialStylingApplied;

    public FloatingBottomNavViewAppearanceTracker(
        IShellContext shellContext,
        ShellItem shellItem,
        FloatingShellTabBarOptions options)
        : base(shellContext, shellItem)
    {
        _options = options;
    }

    public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
    {
        if (bottomView.Handle == nint.Zero)
            return;

        try
        {
            base.SetAppearance(bottomView, appearance);
            ApplyCustomStyling(bottomView, _options);

            if (_initialStylingApplied)
                return;

            bottomView.Post(() => ApplyCustomStyling(bottomView, _options));
            _initialStylingApplied = true;
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Floating tab bar error: {ex.Message}");
        }
    }

    private static void ApplyCustomStyling(BottomNavigationView bottomView, FloatingShellTabBarOptions options)
    {
        try
        {
            if (bottomView.Handle == nint.Zero)
                return;

            var density = bottomView.Resources?.DisplayMetrics?.Density ?? 1f;
            var barCornerRadius = options.BarCornerRadiusDp * density;

            bottomView.Background = CreateBarBackground(options, density);
            bottomView.OutlineProvider = new RoundedOutlineProvider(barCornerRadius);
            bottomView.ClipToOutline = true;
            ViewCompat.SetElevation(bottomView, options.ElevationDp * density);

            bottomView.ItemBackground = CreateItemBackground(options, density);
            bottomView.ItemRippleColor = ColorStateList.ValueOf(AColor.Transparent);
            bottomView.LabelVisibilityMode = LabelVisibilityMode.LabelVisibilityLabeled;
            bottomView.ItemHorizontalTranslationEnabled = false;
            bottomView.ItemActiveIndicatorEnabled = false;
            bottomView.LayoutTransition = null;

            var iconColorStates = new ColorStateList(
                new[]
                {
                    new[] { global::Android.Resource.Attribute.StateChecked },
                    Array.Empty<int>()
                },
                new[]
                {
                    AColor.ParseColor(options.SelectedColorHex).ToArgb(),
                    AColor.ParseColor(options.UnselectedColorHex).ToArgb()
                });

            bottomView.ItemIconTintList = iconColorStates;
            bottomView.ItemTextColor = iconColorStates;
            bottomView.ItemIconSize = (int)(options.IconSizeDp * density);

            if (bottomView.LayoutParameters is AViewGroup.MarginLayoutParams marginParams)
            {
                var desiredBottomMargin = (int)(options.BottomMarginDp * density);
                var desiredHorizontalMargin = (int)(options.HorizontalMarginDp * density);

                if (marginParams.BottomMargin != desiredBottomMargin ||
                    marginParams.LeftMargin != desiredHorizontalMargin ||
                    marginParams.RightMargin != desiredHorizontalMargin)
                {
                    marginParams.BottomMargin = desiredBottomMargin;
                    marginParams.LeftMargin = desiredHorizontalMargin;
                    marginParams.RightMargin = desiredHorizontalMargin;
                    bottomView.LayoutParameters = marginParams;
                }
            }

            var verticalPadding = (int)(options.VerticalPaddingDp * density);
            var horizontalPadding = (int)(options.HorizontalPaddingDp * density);
            if (bottomView.PaddingBottom != verticalPadding ||
                bottomView.PaddingTop != verticalPadding ||
                bottomView.PaddingLeft != horizontalPadding ||
                bottomView.PaddingRight != horizontalPadding)
            {
                bottomView.SetPadding(horizontalPadding, verticalPadding, horizontalPadding, verticalPadding);
            }

            var currentParent = bottomView.Parent as AViewGroup;
            var depth = 0;
            while (currentParent != null && depth < options.TransparentParentDepth)
            {
                currentParent.SetBackgroundColor(AColor.Transparent);
                currentParent.SetClipChildren(false);
                currentParent.SetClipToPadding(false);
                currentParent = currentParent.Parent as AViewGroup;
                depth++;
            }

            ResetAllItemViews(bottomView);
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Apply floating tab bar styling error: {ex.Message}");
        }
    }

    private static Drawable CreateBarBackground(FloatingShellTabBarOptions options, float density)
    {
        var background = new GradientDrawable();
        background.SetShape(ShapeType.Rectangle);
        background.SetColor(AColor.ParseColor(options.BackgroundColorHex));
        background.SetCornerRadius(options.BarCornerRadiusDp * density);
        return background;
    }

    private static Drawable CreateItemBackground(FloatingShellTabBarOptions options, float density)
    {
        var selected = new GradientDrawable();
        selected.SetShape(ShapeType.Rectangle);
        selected.SetColor(AColor.ParseColor(options.SelectedItemBackgroundColorHex));
        selected.SetCornerRadius(options.ItemCornerRadiusDp * density);

        var unselected = new GradientDrawable();
        unselected.SetShape(ShapeType.Rectangle);
        unselected.SetColor(AColor.Transparent);

        var background = new StateListDrawable();
        background.AddState(
            new[] { global::Android.Resource.Attribute.StateChecked },
            selected);
        background.AddState(Array.Empty<int>(), unselected);

        return background;
    }

    private static void ResetAllItemViews(BottomNavigationView bottomView)
    {
        try
        {
            if (bottomView.Handle == nint.Zero)
                return;

            var menuView = bottomView.GetChildAt(0) as AViewGroup;
            if (menuView == null)
                return;

            menuView.LayoutTransition = null;

            for (var i = 0; i < menuView.ChildCount; i++)
            {
                var child = menuView.GetChildAt(i);
                if (child == null)
                    continue;

                ResetViewState(child);

                if (child is not AViewGroup childGroup)
                    continue;

                childGroup.LayoutTransition = null;
                for (var j = 0; j < childGroup.ChildCount; j++)
                {
                    var innerChild = childGroup.GetChildAt(j);
                    if (innerChild != null)
                        ResetViewState(innerChild);
                }
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Reset floating tab bar items error: {ex.Message}");
        }
    }

    private static void ResetViewState(AView view)
    {
        view.ScaleX = 1f;
        view.ScaleY = 1f;
        view.TranslationX = 0f;
        view.TranslationY = 0f;
        view.Alpha = 1f;
        view.ClearAnimation();
    }

    private sealed class RoundedOutlineProvider : ViewOutlineProvider
    {
        private readonly float _radius;

        public RoundedOutlineProvider(float radius)
        {
            _radius = radius;
        }

        public override void GetOutline(AView? view, AOutline? outline)
        {
            if (view == null || outline == null)
                return;

            outline.SetRoundRect(0, 0, view.Width, view.Height, _radius);
        }
    }
}
#endif
