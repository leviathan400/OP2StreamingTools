# OP2StreamingTools

A lightweight overlay for recording and streaming **Outpost 2: Divided Destiny** (a 1997 real-time strategy game). It reads live game state directly from the running `Outpost2` process memory and surfaces it in a compact, always-on-top overlay plus a live graph — handy for on-stream stat displays and post-game review.

> Project home: https://github.com/leviathan400/OP2StreamingTools

## Features

- **Compact overlay** (`fMain`) — borderless, always-on-top, draggable window styled in the classic OP2 green-on-black. Shows:
  - Game timer (elapsed play time)
  - Current game mark
  - Morale
  - Total colonists
- **Live colony graph** (`fGraphs`) — "OP2 Colony Graph" window plotting morale (left axis) and total colonists (right axis) against game mark, refreshed every 500ms.
- **Debug log window** (`fLog`) — running activity log (game start/reset events, etc.).
- **Automatic game detection** — polls process memory; auto-starts tracking when a game begins and resets history when a new game is detected (game mark rolls back).

## How it works

The app reads values from the running `Outpost2.exe` process using Windows `ReadProcessMemory` (see [`MemoryReader`](OP2StreamingTools/MemoryReader.cs)). Game state is sampled on a 500ms timer in `fMain.UpdateValues()` from known module-relative offsets — for example:

| Value           | Source                                  |
| --------------- | --------------------------------------- |
| Game tick       | absolute address `0x0056EB1C`           |
| Total colonists | `Outpost2` module offset `0x16EFFC`     |
| Morale          | `Outpost2` module offset `0x16EF5C`     |

Game mark is derived as `gameTick / 100`. As the game progresses, morale / colonist / mark values are appended to in-memory history lists, which the graph reads via `GetMoraleHistory()`, `GetColonistsHistory()`, and `GetGameMarkHistory()`.

## Requirements

- Windows
- .NET Framework 4.8
- Outpost 2: Divided Destiny installed and running (process name `Outpost2`)

> **Note:** Because the tool reads another process's memory, the memory offsets are specific to a particular build of Outpost 2. If values read as `N/A` or look wrong, the offsets may not match your game version.

## Building

Open `OP2StreamingTools.sln` in Visual Studio and build (Debug or Release), or from the command line:

```powershell
msbuild OP2StreamingTools.sln /p:Configuration=Release
```

The Debug build outputs to `..\..\build\`; the Release build outputs to `OP2StreamingTools\bin\Release\`.

## Usage

1. Launch **Outpost 2** and start (or load) a game.
2. Run **OP2StreamingTools.exe**.
3. Three windows appear: the overlay, the colony graph, and the log. Drag the overlay anywhere over your capture area.
4. Start playing — tracking begins automatically once the game mark advances, and the graph fills in over time.

## Project layout

| File | Purpose |
| ---- | ------- |
| [`fMain.cs`](OP2StreamingTools/fMain.cs) | Overlay window; memory polling, game state tracking, history collection |
| [`fGraphs.cs`](OP2StreamingTools/fGraphs.cs) | Live morale / colonists chart |
| [`fLog.cs`](OP2StreamingTools/fLog.cs) | Debug log window |
| [`MemoryReader.cs`](OP2StreamingTools/MemoryReader.cs) | Static helpers for reading ints/strings from external process memory |
| [`Program.cs`](OP2StreamingTools/Program.cs) | Application entry point |
