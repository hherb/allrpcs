extends SceneTree

# Verify ordinary scene presentation without modifying it or invoking providers.
const ROUTE_START := Vector2(380, 300)
const ROUTE_END := Vector2(1500, 1400)


func _initialize() -> void:
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	var routes := scene.get_node("GreyboxNavigation/RoutePresentation")
	if not routes.has_node("ShrineSouthernRoadPath"):
		_fail("Missing runtime Shrine path owner")
		return
	var owner: Node2D = routes.get_node("ShrineSouthernRoadPath")
	if owner.get_child_count() != 2 or owner.position != Vector2.ZERO \
			or owner.scale != Vector2.ONE or owner.rotation != 0 or owner.z_index != 0 \
			or not owner.visible or owner.modulate != Color.WHITE or owner.self_modulate != Color.WHITE:
		_fail("Shrine path owner must retain identity transform and two children")
		return
	var specs := [
		["IncomingPath", Vector2(1008, 940), -72, Vector2i(1152, 1080), "shrine-southern-road-path"],
		["EndpointBlend", Vector2(1480, 1360), -69, Vector2i(160, 160), "shrine-southern-road-endpoint-blend"],
	]
	for spec in specs:
		if not owner.has_node(spec[0]) or not owner.get_node(spec[0]) is Sprite2D:
			_fail("Shrine path child must be an owned sprite")
			return
		var sprite: Sprite2D = owner.get_node(spec[0])
		var resource := "res://art/runtime/terrain/%s.png" % spec[4]
		if sprite.texture == null or sprite.texture.resource_path != resource \
				or sprite.position != spec[1] or sprite.z_index != spec[2] \
				or sprite.scale != Vector2.ONE or sprite.rotation != 0 \
				or sprite.offset != Vector2.ZERO or sprite.show_behind_parent \
				or not sprite.centered or not sprite.z_as_relative or not sprite.visible \
				or sprite.modulate != Color.WHITE or sprite.self_modulate != Color.WHITE \
				or sprite.region_enabled or sprite.texture_repeat != CanvasItem.TEXTURE_REPEAT_DISABLED:
			_fail("Shrine path placement, draw order or texture ownership changed")
			return
		var decoded := sprite.texture.get_image()
		var source := Image.load_from_file(ProjectSettings.globalize_path(resource))
		decoded.convert(Image.FORMAT_RGBA8)
		source.convert(Image.FORMAT_RGBA8)
		if decoded.get_size() != spec[3] or decoded.get_data() != source.get_data():
			_fail("Imported Shrine path pixels differ from their lossless source")
			return
	for child in routes.get_children():
		if child is Line2D and child.points == PackedVector2Array([ROUTE_START, ROUTE_END]):
			_fail("Shrine greybox line remains beneath the accepted textures")
			return
	var road: Line2D = scene.get_node("OpeningRegionSouthernRoadWagonSceneSource/RoadArea")
	if road.visible or road.width != 96 \
			or road.points != PackedVector2Array([Vector2(572, 460), ROUTE_END]):
		_fail("Only the exact retained upstream road fill may be hidden")
		return
	for spec in [["SquareSouthernRoadPath", -71], ["SouthernRoadWagonPath", -70]]:
		var sprite: Sprite2D = routes.get_node(spec[0])
		if not sprite.visible or sprite.z_index != spec[1]:
			_fail("Accepted Square/Road–Wagon presentation changed")
			return
	print("Runtime Shrine–Road smoke passed: identity owner, distinct layers, decoded source parity and exact hidden fill.")
	quit()


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
