# Rig validation

NUMERICAL CHECKS PASS — 64 pass, 0 fail, 2 warnings.

Unity 6000.5.8f1; root Unicorn_Player; 2026-10-26T17:15:39.2963934+07:00

These are static/numerical rig checks. They do not certify final animation, runtime behavior or commercial-game visual quality.

- PASS: MechaRigData exists on the player root
- PASS: VisualRoot and SkeletonRoot references are assigned
- PASS: SkeletonRoot is inside Animator binding hierarchy
- PASS: PlayerRoot local scale is one
- PASS: VisualRoot local scale is one (geometry owns imported unit conversion)
- PASS: Source-to-meters scale is finite and positive
- PASS: Builder rest-pose matrix error is at most 0.5 mm
- PASS: Joint/rest-position/rest-rotation arrays are complete and aligned
- PASS: Expected 375 imported mesh parts are mapped
- PASS: No Missing Script component in player hierarchy (count=0)
- PASS: Joint Pelvis exists inside SkeletonRoot
- PASS: Joint Spine exists inside SkeletonRoot
- PASS: Joint Chest exists inside SkeletonRoot
- PASS: Joint Neck exists inside SkeletonRoot
- PASS: Joint Head exists inside SkeletonRoot
- PASS: Joint Backpack exists inside SkeletonRoot
- PASS: Joint Shoulder_L exists inside SkeletonRoot
- PASS: Joint Shoulder_R exists inside SkeletonRoot
- PASS: Joint UpperArm_L exists inside SkeletonRoot
- PASS: Joint UpperArm_R exists inside SkeletonRoot
- PASS: Joint LowerArm_L exists inside SkeletonRoot
- PASS: Joint LowerArm_R exists inside SkeletonRoot
- PASS: Joint Hand_L exists inside SkeletonRoot
- PASS: Joint Hand_R exists inside SkeletonRoot
- PASS: Joint UpperLeg_L exists inside SkeletonRoot
- PASS: Joint UpperLeg_R exists inside SkeletonRoot
- PASS: Joint LowerLeg_L exists inside SkeletonRoot
- PASS: Joint LowerLeg_R exists inside SkeletonRoot
- PASS: Joint Foot_L exists inside SkeletonRoot
- PASS: Joint Foot_R exists inside SkeletonRoot
- PASS: Joint FrontSkirt_L exists inside SkeletonRoot
- PASS: Joint FrontSkirt_R exists inside SkeletonRoot
- PASS: Joint SideSkirt_L exists inside SkeletonRoot
- PASS: Joint SideSkirt_R exists inside SkeletonRoot
- PASS: Joint RearSkirt_L exists inside SkeletonRoot
- PASS: Joint RearSkirt_R exists inside SkeletonRoot
- PASS: Joint references/names are unique and local transforms are finite
- PASS: SwordSocket follows Hand_R
- PASS: GunSocket follows Hand_L
- PASS: At least two backpack thruster sockets exist
- PASS: All thruster sockets are assigned under Backpack
- PASS: Animator is on VisualRoot
- PASS: Animator root motion is disabled for motor-authoritative movement
- INFO: Animator controller=Assets/MechaCombatDemo/MechaDemo.controller; clip binding/gait validation is handled by animation report.
- PASS: MechaPlayer.animator reference is assigned
- PASS: MechaPlayer.bladeBase reference is assigned
- PASS: MechaPlayer.bladeTip reference is assigned
- PASS: MechaPlayer.muzzle reference is assigned
- PASS: MechaPlayer.effects reference is assigned
- PASS: MechaPlayer.tuning reference is assigned
- PASS: All mapped part/driver references are assigned and part transforms are unique
- PASS: All mapped geometry is directly parented to its recorded skeleton driver
- PASS: Every mapped part retains a nonempty mesh and MeshRenderer
- PASS: No part is marked as unresolved
- PASS: All mapped geometry transforms are finite
- PASS: No duplicated/unmapped source OBJ geometry remains elsewhere under player root (extra=0)
- PASS: 20 preview/reset cycles: no accumulated offset (position=0.00000000 m, rotation=0.00000000 deg)
- PASS: All recorded skeleton rest rotations are identity
- PASS: All geometry offsets/rotations/scales retain their recorded rest transform (max local position=0.00000000 m)
- PASS: Original prefab part names/count match mapping exactly
- PASS: Every mapped part retains the original shared mesh asset (375/375)
- PASS: Every mapped part retains original material-slot references (375/375)
- PASS: Original per-part visibility is preserved (375/375)
- PASS: Neutral source overlay matches converted original geometry within 0.5 mm (max=0.00000098 m)
- INFO: Original source comparison=Assets/Models/Unicorn Gundam/Unicorn Gundam.prefab; checks mesh references, material-slot references and eight transformed bounds corners per part, not a vertex-topology rewrite.
- PASS: Mapped neutral geometry height matches 2.6 m within 3 mm (height=2.60000000)
- INFO: Neutral mapped world bounds center=(0.00000, 1.30000, 0.03141), size=(1.45777, 2.60000, 0.91454); attachments/proxy weapons excluded.
- WARNING: Numerical rig tests do not prove armor clearance, weapon grip, attractive key poses, crossfade quality or runtime collision behavior. Review front/side/three-quarter combat poses and play tests separately.
- WARNING: Finger geometry remains rigid with Hand_L/R in the imported gripping pose; individual finger articulation is not implemented.
