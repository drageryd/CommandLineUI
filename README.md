# CommandLineUI

This is a prefab for unity for enabling a bash-like command line interface for a game.

## Setup

- Import `CommandLineUI` to your Unity Assets directory
- Drag Prefab to scene
- Add UI Eventsystem
- Set some publics under prefab scripts
  - Command buffer size (e.g `100`)
  - Marker (e.g  [Full block](https://en.wikipedia.org/wiki/Block_Elements))
  - Home directory (e.g `<path-to-repo>/workingdir`)
- Play and toggle command line using `escape`