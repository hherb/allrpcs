extends SceneTree

# External-only strip test; never save the disposable scene.
const START := Vector2(1500, 1400)
const END := Vector2(1900, 1640)
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
	var routes := scene.get_node("GreyboxNavigation/RoutePresentation")
	var matches := 0
	for child in routes.get_children():
		if child is Line2D and child.points.size() == 2:
			if child.points[0] == START and child.points[1] == END and child.width == 96:
				matches += 1
	if matches != 1:
		push_error("Road–Wagon proof requires the exact retained 96-pixel route")
		quit(1)
		return
	var strip := Sprite2D.new()
	strip.name = "SouthernRoadWagonPathProof"
	strip.texture = load("res://art/runtime/terrain/straight-path.png")
	strip.texture_repeat = CanvasItem.TEXTURE_REPEAT_ENABLED
	strip.region_enabled = true
	strip.region_rect = Rect2(0, 0, 96, START.distance_to(END))
	strip.position = CENTER
	strip.rotation = (END - START).angle() - PI / 2
	strip.z_index = -70
	routes.add_child(strip)
	# Hide only screen overlays/Seeker and reframe this disposable proof.
	# The retained road presentation, route line, markers, and labels remain visible.
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
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
			var path := "%s/southern-road-wagon-path-proof-%.1fx-%s.png" % [output, zoom, frame[0]]
			if image == null or image.get_size() != viewport.size or image.save_png(path) != OK:
				push_error("External Road–Wagon path capture failed: %s" % path)
				quit(1)
				return
			print("external proof capture: ", path, " ", image.get_size())
	quit()
