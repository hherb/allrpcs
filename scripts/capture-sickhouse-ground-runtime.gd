extends SceneTree

# Explicit disposable runtime capture; never pack/save the scene or inject proof textures.
const CENTER := Vector2(2528, 1290)


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(
		"../art-direction/sickhouse-ground-runtime-promotion"
	).simplify_path()
	if FileAccess.file_exists(output.path_join("owner-runtime-acceptance.json")) \
		or FileAccess.file_exists(output.path_join("capture-evidence.json")):
		push_error("Accepted Sickhouse runtime captures are immutable")
		quit(1)
		return
	if DirAccess.make_dir_recursive_absolute(output.path_join("captures")) != OK:
		push_error("Could not create Sickhouse runtime capture directory")
		quit(1)
		return
	var sources := ["res://Main.tscn", "res://RuntimeSickhouseGroundSmokeCheck.cs",
		"res://art/runtime/terrain/sickhouse-entrance-ground.png",
		"res://art/runtime/terrain/sickhouse-entrance-ground.png.import", get_script().resource_path]
	var record := {"schema_version": 1, "center": [2528, 1290], "inputs": {}, "captures": {}, "frames": {},
		"engine": Engine.get_version_info().get("string"),
		"scope": "Ordinary scene sprites; paired ground-disabled baselines; pending owner visual confirmation"}
	for path in sources:
		var key: String = path if path.begins_with("res://") else "scripts/capture-sickhouse-ground-runtime.gd"
		record["inputs"][key] = FileAccess.get_sha256(path)
	var viewport := SubViewport.new()
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	viewport.size = Vector2i(1280, 720)
	var scene: Node2D = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	seeker.global_position = CENTER
	seeker.visible = false
	for child in scene.get_children():
		if child is CanvasLayer or child is CharacterBody2D:
			child.visible = false
	var camera: Camera2D = seeker.get_node("FollowCamera")
	camera.position_smoothing_enabled = false
	var ground: Node2D = scene.get_node("SickhouseGroundPresentation")
	for zoom in [0.8, 1.0, 2.0]:
		for frame in [["wide", 720], ["tall", 827]]:
			viewport.size = Vector2i(1280, frame[1])
			camera.zoom = Vector2(zoom, zoom)
			camera.reset_smoothing()
			for variant in ["baseline", "runtime"]:
				ground.visible = variant == "runtime"
				for _frame in range(8):
					await process_frame
				RenderingServer.force_draw()
				var image := viewport.get_texture().get_image()
				var filename := "captures/%s-%.1fx-%s.png" % [variant, zoom, frame[0]]
				var path := output.path_join(filename)
				if FileAccess.file_exists(path):
					push_error("Refusing to overwrite runtime evidence: " + path)
					quit(1)
					return
				if image == null or image.get_size() != viewport.size or image.save_png(path) != OK:
					push_error("Sickhouse runtime capture failed: " + path)
					quit(1)
					return
				record["captures"][filename] = FileAccess.get_sha256(path)
				var transform := viewport.canvas_transform
				record["frames"][filename] = {"zoom": zoom, "viewport": [1280, frame[1]],
					"screen_center": [camera.get_screen_center_position().x, camera.get_screen_center_position().y],
					"world_to_canvas": [transform.x.x, transform.x.y, transform.y.x, transform.y.y,
						transform.origin.x, transform.origin.y]}
				print("Sickhouse runtime capture: ", filename)
	for path in sources:
		var key: String = path if path.begins_with("res://") else "scripts/capture-sickhouse-ground-runtime.gd"
		if record["inputs"][key] != FileAccess.get_sha256(path):
			push_error("Sickhouse runtime input changed during capture: " + path)
			quit(1)
			return
	var evidence := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if evidence == null:
		push_error("Sickhouse runtime evidence write failed")
		quit(1)
		return
	evidence.store_string(JSON.stringify(record, "\t") + "\n")
	evidence.close()
	quit()
