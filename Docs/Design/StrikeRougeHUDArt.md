# Strike Rouge HUD Art

## Tài nguyên

Atlas `Assets/Game/Features/Combat/UI/StrikeRouge/Art/MechaWeaponsAtlas.png` được tạo bằng công cụ Imagegen tích hợp ngày 31 tháng 10 năm 2026. Đây là minh họa cho UI, không phải mesh, animation hoặc ảnh gameplay.

Bốn sprite trong atlas là Weapon_0 đến Weapon_3, dùng theo thứ tự rifle, đại kiếm, tên lửa và Full Burst. Import ở chế độ Sprite Multiple, giữ alpha và không tạo mipmap. Tài nguyên gốc sinh bởi công cụ được giữ nguyên; file sử dụng trong project là bản sao.

Các file Portrait_Rouge và Portrait_Enemy được bake từ bản sao của model đang có trong scene. Các file Icon_Rifle, Icon_Blade và Icon_Missiles là ảnh bake thay thế từ rig. Không thay đổi các model nguồn trong Models.

## Prompt tạo atlas

Create a transparent PNG GAME UI ICON ATLAS, very wide aspect ratio 4:1, FOUR equal square cells in a single horizontal row. No panels, no letters, no numbers, no frames, no backdrop, absolutely transparent background. Polished premium sci-fi mecha weapon illustrations matching a graphite/cyan/amber military Gundam HUD. Each object should be centered inside its own quarter of the canvas with generous transparent padding, fully isolated and never crossing cell boundaries. Each must be a detailed metallic 3D-rendered object with crisp silhouette, strong readable highlights and subtle cool cyan rim light, matching rendering scale and visual finish. Exact left-to-right contents: cell 1 a futuristic long beam rifle with trigger grip, stock and long barrel, pointing diagonally upper-right; cell 2 a very large slender anti-ship sword with blue glowing cutting edge, red-and-white hilt, pointing diagonally upper-right; cell 3 a fan of three sleek self-propelled silver missiles with small fins, pointing diagonally upper-right; cell 4 a heroic front three-quarter Gundam mecha head with white angular armor, pink cheek accents, golden V-fin, emerald eye slit, a restrained amber reactor halo behind it but no broad flare spilling beyond the cell. Objects occupy approximately 75 percent of cell width and 65 percent of cell height. Actual alpha transparency, no checkerboard pattern. No text or typography anywhere. These are reusable UI artwork assets, not a screenshot or an infographic.
