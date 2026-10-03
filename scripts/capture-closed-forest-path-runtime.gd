extends SceneTree

# Capture ordinary runtime; change only framing and Seeker/HUD visibility.
const VIEWS := [
	["gate", Vector2(744, 200)],
	["midpoint", Vector2(825, 250)],
	["closed-end", Vector2(650, 150)],
]


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(
		"../art-direction/forest-path-runtime-promotion/captures"
	).simplify_path()
	DirAccess.make_dir_recursive_absolute(output)
	var repository := ProjectSettings.globalize_path("res://").path_join("..").simplify_path()
	var inputs := {}
	for relative in ["game/Main.tscn", "game/GreyboxNavigation.cs", "scripts/capture-closed-forest-path-runtime.gd",
			"game/art/runtime/terrain/clearing-closed-forest-path.png",
			"game/art/runtime/terrain/clearing-closed-forest-path.png.import"]:
		inputs[relative] = FileAccess.get_sha256(repository.path_join(relative))
	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var scene: Node2D = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	var routes := scene.get_node("GreyboxNavigation/RoutePresentation")
	var sprite: Sprite2D = routes.get_node("ClosedForestPath")
	if sprite.position != Vector2(796, 224) or sprite.z_index != -75 \
			or sprite.get_index() != routes.get_child_count() - 1:
		push_error("Runtime closed-forest placement/order changed")
		quit(1)
		return
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	seeker.visible = false
	var camera: Camera2D = seeker.get_node("FollowCamera")
	camera.position_smoothing_enabled = false
	for view in VIEWS:
		seeker.global_position = view[1]
		for zoom in [0.8, 1.0, 2.0]:
			for frame in [["wide", 720], ["tall", 827]]:
				viewport.size = Vector2i(1280, frame[1])
				camera.zoom = Vector2(zoom, zoom)
				camera.reset_smoothing()
				await process_frame
				await process_frame
				await process_frame
				RenderingServer.force_draw()
				var image := viewport.get_texture().get_image()
				var path := "%s/runtime-%s-%.1fx-%s.png" % [output, view[0], zoom, frame[0]]
				if image == null or image.get_size() != viewport.size or image.save_png(path) != OK:
					push_error("Runtime closed-forest capture failed: %s" % path)
					quit(1)
					return
				print("runtime capture: ", path, " ", image.get_size())
	var record := FileAccess.open(output.path_join("../capture-context.json"), FileAccess.WRITE)
	if record == null:
		push_error("Could not bind runtime capture inputs")
		quit(1)
		return
	record.store_string(JSON.stringify(inputs, "  ") + "\n")
	record.close()
	quit()
