extends SceneTree
## Proves ordinary Main shows the approved broken strap for the wagon clue, standing on the old
## placeholder's base line, above the greybox wagon shell and in depth order with the Seeker,
## without moving the wagon clue's interaction point.

# The interaction ranges, perception and focused search measure from this node; the art must not move it.
const CLUE_POSITION := Vector2(1900, 1640)
const TEXTURE := "res://art/runtime/objects/clue-objects-05.png"
# The approved export's proposed foot pivot (objects-batch inventory).
const PIVOT := Vector2(16, 27)
# The old placeholder polygon's lowest point; the strap's foot stands on it.
const FOOT := Vector2(0, 12)


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging the smoke run.
	create_timer(120).timeout.connect(_fail.bind("Wagon clue check did not finish within 120 seconds"))
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	for _frame in range(12):
		await process_frame
	var clue: Node2D = scene.get_node("InteractionCandidates/WagonClue")
	if clue.position != CLUE_POSITION:
		_fail("The wagon clue interaction point moved")
		return
	var strap := clue.get_node_or_null("Marker") as Sprite2D
	if strap == null or strap.texture == null or strap.texture.resource_path != TEXTURE:
		_fail("The wagon clue Marker is not the approved %s artwork" % TEXTURE)
		return
	# An unscaled, unrotated, unflipped uncentred sprite's texture pixel p sits at position + offset + p.
	var foot: Vector2 = strap.position + strap.offset + PIVOT
	if strap.centered or strap.scale != Vector2.ONE or strap.rotation != 0.0 or strap.flip_h or strap.flip_v \
			or foot != FOOT:
		_fail("The strap is centred, transformed or flipped, or its foot is not on the old placeholder's base line: %s" % foot)
		return
	if not strap.is_visible_in_tree() or strap.z_index != 0 or not strap.z_as_relative:
		_fail("The strap is hidden or gained its own draw order")
		return
	if not clue.z_as_relative or clue.get_parent().z_index != 0:
		_fail("The wagon clue's z is no longer relative to a z-0 parent")
		return
	# The greybox wagon shell comes later in the tree at z 0; the clue's depth z (at least 1) keeps it on top.
	var shell: Node2D = scene.get_node("OpeningRegionSouthernRoadWagonSceneSource/WagonShell")
	if not _draws_in_front(clue, shell):
		_fail("The wagon shell draws over the wagon clue")
		return
	# At equal z the presentation relies on the Healer and Seeker coming after the clue in the tree.
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	for figure in [scene.get_node("Healer"), seeker]:
		if not figure.is_greater_than(clue):
			_fail("%s no longer comes after the wagon clue in Main.tscn" % figure.name)
			return
	# Depth: the Seeker in front of the strap when its feet are level with or south of the strap's foot,
	# behind it 1 px north, so the presentation's foot offset must match the sprite's foot.
	# Hold both characters so only this check moves them.
	var foot_y := clue.global_position.y + FOOT.y
	seeker.set_physics_process(false)
	scene.get_node("Healer").call("CommandWait")
	for visit in [[Vector2(1900, foot_y), true], [Vector2(1900, foot_y - 1.0), false]]:
		seeker.global_position = visit[0]
		for _frame in range(3):
			await process_frame
		if _draws_in_front(seeker, clue) != visit[1]:
			_fail("The Seeker at %s does not draw %s the wagon clue" % [visit[0], "in front of" if visit[1] else "behind"])
			return
	print("PASS: ordinary Main shows the approved broken strap for the wagon clue in depth order with the Seeker; the interaction point is unchanged")
	quit(0)


## Both share Main's canvas: a higher z draws in front, and at equal z the later node in the tree.
func _draws_in_front(a: Node2D, b: Node2D) -> bool:
	return a.z_index > b.z_index or (a.z_index == b.z_index and a.is_greater_than(b))


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)
