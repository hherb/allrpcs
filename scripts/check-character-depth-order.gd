extends SceneTree
## Proves ordinary Main draws the Village Sage, water cask, Healer and Seeker in depth order: the
## one standing further south is drawn in front (#479), so the cask north of the Sage no longer
## covers the Sage's legs (#477). They share one canvas, so a higher z_index draws in front.

const SAGE_POSITION := Vector2(2240, 950)
const CASK_POSITION := Vector2(2200, 900)


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging the smoke run.
	create_timer(120).timeout.connect(_fail.bind("Depth-order check did not finish within 120 seconds"))
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	var healer: CharacterBody2D = scene.get_node("Healer")
	var sage: Node2D = scene.get_node("SageHearingPosition")
	var cask: Node2D = scene.get_node("InteractionCandidates/WaterCask")
	if sage.position != SAGE_POSITION or cask.position != CASK_POSITION:
		_fail("The Sage hearing position or the water cask moved")
		return
	# Hold both characters so only this check moves them.
	seeker.set_physics_process(false)
	healer.call("CommandWait")
	await _settle()
	var depths := [sage.z_index, cask.z_index, healer.z_index, seeker.z_index]
	var sorted_depths := depths.duplicate()
	sorted_depths.sort()
	if sorted_depths != [1, 2, 3, 4]:
		_fail("The four figures are not ranked into z 1..4: " + str(depths))
		return
	if sage.z_index <= cask.z_index:
		_fail("The water cask north of the Sage still draws over the Sage")
		return
	for sprite in [sage.get_node("Figure"), healer.get_node("WalkAnimation"), seeker.get_node("WalkAnimation")]:
		if sprite.z_index != 0 or not sprite.z_as_relative:
			_fail(sprite.get_path().get_concatenated_names() + " gained its own draw order")
			return
	# The Healer following north stops 72 px behind the Seeker: the Healer is nearer, so in front.
	seeker.global_position = healer.global_position + Vector2(0, -72)
	await _settle()
	if healer.z_index <= seeker.z_index:
		_fail("The Seeker standing north of the Healer still draws over the Healer")
		return
	seeker.global_position = healer.global_position + Vector2(0, 72)
	await _settle()
	if seeker.z_index <= healer.z_index:
		_fail("The Seeker standing south of the Healer does not draw in front")
		return
	# The Seeker at the cask, just north of the Sage, must draw behind the Sage.
	seeker.global_position = Vector2(2240, 920)
	await _settle()
	if seeker.z_index >= sage.z_index:
		_fail("The Seeker standing north of the Sage still draws over the Sage")
		return
	print("PASS: Sage, water cask, Healer and Seeker draw in depth order; the nearer one is in front")
	quit(0)


## Lets the game's own processing run, so the presenter in Main re-ranks by itself.
func _settle() -> void:
	for _frame in range(3):
		await process_frame


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)
