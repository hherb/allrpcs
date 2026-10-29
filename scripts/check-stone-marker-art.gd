extends SceneTree
## Proves ordinary Main shows the approved standing-tablet art for the stone marker, standing on
## the old placeholder's base line and in depth order with the Seeker, without moving the stone
## marker's interaction point.

const TABLET_TEXTURE := "res://art/runtime/objects/inscriptions-01.png"
# The interaction ranges and perception measure from this node; the art must not move it.
const MARKER_POSITION := Vector2(1080, 350)
# The approved export's proposed foot pivot (objects-batch inventory), in texture pixels.
const TABLET_FOOT := Vector2(41, 75)
# The old placeholder polygon's base line; the tablet's foot stands on it.
const FOOT_Y := 18.0


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging the smoke run.
	create_timer(120).timeout.connect(_fail.bind("Stone marker check did not finish within 120 seconds"))
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	for _frame in range(12):
		await process_frame
	var marker: Node2D = scene.get_node("InteractionCandidates/StoneMarker")
	if marker.position != MARKER_POSITION:
		_fail("The stone marker interaction point moved")
		return
	var tablet := marker.get_node("Marker") as Sprite2D
	if tablet == null or tablet.texture == null or tablet.texture.resource_path != TABLET_TEXTURE:
		_fail("The stone marker is not the approved inscriptions-01 artwork")
		return
	# A centred sprite's texture pixel p sits at position + offset + p - size / 2.
	var foot := tablet.position + tablet.offset + TABLET_FOOT - tablet.texture.get_size() / 2.0
	if not tablet.centered or foot != Vector2(0, FOOT_Y):
		_fail("The tablet's foot is not centred on the old placeholder's base line: %s" % foot)
		return
	if not tablet.is_visible_in_tree():
		_fail("The stone marker art is hidden")
		return
	if tablet.z_index != 0 or not tablet.z_as_relative:
		_fail("The tablet gained its own draw order")
		return
	# Depth: the Seeker in front of the tablet when its feet are south of the tablet's foot (y 368),
	# behind it when north. Hold both characters so only this check moves them.
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	scene.get_node("Healer").call("CommandWait")
	for visit in [[Vector2(1080, 470), true], [Vector2(1080, 345), false]]:
		seeker.global_position = visit[0]
		for _frame in range(3):
			await process_frame
		if _draws_in_front(seeker, marker) != visit[1]:
			_fail("The Seeker at %s does not draw %s the stone marker" % [visit[0], "in front of" if visit[1] else "behind"])
			return
	print("PASS: ordinary Main shows the approved standing tablet for the stone marker in depth order with the Seeker; the interaction point is unchanged")
	quit(0)


## Both share Main's canvas: a higher z draws in front, and at equal z the later node in the tree.
func _draws_in_front(a: Node2D, b: Node2D) -> bool:
	return a.z_index > b.z_index or (a.z_index == b.z_index and a.is_greater_than(b))


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)
