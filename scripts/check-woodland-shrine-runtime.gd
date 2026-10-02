extends SceneTree

# Check the ordinary imported sprite and retained junction without scene mutation.
const START := Vector2(1000, 350)
const END := Vector2(380, 300)


func _initialize() -> void:
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	var routes := scene.get_node("GreyboxNavigation/RoutePresentation")
	if not routes.has_node("WoodlandShrinePath") or not routes.get_node("WoodlandShrinePath") is Sprite2D:
		_fail("Missing owned Woodland–Shrine runtime sprite")
		return
	var sprite: Sprite2D = routes.get_node("WoodlandShrinePath")
	var resource := "res://art/runtime/terrain/woodland-shrine-path.png"
	if sprite.texture == null or sprite.texture.resource_path != resource \
			or sprite.position != Vector2(704, 328) or sprite.z_index != -74 \
			or sprite.scale != Vector2.ONE or sprite.rotation != 0 or sprite.offset != Vector2.ZERO \
			or not sprite.centered or not sprite.z_as_relative or not sprite.visible \
			or sprite.show_behind_parent or sprite.region_enabled \
			or sprite.modulate != Color.WHITE or sprite.self_modulate != Color.WHITE \
			or sprite.texture_repeat != CanvasItem.TEXTURE_REPEAT_DISABLED:
		_fail("Woodland–Shrine placement, draw order or source ownership changed")
		return
	var decoded := sprite.texture.get_image()
	var source := Image.load_from_file(ProjectSettings.globalize_path(resource))
	decoded.convert(Image.FORMAT_RGBA8)
	source.convert(Image.FORMAT_RGBA8)
	if decoded.get_size() != Vector2i(704, 208) or decoded.get_data() != source.get_data():
		_fail("Imported Woodland–Shrine pixels differ from accepted source")
		return
	for child in routes.get_children():
		if child is Line2D and child.points == PackedVector2Array([START, END]):
			_fail("Woodland–Shrine greybox visual line remains")
			return
	var laundry: Sprite2D = routes.get_node("LaundryClearingPath")
	var cap: Sprite2D = routes.get_node("LaundryClearingPathEndCap")
	if laundry.position != Vector2(1250, 285) or cap.position != START \
			or laundry.z_index != -70 or cap.z_index != -70 \
			or not laundry.visible or not cap.visible \
			or laundry.texture.resource_path != "res://art/runtime/terrain/straight-path.png" \
			or cap.texture.resource_path != "res://art/runtime/terrain/path-end-cap.png" \
			or cap.rotation != laundry.rotation:
		_fail("Accepted Laundry path/end cap changed")
		return
	print("Runtime Woodland–Shrine smoke passed: exact imported pixels, placement and retained Laundry join.")
	quit()


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
