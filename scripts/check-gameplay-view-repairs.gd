extends SceneTree

const OUTPUT := "res://../art-direction/gameplay-view-repairs"
const SIZES := [Vector2i(1280, 720), Vector2i(1280, 827), Vector2i(1920, 1080), Vector2i(960, 720)]


func _initialize() -> void:
	call_deferred("_check")


func _check() -> void:
	var capture := OS.get_environment("ALLRPCS_GAMEPLAY_VIEW_CAPTURE") == "1"
	var output := ProjectSettings.globalize_path(OUTPUT).simplify_path()
	if capture and FileAccess.file_exists(output.path_join("owner-acceptance.json")):
		_fail("Accepted gameplay views cannot be overwritten")
		return
	DirAccess.make_dir_recursive_absolute(output.path_join("captures"))
	var report := {"status": "passed", "provenance": "controlled ordinary Main; UI retained; synthetic input", "sizes": [], "captures": {}, "inputs": {}}
	for path in ["res://Main.tscn", "res://GameplaySettingsLayout.cs", "res://RetainedStreamCrossingPresentation.cs",
		"res://art/runtime/terrain/channel-decks.png", "res://../scripts/check-gameplay-view-repairs.gd"]:
		report.inputs[path] = FileAccess.get_sha256(path)
	var viewport := SubViewport.new()
	viewport.size = SIZES[0]
	viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
	root.add_child(viewport)
	var scene: Node2D = load("res://Main.tscn").instantiate()
	viewport.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	seeker.global_position = Vector2(1800, 440)
	scene.get_node("Healer").set_physics_process(false)
	seeker.get_node("FollowCamera").enabled = false
	var camera := Camera2D.new()
	camera.position = Vector2(1800, 500)
	viewport.add_child(camera)
	camera.make_current()
	await _frames(10)
	var physical_before := _physics(scene)
	var settings: CanvasLayer = scene.get_node("SettingsOverlay")
	var model: Control = settings.get_node("Panel")
	for size in SIZES:
		viewport.size = size
		await _frames(4)
		var label := "%dx%d" % [size.x, size.y]
		var review: Control = scene.get_node("TemporaryCharacterDialogueMisquotationEvidenceReviewPanel/Review")
		if review.get_global_rect().intersects(scene.get_node("CommandOverlay/SaveLoadPanel").get_global_rect()) \
			or review.get_global_rect().intersects(scene.get_node("CommandOverlay/Panel").get_global_rect()):
			_fail("Evidence entry overlaps the retained HUD at " + label)
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
		# Native events follow the unchanged entry cycle, including reverse traversal.
		for name in ["AudioEntry", "DialogueEntry", "SettingsClose", "ControlsEntry"]:
			await _action(viewport, "ui_focus_next")
			if viewport.gui_get_focus_owner() != settings.get_node(name):
				_fail("Settings entry focus cycle changed")
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
	if deck.texture.resource_path != "res://art/runtime/terrain/channel-decks.png" or deck.z_index != 0:
		_fail("Retained crossing lost its existing deck paint or actor ordering")
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
	var result_name := "capture-results.json" if capture else "native-results.json"
	var file := FileAccess.open(output.path_join(result_name), FileAccess.WRITE)
	file.store_string(JSON.stringify(report, "\t") + "\n")
	scene.queue_free()
	await _frames(2)
	print("PASS: gameplay view repairs: four viewport sizes, contained non-overlapping settings, retained focus/close, UI-visible views and existing painted crossing with unchanged physics")
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


func _physics(scene: Node) -> Array:
	var result: Array = []
	for node in scene.find_children("*", "CollisionShape2D", true, false):
		if not node.get_parent() is CharacterBody2D:
			result.append([node.get_path(), node.shape.get_rid(), node.global_transform, node.disabled])
	for node in scene.find_children("*", "CollisionPolygon2D", true, false):
		result.append([node.get_path(), node.polygon, node.global_transform, node.disabled])
	return result


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
