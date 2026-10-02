extends SceneTree

# External-only strip test; never pack or save the disposable scene.
const START := Vector2(1700, 900)
const END := Vector2(1800, 500)
const VIEWS := [
	["midpoint", Vector2(1769, 624)],
	["stream-entry", Vector2(1788, 548)],
	["village-entry", Vector2(1750, 700)],
]


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(
		"../art-direction/production-proof/corrected/review"
	).simplify_path()
	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var scene: Node2D = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	var routes := scene.get_node("GreyboxNavigation/RoutePresentation")
	var matches := 0
	for child in routes.get_children():
		if child is Line2D and child.points == PackedVector2Array([START, END]) and child.width == 96:
			matches += 1
			child.visible = false
	if matches != 1:
		push_error("Village Edge–Stream proof requires exactly one retained 96px route line")
		quit(1)
		return
	var overlay := Image.load_from_file(output.path_join("village-edge-stream-path-feasibility-overlay.png"))
	if overlay == null or overlay.get_size() != Vector2i(224, 528):
		push_error("Village Edge–Stream proof requires the exact external overlay")
		quit(1)
		return
	var sprite := Sprite2D.new()
	sprite.name = "VillageEdgeStreamPathFeasibility"
	sprite.texture = ImageTexture.create_from_image(overlay)
	sprite.position = Vector2(1752, 704)
	sprite.z_index = -75
	routes.add_child(sprite)
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
				await RenderingServer.frame_post_draw
				var image := viewport.get_texture().get_image()
				var path := "%s/village-edge-stream-path-feasibility-%s-%.1fx-%s.png" % [output, view[0], zoom, frame[0]]
				if image == null or image.get_size() != viewport.size or image.save_png(path) != OK:
					push_error("External Village Edge–Stream capture failed: %s" % path)
					quit(1)
					return
				print("external feasibility capture: ", path, " ", image.get_size())
	quit()
