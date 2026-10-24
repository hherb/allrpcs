extends SceneTree


func _initialize() -> void:
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	var healer: CharacterBody2D = scene.get_node("Healer")
	seeker.set_physics_process(false)
	healer.set_physics_process(false)
	for _frame in range(12):
		await process_frame
	var owner: Node2D = scene.get_node("MapStructuresPresentation")
	var physics_before := _physical_snapshot(scene)
	var patient: Node2D = scene.get_node("OpeningRegionFirstPatientSceneSource/Patient")
	var inscription: Node2D = scene.get_node("OpeningRegionOldShrineSceneSource/LayeredInscription")
	var initially_hidden := not inscription.visible
	# Approaches, interior points and a repeat visit exercise roof restoration in both directions.
	for pair in [["Sickhouse", Vector2(2528, 1270)], ["Sickhouse", Vector2(2640, 1450)],
		["Laundry", Vector2(1380, 220)], ["Laundry", Vector2(1620, 220)],
		["Laundry", Vector2(1500, 220)], ["Shrine", Vector2(660, 280)],
		["Shrine", Vector2(542.9, 500)], ["Shrine", Vector2(380, 300)],
		["Sickhouse", Vector2(2528, 1410)]]:
		seeker.global_position = pair[1]
		await process_frame
		await process_frame
		var sprite: Sprite2D = owner.get_node(pair[0])
		if sprite.material.get_shader_parameter("cutaway") != 1.0 or not patient.visible or not seeker.visible:
			_fail("Roof cutaway hid the player/patient or failed at an approach/interior")
			return
		if pair[0] == "Sickhouse" and patient.self_modulate.a != 1.0:
			_fail("Sickhouse cutaway concealed the patient")
			return
		if not initially_hidden or inscription.visible:
			_fail("Structure presentation exposed the concealed inscription")
			return
		seeker.global_position = Vector2(3000, 1700)
		await process_frame
		await process_frame
		if sprite.material.get_shader_parameter("cutaway") != 0.0:
			_fail("Exterior did not recover after the player left")
			return
		if patient.self_modulate.a != 0.0:
			_fail("The patient leaked through the closed sickhouse exterior")
			return
		for other in ["Sickhouse", "Laundry", "Shrine"]:
			if scene.get_node("MapStructuresPresentation/" + other).material.get_shader_parameter("cutaway") != 0.0:
				_fail("One building changed another building's roof material")
				return
	for pair in [["Sickhouse", Vector2(2640, 1450)], ["Laundry", Vector2(1500, 220)],
		["Shrine", Vector2(380, 300)]]:
		# The ordinary scaffold can start with the companion away on an independent task.
		# Present it explicitly for the visible-companion branch, then test its absence separately.
		healer.visible = true
		healer.global_position = pair[1]
		await process_frame
		await process_frame
		var roof: ShaderMaterial = owner.get_node(pair[0]).material
		if roof.get_shader_parameter("cutaway") != 1.0:
			_fail("A companion inside " + pair[0] + " appeared over its closed exterior")
			return
		healer.visible = false
		await process_frame
		await process_frame
		if roof.get_shader_parameter("cutaway") != 0.0:
			_fail("An absent companion kept a roof cut away")
			return
		healer.visible = true
		healer.global_position = Vector2(3000, 1700)
		await process_frame
		await process_frame
		if roof.get_shader_parameter("cutaway") != 0.0:
			_fail("A companion's departure did not restore the exterior")
			return
	if physics_before != _physical_snapshot(scene):
		_fail("Presentation changed collision nodes/shapes/transforms")
		return
	for name in ["Sickhouse", "Laundry", "Shrine", "Wagon", "VillageMeetingHouse"]:
		var sprite: Sprite2D = owner.get_node(name)
		if sprite.texture == null or sprite.z_index >= 0 or sprite.texture_filter == CanvasItem.TEXTURE_FILTER_NEAREST:
			_fail("Structure lost its texture, actor separation, or smooth filtering")
			return
	scene.queue_free()
	await process_frame
	await process_frame
	print("PASS: building gameplay preserves physical owners and concealed evidence; nine player and three companion visits keep actors readable, ignore absent companions and restore independent roofs")
	quit()


func _physical_snapshot(scene: Node) -> Array:
	var snapshot: Array = []
	for node in scene.find_children("*", "CollisionShape2D", true, false):
		if node.get_parent() is CharacterBody2D:
			continue
		snapshot.append([node.get_path(), node.shape.get_rid() if node.shape else RID(), node.global_transform, node.disabled])
	for node in scene.find_children("*", "CollisionPolygon2D", true, false):
		snapshot.append([node.get_path(), node.polygon, node.global_transform, node.disabled])
	return snapshot


func _fail(message: String) -> void:
	push_error("FAIL: " + message)
	quit(1)
