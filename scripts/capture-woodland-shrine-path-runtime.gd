extends SceneTree

# Capture ordinary runtime; change only framing and Seeker/HUD visibility.
const START := Vector2(1000, 350)
const END := Vector2(380, 300)
const VIEWS := [
	["midpoint", Vector2(690, 325)],
	["clearing-junction", Vector2(1000, 350)],
	["shrine-entry", Vector2(620, 319.354839)],
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
	var sprite: Sprite2D = scene.get_node("GreyboxNavigation/RoutePresentation/WoodlandShrinePath")
	if sprite.position != Vector2(704, 328) or sprite.z_index != -74:
		push_error("Runtime Woodland–Shrine placement changed")
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
				await RenderingServer.frame_post_draw
				var image := viewport.get_texture().get_image()
				var path := "%s/woodland-shrine-path-runtime-%s-%.1fx-%s.png" % [output, view[0], zoom, frame[0]]
				if image == null or image.get_size() != viewport.size or image.save_png(path) != OK:
					push_error("External Woodland–Shrine capture failed: %s" % path)
					quit(1)
					return
				print("runtime capture: ", path, " ", image.get_size())
	quit()
