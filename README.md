[Beamable Docs](https://docs.beamable.com/docs)

![Battling against bots in HATS](./images/hats_promo.gif)

# Beamable HATS
HATS is a turn based game multiplayer built on Beamable technology. Up to 4 players can battle on a hexagonial grid, compete on the leaderboard, earn rewards, and customize their characters. HATS demonstrates the following Beamable features.
- Content Management
- Multiplayer
- Inventory
- Commerce
- Leaderboards
- Accounts

This project is meant to be a sample for your own multiplayer turn based game. You should fork this project and do whatever you like with it. You could change out the assets, modify the game rules, flip the game upside-down, or use the source code as inspiration for a brand new game. Its completely up to you. 

# Getting Started
Follow these steps to build the game and run it locally. 
- Open the root folder as a Unity Project. Unity 2020.3.11 (LTS) is recommended. 
- Create a new Beamable Customer account. HATS already has the Beamable package installed, but no customer account has been set up. Use the [Beamable Docs](https://docs.beamable.com/docs/getting-started#usage) to learn how to create a new account.
- Run the Matchmaking scene. 

# Making it your own
You can use HATS however you'd like to create your own game. If you want to learn how various social features are implemented in HATS, check out the sections below. 

## Multiplayer
HATS uses a deterministic simulation networking model. All network messages get sent to a [GameSimulation class](./Assets/Scripts/Simulation/GameSimulation.cs). The code runs on every player's machine, and produces the same game outputs. The simulation code creates a set of [Game Events](./Assets/Scripts/Simulation/HatsGameEvent.cs). That sequence of events can be consumed with MonoBehaviours or other classes to create rich onscreen visuals and sounds. 
![A design diagram for HATS](./images/hats_networking_arch.png)

A player's input move is sent to a central Beamable Game Relay server, where it is then rebroadcast to all connected players. If you submit a move, you can expect to see your own move show up as a network message. Those network messages are sent to the `GameSimulation`, and converted into a sequence of `GameEvent`s. Check out the [PlayerMoveBuilder](.blob/main/Assets/Scripts/Game/PlayerMoveBuilder.cs) if you want to change how network messages are created for each player.

The `GameEvent`s are handled by `GameEventHandler`s, which are subclasses of MonoBehaviours. You can create your own subclass of a `GameEventHandler`, and implement the methods you care about. Check out the [PlayerController](.blob/main/Assets/Scripts/Game/PlayerController.cs) for an example. 

If you want to change how the [GameSimulation](./Assets/Scripts/Simulation/GameSimulation.cs) works in general, you can create new `GameEvent`s, or change how the logic works. Anytime the `PlayGame()` method yield returns a `GameEvent`, the [GameProcessor](./Assets/Scripts/Game/GameProcessor.cs) has a chance to broadcast it to any listening `GameEventHandler`s. 




# Development Requirements
If you clone this repository, you may want to get [Git LFS](https://dzone.com/articles/git-lfs-why-and-how-to-use#:~:text=Git%20LFS%20is%20an%20open,binary%20files%20into%20your%20repository.&text=An%20update%20of%20a%20binary,to%20the%20file%20are%20stored.) installed before you make any commits. It isn't required.