# M'Organs

AR anatomy app for Android and iOS. Point your phone at the floor, tap, and a life-size 3D organ appears in your room. Built as a student project to make anatomy more tangible than a textbook diagram.

The app covers heart, brain, and stomach — each with a detail screen, a fact table, and a short quiz. UI is fully bilingual (Indonesian/English) with three color themes and haptics support.

## Screenshots

| Splash | Onboarding | Picker | Detail | AR |
|--------|------------|--------|--------|----|
| <img src="Screenshots/01_Splash.png" width="150"> | <img src="Screenshots/02_Onboarding.png" width="150"> | <img src="Screenshots/03_Picker.png" width="150"> | <img src="Screenshots/04_Detail.png" width="150"> | <img src="Screenshots/05_AR.png" width="150"> |

## Tech Stack

- **Engine:** Unity 6 (URP)
- **AR:** AR Foundation + ARCore (Android) / ARKit (iOS)
- **UI:** TextMeshPro, custom procedural mesh graphics (HatchGraphic, CornerBracketsGraphic)
- **Architecture:** Singleton managers (AppState, ThemeManager, LocalizationManager, SceneLoader), ScriptableObject-driven content (OrganDefinition, MO2ThemeData)
- **Language:** C#

Content and theme data live in ScriptableObjects so organ facts, quiz questions, and color palettes can be edited in the Unity inspector without touching code.

## Project Structure

```
Assets/
  _Project/         ← All custom project code
    AR/             ← AR bridge and placement logic
    Core/           ← App state, localization, theming, scene loading
    Data/           ← ScriptableObjects (organ definitions, theme data)
    Fonts/          ← Inter, Fraunces, SF Mono + generated TMP assets
    Icons/          ← SVG-sourced organ line-art sprites
    Prefabs/        ← UI component prefabs
    Scenes/         ← 8 scenes (_Persistent through 07_Settings)
    UI/             ← Screen controllers + reusable components
  Editor/           ← Custom build tools (scene builder, font builder, etc.)
  Organs/           ← 3D organ FBX models (Heart, Brain, Stomach)
```

## Scenes

| Scene | Purpose |
|-------|---------|
| _Persistent | DontDestroyOnLoad managers (loads first, stays loaded) |
| 01_Splash | App entry |
| 02_Onboarding | First-launch walkthrough |
| 03_Picker | Organ selection |
| 04_Detail | Facts, fun fact, navigation to AR and quiz |
| 05_AR | AR placement, pinch to scale, drag to reposition |
| 06_Quiz | 3-question quiz per organ |
| 07_Settings | Language, theme, sound, haptics |

## Getting Started

1. Clone the repository
2. Download Vuforia Engine 11.4.4 from the [Vuforia Developer Portal](https://developer.vuforia.com/downloads/sdk) and place `com.ptc.vuforia.engine-11.4.4.tgz` inside the `Packages/` folder
3. Open in Unity 6 (6000.0.x or later) — Unity will resolve the package automatically
4. Open `Assets/_Project/Scenes/_Persistent.unity` and press Play, or build for Android/iOS

AR requires a physical device with ARCore (Android 8.0+) or ARKit (iOS 11+). The AR scene will fall back to in-editor placement for testing.

## License

MIT
