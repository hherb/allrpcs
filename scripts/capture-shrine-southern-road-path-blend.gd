extends SceneTree

# External-only correction. Never pack or save the disposable scene.
const START := Vector2(380, 300)
const END := Vector2(1500, 1400)
const ORIGIN := Vector2(432, 400)
const SIZE := Vector2i(1152, 1080)
const VIEWS := [
	["midpoint", Vector2(940, 850)],
	["shrine-exit", Vector2(543, 460)],
	["junction", END],
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
		if child is Line2D and child.points.size() == 2:
			if child.points[0] == START and child.points[1] == END and child.width == 96:
				matches += 1
				child.visible = false
	if matches != 1:
		push_error("Correction requires the exact retained 96-pixel Shrine–Road route")
		quit(1)
		return
	var road: Line2D = scene.get_node("OpeningRegionSouthernRoadWagonSceneSource/RoadArea")
	if road.points != PackedVector2Array([Vector2(572, 460), END]) or road.width != 96:
		push_error("Correction requires the retained upstream-only road fill")
		quit(1)
		return
	road.visible = false
	# The accepted Square and Road–Wagon sprites and every shell/boundary remain intact.
	var overlay := Image.load_from_file(output.path_join("shrine-southern-road-path-correction-overlay.png"))
	if overlay == null or overlay.get_size() != SIZE:
		push_error("Correction requires the external world-aligned overlay")
		quit(1)
		return
	var sprite := Sprite2D.new()
	sprite.name = "ShrineSouthernRoadPathCorrection"
	sprite.texture = ImageTexture.create_from_image(overlay)
	sprite.position = ORIGIN + Vector2(SIZE) / 2
	sprite.z_index = -72
	routes.add_child(sprite)
	var patch := Image.load_from_file(output.path_join("shrine-southern-road-path-blend-overlay.png"))
	if patch == null or patch.get_size() != Vector2i(160, 160):
		push_error("Endpoint blend requires the external 160x160 patch")
		quit(1)
		return
	var blend := Sprite2D.new()
	blend.name = "ShrineSouthernRoadEndpointBlend"
	blend.texture = ImageTexture.create_from_image(patch)
	blend.position = Vector2(1480, 1360)
	blend.z_index = -69
	routes.add_child(blend)
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
				var path := "%s/shrine-southern-road-path-blend-%s-%.1fx-%s.png" % [output, view[0], zoom, frame[0]]
				if image == null or image.get_size() != viewport.size or image.save_png(path) != OK:
					push_error("External Shrine–Road correction capture failed: %s" % path)
					quit(1)
					return
				print("external correction capture: ", path, " ", image.get_size())
	quit()
