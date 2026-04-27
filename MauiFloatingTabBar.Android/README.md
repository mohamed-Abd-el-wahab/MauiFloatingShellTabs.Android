# MauiFloatingTabBar.Android

Reusable NuGet package for adding an Android floating Shell tab bar to another .NET MAUI app.

Repository: https://github.com/mohamed-Abd-el-wahab/MauiFloatingTabBar.Android

## Folder layout

```text
MauiFloatingTabBar.Android/
  Platforms/
    Android/
      Resources/
        values/
          styles.xml
      TabBar/
        FloatingShellTabBar.cs
  snippets/
    MainActivity.cs.snippet
    MauiProgram.cs.snippet
```

## Build the package

```bash
dotnet build MauiFloatingTabBar.Android/MauiFloatingTabBar.Android.csproj -c Release
```

The `.nupkg` is written to:

```text
LocalPackages
```

## Consume in another app

1. Add `reusable/LocalPackages` as a local NuGet source (or install from nuget.org).
2. Add the package only for Android.
3. Apply the `snippets/MainActivity.cs.snippet` change.
4. Apply the `snippets/MauiProgram.cs.snippet` change.

Example `csproj`:

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net10.0-android'">
  <PackageReference Include="MohamedAbdelwahab.FloatingTabBar.Android" Version="1.0.0" />
</ItemGroup>
```

## Required result

- The app keeps `@style/Maui.SplashTheme` on `MainActivity`
- `MainActivity.OnCreate()` calls `SetTheme(Resource.Style.FloatingShellTabsMaterialTheme);` before `base.OnCreate(...)`
- `MauiProgram` calls `builder.UseFloatingShellTabBar()` on Android

## Optional customization

Inside `FloatingShellTabBar.cs`:

- `BackgroundColorHex`
- `SelectedItemBackgroundColorHex`
- `SelectedColorHex`
- `UnselectedColorHex`
- spacing, radius, and icon size options

Example:

```csharp
#if ANDROID
builder.UseFloatingShellTabBar(options =>
{
    options.BackgroundColorHex = "#FFFFFF";
    options.SelectedItemBackgroundColorHex = "#F3F4F6";
    options.SelectedColorHex = "#111827";
    options.UnselectedColorHex = "#9CA3AF";
});
#endif
```

## What the package includes

- The `MauiFloatingTabBar.Android` assembly
- A transitive Android resource file that adds `FloatingShellTabsMaterialTheme`

## Notes

- This package is Android-only.
- Reference it only for the Android target in multi-target MAUI apps.
- It expects the app to use `Shell` with a `TabBar`.
