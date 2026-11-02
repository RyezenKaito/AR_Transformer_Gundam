# Turn Battle — Play Mode verification

2026-10-28 11:42:58 · Unity 6000.5.8f1

Results: 15 passed, 0 failed.

- PASS — Serialized scene, animation clips, shaders and inactive realtime controllers
- PASS — Mouse buttons survive scene serialization; confirmation is single-use
- PASS — Ultimate selection/cancellation has no cost; commit consumes exactly 100
- PASS — Guard halves damage for the whole 1v3 enemy phase and then expires
- PASS — Skill cooldown blocks the next two command opportunities
- PASS — Dodge boundary timestamps, early consumption, missing press and no retry
- PASS — Timestamp dodge/turn/HP invariance with 30 Hz simulation steps
- PASS — Timestamp dodge/turn/HP invariance with 60 Hz simulation steps
- PASS — Timestamp dodge/turn/HP invariance with 120 Hz simulation steps
- PASS — Multi-hit attack has independent attempts and one meter award
- PASS — Earth Prison: five timed hits × three enemies; 360 damage each; cleanup before retaliation
- PASS — Dead targets stop receiving pulses and skip their enemy action
- PASS — Victory waits for ultimate cleanup; defeat stops remaining actions
- PASS — Pause, speed changes, reset IDs and fixed transforms
- INFO — Input System event mapped to action time 1.02073 s, valid window .95–1.15 s.
- PASS — Actual Input System keyboard edge reaches timestamp-based dodge queue

The 30/60/120 Hz checks use deterministic simulation steps, not measured rendered FPS. Keyboard integration uses a temporary Input System keyboard device and the real input callback. Visual review and performance sampling are recorded separately.
