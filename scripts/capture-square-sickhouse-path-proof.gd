extends SceneTree

# External feasibility only: retain the route line for the failed strip; hide it
# only in the corrected disposable scene instance. Never save the scene.
const ANCHOR := Vector2(2200, 900)
const START := Vector2(2404.8, 1156)
const END := Vector2(2640, 1450)
const CENTER := (START + END) / 2


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
	# Hide screen overlays only in this disposable proof so the seam is visible.
	# World markers, structures, and labels keep their ordinary ownership.
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
	var routes := scene.get_node("GreyboxNavigation/RoutePresentation")
	var junction: Sprite2D = routes.get_node("VillageSquareJunction")
	if junction.position != ANCHOR or junction.texture.get_size() != Vector2(512, 512):
		push_error("Square–Sickhouse proof requires the retained 512-pixel junction")
		quit(1)
		return
	var corrected := OS.get_cmdline_user_args().has("--corrected")
	var matches := 0
	for child in routes.get_children():
		if child is Line2D and child.points.size() == 2:
			if child.points[0].is_equal_approx(START) and child.points[1].is_equal_approx(END):
				matches += 1
				if corrected:
					child.visible = false
	if matches != 1:
		push_error("Square–Sickhouse proof requires the exact retained presentation route")
		quit(1)
		return
	var path := Sprite2D.new()
	path.name = "SquareSickhousePathProof"
	path.texture = load("res://art/runtime/terrain/straight-path.png")
	path.texture_repeat = CanvasItem.TEXTURE_REPEAT_ENABLED
	path.region_enabled = true
	path.region_rect = Rect2(0, 0, 96, START.distance_to(END))
	path.position = CENTER
	path.rotation = (END - START).angle() - PI / 2
	path.z_index = -70
	if corrected:
		var overlay := Image.load_from_file(output.path_join("square-sickhouse-path-correction-overlay.png"))
		if overlay == null or overlay.get_size() != Vector2i(380, 360):
			push_error("Square–Sickhouse correction requires the external 380x360 overlay")
			quit(1)
			return
		path.texture = ImageTexture.create_from_image(overlay)
		path.region_enabled = false
		path.position = Vector2(2510, 1320)
		path.rotation = 0
		path.z_index = -71
	routes.add_child(path)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	seeker.global_position = CENTER
	seeker.visible = false
	var camera: Camera2D = seeker.get_node("FollowCamera")
	camera.position_smoothing_enabled = false
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
			var mode := "correction" if corrected else "proof"
			var filename := "%s/square-sickhouse-path-%s-%.1fx-%s.png" % [output, mode, zoom, frame[0]]
			if image == null or image.get_size() != viewport.size or image.save_png(filename) != OK:
				push_error("Square–Sickhouse proof capture failed: %s" % filename)
				quit(1)
				return
			print("external proof capture: ", filename, " ", image.get_size())
	quit()
