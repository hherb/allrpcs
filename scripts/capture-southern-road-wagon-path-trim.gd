extends SceneTree

# External-only correction; never save the disposable scene.
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
				child.visible = false
	if matches != 1:
		push_error("Road–Wagon proof requires the exact retained 96-pixel route")
		quit(1)
		return
	var road: Line2D = scene.get_node("OpeningRegionSouthernRoadWagonSceneSource/RoadArea")
	if road.points != PackedVector2Array([Vector2(572, 460), START, Vector2(1820, 1580), END]) or road.width != 96:
		push_error("Correction requires the exact retained road presentation")
		quit(1)
		return
	# Replace only the two final fill segments in this disposable instance.
	# All boundary outlines/collisions and wagon presentation keep ordinary ownership.
	road.points = PackedVector2Array([Vector2(572, 460), START])
	var overlay := Image.load_from_file(output.path_join("southern-road-wagon-path-trim-overlay.png"))
	if overlay == null or overlay.get_size() != Vector2i(560, 432):
		push_error("Correction requires the external world-aligned overlay")
		quit(1)
		return
	var sprite := Sprite2D.new()
	sprite.name = "SouthernRoadWagonPathCorrection"
	sprite.texture = ImageTexture.create_from_image(overlay)
	sprite.position = Vector2(1680, 1496)
	sprite.z_index = -70
	routes.add_child(sprite)
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
			var path := "%s/southern-road-wagon-path-trim-%.1fx-%s.png" % [output, zoom, frame[0]]
			if image == null or image.get_size() != viewport.size or image.save_png(path) != OK:
				push_error("External Road–Wagon path capture failed: %s" % path)
				quit(1)
				return
			print("external proof capture: ", path, " ", image.get_size())
	quit()
