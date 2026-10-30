extends SceneTree
## Proves ordinary Main shows the approved herb sprigs for the herb patch, standing on the old
## placeholder's base line and in depth order with the Seeker, without moving the herb patch's
## interaction point.

# The interaction ranges, perception and collection measure from this node; the art must not move it.
const PATCH_POSITION := Vector2(920, 350)
# The old placeholder polygon's base line; every sprig's foot stands on it.
const FOOT_Y := 16.0
# Child name -> [texture, the approved export's proposed foot pivot (objects-batch inventory), foot X].
# Tree order is draw order: the purple sprig stands in front of the other two.
const SPRIGS := {
	"Herbs01": ["res://art/runtime/objects/herbs-01.png", Vector2(23, 41), -18.0],
	"Herbs03": ["res://art/runtime/objects/herbs-03.png", Vector2(28, 49), 18.0],
	"Herbs02": ["res://art/runtime/objects/herbs-02.png", Vector2(31, 55), 0.0],
}


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging the smoke run.
	create_timer(120).timeout.connect(_fail.bind("Herb patch check did not finish within 120 seconds"))
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	for _frame in range(12):
		await process_frame
	var patch: Node2D = scene.get_node("InteractionCandidates/HerbPatch")
	if patch.position != PATCH_POSITION:
		_fail("The herb patch interaction point moved")
		return
	var marker := patch.get_node_or_null("Marker")
	if marker == null or marker.get_class() != "Node2D" or marker.position != Vector2.ZERO \
			or marker.scale != Vector2.ONE or marker.rotation != 0.0:
		_fail("The herb patch Marker is not an unmoved Node2D holding the sprigs")
		return
	if marker.get_children().map(func(child): return String(child.name)) != SPRIGS.keys():
		_fail("The herb patch sprigs are not %s in that order" % [SPRIGS.keys()])
		return
	for sprig_name in SPRIGS:
		var expected: Array = SPRIGS[sprig_name]
		var sprig := marker.get_node(NodePath(sprig_name)) as Sprite2D
		if sprig == null or sprig.texture == null or sprig.texture.resource_path != expected[0]:
			_fail("%s is not the approved %s artwork" % [sprig_name, expected[0]])
			return
		# An unscaled, unrotated, unflipped uncentred sprite's texture pixel p sits at position + offset + p.
		var foot: Vector2 = sprig.position + sprig.offset + expected[1]
		if sprig.centered or sprig.scale != Vector2.ONE or sprig.rotation != 0.0 or sprig.flip_h or sprig.flip_v \
				or foot != Vector2(expected[2], FOOT_Y):
			_fail("%s is centred, transformed or flipped, or its foot is not on the old placeholder's base line: %s" % [sprig_name, foot])
			return
		if not sprig.is_visible_in_tree() or sprig.z_index != 0 or not sprig.z_as_relative:
			_fail("%s is hidden or gained its own draw order" % sprig_name)
			return
	if marker.z_index != 0 or not marker.z_as_relative:
		_fail("The herb patch Marker gained its own draw order")
		return
	# At equal z the presentation relies on these coming after the patch in the tree, and on the
	# patch's own z being its effective z.
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	for figure in [scene.get_node("InteractionCandidates/WaterCask"), scene.get_node("Healer"), seeker]:
		if not figure.is_greater_than(patch):
			_fail("%s no longer comes after the herb patch in Main.tscn" % figure.name)
			return
	if not patch.z_as_relative or patch.get_parent().z_index != 0:
		_fail("The herb patch's z is no longer relative to a z-0 parent")
		return
	# Depth: the Seeker in front of the sprigs when its feet are level with or south of their feet,
	# behind them 1 px north, so the presentation's foot offset must match the sprites' feet.
	# Hold both characters so only this check moves them.
	var foot_y := patch.global_position.y + FOOT_Y
	seeker.set_physics_process(false)
	scene.get_node("Healer").call("CommandWait")
	for visit in [[Vector2(920, foot_y), true], [Vector2(920, foot_y - 1.0), false]]:
		seeker.global_position = visit[0]
		for _frame in range(3):
			await process_frame
		if _draws_in_front(seeker, patch) != visit[1]:
			_fail("The Seeker at %s does not draw %s the herb patch" % [visit[0], "in front of" if visit[1] else "behind"])
			return
	print("PASS: ordinary Main shows the approved herb sprigs for the herb patch in depth order with the Seeker; the interaction point is unchanged")
	quit(0)


## Both share Main's canvas: a higher z draws in front, and at equal z the later node in the tree.
func _draws_in_front(a: Node2D, b: Node2D) -> bool:
	return a.z_index > b.z_index or (a.z_index == b.z_index and a.is_greater_than(b))


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)
