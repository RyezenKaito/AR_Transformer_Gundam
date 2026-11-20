# Playable offline gameplay systems

Open Assets/GundamLobby/Scenes/HomeLobby.unity and Play. The UI stays in English.

- Shop: browse 2 Gundam, 15 supply/equipment entries and 2 cosmetic designs; search, category, consumable/equipment tabs, rarity filter, sorting, quantities and purchases work. The old mock auctions/listings are replaced by Transactions and Item Guide. Credit Caches are reward-only; Shop offers two Crystal designs for the active Gundam.
- Inventory: shared persisted stacks, filtering, sorting, item descriptions, use/open/equip/unequip, full-stack sale, bulk sale/discard confirmation, expandable storage. F-Cores and equipped modules are protected from sale/discard. Old currency/skin/Gundam/storage saves are preserved.
- Laboratory > Update: levels 20 to 40. Each level consumes 1 Data Chip, 2 + floor(rank/3) F-Cores and 400 + 150*rank Credits. Each rank adds 5% base HP/ATK/DEF/Armor/Break; every 2 ranks adds 1 Speed. Bonuses actually reach combat fighters.
- Weaponry: actual four skills of selected combat profile, module information, Test in Training.
- Profile: saved callsign and progression overview. Mail: one-time starter delivery. Missions: four milestone rewards with one-time claims. Settings: volume and fullscreen persisted. COMMS: local transaction activity.
- Battle: Campaign -> Chapter -> mission briefing/rewards -> Deploy is retained. AI Skirmish is local; Challenge scales enemy HP/ATK/damage to 125% and rewards to 150%; Training/Sandbox have zero rewards and do not mark campaign progress. Available battlefields remain the existing two stages. Chapters without content stay locked. No multiplayer server or real-money payment processing was added.
- Sorties no longer use Energy. All modes can be entered without a stamina cost. Only Credits and Crystals appear in the wallet. Old Reactor Cells are converted once to 250 Credits each.

| Item | Use |
|---|---|
| F-Core | Permanent Gundam upgrades with Data Chip and Credits |
| Combat Data Chip | One consumed per upgrade |
| Credit Cache | Open for 750 Credits |
| Maintenance Crate | 3 Data Chips + 1 Credit Cache + 2 F-Cores |
| Assault Circuit | Reusable module: +15% ATK |
| Aegis Plating | Reusable module: +15% HP and +20% Armor |
| Vector Thruster | Reusable module: +10 Speed |

One module can be mounted on each of seven body parts. Equipping does not consume an item. Crystals remain cosmetic currency for the existing two premium skins.

Item artwork is rendered from seventeen native mechanical models with machined bevels. Replace model prefabs or sprites in MechaRefresh/Resources/MechaItemVisuals.asset. UI panels, text and controls remain editable uGUI elements. Inventory keeps the large Gundam preview and compact floating item details.

Balance definitions: Progression/Runtime/GameItems.cs, Progression/Runtime/EconomyRules.cs and Resources/GundamEconomyRules.asset. Shop catalog is serialized in the existing Shop catalog asset. Save key remains Gundam.Progression.v1; schema 3 migrates old saves and grants a small starter kit once. No user balance is reset.

Validation: 27 isolated transaction checks, 19 real UI checks, plus Play Mode visual inspection and a live mission/energy verification. Transaction tests restore the original player save in finally.

## Additional useful items

| Item | Effect |
|---|---|
| F-Core Container | Opens for 5 F-Cores |
| Tactical Data Archive | Opens for 10 Combat Data Chips |
| Treasury Vault | Reward-only pack: 2,500 Credits |
| Frontline Supply Cache | 4 F-Cores, 4 Data Chips and 1 Treasury Vault |
| Breach Coupler | Reusable module: +30% Break Damage |
| Guard Matrix | Reusable module: +20% DEF |
| Precision Array | Reusable module: +20% ATK, -5 Speed |
| Seraph Controller | Reusable module: +8% HP/ATK/DEF, +4 Speed |
| Nano Repair Gel | Reserve +15% max HP for the next sortie |
| Overdrive Ampoule | Reserve +12% ATK for the next sortie |

Mail > Armory Field Trial Delivery grants a one-time kit: 1 Core Container, 1 Data Archive, 2 Gels and 2 Ampoules. No existing currency or storage is reset.

A Gundam has one equipment module and one prepared supply. Preparation reserves an owned copy; it can be cancelled or replaced without spending it. The copy is consumed once when a real Campaign/Skirmish/Challenge formation starts. Training and Sandbox apply the boost without consuming the reserved copy. The bonus clears when leaving combat. Armed supplies and equipped modules are protected from bulk sale/discard.

Item Guide scrolls through all 17 types. Containers check capacity for their complete contents while accounting for the container slot freed by opening.


## Body-part equipment

Laboratory > Weaponry opens LOADOUT. Click a numbered mount or the head, torso, either arm, either leg or backpack on the Gundam. Drag to rotate; scroll to zoom. SKILLS retains the four combat skills. Select a compatible module, inspect NOW > PREVIEW, then EQUIP PART. RESTORE STOCK removes only that part and returns the reusable copy to storage. GET MODULES opens Shop; Inventory module actions return to this picker. CLAIM ENGINEERING KIT gives a one-time set of seven module copies.

Each Gundam stores seven independent mounts. A copy can be linked to only one mount within that Gundam; owning two copies allows two mounts. Saved loadouts may reuse copies across Gundam because only one Gundam deploys at a time. Linked gear is protected from selling/discarding until removed from every loadout. Existing single-module saves migrate into the appropriate part. Percent bonuses add across mounts, then multiply the permanent rank bonus; Speed bonuses add. Combat uses the same stats as the preview. Credits, Crystals, upgrades and preparations keep their existing behavior.

Compatibility and effects: GundamEquipment.cs. Mount bones, proportions and visual module attachments: GundamPartRig.cs. WeaponryLoadout owns the editable uGUI/TMP interface; its EnsureUi context is generated into HomeLobby. This installs visible module add-ons; it does not replace the original limb meshes.
