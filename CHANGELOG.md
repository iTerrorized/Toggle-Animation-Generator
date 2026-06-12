# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.7] - 2026-06-12

### Fixed

- **Create DBT Decal Reveal** now correctly detects Poiyomi properties marked animated. The "animated" flag is stored as a material string tag (`<Prop>Animated`), read via `Material.GetTag`, not as a float property — so it no longer reports "nothing is marked animated".

## [1.0.6] - 2026-06-12

### Added

- **Create DBT Decal Reveal**: generates a cumulative decal-reveal Simple1D blend tree from the Poiyomi decal properties marked animated on a mesh's material (each step turns on one more decal alpha/hue), wired under a chosen Direct Blend Tree.
- **Create DBT Material Swaps**, **Create DBT Group Material Swap**, and **Create Single DBT Toggles**.

## [1.0.0] - 20YY-MM-DD

### Added

- This CHANGELOG file to hopefully serve as an evolving example of a standardized open source project CHANGELOG.
