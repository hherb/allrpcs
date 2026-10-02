extends SceneTree

# Check the ordinary imported sprite and retained junction without scene mutation.
const START := Vector2(1700, 900)
const END := Vector2(1800, 500)


func _initialize() -> void:
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	var routes := scene.get_node("GreyboxNavigation/RoutePresentation")
	if not routes.has_node("VillageEdgeStreamPath") or not routes.get_node("VillageEdgeStreamPath") is Sprite2D:
		_fail("Missing owned Village Edge–Stream runtime sprite")
		return
	var sprite: Sprite2D = routes.get_node("VillageEdgeStreamPath")
	var resource := "res://art/runtime/terrain/village-edge-stream-path.png"
	if sprite.texture == null or sprite.texture.resource_path != resource \
			or sprite.position != Vector2(1752, 704) or sprite.z_index != -75 \
			or sprite.scale != Vector2.ONE or sprite.rotation != 0 or sprite.offset != Vector2.ZERO \
			or not sprite.centered or not sprite.z_as_relative or not sprite.visible \
			or sprite.show_behind_parent or sprite.region_enabled \
			or sprite.modulate != Color.WHITE or sprite.self_modulate != Color.WHITE \
			or sprite.texture_repeat != CanvasItem.TEXTURE_REPEAT_DISABLED:
		_fail("Village Edge–Stream placement, draw order or source ownership changed")
		return
	if sprite.get_index() != routes.get_child_count() - 1:
		_fail("Village Edge–Stream must retain final sibling order")
		return
	var previous: Sprite2D = routes.get_node("WoodlandVillageEdgePath")
	if previous.get_index() != sprite.get_index() - 1 or previous.position != Vector2(1352, 632) \
			or previous.z_index != -75 or not previous.visible:
		_fail("Accepted Woodland–Village Edge order or placement changed")
		return
	var shrine: Sprite2D = routes.get_node("WoodlandShrinePath")
	if not shrine.visible or shrine.position != Vector2(704, 328) or shrine.z_index != -74:
		_fail("Accepted Woodland–Shrine presentation changed")
		return
	var decoded := sprite.texture.get_image()
	var source := Image.load_from_file(ProjectSettings.globalize_path(resource))
	decoded.convert(Image.FORMAT_RGBA8)
	source.convert(Image.FORMAT_RGBA8)
	if decoded.get_size() != Vector2i(224, 528) or decoded.get_data() != source.get_data():
		_fail("Imported Village Edge–Stream pixels differ from accepted source")
		return
	for child in routes.get_children():
		if child is Line2D and child.points == PackedVector2Array([START, END]):
			_fail("Village Edge–Stream greybox visual line remains")
			return
	var bridge: Polygon2D = scene.get_node("OpeningRegionStreamLaundrySceneSource/StreamCrossingArea")
	if bridge.polygon != PackedVector2Array([Vector2(1736, 452), Vector2(1864, 452),
			Vector2(1864, 548), Vector2(1736, 548)]) \
			or bridge.color != Color(0.62, 0.49, 0.31, 0.82) or not bridge.visible:
		_fail("Existing Stream bridge geometry, color or opacity changed")
		return
	print("Runtime Village Edge–Stream smoke passed: exact imported pixels, placement and retained bridge and sibling order.")
	quit()


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
