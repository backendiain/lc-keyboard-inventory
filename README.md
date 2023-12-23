# Lethal Company KeyboardInventory
A simple mod that rebinds the top number keys `1-4` to change the currently held item to one of the inventory slots (1-4).

The emotes are currently bound to `-` and `=`.

Hiting `*` on your numpad will bring up a mod menu to enable/disable the mod (todo).

## Overview
I really only made this as a quick way to tinker in LC and Unity. There are no doubt better ways to do even what little I've done here.

For example, I'm using Unity's legacy input system just to see the implementation differences.

If you want to play with the source code, just nab the libraries from your Lethal Company
`Managed Code` directory and plank them in `libs`.

## Installation
### Thunderstore Install
Install the package using the Thunderstore App or any other mod manager that is compatible with Thunderstore.

The mod has [BepInEx](https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/) as a dependency in order to run.

Thunderstore listing [here](https://thunderstore.io/c/lethal-company/p/backendiain/KeyboardInventory/).

### Manual Install
#### If you do not already have BepInEx

* Download this version of [BepInEx 5](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.22).
* Extract the contents of this `.zip`-File into your Lethal Company directory.
* Start the game once and then quit after reaching the main screen.

#### Install the mod

* Download the latest release from the [Releases](https://github.com/backendiain/lc-keyboard-inventory/releases) tab and
  put those DLLs into your `BepInEx\plugins` folder under
  `steamapps/common/<lethal company root>/BepInEx`.
* Inside the game folder, there should now be a folder called `BepInEx`, and inside should be a folder called
  `plugins`.
  
If you run into problems post into the repo's [Issues](https://github.com/backendiain/lc-keyboard-inventory/issues) section.

## To-Do
* Figure out a way to disable the mod by toggle, as the GUI toggle control doesn't seem to update base mod class enabled prop val
* Add a key bindings menu for the mod where you can reset the key bindings to whatever works for you.

## Acknowledgements
I just flat out missed some of the private methods in dotPeek of PlayerControllerB I could call to achieve the item switches.

The [ItemQuickSwitchMod](https://github.com/vasanex/ItemQuickSwitchMod) by vasanex enlightened me on how best to achieve this so special thanks to him.

The [LethalCompanyGameMaster](https://github.com/lawrencea13/LethalCompanyGameMaster) repo lawrencea13 was also beneficial.

Check out the Lethal Company [topic](https://github.com/topics/lethal-company) and [wiki](https://lethal.wiki/) for more examples.