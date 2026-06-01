# Station 01: Golden Village Keyframe Route

## Locked Master

- `K01_master_locked.png` is the approved and locked full-frame base.
- Do not regenerate the whole landscape for later keyframes.
- Lock the railway centerline, vanishing point, camera height, horizon, sky, hills,
  railway color, ballast color, and overall late-afternoon grade.

## Route Assumptions

- Comfortable visual speed: approximately `30 km/h`.
- Keyframe interval: approximately `6 seconds`.
- Distance per interval: approximately `50 meters`.
- Total planned distance: approximately `350 meters`.
- Station platform remains on the right side of the track.

## Planned Progression

| Frame | Distance | Visible progression |
| --- | ---: | --- |
| K01 | 0 m | Open wheat fields. Distant village establishes the destination. |
| K02 | 50 m | Village enlarges subtly. Station roof is barely visible on the right. |
| K03 | 100 m | Right-side station building becomes readable in the middle distance. |
| K04 | 150 m | Platform entrance appears on the right. |
| K05 | 200 m | Train enters the platform area. |
| K06 | 250 m | Train passes the platform. A distant woman waves slowly. |
| K07 | 300 m | Platform end moves out of the right edge. |
| K08 | 350 m | Open fields return. Village remains ahead, slightly closer than in K01. |

## Continuity Rules

- Reuse the locked track layer whenever the track remains unobstructed.
- Apply deterministic transforms and local compositing for later frames.
- Use AI generation only for isolated replaceable elements or small masked areas.
- Do not use full-frame AI edits to simulate forward movement.
- Validate each adjacent keyframe pair before using Vidu interpolation.
