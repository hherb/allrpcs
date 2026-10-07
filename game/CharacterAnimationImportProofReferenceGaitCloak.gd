extends "res://CharacterAnimationImportProof.gd"
## Additive successor of the frozen proof; replace only the cloak walking display.


func make_board(scale_value: int, selected_action: String, sample_ms := -1) -> Node2D:
	## Retain every other family/action and show the corrected eight-phase cloak row.
	var result := super.make_board(scale_value, selected_action, sample_ms)
	for child in result.get_children():
		if child is Label and child.text.begins_with("All 480 accepted frames"):
			child.text = "Character comparison / side-revised cloak gait candidate / %s / %dx native" \
				% [selected_action, scale_value]
		if selected_action == "walk" and child is Label and child.text.begins_with("6 frames / pivot"):
			child.text = child.text.replace("6 frames", "16 side / 8 front-back phases")
	if selected_action != "walk":
		return result
	var corrected: SpriteFrames = load("res://art/seeker-cloak-reference-gait-revision/walk.tres")
	for sprite in result.get_meta("sprites"):
		if sprite.get_meta("family") != "seeker-cloak":
			continue
		var direction: String = sprite.get_meta("direction")
		sprite.sprite_frames = corrected
		sprite.speed_scale = 1.0
		var rig: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://art/seeker-cloak-reference-gait-revision/rig.json"))
		for asset in rig["assets"]:
			if asset["direction"] == direction and asset["index"] == 1:
				sprite.offset = -Vector2(asset["pivot"][0], asset["pivot"][1])
		sprite.play("walk_" + direction)
		if sample_ms >= 0:
			sprite.pause()
			var count := corrected.get_frame_count(sprite.animation)
			sprite.frame = int(sample_ms / (720.0 / count)) % count
	return result
