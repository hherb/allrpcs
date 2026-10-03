extends SceneTree

# Inspect ordinary imported presentation and the retained collision-backed closed gate.
func _initialize() -> void:
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	var routes := scene.get_node("GreyboxNavigation/RoutePresentation")
	if not routes.has_node("ClosedForestPath") or not routes.get_node("ClosedForestPath") is Sprite2D:
		_fail("Missing owned closed-forest runtime sprite")
		return
	var sprite: Sprite2D = routes.get_node("ClosedForestPath")
	var resource := "res://art/runtime/terrain/clearing-closed-forest-path.png"
	if sprite.texture == null or sprite.texture.resource_path != resource \
			or sprite.position != Vector2(796, 224) or sprite.z_index != -75 \
			or sprite.scale != Vector2.ONE or sprite.rotation != 0 or sprite.offset != Vector2.ZERO \
			or not sprite.centered or not sprite.z_as_relative or not sprite.visible \
			or sprite.show_behind_parent or sprite.region_enabled or sprite.get_child_count() != 0 \
			or sprite.modulate != Color.WHITE or sprite.self_modulate != Color.WHITE \
			or sprite.texture_repeat != CanvasItem.TEXTURE_REPEAT_DISABLED:
		_fail("Closed-forest placement, transform or source ownership changed")
		return
	var names := ["WoodlandVillageEdgePath", "VillageEdgeStreamPath", "StreamLaundryPath", "ClosedForestPath"]
	for index in range(names.size()):
		if routes.get_node(names[index]).get_index() != routes.get_child_count() - names.size() + index:
			_fail("Final consecutive forest/Stream sibling order changed")
			return
	var decoded := sprite.texture.get_image()
	var source := Image.load_from_file(ProjectSettings.globalize_path(resource))
	decoded.convert(Image.FORMAT_RGBA8)
	source.convert(Image.FORMAT_RGBA8)
	if decoded.get_size() != Vector2i(552, 384) or decoded.get_data() != source.get_data():
		_fail("Imported closed-forest pixels differ from source")
		return
	for child in routes.get_children():
		if child is Line2D and child.points == PackedVector2Array([Vector2(1000, 350), Vector2(650, 150)]):
			_fail("Closed-forest greybox visual line remains")
			return
	var owner := scene.get_node("OpeningRegionClosedForestPathSceneSource")
	var gate := owner.get_node("ClosedGate")
	var collision: CollisionShape2D = gate.get_node("CollisionShape2D")
	var line: Line2D = gate.get_node("Line2D")
	var shape: SegmentShape2D = collision.shape
	if collision.disabled or shape.a != Vector2(696, 200) or shape.b != Vector2(792, 200) \
			or not line.visible or line.width != 8 \
			or line.points != PackedVector2Array([shape.a, shape.b]) \
			or owner.get_node("ClosedGateMarker").position != Vector2(744, 200) \
			or owner.get_node("ApproachBoundaries").get_child_count() != 2:
		_fail("Retained closed gate or approach geometry changed")
		return
	print("Runtime closed-forest smoke passed: exact imported pixels, placement, order and closed physical gate.")
	quit()


func _fail(message: String) -> void:
	push_error(message)
	quit(1)
