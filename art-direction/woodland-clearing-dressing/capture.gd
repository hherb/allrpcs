extends SceneTree

# Approved images are loaded into disposable viewports only; never save/pack Main.
const CENTER := Vector2(924, 380)
const VARIANTS := ["baseline", "sparse", "grouped"]


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(
		"../art-direction/woodland-clearing-dressing").simplify_path()
	if FileAccess.file_exists(output.path_join("capture-evidence.json")) \
		or FileAccess.file_exists(output.path_join("owner-acceptance.json")):
		push_error("Existing dressing evidence is immutable; prepare a separate revision")
		quit(1)
		return
	var file := FileAccess.open(output.path_join("inventory.json"), FileAccess.READ)
	if file == null:
		push_error("Missing dressing inventory")
		quit(1)
		return
	var inventory = JSON.parse_string(file.get_as_text())
	file.close()
	if not inventory is Dictionary or not inventory.get("sources") is Dictionary \
		or not inventory.get("variants") is Dictionary:
		push_error("Invalid dressing inventory")
		quit(1)
		return
	var frames := []
	for zoom in [0.8, 1.0, 2.0]:
		for size in [["wide", 1280, 720], ["tall", 1280, 827]]:
			frames.append({"id": "%.1fx-%s" % [zoom, size[0]], "zoom": zoom,
				"size": Vector2i(size[1], size[2]), "center": CENTER})
	for zoom in [1.0, 2.0]:
		frames.append({"id": "%.1fx-placement" % zoom, "zoom": zoom,
			"size": Vector2i(int(800 * zoom), int(480 * zoom)), "center": Vector2(924, 240)})
	# Preflight every output before instantiating a scene or writing any PNG.
	for frame in frames:
		for variant in VARIANTS:
			if FileAccess.file_exists(output.path_join("captures/%s-%s.png" % [variant, frame["id"]])):
				push_error("Refusing existing dressing capture")
				quit(1)
				return
	var repo := output.path_join("../..").simplify_path()
	var inputs := {}
	for path in inventory["sources"]:
		var absolute := repo.path_join(path)
		if FileAccess.get_sha256(absolute) != inventory["sources"][path]:
			push_error("Bound dressing source drift: " + path)
			quit(1)
			return
		inputs[absolute] = inventory["sources"][path]
	inputs[output.path_join("inventory.json")] = FileAccess.get_sha256(output.path_join("inventory.json"))
	if DirAccess.make_dir_recursive_absolute(output.path_join("captures")) != OK:
		push_error("Cannot create dressing capture directory")
		quit(1)
		return
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
	for _frame in range(8):
		await process_frame
	# Freeze ordinary controllers after startup; camera scroll remains explicit.
	scene.process_mode = Node.PROCESS_MODE_DISABLED
	var owners := _owners(scene)
	var groups := {}
	var placement_record := {}
	for variant in VARIANTS:
		var group := Node2D.new()
		group.name = "ExternalDressing" + variant
		group.visible = false
		scene.add_child(group)
		groups[variant] = group
		placement_record[variant] = []
		for row in inventory["variants"][variant]:
			var image := Image.new()
			if image.load(repo.path_join(row["export"])) != OK \
				or image.get_size() != Vector2i(row["size"][0], row["size"][1]):
				push_error("Approved dressing image failed: " + row["asset"])
				quit(1)
				return
			var sprite := Sprite2D.new()
			sprite.name = row["instance"]
			sprite.texture = ImageTexture.create_from_image(image)
			sprite.centered = false
			sprite.position = Vector2(row["origin"][0], row["origin"][1])
			sprite.z_index = -60
			sprite.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
			sprite.texture_repeat = CanvasItem.TEXTURE_REPEAT_DISABLED
			group.add_child(sprite)
			placement_record[variant].append({"instance": row["instance"], "origin": row["origin"],
				"size": row["size"], "world_pivot": row["world_pivot"], "proposed_pivot": row["proposed_pivot"],
				"sha256": FileAccess.get_sha256(repo.path_join(row["export"])), "z_index": sprite.z_index,
				"centered": sprite.centered, "offset": [sprite.offset.x, sprite.offset.y],
				"scale": [sprite.scale.x, sprite.scale.y], "rotation": sprite.rotation,
				"filter": sprite.texture_filter, "repeat": sprite.texture_repeat,
				"script_free": sprite.get_script() == null, "child_count": sprite.get_child_count()})
	var record := {"engine": Engine.get_version_info().get("string"), "inputs": inputs,
		"published_head": inventory["published_head"], "owners": owners, "placements": placement_record,
		"scope": "External Woodland Clearing dressing; unchanged Main in disposable viewports",
		"hidden": "Root CanvasLayers and CharacterBody2Ds only; labels, shells, interactions and concealed state retained",
		"captures": {}, "frames": {}}
	for frame in frames:
		viewport.size = frame["size"]
		seeker.global_position = frame["center"]
		camera.zoom = Vector2(frame["zoom"], frame["zoom"])
		camera.reset_smoothing()
		camera.force_update_scroll()
		for variant in VARIANTS:
			for name in groups:
				groups[name].visible = name == variant
			for _frame in range(8):
				await process_frame
			RenderingServer.force_draw()
			var image := viewport.get_texture().get_image()
			var name := "captures/%s-%s.png" % [variant, frame["id"]]
			if _owners(scene) != owners or image == null or image.get_size() != viewport.size \
				or FileAccess.file_exists(output.path_join(name)) or image.save_png(output.path_join(name)) != OK:
				push_error("Dressing capture/owner preservation failed: " + name)
				quit(1)
				return
			record["captures"][name] = FileAccess.get_sha256(output.path_join(name))
			var transform := viewport.canvas_transform
			record["frames"][name] = {"zoom": frame["zoom"], "viewport": [viewport.size.x, viewport.size.y],
				"target_center": [frame["center"].x, frame["center"].y],
				"screen_center": [camera.get_screen_center_position().x, camera.get_screen_center_position().y],
				"world_to_canvas": [transform.x.x, transform.x.y, transform.y.x, transform.y.y,
					transform.origin.x, transform.origin.y]}
			print("Woodland dressing external preview: ", name)
	for path in inputs:
		if FileAccess.get_sha256(path) != inputs[path]:
			push_error("Dressing input drift during capture: " + path)
			quit(1)
			return
	var evidence := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if evidence == null:
		push_error("Dressing evidence write failed")
		quit(1)
		return
	evidence.store_string(JSON.stringify(record, "\t") + "\n")
	evidence.close()
	print("PASS: complete 24-view Woodland Clearing external dressing comparison")
	quit()


func _owners(scene: Node) -> Dictionary:
	var result := {}
	var prefixes := ["GreyboxNavigation/RoutePresentation", "OpeningRegionVillageTransitionSceneSource",
		"OpeningRegionWoodlandClearingSceneSource", "OpeningRegionClosedForestPathSceneSource", "WholeChannelPresentation",
		"GardenGroundPresentation", "SquareGroundPresentation", "SickhouseGroundPresentation",
		"WagonGroundPresentation", "ShrineGroundPresentation", "LaundryGroundPresentation", "VillageEdgeGroundPresentation", "Ground", "InteractionCandidates"]
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
		if node is Line2D:
			var points := []
			for point in node.points:
				points.append([point.x, point.y])
			data["points"] = points
			data["width"] = node.width
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
	if node is Control:
		var rect: Rect2 = node.get_global_rect()
		var data := {"class": node.get_class(), "visible": node.is_visible_in_tree(),
			"rect": [rect.position.x, rect.position.y, rect.size.x, rect.size.y]}
		if node is Label:
			data["text"] = node.text
		result[str(scene.get_path_to(node))] = data
	for child in node.get_children():
		_record_owner(scene, child, result)
