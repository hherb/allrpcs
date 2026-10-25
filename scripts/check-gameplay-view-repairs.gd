extends SceneTree

const OUTPUT := "res://../art-direction/gameplay-view-repairs"
const SIZES := [Vector2i(1280, 720), Vector2i(1280, 827), Vector2i(1920, 1080), Vector2i(960, 720)]


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging the smoke run.
	create_timer(300).timeout.connect(_fail.bind("Gameplay view check did not finish within 300 seconds"))
	call_deferred("_check")


func _check() -> void:
	var capture := OS.get_environment("ALLRPCS_GAMEPLAY_VIEW_CAPTURE") == "1"
	# Ordinary smoke runs only verify; evidence is written when explicitly recorded or captured.
	var record := capture or OS.get_environment("ALLRPCS_GAMEPLAY_VIEW_RECORD") == "1"
	var output := ProjectSettings.globalize_path(OUTPUT).simplify_path()
	if record and FileAccess.file_exists(output.path_join("owner-acceptance.json")):
		_fail("Accepted gameplay views cannot be overwritten")
		return
	if capture:
		DirAccess.make_dir_recursive_absolute(output.path_join("captures"))
	var report := {"status": "passed", "provenance": "controlled ordinary Main; UI retained; synthetic input", "sizes": [], "captures": {}, "inputs": {}}
	for path in ["res://Main.tscn", "res://GameplaySettingsLayout.cs", "res://RetainedStreamCrossingPresentation.cs",
		"res://art/runtime/terrain/channel-decks.png", "res://../scripts/check-gameplay-view-repairs.gd"]:
		report.inputs[path] = FileAccess.get_sha256(path)
	var viewport := SubViewport.new()
	viewport.size = SIZES[0]
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var camera := Camera2D.new()
	camera.position = Vector2(1800, 500)
	viewport.add_child(camera)
	camera.make_current()
	var packed: PackedScene = load("res://Main.tscn")
	# Baseline: the same ready Main without the two repair owners, so their startup changes are visible.
	var baseline := await _start(viewport, packed, true)
	var physical_before := _physics(baseline)
	baseline.queue_free()
	await _frames(2)
	var scene := await _start(viewport, packed, false)
	var settings: CanvasLayer = scene.get_node("SettingsOverlay")
	var model: Control = settings.get_node("Panel")
	for size in SIZES:
		viewport.size = size
		await _frames(4)
		var label := "%dx%d" % [size.x, size.y]
		for name in ["Review", "Statements", "Close"]:
			var review: Control = scene.get_node("TemporaryCharacterDialogueMisquotationEvidenceReviewPanel/" + name)
			if review.get_global_rect().intersects(scene.get_node("CommandOverlay/SaveLoadPanel").get_global_rect()) \
				or review.get_global_rect().intersects(scene.get_node("CommandOverlay/Panel").get_global_rect()):
				_fail("Evidence " + name + " overlaps the retained HUD at " + label)
				return
		if settings.visible:
			_fail("Settings did not start closed")
			return
		if capture:
			await _capture(viewport, output, "crossing-" + label, report)
		await _action(viewport, "optional_overlays_toggle")
		if not settings.visible or viewport.gui_get_focus_owner() != settings.get_node("ControlsEntry"):
			_fail("Settings did not open with the existing Controls entry focus")
			return
		# Exercise the real in-memory Apply path; do not save, activate, read a key or connect.
		model.get_node("Provider").select(3)
		model.get_node("ModelName").text = "gemma4:e4b"
		model.get_node("Apply").emit_signal("pressed")
		await _frames(2)
		if model.get_node("Status").text != "Provider settings: APPLIED · SAVE TO KEEP":
			_fail("Fitted model controls did not retain the in-memory Apply path")
			return
		var frame: Control = settings.get_node("SettingsFrame")
		var screen := Rect2(Vector2.ZERO, Vector2(size))
		if not screen.encloses(frame.get_global_rect()) or not frame.get_global_rect().encloses(model.get_global_rect()):
			_fail("Settings card or model panel exceeds viewport " + label)
			return
		var controls: Array[Control] = []
		for child in model.get_children():
			if child is Control and child.visible:
				if not model.get_global_rect().encloses(child.get_global_rect()):
					_fail("Model control out of bounds: " + str(child.name) + " at " + label)
					return
				controls.append(child)
		for i in range(controls.size()):
			for j in range(i + 1, controls.size()):
				if controls[i].get_global_rect().intersects(controls[j].get_global_rect()):
					_fail("Settings controls overlap: %s/%s at %s" % [controls[i].name, controls[j].name, label])
					return
		for name in ["ControlsEntry", "AudioEntry", "DialogueEntry", "SettingsClose"]:
			var entry: Control = settings.get_node(name)
			if not frame.get_global_rect().encloses(entry.get_global_rect()) or entry.get_global_rect().intersects(model.get_global_rect()):
				_fail("Settings entry out of card bounds or overlapping model panel")
				return
		# Native events follow the unchanged entry cycle, forwards then in reverse.
		for name in ["AudioEntry", "DialogueEntry", "SettingsClose", "ControlsEntry"]:
			await _action(viewport, "ui_focus_next")
			if viewport.gui_get_focus_owner() != settings.get_node(name):
				_fail("Settings entry focus cycle changed")
				return
		for name in ["SettingsClose", "DialogueEntry", "AudioEntry", "ControlsEntry"]:
			await _action(viewport, "ui_focus_prev")
			if viewport.gui_get_focus_owner() != settings.get_node(name):
				_fail("Settings entry reverse focus cycle changed")
				return
		if capture:
			await _capture(viewport, output, "settings-" + label, report)
		settings.get_node("SettingsClose").emit_signal("pressed")
		await _frames(2)
		if settings.visible or viewport.gui_get_focus_owner() != null:
			_fail("Settings close did not release focus")
			return
		report.sizes.append([size.x, size.y])
	var stream := scene.get_node("OpeningRegionStreamLaundrySceneSource")
	var deck: Polygon2D = stream.get_node("PaintedRetainedCrossing")
	if deck.texture.resource_path != "res://art/runtime/terrain/channel-decks.png" or deck.z_index != 0 \
		or not deck.is_visible_in_tree():
		_fail("Retained crossing lost its existing visible deck paint or actor ordering")
		return
	var crossing: Polygon2D = stream.get_node("StreamCrossingArea")
	if _bounds(deck.polygon) != _bounds(crossing.polygon).grow_individual(0, 16, 0, 16) \
		or deck.transform != crossing.transform:
		_fail("Painted deck does not cover the retained crossing and its bank aprons")
		return
	if not _opaque(deck):
		_fail("Crossing UV samples transparent deck pixels")
		return
	for name in ["StreamCrossingArea", "NorthWestCap", "NorthEastCap", "SouthWestCap", "SouthEastCap"]:
		if stream.get_node(name).self_modulate.a != 0:
			_fail("A flat crossing/cap placeholder remains painted")
			return
	if _physics(scene) != physical_before:
		_fail("View repairs changed collision shapes or transforms")
		return
	for path in report.inputs:
		if report.inputs[path] != FileAccess.get_sha256(path):
			_fail("Gameplay view source changed during verification")
			return
	if record:
		var result_path := output.path_join("capture-results.json" if capture else "native-results.json")
		var file := FileAccess.open(result_path, FileAccess.WRITE)
		if file == null:
			_fail("Cannot write %s: %s" % [result_path, FileAccess.get_open_error()])
			return
		file.store_string(JSON.stringify(report, "\t") + "\n")
	scene.queue_free()
	await _frames(2)
	print("PASS: gameplay view repairs: four viewport sizes, contained non-overlapping settings and evidence review, retained focus/close, opaque painted crossing over the retained bounds, physics unchanged from Main without the repair owners")
	quit()


