# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.0.1] - Unreleased

### Added
- Player movement via `PlayerMotor` (physics-based, `Rigidbody2D.MovePosition` in `FixedUpdate`) with walk speed, sprint multiplier, and diagonal normalization.
- Input layer: `IMovementInput` interface implemented by `PlayerInputReader` (single owner of the new Input System actions).
- Movement gating: `IMovementGate` / `CanMove`, owned by `PlayerController` (the player "brain") with a single `SetState` mutation point.
- `PlayerState` commanded modes: `Free`, `UsingTool`, `InEvent`.
- `PlayerMotor.Velocity` exposed (units/second) for systems that need to read motion.
- Four-directional sprite animation: `ICharacterAnimator` interface, `AnimatorView` (owns Unity `Animator`), and `PlayerAnimationDriver` (reads `Velocity`, snaps to cardinal facing, drives the controller).
- `Player.controller` animator with Idle/Walk 2D blend trees keyed on `MoveX`/`MoveY` and a `Moving` bool.
- Tilemap world (Grid layers: Ground, Walls, Water, Trees, and others) using the Serene Village 16×16 tileset.
- Animated water layer via an `AnimatedTile` (`Water_Anim`).
- `Player` prefab.
- `.gitignore` for Unity projects.

### Changed
- `PlayerState` reduced to commanded modes only; locomotion (idle/walk) is derived from velocity by the animation layer rather than stored as state.
- `PlayerController` no longer resolves an input source — `PlayerMotor` is the sole input consumer.

### Removed
- Stealth/disguise/crouch scope (`Hide`/`Disguised` states, `CrouchPressed` input intent) as out-of-genre for a Stardew-like.

### Fixed
- Player passing through walls (Rigidbody2D set to Dynamic; collider was a trigger).
- Animator not transitioning (parameter name and Move blend-tree Y-axis mismatch; transitions' Has Exit Time).
- Water tiles animating out of sync (`AnimatedTile` min/max speed set equal).

[0.0.1]: https://semver.org/
