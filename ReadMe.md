# PlayerInteractionController

Tag-driven, raycast-based interaction system for Unity with optional reticle UI feedback. Uses Unity's Input System (`UnityEngine.InputSystem`).

## Requirements

| Dependency | Source |
|---|---|
| **Unity 2020.1+** | — |
| `com.unity.inputsystem` | Unity Package Manager |

Install the Input System package from UPM if it's not already present.


## Setup

1. Add `PlayerInteractionController` to the player object or camera rig.
   - Assign the **Camera Source** reference.
   - Configure each mapping's **Button** as an `InputActionReference`.
   - Set **Interactable Distance** and **Interact Cooldown** to taste.
2. Add entries to **Interaction Tags Map**:
   - Set each **Tag** to match the `IInteractable.Tag` string on your target objects.
   - Assign the **Button** (`InputActionReference`) that should trigger the interaction.
   - Wire **OnInteractableHit**, **OnInteractableLost**, and **OnInteracted** events as needed.
3. Implement `IInteractable` on any object the player should be able to interact with.
4. *(Optional)* Add a `ReticleController` and populate **Reticle References** with UI `Image` components whose **Name** values match your interaction tags.
   - Hook `ActivateReticle` / `DeactivateReticles` / `AnimateReticle` into the interaction events above.


## Architecture

```
Runtime/
├── Abstractions/
│   ├── IInteractable.cs   – Contract for objects the player can interact with
│   └── IInteractor.cs     – Contract for the entity performing the interaction
└── Adapters/
    ├── PlayerInteractionController.cs – Raycast + input → interaction pipeline
    └── ReticleController.cs           – Tag-driven reticle UI activation/animation
```

### Abstractions

#### `IInteractable`
Any object that can be interacted with must implement this interface.

| Member | Description |
|---|---|
| `GameObject GameObject` | The underlying `GameObject`. |
| `string Tag` | An arbitrary string used to match against interaction mappings (e.g. `"Click"`, `"Push"`). |
| `void Interact(IInteractor interactor)` | Called when the player successfully interacts with this object. |

#### `IInteractor`
Represents the entity performing interactions. `PlayerInteractionController` implements this.

| Member | Description |
|---|---|
| `GameObject GameObject` | The interactor's `GameObject`. |
| `int UserIndex` | Index identifying the user, useful in multi-player scenarios. |
| `void AssignUserIndex(int userIndex)` | Sets the user index at runtime. |

### Adapters

#### `PlayerInteractionController`
`MonoBehaviour` · implements `IInteractor`

Every frame, this component:
1. Casts a ray from the assigned `Camera` (position + forward) up to `Interaction Distance`.
2. Collects all colliders hit via `Physics.RaycastNonAlloc` (up to 16 hits) and filters for `IInteractable` components.
3. Selects the **closest** interactable.
4. Compares the result against the previous frame — fires `OnInteractableHit` or `OnInteractableLost` events on change.
5. If the mapped `InputActionReference` is pressed (`WasPressedThisFrame`) and the cooldown has elapsed, calls `Interact()` on the target and fires `OnInteracted`.

**Inspector fields:**

| Field | Type | Default | Description |
|---|---|---|---|
| Allow Interactions | `bool` | `true` | Global toggle. When `false`, the raycast is skipped and the current target is cleared. |
| Camera Source | `Camera` | — | Camera used as the ray origin and direction. |
| Interactable Distance | `float` | `5` | Maximum raycast distance in world units. |
| Interact Cooldown | `float` | `0.2` | Minimum seconds between successive interactions. |
| Interaction Tags Map | `List<InteractionTagMapping>` | — | Tag → input button → event bindings (see below). |

**`InteractionTagMapping`** (serializable, per-entry):

| Field | Type | Default | Description |
|---|---|---|---|
| Tag | `string` | `"Click"` | Matched against `IInteractable.Tag`. |
| Button | `InputActionReference` | — | The input action reference that triggers the interaction (`WasPressedThisFrame`). |
| OnInteractableHit | `UnityEvent` | — | Fired when a matching interactable enters the raycast. |
| OnInteractableLost | `UnityEvent` | — | Fired when a matching interactable leaves the raycast. |
| OnInteracted | `UnityEvent` | — | Fired after a successful interaction. |

> **Fallback:** If no mapping matches the interactable's tag, the **first** mapping in the list is used.

**Public API:**

| Method | Description |
|---|---|
| `AllowInteractions(bool allow)` | Enable or disable the entire interaction system at runtime. |
| `AssignUserIndex(int userIndex)` | Set the `UserIndex` on this interactor. |

**Gizmos:** Draws a cyan line and sphere in the Scene view showing the raycast origin and endpoint.

---

#### `ReticleController`
`MonoBehaviour`

Manages a set of named reticle `Image` references. Only one reticle is active at a time — matched by a tag string that corresponds to interaction tags.

**Inspector fields:**

| Field | Type | Default | Description |
|---|---|---|---|
| Allow Reticles | `bool` | `true` | Global toggle. When `false`, all reticles are deactivated. |
| Reticle References | `List<ReticleReference>` | — | Named `Image` references (Name + Reticle image). |
| Default Reticle Animator Trigger Name | `string` | `"Animate"` | The `Animator` trigger parameter hashed and fired during animation. |

**`ReticleReference`** (serializable, per-entry):

| Field | Description |
|---|---|
| `Name` | String identifier matched against interaction tags. |
| `Reticle` | The UI `Image` component to enable/disable. |
| *(auto)* `Animator` | Lazily resolved from the `Reticle` `Image`'s `GameObject`. |

**Public API:**

| Method | Description |
|---|---|
| `ActivateReticle(string tag)` | Enables the reticle whose `Name` matches `tag`; disables all others. |
| `AnimateReticle(string tag)` | Fires the animator trigger on the matching reticle. |
| `DeactivateReticles()` | Disables all reticle images. |
| `AllowReticles(bool allow)` | Enable or disable the reticle system. Disabling also deactivates all reticles. |


## Package Resources

The `PackageResources/` folder includes starter assets:

| Folder | Contents |
|---|---|
| `Animations/` | `Click.anim` clip and `InteractionUIAnimCtrl` animator controller for reticle animation. |
| `Images/` | Cursor sprites (`cursor.png`, `cursor_click_2.png`, `cursor_push.png`). |
| `Prefabs/` | A pre-configured `Main Camera` prefab. |

## License

See repository for license details.
