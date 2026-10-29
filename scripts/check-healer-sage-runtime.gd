extends SceneTree
## Proves ordinary Main shows the approved Healer and Village Sage (Orin) art in place of their
## greybox polygons: four-direction idle/walk/inspect frames on the unchanged nodes, the Healer's
## walk following its own movement, and the Sage breathing at the unchanged hearing position.

const DIRECTIONS := ["south", "west", "east", "north"]
# Animation -> [runtime file stem, frame count, frames per second, loops].
# Walks run the reference gait's 720 ms cycle (8 x 90 ms); idle/inspect match the Seeker.
const ANIMATIONS := {
	"idle": ["idle", 2, 1000.0 / 600.0, true],
	"walk": ["walk", 8, 1000.0 / 90.0, true],
	"interact": ["inspect", 2, 1000.0 / 160.0, false],
}
# Feet sit on the bottom row of the 128x192 frames; this keeps them on the node origin.
const OFFSET := Vector2(0, -96)
const HEALER_START := Vector2(2000, 950)
const SAGE_POSITION := Vector2(2240, 950)


func _initialize() -> void:
	# A GDScript error ends the coroutine without quitting; fail instead of hanging the smoke run.
	create_timer(120).timeout.connect(_fail.bind("Healer/Sage check did not finish within 120 seconds"))
	call_deferred("_check")


func _check() -> void:
	var scene: Node2D = load("res://Main.tscn").instantiate()
	root.add_child(scene)
	var seeker: CharacterBody2D = scene.get_node("Seeker")
	var healer: CharacterBody2D = scene.get_node("Healer")
	var sage: Node2D = scene.get_node("SageHearingPosition")
	if healer.position != HEALER_START or sage.position != SAGE_POSITION:
		_fail("The Healer start or Sage hearing position moved")
		return
	for placeholder in ["Healer/Body", "Healer/Satchel", "SageHearingPosition/Body"]:
		if scene.has_node(placeholder):
			_fail("Greybox placeholder remains: " + placeholder)
			return
	var healer_art := healer.get_node_or_null("WalkAnimation") as AnimatedSprite2D
	var sage_art := sage.get_node_or_null("Figure") as AnimatedSprite2D
	if healer_art == null or sage_art == null:
		_fail("Healer/WalkAnimation or SageHearingPosition/Figure is missing")
		return
	for entry in [[healer_art, "healer"], [sage_art, "orin"]]:
		var problem := _frames_problem(entry[0], entry[1])
		if not problem.is_empty():
			_fail(problem)
			return
	if not sage.get_node("Label").visible:
		_fail("The Sage label is hidden")
		return
	for _frame in range(12):
		await process_frame
	if sage_art.animation != "idle_south" or not sage_art.is_playing():
		_fail("The Sage is not breathing in the south-facing idle loop")
		return
	# The Healer follows the Seeker; its own movement must choose the walk, and arrival the idle.
	seeker.set_physics_process(false)
	seeker.global_position = healer.global_position + Vector2(240, 0)
	if not await _await_animation(healer_art, "walk_east"):
		_fail("The following Healer did not walk east toward the Seeker")
		return
	seeker.global_position = healer.global_position + Vector2(-40, 0)
	if not await _await_animation(healer_art, "idle_east"):
		_fail("The arrived Healer did not settle into the east-facing idle loop")
		return
	print("PASS: Healer and Sage show their approved directional art on unchanged nodes; the Healer walks with its movement")
	quit(0)


## Returns "" when the sprite shows exactly the role's twelve runtime animations.
func _frames_problem(sprite: AnimatedSprite2D, role: String) -> String:
	if sprite.offset != OFFSET or not sprite.centered or sprite.position != Vector2.ZERO:
		return role + " art is not anchored with its feet on the node origin"
	var frames := sprite.sprite_frames
	if frames == null or frames.get_animation_names().size() != DIRECTIONS.size() * ANIMATIONS.size():
		return role + " SpriteFrames does not hold exactly twelve animations"
	for direction in DIRECTIONS:
		for animation in ANIMATIONS:
			var spec: Array = ANIMATIONS[animation]
			var name: String = animation + "_" + direction
			if not frames.has_animation(name) or frames.get_frame_count(name) != spec[1]:
				return role + " " + name + " is missing or has the wrong frame count"
			if absf(frames.get_animation_speed(name) - spec[2]) > 0.001 or frames.get_animation_loop(name) != spec[3]:
				return role + " " + name + " has the wrong timing or looping"
			for index in range(spec[1]):
				var expected := "res://art/runtime/characters/%s/%s/%s-%02d.png" % [role, direction, spec[0], index + 1]
				if frames.get_frame_texture(name, index).resource_path != expected:
					return role + " " + name + " frame " + str(index + 1) + " is not " + expected
	return ""


func _await_animation(sprite: AnimatedSprite2D, animation: String) -> bool:
	for _frame in range(90):
		await physics_frame
		if sprite.animation == animation and sprite.is_playing():
			return true
	return false


func _fail(message: String) -> void:
	push_error(message)
	print("FAIL: " + message)
	quit(1)
