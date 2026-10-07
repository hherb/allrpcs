extends Control
## Separate correction candidate: retained candidate beside side-view knee revision.

const OLD := "res://art/seeker-cloak-gait-side-revision/walk.tres"
const NEW := "res://art/seeker-cloak-pelvis-hinge-revision/walk.tres"
const DIRECTIONS := ["south", "west", "east", "north"]
var native_scale := 1
var paused := false
var canvas: Control
var board: Node2D


func _ready() -> void:
	var panel := VBoxContainer.new()
	panel.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(panel)
	var buttons := HBoxContainer.new()
	panel.add_child(buttons)
	for scale_value in [1, 2]:
		var button := Button.new()
		button.text = "%dx native" % scale_value
		button.pressed.connect(func(): native_scale = scale_value; _refresh())
		buttons.add_child(button)
	var pause := Button.new()
	pause.text = "Pause / play"
	pause.pressed.connect(func():
		paused = not paused
		for sprite in board.get_meta("sprites"):
			if paused:
				sprite.pause()
			else:
				sprite.play())
	buttons.add_child(pause)
	var step := Button.new()
	step.text = "Next frame"
	step.pressed.connect(func():
		paused = true
		for sprite in board.get_meta("sprites"):
			sprite.pause()
			var count: int = sprite.sprite_frames.get_frame_count(sprite.animation)
			sprite.set_frame_and_progress((sprite.frame + 1) % count, 0.0))
	buttons.add_child(step)
	var scroll := ScrollContainer.new()
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	panel.add_child(scroll)
	canvas = Control.new()
	scroll.add_child(canvas)
	_refresh()


func board_size(scale_value: int) -> Vector2i:
	## Retain complete rectangles and native pixels at each comparison scale.
	return Vector2i(142 + 4 * (160 * scale_value + 32), 70 + 2 * (180 * scale_value + 66))


func make_board(scale_value: int, sample_ms := -1) -> Node2D:
	## Display previous candidate and revision with the same 720ms cycle and fixed foot baseline.
	var result := Node2D.new()
	var background := ColorRect.new()
	background.color = Color("e9e5d9")
	background.size = board_size(scale_value)
	result.add_child(background)
	_label(result, Vector2(8, 8), "Seeker cloak / previous side rig vs central pelvis / backward knee hinge / %dx native" % scale_value)
	_label(result, Vector2(8, 28), "720ms cycles; anatomical left/right retained through passing; Main unchanged")
	var sprites: Array[AnimatedSprite2D] = []
	var column_width := 160 * scale_value + 32
	var row_height := 180 * scale_value + 66
	for row in range(2):
		var frames: SpriteFrames = load(OLD if row == 0 else NEW)
		var rig_path := "res://art/seeker-cloak-gait-side-revision/rig.json" if row == 0 else "res://art/seeker-cloak-pelvis-hinge-revision/rig.json"
		var rig: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(rig_path))
		_label(result, Vector2(8, 82 + row * row_height), "Previous\n8 phases" if row == 0 else "Candidate\n8 phases")
		for column in range(4):
			var direction: String = DIRECTIONS[column]
			var sprite := AnimatedSprite2D.new()
			sprite.sprite_frames = frames
			sprite.centered = false
			sprite.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
			sprite.scale = Vector2.ONE * scale_value
			sprite.position = Vector2(142 + column * column_width + column_width / 2.0,
				70 + row * row_height + 180 * scale_value)
			var width := 144 if direction in ["west", "east"] else 108
			sprite.offset = Vector2(-width / 2.0, -156)
			for asset in rig["assets"]:
				if asset["direction"] == direction and asset["index"] == 1:
					sprite.offset = -Vector2(asset["pivot"][0], asset["pivot"][1])
			result.add_child(sprite)
			sprite.play("walk_" + direction)
			if sample_ms >= 0:
				sprite.pause()
				var frame_ms := 90
				sprite.frame = int(sample_ms / frame_ms) % 8
			sprites.append(sprite)
			_label(result, Vector2(146 + column * column_width, 48 + row * row_height), direction)
			_label(result, Vector2(146 + column * column_width, 70 + row * row_height + 180 * scale_value + 12),
				"90ms/phase" if row == 0 else "90ms/phase; left/right alternate")
	result.set_meta("sprites", sprites)
	return result


func _label(parent: Node2D, point: Vector2, text: String) -> void:
	## Place legible diagnostic labels outside the sprite rectangles.
	var label := Label.new()
	label.text = text
	label.position = point
	label.add_theme_color_override("font_color", Color("343c36"))
	label.add_theme_font_size_override("font_size", 14)
	parent.add_child(label)


func _refresh() -> void:
	## Apply only display scale and playback state to this isolated candidate.
	if is_instance_valid(board):
		canvas.remove_child(board)
		board.queue_free()
	board = make_board(native_scale)
	canvas.custom_minimum_size = board_size(native_scale)
	canvas.add_child(board)
	if paused:
		for sprite in board.get_meta("sprites"):
			sprite.pause()
