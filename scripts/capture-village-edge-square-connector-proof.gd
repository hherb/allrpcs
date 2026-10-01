extends SceneTree

# Render the proposed strip in a temporary scene instance, keeping gameplay files intact.
const CENTER := Vector2(1882, 900)
const REGION := Rect2(0, 0, 96, 124)


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
	var connector := Sprite2D.new()
	connector.name = "VillageEdgeSquareConnectorProof"
	connector.texture = load("res://art/runtime/terrain/straight-path.png")
	connector.region_enabled = true
	connector.region_rect = REGION
	connector.position = CENTER
	connector.rotation = PI / 2
	connector.z_index = -70
	scene.get_node("GreyboxNavigation/RoutePresentation").add_child(connector)
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
			var path := "%s/village-edge-square-connector-%.1fx-%s.png" % [output, zoom, frame[0]]
			if image == null or image.get_size() != viewport.size or image.save_png(path) != OK:
				push_error("Connector proof capture failed: %s" % path)
				quit(1)
				return
			print("proof capture: ", path, " ", image.get_size())
	quit()
