# ArenaByte (Prototype)

Lightweight multiplayer arena shooter prototype built for the Gape Labs Game Developer Assignment.

## Unity Version

- **6000.0.58f2**

## Networking

- **Photon PUN 2**
- Quick Match:
  - Play button connects to Photon, then **JoinOrCreateRoom**
  - Minimum **2 players** to start the match
  - **No backend login**, a PlayerPrefs user ID is generated
  - Movement sync uses thresholds + send interval (not every frame)
  - Synced gameplay variable: **Health**

## Gameplay

- Camera: orbit/follow, mouse drag look
- Movement: virtual joystick (Joystick Pack)
- Shooting:
  - Crosshair (screen center) aiming
  - Projectile visuals are pooled
  - Damage is applied with a travel delay to match projectile visuals
- Health / Respawn:
  - Respawn after ~2 seconds
- Score:
  - +1 score on kill
  - Game ends when a player reaches **maxScore** (default: 5)
  - Game over popup shows **YOU WIN / YOU LOSE**

## Addressables

- **Addressable prefab:** Projectile visual
- Loaded asynchronously via `Addressables.LoadAssetAsync`
- Projectile pool initializes only after the Addressable loads
- Visible in the demo via a simple “loaded” marker

## AssetBundle (Cloud Hosted)

- **AssetBundle content:** Ground prefab
- Downloaded at runtime using `UnityWebRequest`
- Loaded from bytes using `AssetBundle.LoadFromMemoryAsync`
- Instantiated in the scene
- Player spawn is delayed until ground exists (prevents falling)
- Url: https://github.com/DeepanshuManocha/GroundAssetBundle/raw/refs/heads/main/groundassetbundle

## Lightweight Optimization

- Remote players do not run local input/motor logic
- Projectile pooling (no Instantiate/Destroy per shot)
- Reduced network sync frequency:
  - sendInterval + movement thresholds
- Texture compression example:
  - One texture uses Android platform override with **ASTC/ETC2**

## Error Handling

Simple popup + Retry for:

- Network join / disconnect errors
- Addressables load failure
- AssetBundle download/load failure

## Scenes

- `MainMenu`: player name input + Play button
- `Game`: arena gameplay, HUD, Addressables + AssetBundle runtime loading

## How to Run (2 clients)

1. Play from `MainMenu`.
2. Enter player name, click Play.
3. Run a second client (standalone build).
4. When 2 players are in the room, the game loads `Game` (scene sync enabled).

## Controls

- Move: joystick
- Look: mouse drag (configurable in `OrbitFollowCamera`)
- Fire: UI fire button (mouse fire optional for testing)

## AI Usage (Disclosure)

AI was used for:

- **Optimization suggestions**
- **Bug fixes / debugging**
- **Code structure improvements**
- **Creating Read.md file**
