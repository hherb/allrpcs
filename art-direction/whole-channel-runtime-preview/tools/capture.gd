extends SceneTree

# Run only in the disposable project prepared by tools/prepare.py.
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
var overlay: Node2D
var water: Polygon2D
var northwest: Sprite2D
var southwest: Sprite2D
var original_materials: Array
var overlap_clip: ShaderMaterial
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


func _candidate(enabled: bool) -> void:
	overlay.visible = enabled
	for index in range(3):
		var node: CanvasItem = [water, northwest, southwest][index]
		node.material = overlap_clip if enabled else original_materials[index]
	if _physical_stamp() != physical_before:
		push_error("Preview changed physical nodes")
		quit(1)


func _view(name: String, target: Vector2, zoom: float, size: Vector2i) -> void:
	viewport.size = size
	seeker.global_position = target
	camera.zoom = Vector2(zoom, zoom)
	camera.reset_smoothing()
	var record := {}
	for stage in ["ordinary", "candidate"]:
		_candidate(stage == "candidate")
		camera.force_update_scroll()
		await process_frame
		await process_frame
		camera.force_update_scroll()
		RenderingServer.force_draw()
		var center := camera.get_screen_center_position()
		var current := {"name": name, "target": [target.x, target.y], "zoom": zoom,
			"size": [size.x, size.y], "camera_center": [center.x, center.y]}
		if stage == "ordinary":
			record = current
		elif current != record:
			push_error("Ordinary/candidate camera framing differs")
			quit(1)
			return
		var image := viewport.get_texture().get_image()
		if image == null or image.get_size() != size or image.save_png(output.path_join(
			"captures/%s-%s.png" % [stage, name])) != OK:
			push_error("Capture failed: " + name)
			quit(1)
			return
	views.append(record)
	print("capture pair: ", name)


func _capture() -> void:
	var config: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://preview/config.json"))
	output = config["output"]
	DirAccess.make_dir_recursive_absolute(output.path_join("captures"))
	DirAccess.make_dir_recursive_absolute(output.path_join("imports"))
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
	var stream := scene.get_node("OpeningRegionStreamLaundrySceneSource")
	water = stream.get_node("StreamWater")
	northwest = stream.get_node("NorthWestBank")
	southwest = stream.get_node("SouthWestBank")
	original_materials = [water.material, northwest.material, southwest.material]
	var clip_shader := Shader.new()
	clip_shader.code = "shader_type canvas_item; varying vec2 world_position; void vertex() { world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy; } void fragment() { if (world_position.x < 1664.0) { discard; } }"
	overlap_clip = ShaderMaterial.new()
	overlap_clip.shader = clip_shader
	var route_names: Array = []
	for child in scene.get_node("GreyboxNavigation/RoutePresentation").get_children():
		route_names.append(str(child.name))
	if route_names.back() != "ClosedForestPath":
		push_error("Accepted route order differs")
		quit(1)
		return
	overlay = Node2D.new()
	overlay.name = "DisposableChannelPresentation"
	scene.add_child(overlay)
	var manifest: Dictionary = JSON.parse_string(FileAccess.get_file_as_string("res://preview/manifest.json"))
	var imports := {}
	for layer in manifest["layers"]:
		var sprite := Sprite2D.new()
		sprite.name = layer["name"]
		var path := "res://preview/" + str(layer["filename"])
		sprite.texture = load(path)
		sprite.position = Vector2(1238, 522)
		sprite.z_index = int(layer["z_index"])
		sprite.texture_repeat = CanvasItem.TEXTURE_REPEAT_DISABLED
		sprite.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
		overlay.add_child(sprite)
		if sprite.texture.get_size() != Vector2(2604, 620):
			push_error("Preview imported texture dimensions differ")
			quit(1)
			return
		var imported := sprite.texture.get_image()
		if imported.save_png(output.path_join("imports/" + layer["filename"])) != OK:
			push_error("Could not record imported RGBA")
			quit(1)
			return
		var sidecar := FileAccess.open(output.path_join("imports/" + layer["filename"] + ".import"), FileAccess.WRITE)
		sidecar.store_string(FileAccess.get_file_as_string(path + ".import"))
		sidecar.close()
		imports[layer["filename"]] = {"png_sha256": FileAccess.get_sha256(path),
			"import_sha256": FileAccess.get_sha256(path + ".import"),
			"uid": str(ResourceLoader.get_resource_uid(path))}
	for target in TARGETS:
		for zoom in [0.8, 1.0, 2.0]:
			for frame in [["wide", 720], ["tall", 827]]:
				await _view("%s-%.1fx-%s" % [target[0], zoom, frame[0]], target[1], zoom, Vector2i(1280, frame[1]))
	await _view("overview", Vector2(1238, 522), 0.45, Vector2i(1280, 720))
	_candidate(false)
	var record := {"manifest_sha256": FileAccess.get_sha256("res://preview/manifest.json"),
		"capture_script_sha256": FileAccess.get_sha256("res://preview/capture.gd"),
		"main_sha256": FileAccess.get_sha256("res://Main.tscn"), "views": views,
		"imports": imports, "route_siblings": route_names,
		"physical_nodes_unchanged": _physical_stamp() == physical_before,
		"capture_only_hidden": ["Seeker", "root CanvasLayer children"],
		"suppression": ["StreamWater, NorthWestBank and SouthWestBank: fragment discard at world x<1664",
			"Original polygons, UVs, regions, transforms and other cap/crossing materials retained"],
		"effective_z": {"ground": -100, "existing_routes": [-75, -74, -72, -71, -70, -69],
			"water": -68, "banks": -67, "decks": -66, "detail": -66, "fringe": -65,
			"retained_stream": 0}}
	var file := FileAccess.open(output.path_join("capture-context.json"), FileAccess.WRITE)
	file.store_string(JSON.stringify(record, "  ") + "\n")
	file.close()
	quit()
