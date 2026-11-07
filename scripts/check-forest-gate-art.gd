extends SceneTree
## Proves ordinary Main shows the approved forest gate at the closed deeper-forest gate, posts on the
## gate's ends, drawn behind the Seeker, and hidden with the gate line when Forester access opens it,
## without changing the gate's collision.

const TEXTURE := "res://art/runtime/structures/woodland-04.png"
# The gate collision segment (OpeningRegionClosedForestPathLayout.GateBoundary); the art must not move it.
const GATE_LEFT := Vector2(696, 200)
const GATE_RIGHT := Vector2(792, 200)
# Foot centres of the left and right posts in the approved export (structures-batch woodland-04).
const LEFT_POST_FOOT := Vector2(54, 180)
const RIGHT_POST_FOOT := Vector2(278, 230)


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging the smoke run.
	create_timer(120).timeout.connect(_fail.bind("Forest gate check did not finish within 120 seconds"))
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	for _frame in range(12):
		await process_frame
	var gate: StaticBody2D = scene.get_node("OpeningRegionClosedForestPathSceneSource/ClosedGate")
	var collision: CollisionShape2D = gate.get_child(0)
	var line: Line2D = gate.get_child(1)
	var shape := collision.shape as SegmentShape2D
	if collision.disabled or shape == null or shape.a != GATE_LEFT or shape.b != GATE_RIGHT:
		_fail("The closed gate's collision changed")
		return
	var art := line.get_node_or_null("GateArt") as Sprite2D
	if art == null or art.texture == null or art.texture.resource_path != TEXTURE:
		_fail("ClosedGate/Line2D/GateArt is not the approved %s artwork" % TEXTURE)
		return
	# The line stays visible (older checks pin that) but draws nothing itself; the art is what shows.
	if not line.visible or line.self_modulate.a != 0.0 or not art.is_visible_in_tree():
		_fail("The gate art is hidden or the red greybox line still draws")
		return
	# Uncentred sprite: texture pixel p sits at global_transform * (offset + p).
	var left: Vector2 = art.global_transform * (art.offset + LEFT_POST_FOOT)
	var right: Vector2 = art.global_transform * (art.offset + RIGHT_POST_FOOT)
	if art.centered or art.rotation != 0.0 or art.flip_h or art.flip_v \
			or absf(left.x - GATE_LEFT.x) > 0.5 or absf(right.x - GATE_RIGHT.x) > 0.5 \
			or absf((left.y + right.y) / 2.0 - GATE_LEFT.y) > 0.5:
		_fail("The gate posts do not stand on the gate ends: %s, %s" % [left, right])
		return
	for node in [art, line, gate, gate.get_parent()]:
		if node.z_index != 0 or not node.z_as_relative:
			_fail("%s gained its own draw order" % node.name)
			return
	# The gate blocks the Seeker south of it, so the Seeker (depth z at least 1) must draw in front.
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	seeker.set_physics_process(false)
	seeker.global_position = Vector2(744, 230)
	for _frame in range(3):
		await process_frame
	if seeker.z_index <= 0:
		_fail("The Seeker at the gate does not draw in front of it")
		return
	# Opening the gate hides the line (OpeningRegionClosedForestPathSceneSource.TryInstallAccess); the art goes with it.
	line.visible = false
	if art.is_visible_in_tree():
		_fail("The gate art stays visible after the gate line is hidden")
		return
	print("PASS: ordinary Main shows the approved forest gate on the closed gate's ends, behind the Seeker, hidden when the gate opens; collision unchanged")
	quit(0)


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)
