[Beamable Docs](https://docs.beamable.com/docs)

![Battling against bots in HATS](./images/hats_promo.gif)

# Beamable HATS
HATS is a turn based game multiplayer built on Beamable technology. Up to 4 players can battle on a hexagonial grid, compete on the leaderboard, earn rewards, and customize their characters. HATS demonstrates the following Beamable features.
- [Content Management](https://docs.beamable.com/docs/content-feature)
- [Multiplayer](https://docs.beamable.com/docs/multiplayer-feature)
- [Inventory](https://docs.beamable.com/docs/inventory-feature)
- [Commerce](https://docs.beamable.com/docs/store-feature)
- [Leaderboards](https://docs.beamable.com/docs/leaderboards-feature)
- [Accounts](https://docs.beamable.com/docs/accounts-feature)

This project is meant to be a sample for your own multiplayer turn based game. You should fork this project and do whatever you like with it. You could change out the assets, modify the game rules, flip the game upside-down, or use the source code as inspiration for a brand new game. Its completely up to you. 

At the moment, there are some simple rules...
* 4 players start the match
* You get 10 seconds to commit a move
* You can move in any open direction. If you step in lava, you'll die. If you step on ice, you'll slide across it. 
* You can cast a fireball in any open direction
* You can throw an arrow in any open direction
* You can cast a shield around yourself for a turn. Shields reflect fireball attacks back to their caster. 
* If you get hit with a fireball or an arrow, you'll die.
* Eventually, tiles under players start to turn to lava! Keep on the move!
* Last player left alive gets the most points.

![Hats Screenshots](./images/hats.png)

# Getting Started
Follow these steps to build the game and run it locally. 
- Open the root folder as a Unity Project. Unity 2020.3.11 (LTS) is recommended. 
- Create a new Beamable Customer account. HATS already has the Beamable package installed, but no customer account has been set up. Use the [Beamable Docs](https://docs.beamable.com/docs/getting-started#usage) to learn how to create a new account.
- You may need to rebuild the [Addressable Asset Groups](https://docs.unity3d.com/Packages/com.unity.addressables@1.4/manual/AddressableAssetsGettingStarted.html). 
    >To build content in the Editor, open the Addressables Groups window, then select Build > New Build > Default Build Script
- Run the Matchmaking scene. 

# Making it your own
You can use HATS however you'd like to create your own game. Here are some ideas to get you going...
- Change out the game assets to create a scifi or fantasy theme.
- Add more characters, hats, or tile types
- Add in-game pickups that give your player powerups and new attacks
- Make the game 3D!

If you want to learn how various social features are implemented in HATS, check out the sections below. 

## Multiplayer
HATS uses a deterministic simulation networking model. All network messages get sent to a [GameSimulation class](./Assets/Scripts/Simulation/GameSimulation.cs). The code runs on every player's machine, and produces the same game outputs. The simulation code creates a set of [Game Events](./Assets/Scripts/Simulation/HatsGameEvent.cs). That sequence of events can be consumed with MonoBehaviours or other classes to create rich onscreen visuals and sounds. 
![A design diagram for HATS](./images/hats_networking_arch.png)

A player's input move is sent to a central Beamable Game Relay server, where it is then rebroadcast to all connected players. If you submit a move, you can expect to see your own move show up as a network message. Those network messages are sent to the `GameSimulation`, and converted into a sequence of `GameEvent`s. Check out the [PlayerMoveBuilder](./Assets/Scripts/Game/PlayerMoveBuilder.cs) if you want to change how network messages are created for each player.

The `GameEvent`s are handled by `GameEventHandler`s, which are subclasses of MonoBehaviours. You can create your own subclass of a `GameEventHandler`, and implement the methods you care about. Check out the [PlayerController](./Assets/Scripts/Game/PlayerController.cs) for an example. Each event that you want to repond to runs inside of a Unity Coroutine. You can play animations, sounds, or add pauses into the game. The Game Simulation won't continue until you invoke the `completeCallback` argument on each event method. 

If you want to change how the [GameSimulation](./Assets/Scripts/Simulation/GameSimulation.cs) works in general, you can create new `GameEvent`s, or change how the logic works. Anytime the `PlayGame()` method yield returns a `GameEvent`, the [GameProcessor](./Assets/Scripts/Game/GameProcessor.cs) has a chance to broadcast it to any listening `GameEventHandler`s. 

## Leaderboards
The [LeaderboardScreenController](.Assets/Scripts/Game/UI/LeaderboardScreenController.cs) class is where you should look to see Beamable's leaderboard SDK. Scores on the leaderboard set via the Multiplayer Game Relay server. The Beamable server awards scores to players based on their scores. Scores are calculated in the [GameSimulation.CalcualteScore() method](./Assets/Scripts/Simulation/GameSimulation.cs#L254) 

Checkout the [Beamable Leaderboard Docs](https://docs.beamable.com/docs/leaderboards-feature) to learn more.

## Inventory 
In HATS, players can spend earned gems to buy new characters and hats. In this game, the characters and hats don't have any effect on the gameplay. Characters and Hats are subtypes of Beamable's `ItemContent`. You can take a look at the [CharacterContent](./Assets/Scripts/Content/CharacterContent.cs) and [HatContent](./Assets/Scripts/Content/HatContent.cs) classes. The Content is managed through the [Beamable Content Manager](https://docs.beamable.com/docs/content-manager). 

In the game, there is one scene that shows what characters and hats a player has in their inventory, and what items are still available for purchase. Check out the [Character Panel Controller](./Assets/Scripts/Game/UI/CharacterPanelController.cs) class for details. 

# Development Requirements
If you clone this repository, you may want to get [Git LFS](https://dzone.com/articles/git-lfs-why-and-how-to-use#:~:text=Git%20LFS%20is%20an%20open,binary%20files%20into%20your%20repository.&text=An%20update%20of%20a%20binary,to%20the%20file%20are%20stored.) installed before you make any commits. It isn't required.
