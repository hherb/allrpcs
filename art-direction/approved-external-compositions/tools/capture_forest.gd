extends SceneTree

# Explicit external capture only: never pack or save this disposable scene.
const START := Vector2(1000, 350)
const END := Vector2(650, 150)
const ORIGIN := Vector2(584, 80)
const SIZE := Vector2i(488, 336)
const VIEWS := [
	["gate", Vector2(744, 200)],
	["midpoint", Vector2(825, 250)],
	["closed-end", END],
]


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(
		"../art-direction/approved-external-compositions/forest"
	).simplify_path()
	var manifest_path := output.path_join("manifest.json")
	var manifest: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(manifest_path))
	var repository := ProjectSettings.globalize_path("res://").path_join("..").simplify_path()
	for relative in manifest["source_sha256"]:
		if FileAccess.get_sha256(repository.path_join(relative)) != manifest["source_sha256"][relative]:
			push_error("Forest capture input changed: %s" % relative)
			quit(1)
			return
	var context := {"manifest_sha256": FileAccess.get_sha256(manifest_path)}
	context["overlay_sha256"] = {}
	for filename in ["raw-strip.png", "bounded-strip.png"]:
		context["overlay_sha256"][filename] = FileAccess.get_sha256(output.path_join("exports").path_join(filename))
	DirAccess.make_dir_recursive_absolute(output.path_join("captures"))
	for kind in ["retained", "raw", "bounded"]:
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
				if kind != "retained":
					child.visible = false
		if matches != 1:
			push_error("Forest proof requires exactly one retained 96px centreline")
			quit(1)
			return
		var gate := scene.get_node("OpeningRegionClosedForestPathSceneSource/ClosedGate")
		if gate.get_child(0).disabled or not gate.get_child(1).visible:
			push_error("Forest proof requires the retained closed physical gate")
			quit(1)
			return
		if kind != "retained":
			var filename := "raw-strip.png" if kind == "raw" else "bounded-strip.png"
			var overlay := Image.load_from_file(output.path_join("exports").path_join(filename))
			if overlay == null or overlay.get_size() != SIZE:
				push_error("Forest proof requires exact generated external overlay")
				quit(1)
				return
			var sprite := Sprite2D.new()
			sprite.name = "ExternalClosedForestProof"
			sprite.texture = ImageTexture.create_from_image(overlay)
			sprite.position = ORIGIN + Vector2(SIZE) / 2
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
					# Draw explicitly: an obscured macOS window can stop emitting frame_post_draw.
					RenderingServer.force_draw()
					var image := viewport.get_texture().get_image()
					var path := "%s/captures/%s-%s-%.1fx-%s.png" % [output, kind, view[0], zoom, frame[0]]
					if image == null or image.get_size() != viewport.size or image.save_png(path) != OK:
						push_error("External forest capture failed: %s" % path)
						quit(1)
						return
					print("external forest capture: ", path)
		viewport.queue_free()
		await process_frame
	var record := FileAccess.open(output.path_join("capture-context.json"), FileAccess.WRITE)
	if record == null:
		push_error("Forest capture context could not be written")
		quit(1)
		return
	record.store_string(JSON.stringify(context, "  ") + "\n")
	record.close()
	quit()
