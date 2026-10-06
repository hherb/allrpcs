extends SceneTree

# Render the actual imported Main owner; only its visibility changes for controls.
const CENTER := Vector2(924, 380)
const VARIANTS := ["baseline", "grouped"]
const OWNER := "WoodlandClearingDressingPresentation"


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var repo := ProjectSettings.globalize_path("res://").path_join("..").simplify_path()
	var output := repo.path_join("art-direction/woodland-clearing-dressing-runtime-promotion")
	if FileAccess.file_exists(output.path_join("capture-evidence.json")) \
		or FileAccess.file_exists(output.path_join("owner-runtime-acceptance.json")):
		push_error("Runtime capture evidence is immutable")
		quit(1)
		return
	var inputs_path := output.path_join("capture-inputs.json")
	var inputs = JSON.parse_string(FileAccess.get_file_as_string(inputs_path))
	var proposal_path := repo.path_join("art-direction/woodland-clearing-dressing-runtime-scope/proposal-inputs.json")
	var proposal = JSON.parse_string(FileAccess.get_file_as_string(proposal_path))
	if not inputs is Dictionary or not proposal is Dictionary:
		push_error("Missing bound runtime inputs/proposal")
		quit(1)
		return
	for path in inputs:
		if FileAccess.get_sha256(repo.path_join(path)) != inputs[path]:
			push_error("Runtime capture input drift: " + path)
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
	for frame in frames:
		for variant in VARIANTS:
			if FileAccess.file_exists(output.path_join("captures/%s-%s.png" % [variant, frame["id"]])):
				push_error("Refusing existing runtime capture")
				quit(1)
				return
	if DirAccess.make_dir_recursive_absolute(output.path_join("captures")) != OK:
		push_error("Cannot create runtime capture directory")
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
	scene.process_mode = Node.PROCESS_MODE_DISABLED
	var owners := _owners(scene)
	var dressing := _dressing(scene, proposal["assets"])
	if dressing.is_empty():
		quit(1)
		return
	var group: Node2D = scene.get_node(OWNER)
	var record := {"engine": Engine.get_version_info().get("string"), "inputs": inputs,
		"input_binding_sha256": FileAccess.get_sha256(inputs_path), "owners": owners, "dressing": dressing,
		"scope": "Actual imported grouped Main; paired controls hide only its dressing owner",
		"hidden": "Root CanvasLayers and CharacterBody2Ds only; labels, shells, interactions and concealed state retained",
		"captures": {}, "frames": {}}
	for frame in frames:
		viewport.size = frame["size"]
		seeker.global_position = frame["center"]
		camera.zoom = Vector2(frame["zoom"], frame["zoom"])
		camera.reset_smoothing()
		camera.force_update_scroll()
		for variant in VARIANTS:
			group.visible = variant == "grouped"
			for _frame in range(8):
				await process_frame
			RenderingServer.force_draw()
			var image := viewport.get_texture().get_image()
			var name := "captures/%s-%s.png" % [variant, frame["id"]]
			if _owners(scene) != owners or _dressing(scene, proposal["assets"]) != dressing \
				or image == null or image.get_size() != viewport.size \
				or FileAccess.file_exists(output.path_join(name)) or image.save_png(output.path_join(name)) != OK:
				push_error("Runtime capture/preservation failed: " + name)
				quit(1)
				return
			record["captures"][name] = FileAccess.get_sha256(output.path_join(name))
			var transform := viewport.canvas_transform
			record["frames"][name] = {"zoom": frame["zoom"], "viewport": [viewport.size.x, viewport.size.y],
				"target_center": [frame["center"].x, frame["center"].y],
				"screen_center": [camera.get_screen_center_position().x, camera.get_screen_center_position().y],
				"world_to_canvas": [transform.x.x, transform.x.y, transform.y.x, transform.y.y,
					transform.origin.x, transform.origin.y]}
			print("Woodland dressing actual runtime: ", name)
	for path in inputs:
		if FileAccess.get_sha256(repo.path_join(path)) != inputs[path]:
			push_error("Runtime input drift during capture: " + path)
			quit(1)
			return
	var evidence := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	if evidence == null:
		push_error("Runtime evidence write failed")
		quit(1)
		return
	evidence.store_string(JSON.stringify(record, "\t") + "\n")
	evidence.close()
	print("PASS: complete sixteen-view imported grouped runtime/control series")
	quit()


func _dressing(scene: Node, assets: Array) -> Dictionary:
	var owner: Node2D = scene.get_node_or_null(OWNER)
	if owner == null or owner.get_child_count() != 16 or owner.get_script() != null \
		or owner.transform != Transform2D.IDENTITY or owner.z_index != 0 or owner.y_sort_enabled:
		push_error("Runtime dressing owner drift")
		return {}
	var result := {"name": OWNER, "origin": [0, 0], "z_index": 0, "placements": []}
	for index in range(assets.size()):
		var asset: Dictionary = assets[index]
		var sprite: Sprite2D = owner.get_child(index) as Sprite2D
		if sprite == null or sprite.name != asset["node_name"] or not sprite.texture is CompressedTexture2D:
			push_error("Missing/reordered/importless runtime sprite")
			return {}
		var path: String = sprite.texture.resource_path
		var uid := ResourceLoader.get_resource_uid(path)
		var uid_text := ResourceUID.id_to_text(uid)
		if uid == ResourceUID.INVALID_ID or ResourceUID.text_to_id(uid_text) != uid:
			push_error("Noncanonical runtime UID")
			return {}
		result["placements"].append({"node_name": str(sprite.name), "texture": path,
			"uid": uid_text, "origin": [sprite.position.x, sprite.position.y],
			"size": [sprite.texture.get_width(), sprite.texture.get_height()], "z_index": sprite.z_index,
			"z_as_relative": sprite.z_as_relative, "centered": sprite.centered,
			"offset": [sprite.offset.x, sprite.offset.y], "scale": [sprite.scale.x, sprite.scale.y],
			"rotation": sprite.rotation, "filter": sprite.texture_filter, "repeat": sprite.texture_repeat,
			"flip_h": sprite.flip_h, "flip_v": sprite.flip_v, "region_enabled": sprite.region_enabled,
			"modulate": [sprite.modulate.r, sprite.modulate.g, sprite.modulate.b, sprite.modulate.a],
			"script_free": sprite.get_script() == null, "child_count": sprite.get_child_count()})
	return result


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
