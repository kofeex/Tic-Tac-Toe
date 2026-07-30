# Tic-Tac-Toe

A local-multiplayer (hot-seat) tic-tac-toe game for mobile, built in Unity 6 with the 2D URP template.
Two players share one device, alternating X and O until someone gets three in a row.

Both **Portrait and Landscape** are supported and switch freely at runtime — no separate layouts, no
reload.

---

## Running the project

- **Unity 6000.0.71f1** (2D URP template). Open the project folder with that editor version.
- Open `Assets/_Project/Scenes/MainMenu.unity` and press Play. Any scene can be entered directly —
  the managers bootstrap themselves (see [Architecture](#architecture)), so pressing Play on
  `Game.unity` works too.
- Build settings: `MainMenu` is index 0, `Game` is index 1.

**Tests**: `Window → General → Test Runner → EditMode → Run All`. 77 tests covering the board rules,
match statistics and the theme catalogues.

---

## Features

### Main Menu
| Button | Behaviour |
| --- | --- |
| **Play** | Theme popup — each player picks the artwork for their own mark, then **Start** loads the game |
| **Stats** | Games played, Player 1 / Player 2 wins, draws, and average match duration |
| **Settings** | Background music and SFX toggles |
| **Exit** | Confirmation popup, then quits |

### Game
- 3×3 grid, players alternate placing X and O.
- Three in a row ends the match with an animated **strike line** sweeping through the winning cells,
  drawn in the winner's own theme colour, with a particle flash.
- **HUD**: live match duration, both players' move counts, and a Settings button (the same popup as
  the menu).
- **Game Over popup**: the result and the match duration, with **Retry** and **Exit** buttons.

### Audio
Looping background music, plus SFX for button clicks, mark placement, the strike, and popup
animations. Music and SFX are routed through separate mixer groups so each can be muted
independently; both toggles persist.

### Persistence
Statistics, audio settings and theme choices are saved as JSON in `Application.persistentDataPath`
(`stats.json`, `settings.json`, `theme.json`) and survive app restarts.

---

## Architecture

The guiding rule: **game rules know nothing about Unity, and content is data rather than code.**

### Rules layer — `TicTacToe.Core`
`Board`, `Mark`, `GameStatus` and `MatchStatistics` are plain C# with **no `UnityEngine`
dependency**. That keeps them unit-testable in isolation and independent of any scene setup.

`Board` is written for **N×N** boards with a configurable win length, and ships configured as 3×3.
Win detection scans four directions from each cell rather than checking a hardcoded list of eight
winning triples, so a 5×5 board needing 4-in-a-row already works — `GameController._boardSize` is a
serialized field, and `BoardView` builds its grid at runtime from whatever size the board reports.

### Presentation — `TicTacToe.Gameplay`, `TicTacToe.UI`
`BoardView` is a pure view: it builds the cell grid, draws marks, animates the strike, and reports
clicks through a callback. It never asks who won. `GameController` owns the `Board`, feeds clicks
into it, and raises a `StateChanged` event that the HUD listens to — no polling, no cross-references
between UI components.

`Popup` is a shared base class handling the fade and scale-overshoot animation; every popup in the
game (theme, stats, settings, exit, game over) derives from it and only implements its own content.
Adding a new popup means a prefab and a small subclass.

### Data — `TicTacToe.Themes`
Themes are **ScriptableObjects**, not code. `MarkTheme` holds one mark's id, display name, sprite and
strike colour; `XMarkTheme` and `OMarkTheme` subclass it purely so Unity's object picker refuses a
mis-dragged asset. `ThemeLibrary` (in `Resources/`) holds a separate catalogue per mark.

The two catalogues are independent, so **X and O can offer different numbers of looks**. The theme
popup builds both dropdowns at runtime from the library, sizing the open list from the real option
count and scrolling past a cap — so *adding a theme is adding an asset*, with no UI work and no
layout retuning.

### Cross-cutting managers
`AudioManager` and `SceneFader` are bootstrapped from `Resources/` prefabs via
`[RuntimeInitializeOnLoadMethod]` and marked `DontDestroyOnLoad`. This is why any scene can be
entered directly in the editor, and why `SceneLoader` (which lives on prefabs) never needs a scene
object reference — prefabs cannot store one.

`JsonFileStore` is a small generic load/save helper shared by all three persisted preference types.

---

## Project layout

```
Assets/_Project/
  Art/Sprites/      XO.psd (sliced into the mark artwork), popup and button sprites
  Art/VFX/          Particle textures and materials
  Audio/            Music, SFX, and the audio mixer
  Data/             ScriptableObjects — AudioLibrary, Themes/
  Prefabs/          Gameplay/ (VFX), UI/ (popups)
  Resources/        Runtime-loaded: AudioManager, SceneFader, ThemeLibrary
  Scenes/           MainMenu.unity, Game.unity
  Scripts/          asmdef `TicTacToe` (+ `TicTacToe.Tests`)
    Core/           Pure C# rules and statistics — no UnityEngine dependency
    Gameplay/       GameController, BoardView, MatchHud
    UI/             Popup base + screens, scene transitions, button feedback
    Audio/          AudioManager, AudioLibrary, preferences
    Themes/         MarkTheme, ThemeLibrary, ThemeService
    Persistence/    JsonFileStore, StatisticsService
    Tests/          EditMode tests
  Settings/         URP assets, input actions
```

---

## Conventions

- Namespaces `TicTacToe.*` mirror the folder layout; one public type per file.
- `[SerializeField] private` fields over public fields; `_camelCase` privates, PascalCase publics.
- C# events for cross-system communication — no scattered singletons; managers bootstrap from a
  single entry point.
- uGUI + TextMeshPro with anchor-based responsive layout, a `CanvasScaler` (1080×1920 reference) and
  safe-area handling.
- Coroutines on unscaled time for UI animation, so transitions stay responsive regardless of
  timescale.
