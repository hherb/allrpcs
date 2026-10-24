extends SceneTree

# Captures the ordinary Main scene. No replacement textures or scene packing.
const OUTPUT := "../art-direction/buildings-gameplay-integration"
const SITES := [
	["sickhouse", Vector2(2640, 1350), Vector2(2528, 1410)],
	["laundry", Vector2(1500, 230), Vector2(1500, 220)],
	["shrine", Vector2(430, 290), Vector2(380, 300)],
	["wagon", Vector2(1880, 1610), Vector2(1650, 1500)],
	["village", Vector2(2200, 790), Vector2(2200, 950)],
]


func _initialize() -> void:
	call_deferred("_capture")


func _capture() -> void:
	var output := ProjectSettings.globalize_path("res://").path_join(OUTPUT).simplify_path()
	if FileAccess.file_exists(output.path_join("owner-acceptance.json")):
		push_error("Accepted building gameplay captures are immutable")
		quit(1)
		return
	DirAccess.make_dir_recursive_absolute(output.path_join("captures"))
	var viewport := SubViewport.new()
	viewport.size = Vector2i(1280, 720)
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var scene: Node2D = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	var follow: Camera2D = seeker.get_node("FollowCamera")
	follow.enabled = false
	var camera := Camera2D.new()
	camera.position_smoothing_enabled = false
	viewport.add_child(camera)
	camera.make_current()
	for child in scene.get_children():
		if child is CanvasLayer:
			child.visible = false
	# Retain the ordinary actors and all interactables; only stop autonomous motion.
	var healer: CharacterBody2D = scene.get_node("Healer")
	healer.set_physics_process(false)
	for _frame in range(12):
		await process_frame
	var record := {"schema_version": 1, "engine": Engine.get_version_info().get("string"),
		"inputs": {}, "captures": {}, "frames": {},
		"status": "candidate runtime views; owner visual acceptance not yet recorded"}
	for path in ["res://Main.tscn", "res://MapStructuresPresentation.tscn",
		"res://MapStructuresPresentation.cs", "res://MapStructureCutawayFloor.cs", "res://shaders/structure-cutaway.gdshader",
		"res://../scripts/capture-building-gameplay.gd"]:
		record.inputs[path] = FileAccess.get_sha256(path)
	for filename in DirAccess.get_files_at("res://art/runtime/structures"):
		if filename.ends_with(".png") or filename.ends_with(".import"):
			var asset_path := "res://art/runtime/structures/" + filename
			record.inputs[asset_path] = FileAccess.get_sha256(asset_path)
	for site in SITES:
		camera.position = site[1]
		camera.zoom = Vector2.ONE
		seeker.global_position = Vector2(3000, 1700)
		seeker.visible = false
		await _save(viewport, camera, output, site[0] + "-exterior", record)
		if site[0] in ["sickhouse", "laundry", "shrine"]:
			seeker.global_position = site[2]
			seeker.visible = true
			await _save(viewport, camera, output, site[0] + "-cutaway", record)
			viewport.size = Vector2i(1280, 827)
			await _save(viewport, camera, output, site[0] + "-cutaway-tall", record)
			viewport.size = Vector2i(1280, 720)
	seeker.global_position = Vector2(3000, 1700)
	seeker.visible = false
	camera.position = Vector2(1600, 900)
	camera.zoom = Vector2(0.4, 0.4)
	await _save(viewport, camera, output, "map-overview", record)
	for path in record.inputs:
		if record.inputs[path] != FileAccess.get_sha256(path):
			push_error("Capture input changed: " + path)
			quit(1)
			return
	var file := FileAccess.open(output.path_join("capture-evidence.json"), FileAccess.WRITE)
	file.store_string(JSON.stringify(record, "\t") + "\n")
	print("PASS: actual Main building gameplay captures with exterior/cutaway and tall views")
	quit()


func _save(viewport: SubViewport, camera: Camera2D, output: String, name: String, record: Dictionary) -> void:
	camera.position.x = clampf(camera.position.x, viewport.size.x / camera.zoom.x / 2, 3200 - viewport.size.x / camera.zoom.x / 2)
	camera.position.y = clampf(camera.position.y, viewport.size.y / camera.zoom.y / 2, 1800 - viewport.size.y / camera.zoom.y / 2)
	for _frame in range(8):
		await process_frame
	# UI controllers can show themselves after readiness; suppress only in this capture.
	for control in viewport.find_children("*", "Control", true, false):
		control.visible = false
	RenderingServer.force_draw()
	var picture := viewport.get_texture().get_image()
	var relative := "captures/" + name + ".png"
	if picture == null or picture.get_size() != viewport.size or picture.save_png(output.path_join(relative)) != OK:
		push_error("Building gameplay capture failed: " + relative)
		quit(1)
		return
	record.captures[relative] = FileAccess.get_sha256(output.path_join(relative))
	record.frames[relative] = {"center": [camera.position.x, camera.position.y],
		"zoom": camera.zoom.x, "viewport": [viewport.size.x, viewport.size.y]}
	print("Captured ", relative)
