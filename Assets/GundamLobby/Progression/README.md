# Campaign, rewards and customization

Open Assets/GundamLobby/Scenes/HomeLobby.unity and press Play.
Battle > Deploy opens the original AR chapter UI. Choose Chapter 1, then Stage 1-1
or 1-2, review the rewards and press Deploy to start that selected battle.

Victory grants Credits and F-Core automatically, with one claim per completed battle.
Stage 1-1: 1,200 Credits + 2 F-Core. Stage 1-2: 1,800 Credits + 3 F-Core.
Credits buy Gundam ownership in Shop or Switch Gundam and unlock inventory slots.
F-Core is stored as a stack in Inventory for future Gundam upgrades.
Crystals are the premium cosmetic balance and unlock skins in Strike Design.
The current prototype uses local game balances; no real payment is submitted.

Inventory starts with 10 available slots, including a reserved F-Core stack.
There are 40 slots total. Locked slots and Unlock Storage open a purchase confirmation.
The first extra slot costs 1,000 Credits; each further slot adds 250 Credits.
An insufficient balance leaves both capacity and wallet unchanged.

Strike Design has three editable color designs per Gundam. Factory Original is free,
Midnight Sentinel costs 300 Crystals and Crimson Comet costs 450 Crystals.
Purchased designs persist and apply to the lobby, inventory preview and battle player.
Existing equipped Gundam ownership is preserved when creating a new progression save.

Editable balance:
Assets/GundamLobby/Progression/Resources/GundamEconomyRules.asset
Stage reward values: the imported StoryStage assets in ARCombat.
The original AR project is unchanged. Progression is saved locally to PlayerPrefs key
Gundam.Progression.v1. Test data is restored after verification.

Backups and verification source are in the workspace Progression20261119 folder.
