extends SceneTree

# Check ordinary startup's new presentation without altering authoritative geometry.
func _initialize() -> void:
	call_deferred("_check")


func _fail(message: String) -> void:
	push_error(message)
	quit(1)


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	var owner := scene.get_node("WholeChannelPresentation")
	var names := ["water", "banks", "decks", "detail", "fringe"]
	var z_values := [-68, -67, -66, -66, -65]
	if owner.get_child_count() != 5 or owner.get_index() != scene.get_child_count() - 1:
		_fail("Channel owner must be final root child with exactly five sprites")
		return
	for index in range(names.size()):
		var sprite: Sprite2D = owner.get_child(index)
		var path := "res://art/runtime/terrain/channel-%s.png" % names[index]
		if str(sprite.name) != names[index] or sprite.texture.resource_path != path \
				or sprite.position != Vector2(1238, 522) or sprite.scale != Vector2.ONE \
				or sprite.rotation != 0 or sprite.offset != Vector2.ZERO or not sprite.centered \
				or sprite.z_index != z_values[index] or sprite.region_enabled or sprite.flip_h or sprite.flip_v \
				or sprite.texture_filter != CanvasItem.TEXTURE_FILTER_LINEAR \
				or sprite.texture_repeat != CanvasItem.TEXTURE_REPEAT_DISABLED \
				or sprite.modulate != Color.WHITE or sprite.self_modulate != Color.WHITE \
				or not sprite.visible or sprite.material != null or sprite.get_child_count() != 0:
			_fail("Channel sprite source/transform/order/filter changed")
			return
		var source := Image.load_from_file(ProjectSettings.globalize_path(path))
		var imported := sprite.texture.get_image()
		source.convert(Image.FORMAT_RGBA8)
		imported.convert(Image.FORMAT_RGBA8)
		if source.get_size() != Vector2i(2604, 620) or imported.get_data() != source.get_data():
			_fail("Channel imported RGBA differs from exact PNG source")
			return
	var stream := scene.get_node("OpeningRegionStreamLaundrySceneSource")
	var clipped := [stream.get_node("StreamWater"), stream.get_node("NorthWestBank"),
		stream.get_node("SouthWestBank")]
	var material: ShaderMaterial = clipped[0].material
	for node in clipped:
		if node.material != material or material.shader.resource_path != \
				"res://art/runtime/terrain/channel-retained-overlap.gdshader":
			_fail("Channel mask ownership differs")
			return
	for node in stream.get_children():
		if node is CanvasItem and not clipped.has(node) and node.material != null:
			_fail("Channel mask escaped retained western overlap targets")
			return
	if stream.get_node("StreamWater").polygon != PackedVector2Array([
		Vector2(1600, 452), Vector2(2000, 452), Vector2(2000, 548), Vector2(1600, 548)]) \
			or stream.get_node("StreamWater").uv != PackedVector2Array([
				Vector2(0, 0), Vector2(400, 0), Vector2(400, 96), Vector2(0, 96)]):
		_fail("Native Stream polygon/UV changed")
		return
	if stream.get_node("StreamCrossingArea").color != Color(0.62, 0.49, 0.31, 0.82):
		_fail("Native crossing colour changed")
		return
	for node in owner.find_children("*", "", true, false):
		if not node is Sprite2D:
			_fail("Channel owner gained physical or gameplay children")
			return
	var routes := scene.get_node("GreyboxNavigation/RoutePresentation")
	if str(routes.get_child(routes.get_child_count() - 1).name) != "ClosedForestPath":
		_fail("Accepted final route sibling changed")
		return
	var collision := scene.get_node("StreamCollision")
	var expected := {
		"WestCollision": PackedVector2Array([Vector2(1600, 452), Vector2(1700, 452),
			Vector2(1700, 548), Vector2(1600, 548)]),
		"EastCollision": PackedVector2Array([Vector2(1900, 452), Vector2(2000, 452),
			Vector2(2000, 548), Vector2(1900, 548)]),
	}
	for name in expected:
		if collision.get_node(name).polygon != expected[name]:
			_fail("Channel promotion changed Stream collision polygons")
			return
	print("Whole-channel runtime smoke passed: five exact imported sprites, mask-only native overlap, retained UV/crossing/route order and no physical owner.")
	quit()
