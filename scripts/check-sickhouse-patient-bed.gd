extends SceneTree
## Proves the sickhouse shows the approved bed and Mira's breathing dormant poses in ordinary
## Main, only while the sickhouse is cut away, without moving the first-patient interaction point.

const BED_TEXTURE := "res://art/runtime/objects/sickbed-01.png"
const POSE_TEXTURES := ["res://art/runtime/characters/mira/dormant-01.png",
	"res://art/runtime/characters/mira/dormant-02.png"]
# Pose 2 is painted slightly apart from pose 1; this offset registers it (see the package README).
const POSE_OFFSETS := [Vector2(0, 0), Vector2(4, -3)]
# The interaction ranges measure from this node; the art must not move it.
const PATIENT_POSITION := Vector2(2640, 1390)


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging the smoke run.
	create_timer(120).timeout.connect(_fail.bind("Sickhouse bed check did not finish within 120 seconds"))
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	var healer: CharacterBody2D = scene.get_node("Healer")
	seeker.set_physics_process(false)
	healer.set_physics_process(false)
	healer.visible = false
	for _frame in range(12):
		await process_frame
	var source: Node2D = scene.get_node("OpeningRegionFirstPatientSceneSource")
	var bed := source.get_node("Bed") as Sprite2D
	var patient := source.get_node("Patient") as Sprite2D
	if source.position != PATIENT_POSITION:
		_fail("The first-patient interaction point moved")
		return
	if bed == null or bed.texture == null or bed.texture.resource_path != BED_TEXTURE:
		_fail("The sickhouse bed is not the approved sickbed-01 artwork")
		return
	if patient == null or patient.position != Vector2.ZERO:
		_fail("The patient sprite is missing or no longer centred on the interaction point")
		return
	var breathing: AnimationPlayer = patient.get_node("Breathing")
	if not breathing.is_playing() or breathing.current_animation != "breathing":
		_fail("The patient breathing loop is not playing")
		return
	if breathing.get_animation("breathing").loop_mode != Animation.LOOP_LINEAR:
		_fail("The patient breathing loop does not repeat")
		return
	# The frames above ran the game's own processing; the loop must have advanced by itself.
	if breathing.current_animation_position <= 0.0:
		_fail("The patient breathing loop does not advance during ordinary play")
		return
	# Play forward as the game does: pose 1, the registered pose 2, then back to pose 1 after
	# the loop wraps. (Seeking backwards over a discrete key would not re-apply it.)
	breathing.seek(0.0, true)
	for step in [[0.1, 0], [1.6, 1], [1.6, 0]]:
		breathing.advance(step[0])
		var pose: int = step[1]
		if patient.texture.resource_path != POSE_TEXTURES[pose] or patient.offset != POSE_OFFSETS[pose]:
			_fail("Breathing pose %d shows the wrong texture or registration" % (pose + 1))
			return
	# The bed and patient appear only inside the cut-away sickhouse, like the old placeholders.
	if not bed.is_visible_in_tree() or not patient.is_visible_in_tree():
		_fail("The sickhouse bed or patient is hidden outright")
		return
	for visit in [[Vector2(2640, 1450), 1.0], [Vector2(3000, 1700), 0.0]]:
		seeker.global_position = visit[0]
		await process_frame
		await process_frame
		if bed.self_modulate.a != visit[1] or patient.self_modulate.a != visit[1]:
			_fail("Bed/patient visibility does not follow the sickhouse cutaway")
			return
	print("PASS: sickhouse shows approved sickbed-01 and Mira's registered breathing poses only when cut away; the interaction point is unchanged")
	quit(0)


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)