func _action(viewport: SubViewport, name: String) -> void:
	var event := InputEventAction.new()
	event.action = name
	event.pressed = true
	viewport.push_input(event)
	await _frames(2)
	event = InputEventAction.new()
	event.action = name
	event.pressed = false
	viewport.push_input(event)
	await _frames(2)


func _frames(count: int) -> void:
	for _frame in range(count):
		await process_frame


func _start(viewport: SubViewport, packed: PackedScene, without_repairs: bool) -> Node2D:
	var scene: Node2D = packed.instantiate()
	if without_repairs:
		scene.get_node("RetainedStreamCrossingPresentation").free()
		scene.get_node("SettingsOverlay").set_script(null)
	viewport.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	seeker.global_position = Vector2(1800, 440)
	scene.get_node("Healer").set_physics_process(false)
	seeker.get_node("FollowCamera").enabled = false
	await _frames(10)
	return scene


# Character bodies move, so their attachments are compared relative to the body.
func _physics(scene: Node) -> Array:
	var result: Array = []
	for node in scene.find_children("*", "CollisionShape2D", true, false):
		if not node.get_parent() is CharacterBody2D:
			result.append([_path(scene, node), _stored(node.shape), _relative(scene, node), node.disabled])
	for node in scene.find_children("*", "CollisionPolygon2D", true, false):
		result.append([_path(scene, node), node.polygon, _relative(scene, node), node.disabled])
	return result


# Generated names (@StaticBody2D@43) are numbered per instance; tree order still identifies them.
func _path(scene: Node, node: Node) -> String:
	return RegEx.create_from_string("@\\d+").sub(str(scene.get_path_to(node)), "", true)


func _stored(shape: Shape2D) -> Dictionary:
	var values := {}
	for property in shape.get_property_list():
		if property.usage & PROPERTY_USAGE_STORAGE:
			values[property.name] = shape.get(property.name)
	return values


func _relative(scene: Node, node: Node) -> Transform2D:
	var transform := Transform2D.IDENTITY
	while node != scene and not node is CharacterBody2D:
		if node is Node2D:
			transform = node.transform * transform
		node = node.get_parent()
	return transform


func _bounds(points: PackedVector2Array) -> Rect2:
	var rect := Rect2(points[0], Vector2.ZERO)
	for point in points:
		rect = rect.expand(point)
	return rect


# Linear filtering blends the 2x2 texels around each sample, so all of them must be opaque.
func _opaque(deck: Polygon2D) -> bool:
	var image := Image.load_from_file(ProjectSettings.globalize_path(deck.texture.resource_path))
	var uv := deck.uv
	if image == null or uv.size() != 4:
		return false
	for y in range(129):
		for x in range(129):
			var point: Vector2 = uv[0].lerp(uv[1], x / 128.0).lerp(uv[3].lerp(uv[2], x / 128.0), y / 128.0)
			var texel := (point - Vector2(0.5, 0.5)).floor()
			for offset in [Vector2i(0, 0), Vector2i(1, 0), Vector2i(0, 1), Vector2i(1, 1)]:
				if image.get_pixelv(Vector2i(texel) + offset).a < 1.0:
					return false
	return true


func _capture(viewport: SubViewport, output: String, name: String, report: Dictionary) -> void:
	await _frames(3)
	RenderingServer.force_draw()
	var path := output.path_join("captures/" + name + ".png")
	if viewport.get_texture().get_image().save_png(path) != OK:
		_fail("Could not save UI-visible capture")
		return
	report.captures["captures/" + name + ".png"] = FileAccess.get_sha256(path)


func _fail(message: String) -> void:
	push_error("FAIL: " + message)
	quit(1)
