# Spawner Tweaks

Changes the game logic to allow modifying altar, pickable, spawn point and spawner behavior.

Install on all clients and on the server (modding [guide](https://youtu.be/L9ljm2eKLrk)).

# Usage

This mod only provides game logic.

To modify behavior, you need [World Edit Commands](https://valheim.thunderstore.io/package/JereKuusela/World_Edit_Commands/).

# Config

- Components: Altars, pickables, spawn points and spawners can be attached to any object.
- Boss altars: Boss altar properties can be overridden.
- Chests: Chest properties can be overridden.
- Item stands: Item stand properties can be overridden.
- Pickables: Pickable properties can be overridden.
- Spawn points: Spawn point properties can be overridden.
- Spawners: Spawner properties can be overridden.
- No spawn point suppression (one time): One time spawn points can't be suppressed with player base structures (even if configured to be respawning).
- No spawn point suppression (respawning): Respawning spawn points can't be suppressed with player base structures (even if configured to be one time). This is off by default because it can affect vanilla game play.

# Credits

Thanks for Azumatt for creating the mod icon!

Sources: [GitHub](https://github.com/JereKuusela/valheim-spawner_tweaks)

Donations: [Buy me a computer](https://www.buymeacoffee.com/jerekuusela)

# Changelog

- v1.6
	- Adds support for changing faction, health and level of all spawner creatures.
	- Fixes CLLC overriding spawner levels.
	- Fixes the spawn condition not working for spawners.

- v1.5
	- Fixes the black screen.

- v1.4
	- Adds support for chests.
	- Adds support for item stands.

- v1.3
	- Adds support for faction overrides.

- v1.2
	- Fixes an issue with the Marketplace mod.
	- Fixes pickables showing error if name and dropped item is not defined.

- v1.1
	- Fixes trigger distance not working for spawners.
	- Fixes error when spawning non-creatures from spawners.
	- Fixes the default weight for spawners being 0.
	- Fixes pickables.
