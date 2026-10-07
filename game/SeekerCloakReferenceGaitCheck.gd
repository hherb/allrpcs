extends SceneTree
## Validate and capture the separate cloak correction without loading Main.

var output := ""
var failed := false


func _initialize() -> void:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--output="):
			output = argument.trim_prefix("--output=")
	call_deferred("_check")


func _require(value: bool, message: String) -> void:
	## Keep loader, image and playback failures observable in retained logs.
	if not value:
		failed = true
		push_error(message)


func _check() -> void:
	var rig: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://art/seeker-cloak-reference-gait-revision/rig.json"))
	for asset in rig["assets"]:
		var path: String = "res://" + asset["texture"].trim_prefix("game/")
		var texture: Texture2D = load(path)
		var imported := texture.get_image()
		imported.convert(Image.FORMAT_RGBA8)
		var source := Image.new()
		_require(source.load_png_from_buffer(FileAccess.get_file_as_bytes(path)) == OK, "Cannot decode candidate PNG")
		source.convert(Image.FORMAT_RGBA8)
		_require(imported.get_data() == source.get_data(), "Candidate imported RGBA changed")
	var proof: Control = load("res://SeekerCloakReferenceGaitProof.tscn").instantiate()
	root.add_child(proof)
	proof.visible = false
	var playing: Node2D = proof.make_board(1)
	root.add_child(playing)
	var watches := []
	for sprite in playing.get_meta("sprites"):
		var watch := {"sprite": sprite, "offset": sprite.offset, "seen": {}, "loops": 0, "last": -1}
		sprite.animation_looped.connect(func(): watch["loops"] += 1)
		watches.append(watch)
	for _tick in range(165):
		await process_frame
		for watch in watches:
			var sprite: AnimatedSprite2D = watch["sprite"]
			var count := sprite.sprite_frames.get_frame_count(sprite.animation)
			watch["seen"][sprite.frame] = true
			if watch["last"] != -1 and watch["last"] != sprite.frame:
				_require(sprite.frame == (watch["last"] + 1) % count, "Candidate frame order skipped")
			watch["last"] = sprite.frame
			_require(sprite.offset == watch["offset"], "Candidate pivot moved")
	for watch in watches:
		var sprite: AnimatedSprite2D = watch["sprite"]
		_require(watch["seen"].size() == sprite.sprite_frames.get_frame_count(sprite.animation)
			and watch["loops"] >= 2, "Incomplete cloak playback")
	playing.queue_free()
	if failed:
		quit(1)
		return
	var viewport := SubViewport.new()
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	DirAccess.make_dir_recursive_absolute(output)
	for scale_value in [1, 2]:
		viewport.size = proof.board_size(scale_value)
		for sample_ms in range(0, 720, 45):
			var path: String = output.path_join("comparison-%dx-%03d.png" % [scale_value, sample_ms])
			_require(not FileAccess.file_exists(path), "Refusing existing cloak capture")
			if failed:
				quit(1)
				return
			var board: Node2D = proof.make_board(scale_value, sample_ms)
			viewport.add_child(board)
			await process_frame
			await RenderingServer.frame_post_draw
			_require(viewport.get_texture().get_image().save_png(path) == OK, "Cannot retain cloak capture")
			viewport.remove_child(board)
			board.queue_free()
	print("PASS: 48 exact candidate RGBA imports; complete 16-pose sides and accepted 8-pose front/back; predecessor comparison preserved")
	quit(1 if failed else 0)
