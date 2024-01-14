# Muffin

Muffin is a top-down, 3D pixel art, action RPG using 2D sprites, developed in C#/Unity. I worked on it in 2022/23 with the goal of learning about game/physics engines, and improving my skills in C#, 3D, shaders, render pipelines, traditional AI design, and many other topics. 

The game is in a pre-alpha stage. It is fully playable but is not feature complete and only has about 5 minutes worth of content. However, it is considered complete as this was a learning project and I have no intention of turning it into a releasable game.

_This repo only contains the C# scripts and no other files or assets related to the game. Plastic SCM was used for full version control for this project as it natively integrates with Unity._

![Muffin banner bar 02](https://github.com/kimgoetzke/game-muffin/assets/120580433/7bbfa92a-a095-4344-a9e7-681d2af619ce)

### Some technical features
+ Unified, hierarchical state machine AI model that powers friends and foes alike and allows them to interact with one another
+ Faction management that allows changing NPC behaviours real-time (towards player and other NPCs)
+ (I also experimented with a behavioural tree model for AI which is in this repo but not used as it was not required)
+ Equipment is swappable in real-time using scriptable objects (any range type weapon is is supported, extendable to melee)
+ 4 directional player sprite movement while maintaining full 360 degrees, free mouse-controlled aim
+ Object pools to pre-spawn a number of frequently spawned objects
+ Universal Render Pipeline post-processing features, incl. bloom, depth of field, color grading and more
+ Pixelise render feature/pass for a consistent pixelated look of all in-game elements, except UI elements
+ 2D dynamic lightning (e.g. zone based), cloud shadows, etc.
+ Extensive use of shaders to simulate water, fog, sprite lightning, and more
+ Basic 3D modelling of rocks, crates and other objects
+ Cinemachine with camera zones and target groups to enable look-ahead
+ Scalable quest and dialogue system (allowing for 4 quest types) incl. UI (using Inky for dialogue scripts and variables)
+ Unity Terrain and GPU instancing for grass/flower sprites
+ Timeline-powered intro and on-scene-change cutscenes which are skippable
+ A small but rich game worked with grass/trees swaying in the wind, dynamic lighting/shadow effects, fireflies, birds, and marine life such as schools of fish and nautili

![Muffin banner bar 01](https://github.com/kimgoetzke/game-muffin/assets/120580433/c2af44ef-45d1-47a7-b823-d19f05c470c4)

### Game system examples

#### NPC state machine

+ `NPC` state machine is at the core
+ `NpcBehaviour` sets default behavior: anxious, neutral, indifferent, observant, defensive, or aggressive
+ `NpcMovement` sets default movement type: wander, patrol or idle
+ Combination of `AnimationController` and `AudioController` allow customising the visual appear and audio (humanoid and two-legged dinsaur implemented)
+ Separate, optional Game Objects with sphere colliders determine:
    1. Awareness: trigger intermediate action such as observing
    2. Action: switch from default movement to default behaviour
    3. Collaboration: support others of same faction or flee together
    4. Interaction: allow player to interact e.g. trigger dialogue
+ `FactionIdentity` component determines which other factions the NPC is hostile, friendly or neutral towards
+ Factions can change at runtime
+ `HealthManager` determines health and triggers disabling of NPC class
+ `AttackController` determines damage, range, etc.

![Flying_through_space](https://github.com/kimgoetzke/game-muffin/assets/120580433/c12b7970-0fe9-4973-9108-f7d7fb3b4167)

#### Quest system

+ A `Quest` is a collection of tasks which can be linked to a story 
+ Stories are text assets which are arranged into a flow using the third party tool Ink 
+ C# classes share data with Ink through `DialogueVariables`, allowing dialogue to influence game behaviours and vice versa
+ The system supports any task types such as "reach location x", "defeat x number of type y enemies", or "do x" 
+ The active quest, its task and their states are shown on the in-game UI to the player
+ Quest states and dialogue variables (which include story progression) are, by default, saved on scene change

![Quest_system](https://github.com/kimgoetzke/game-muffin/assets/120580433/4d6e4b67-16f3-4e58-84d7-1e1de98b4b09)

![Muffin banner bar 03](https://github.com/kimgoetzke/game-muffin/assets/120580433/292e2993-91dc-4c19-9da4-ae6db1cc56ec)

