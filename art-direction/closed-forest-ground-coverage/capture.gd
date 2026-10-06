extends SceneTree

# External audit only. Instantiate Main without adding/removing paint or saving the scene.
const CENTER := Vector2(924, 260)


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(
		"../art-direction/closed-forest-ground-coverage").simplify_path()
	if FileAccess.file_exists(output.path_join("capture-evidence.json")):
		push_error("Existing Closed forest audit is immutable; use a separate revision")
		quit(1)
		return
	if DirAccess.make_dir_recursive_absolute(output.path_join("captures")) != OK:
		push_error("Cannot create external capture directory")
		quit(1)
		return
	var input_file := FileAccess.open(output.path_join("inputs.json"), FileAccess.READ)
	if input_file == null:
		push_error("Missing audit input bindings")
		quit(1)
		return
	var inventory = JSON.parse_string(input_file.get_as_text())
	input_file.close()
	if not inventory is Dictionary or not inventory.get("sources") is Dictionary:
		push_error("Invalid audit input bindings")
		quit(1)
		return
	var sources := [get_script().resource_path, output.path_join("inputs.json"), output.path_join("baseline.json")]
	for path in inventory["sources"]:
		var absolute: String = output.path_join("../..").path_join(path).simplify_path()
		if FileAccess.get_sha256(absolute) != inventory["sources"][path]:
			push_error("Bound source drift: " + path)
			quit(1)
			return
		sources.append(absolute)
	var record := {"engine": Engine.get_version_info().get("string"),
		"scope": "External Closed forest coverage audit; actual scene; no injected paint",
		"inputs": {}, "captures": {}, "frames": {}, "target_center": [924, 260],
		"hidden": "Root CanvasLayers/CharacterBody2Ds only; all shell, openings, routes, ground, channel and labels retained"}
	for path in sources:
		record["inputs"][path] = FileAccess.get_sha256(path)
	var viewport := SubViewport.new()
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	viewport.size = Vector2i(1280, 720)
	root.add_child(viewport)
	var scene: Node2D = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	seeker.global_position = CENTER
	for child in scene.get_children():
		if child is CanvasLayer or child is CharacterBody2D:
			child.visible = false
	var camera: Camera2D = seeker.get_node("FollowCamera")
	camera.position_smoothing_enabled = false
	record["owners"] = _owners(scene)
	for zoom in [0.8, 1.0, 2.0]:
		for frame in [["wide", 720], ["tall", 827]]:
			viewport.size = Vector2i(1280, frame[1])
			camera.zoom = Vector2(zoom, zoom)
			camera.reset_smoothing()
			for _frame in range(8):
				await process_frame
			RenderingServer.force_draw()
			var image := viewport.get_texture().get_image()
			var filename := "captures/baseline-%.1fx-%s.png" % [zoom, frame[0]]
			if FileAccess.file_exists(output.path_join(filename)):
				push_error("Refusing to overwrite external evidence: " + filename)
				quit(1)
				return
			if image == null or image.get_size() != viewport.size \
				or image.save_png(output.path_join(filename)) != OK:
				push_error("Closed forest capture failed: " + filename)
				quit(1)
				return
			record["captures"][filename] = FileAccess.get_sha256(output.path_join(filename))
			var transform := viewport.canvas_transform
			record["frames"][filename] = {"zoom": zoom, "viewport": [1280, frame[1]],
				"screen_center": [camera.get_screen_center_position().x, camera.get_screen_center_position().y],
				"world_to_canvas": [transform.x.x, transform.x.y, transform.y.x, transform.y.y,
					transform.origin.x, transform.origin.y]}
			print("Closed forest external baseline: ", filename)
	for path in sources:
		if record["inputs"][path] != FileAccess.get_sha256(path):
			push_error("Closed forest input drift during capture: " + path)
			quit(1)
			return
	var evidence := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if evidence == null:
		push_error("Closed forest capture evidence write failed")
		quit(1)
		return
	evidence.store_string(JSON.stringify(record, "\t") + "\n")
	evidence.close()
	quit()


func _owners(scene: Node) -> Dictionary:
	var result := {}
	var prefixes := ["GreyboxNavigation/RoutePresentation", "OpeningRegionVillageTransitionSceneSource",
		"OpeningRegionWoodlandClearingSceneSource", "OpeningRegionClosedForestPathSceneSource", "WholeChannelPresentation",
		"GardenGroundPresentation", "SquareGroundPresentation", "SickhouseGroundPresentation",
		"WagonGroundPresentation", "ShrineGroundPresentation", "LaundryGroundPresentation", "VillageEdgeGroundPresentation", "Ground"]
	for prefix in prefixes:
		var node := scene.get_node_or_null(prefix)
		if node == null:
			continue
		_record_owner(scene, node, result)
	return result


func _record_owner(scene: Node, node: Node, result: Dictionary) -> void:
	if node is Node2D:
		var t: Transform2D = node.global_transform
		var data := {"class": node.get_class(), "visible": node.is_visible_in_tree(),
			"z_index": node.z_index, "z_as_relative": node.z_as_relative,
			"world_transform": [t.x.x, t.x.y, t.y.x, t.y.y, t.origin.x, t.origin.y]}
		if node is Sprite2D:
			data["texture"] = node.texture.resource_path if node.texture else ""
			data["size"] = [node.texture.get_width(), node.texture.get_height()] if node.texture else []
			data["centered"] = node.centered
			data["offset"] = [node.offset.x, node.offset.y]
			data["filter"] = node.texture_filter
			data["repeat"] = node.texture_repeat
		if node is Polygon2D:
			var polygon := []
			for point in node.polygon:
				polygon.append([point.x, point.y])
			data["polygon"] = polygon
			data["color"] = [node.color.r, node.color.g, node.color.b, node.color.a]
		if node is CollisionShape2D:
			data["disabled"] = node.disabled
			if node.shape is SegmentShape2D:
				data["segment"] = [[node.shape.a.x, node.shape.a.y], [node.shape.b.x, node.shape.b.y]]
		result[str(scene.get_path_to(node))] = data
	for child in node.get_children():
		_record_owner(scene, child, result)
