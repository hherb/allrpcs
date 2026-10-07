extends SceneTree
## Exercise imported RGBA, SpriteFrames and real playback; optionally retain rendered boards.

const ART := "res://art/character-animation-import-proof/"
var failed := false
var output := ""
var captures := false
var registration: Dictionary
var proof: Control
var watches: Array[Dictionary] = []


func _initialize() -> void:
	for argument in OS.get_cmdline_user_args():
		if argument.begins_with("--output="):
			output = argument.trim_prefix("--output=")
		captures = captures or argument == "--captures"
	call_deferred("_check")


func _require(value: bool, message: String) -> void:
	## Keep failures observable even if the engine exits zero after a script diagnostic.
	if not value:
		failed = true
		push_error(message)


func _check() -> void:
	registration = JSON.parse_string(FileAccess.get_file_as_string(ART + "registration.json"))
	proof = load("res://CharacterAnimationImportProof.tscn").instantiate()
	root.add_child(proof)
	proof.visible = false
	_require(proof.get_script().resource_path == "res://CharacterAnimationImportProof.gd", "Wrong proof script")
	var uids := {}
	for frame in registration["frames"]:
		var path: String = "res://" + frame["texture"].trim_prefix("game/")
		var texture: Texture2D = load(path)
		var decoded := texture.get_image()
		decoded.convert(Image.FORMAT_RGBA8)
		var source := Image.new()
		_require(source.load_png_from_buffer(FileAccess.get_file_as_bytes(path)) == OK, "Cannot decode source PNG")
		source.convert(Image.FORMAT_RGBA8)
		_require(source.get_size() == decoded.get_size() and source.get_data() == decoded.get_data(),
			"Imported RGBA differs: " + frame["id"])
		var identifier := ResourceLoader.get_resource_uid(path)
		var canonical := ResourceUID.id_to_text(identifier)
		var sidecar := FileAccess.get_file_as_string(path + ".import")
		_require(identifier >= 0 and sidecar.contains('uid="' + canonical + '"'), "Noncanonical UID: " + path)
		_require(not uids.has(identifier), "Reused import UID")
		uids[identifier] = true
	for family in registration["family_order"]:
		var specification: Dictionary = registration["families"][family]
		var frames: SpriteFrames = load("res://" + specification["resource"].trim_prefix("game/"))
		_require(frames.get_animation_names().size() == 12, "Incomplete animation resource")
		for animation in specification["animations"]:
			var name_value: String = animation["name"]
			_require(frames.get_frame_count(name_value) == animation["frames"].size(), "Frame count mismatch")
			_require(frames.get_animation_loop(name_value) == animation["loop"], "Gesture/loop mismatch")
			_require(absf(frames.get_animation_speed(name_value) - 1000.0 / animation["frame_ms"]) < 0.00001,
				"Cadence mismatch")
			for index in range(animation["frames"].size()):
				var expected: String = ART + "frames/" + animation["frames"][index] + ".png"
				_require(frames.get_frame_texture(name_value, index).resource_path == expected, "Frame order mismatch")
				_require(frames.get_frame_duration(name_value, index) == 1.0, "Unexpected frame weighting")
			var sprite := AnimatedSprite2D.new()
			sprite.sprite_frames = frames
			sprite.centered = false
			var direction: String = name_value.get_slice("_", 1)
			var action: String = name_value.get_slice("_", 0)
			var first: Dictionary = proof._first_frame(family, direction, action)
			sprite.offset = -Vector2(first["pivot"][0], first["pivot"][1])
			root.add_child(sprite)
			var watch := {"sprite": sprite, "name": family + "/" + name_value, "seen": {},
				"cycles": 0, "last": -1, "count": animation["frames"].size(), "loop": animation["loop"],
				"offset": sprite.offset}
			sprite.animation_looped.connect(func(): watch["cycles"] += 1)
			if not animation["loop"]:
				sprite.animation_finished.connect(func():
					watch["cycles"] += 1
					sprite.set_frame_and_progress(0, 0.0)
					sprite.play(name_value))
			sprite.play(name_value)
			watches.append(watch)
	for _tick in range(370):
		await process_frame
		for watch in watches:
			var sprite: AnimatedSprite2D = watch["sprite"]
			watch["seen"][sprite.frame] = true
			if watch["last"] != -1 and watch["last"] != sprite.frame:
				_require(sprite.frame == posmod(watch["last"] + 1, watch["count"]), "Playback skipped/reordered a frame")
			watch["last"] = sprite.frame
			_require(sprite.offset == watch["offset"], "Pivot changed during playback")
	var playback := []
	for watch in watches:
		_require(watch["seen"].size() == watch["count"] and watch["cycles"] >= 2,
			"Incomplete actual playback: " + watch["name"])
		playback.append({"animation": watch["name"], "seen_frames": watch["seen"].size(),
			"completed_cycles_or_gestures": watch["cycles"]})
		watch["sprite"].queue_free()
	if failed:
		quit(1)
		return
	var capture_names: Array[String] = []
	if captures:
		_require(not output.is_empty(), "Captures require output directory")
		capture_names = await _capture_boards()
	if not output.is_empty() and not failed:
		DirAccess.make_dir_recursive_absolute(output)
		var report := FileAccess.open(output.path_join("godot-check.json"), FileAccess.WRITE)
		report.store_string(JSON.stringify({"status": "passed", "decoded_rgba_matches": 480,
			"canonical_independent_uids": uids.size(), "playback": playback, "captures": capture_names}, "  ") + "\n")
	if not failed:
		print("PASS: 480 exact imported RGBA textures; canonical UIDs; 120 complete ordered animations and fixed pivots")
	quit(1 if failed else 0)


func _capture_boards() -> Array[String]:
	## Capture actual imported textures at both scales, retaining every source-frame phase.
	var names: Array[String] = []
	var viewport := SubViewport.new()
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	DirAccess.make_dir_recursive_absolute(output.path_join("captures"))
	for scale_value in [1, 2]:
		viewport.size = proof.board_size(scale_value)
		for action in ["idle", "inspect", "walk"]:
			var samples := [0, 600] if action == "idle" else [0, 160, 320, 480]
			if action == "walk":
				samples = [0, 90, 120, 180, 240, 270, 360, 450, 480, 540, 600, 630]
			for sample_ms in samples:
				var name_value := "%s-%dx-%03d.png" % [action, scale_value, sample_ms]
				var path: String = output.path_join("captures").path_join(name_value)
				_require(not FileAccess.file_exists(path), "Refusing to overwrite existing captures")
				if failed:
					return names
				var board: Node2D = proof.make_board(scale_value, action, sample_ms)
				viewport.add_child(board)
				await process_frame
				await RenderingServer.frame_post_draw
				var pixels := viewport.get_texture().get_image()
				_require(pixels != null and not pixels.is_empty(), "Renderer returned no captured pixels")
				if failed:
					return names
				_require(pixels.save_png(path) == OK, "Cannot retain capture")
				names.append("captures/" + name_value)
				viewport.remove_child(board)
				board.queue_free()
	viewport.queue_free()
	return names
