extends SceneTree

# Capture ordinary promoted runtime; never alter its presentation or pack/save the scene.
const TARGETS := [
	["outlet", Vector2(0, 600)], ["B2", Vector2(685.45455, 600)],
	["B1", Vector2(1318.18182, 600)], ["interface", Vector2(1664, 500)],
	["crossing", Vector2(1800, 500)], ["spring", Vector2(2192, 500)],
]
var output: String
var viewport: SubViewport
var scene: Node2D
var camera: Camera2D
var seeker: CharacterBody2D
var physical_before: String
var views: Array = []


func _initialize() -> void:
	call_deferred("_capture")


func _shape_data(shape: Shape2D) -> Array:
	var data: Array = [shape.get_class()]
	for property in shape.get_property_list():
		if int(property["usage"]) & PROPERTY_USAGE_STORAGE:
			data.append([property["name"], str(shape.get(property["name"]))])
	return data


func _physical(node: Node, rows: Array) -> void:
	if node is CollisionPolygon2D:
		rows.append([str(node.get_path()), str(node.transform), str(node.polygon), node.disabled])
	elif node is CollisionShape2D:
		rows.append([str(node.get_path()), str(node.transform), _shape_data(node.shape), node.disabled])
	elif node is StaticBody2D:
		rows.append([str(node.get_path()), str(node.global_transform)])
	for child in node.get_children():
		_physical(child, rows)


func _physical_stamp() -> String:
	var rows: Array = []
	_physical(scene, rows)
	return JSON.stringify(rows)


func _view(name: String, target: Vector2, zoom: float, size: Vector2i) -> void:
	viewport.size = size
	seeker.global_position = target
	camera.zoom = Vector2(zoom, zoom)
	camera.reset_smoothing()
	camera.force_update_scroll()
	await process_frame
	await process_frame
	camera.force_update_scroll()
	RenderingServer.force_draw()
	var center := camera.get_screen_center_position()
	var image := viewport.get_texture().get_image()
	if image == null or image.get_size() != size or image.save_png(output.path_join(
		"captures/runtime-%s.png" % name)) != OK or _physical_stamp() != physical_before:
		push_error("Runtime capture or physical preservation failed: " + name)
		quit(1)
		return
	views.append({"name": name, "target": [target.x, target.y], "zoom": zoom,
		"size": [size.x, size.y], "camera_center": [center.x, center.y]})
	print("runtime capture: ", name)


func _capture() -> void:
	output = ProjectSettings.globalize_path("res://").path_join("../art-direction/whole-channel-runtime-promotion").simplify_path()
	DirAccess.make_dir_recursive_absolute(output.path_join("captures"))
	viewport = SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	scene = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	await process_frame
	scene.process_mode = Node.PROCESS_MODE_DISABLED
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
	seeker = scene.get_node("Seeker")
	seeker.visible = false
	camera = seeker.get_node("FollowCamera")
	camera.position_smoothing_enabled = false
	physical_before = _physical_stamp()
	for target in TARGETS:
		for zoom in [0.8, 1.0, 2.0]:
			for frame in [["wide", 720], ["tall", 827]]:
				await _view("%s-%.1fx-%s" % [target[0], zoom, frame[0]], target[1], zoom, Vector2i(1280, frame[1]))
	await _view("overview", Vector2(1238, 522), 0.45, Vector2i(1280, 720))
	var inputs := {}
	var repository := ProjectSettings.globalize_path("res://").path_join("..").simplify_path()
	for path in ["game/Main.tscn", "game/WholeChannelPresentation.cs", "scripts/capture-whole-channel-runtime.gd",
			"game/art/runtime/terrain/channel-retained-overlap.gdshader"]:
		inputs[path] = FileAccess.get_sha256(repository.path_join(path))
	for name in ["water", "banks", "decks", "detail", "fringe"]:
		var path := "game/art/runtime/terrain/channel-%s.png" % name
		inputs[path] = FileAccess.get_sha256(repository.path_join(path))
		inputs[path + ".import"] = FileAccess.get_sha256(repository.path_join(path + ".import"))
	var record := {"inputs": inputs, "views": views, "physical_nodes_unchanged": _physical_stamp() == physical_before,
		"capture_only_hidden": ["Seeker", "root CanvasLayer children"], "scene_processing": "frozen after initialization"}
	var file := FileAccess.open(output.path_join("capture-context.json"), FileAccess.WRITE)
	file.store_string(JSON.stringify(record, "  ") + "\n")
	file.close()
	quit()
