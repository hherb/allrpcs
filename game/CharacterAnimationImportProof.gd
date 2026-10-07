extends Control
## Isolated collective character display; it never loads Main or gameplay controllers.

const ART := "res://art/character-animation-import-proof/"
const DIRECTIONS := ["south", "west", "east", "north"]
const ACTIONS := ["idle", "inspect", "walk"]
var registration: Dictionary
var action := "walk"
var display_scale := 1
var preview_cadence := false
var paused := false
var scroll: ScrollContainer
var content: Control
var board: Node2D
var status: Label


func _ready() -> void:
	registration = JSON.parse_string(FileAccess.get_file_as_string(ART + "registration.json"))
	var background := ColorRect.new()
	background.color = Color("e9e5d9")
	background.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(background)
	var panel := VBoxContainer.new()
	panel.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	add_child(panel)
	var title := Label.new()
	title.text = "Approved directional characters / isolated import and registration proof"
	panel.add_child(title)
	var controls := HBoxContainer.new()
	panel.add_child(controls)
	for selected in ACTIONS:
		_button(controls, selected.capitalize(), func(): action = selected; _refresh())
	for selected_scale in [1, 2]:
		_button(controls, "%dx native" % selected_scale,
			func(): display_scale = selected_scale; _refresh())
	_button(controls, "Walk: 720 / 800 ms", func(): preview_cadence = not preview_cadence; _refresh())
	_button(controls, "Pause / play", _toggle_pause)
	_button(controls, "Previous frame", func(): _step(-1))
	_button(controls, "Next frame", func(): _step(1))
	status = Label.new()
	panel.add_child(status)
	scroll = ScrollContainer.new()
	scroll.size_flags_vertical = Control.SIZE_EXPAND_FILL
	panel.add_child(scroll)
	content = Control.new()
	scroll.add_child(content)
	_refresh()


func _button(parent: HBoxContainer, text: String, callback: Callable) -> void:
	## Add one proof-only display control.
	var button := Button.new()
	button.text = text
	button.pressed.connect(callback)
	parent.add_child(button)


func board_size(scale_value: int) -> Vector2i:
	## Reserve full texture rectangles at exact native or doubled scale.
	return Vector2i(120 + 4 * (200 * scale_value + 28), 86 + 10 * (192 * scale_value + 60))


func make_board(scale_value: int, selected_action: String, sample_ms := -1) -> Node2D:
	## Show all ten families and four directions with unchanged fixed pivots.
	var result := Node2D.new()
	var dimensions := board_size(scale_value)
	var background := ColorRect.new()
	background.color = Color("e9e5d9")
	background.size = dimensions
	result.add_child(background)
	_label(result, Vector2(12, 8), "All 480 accepted frames / %s / %dx native" % [selected_action, scale_value])
	_label(result, Vector2(12, 30), "Cross = fixed pivot; inspect repeats for comparison only; canvas-edge alpha retained")
	var column_width := 200 * scale_value + 28
	var row_height := 192 * scale_value + 60
	for direction_index in range(4):
		_label(result, Vector2(124 + direction_index * column_width, 58), DIRECTIONS[direction_index])
	var sprites: Array[AnimatedSprite2D] = []
	for family_index in range(10):
		var family: String = registration["family_order"][family_index]
		var row_top := 86 + family_index * row_height
		_label(result, Vector2(8, row_top + 14), family.replace("-", "\n"))
		for direction_index in range(4):
			var direction: String = DIRECTIONS[direction_index]
			var sprite := AnimatedSprite2D.new()
			sprite.sprite_frames = load(ART + "animations/" + family + ".tres")
			sprite.centered = false
			sprite.texture_filter = CanvasItem.TEXTURE_FILTER_LINEAR
			sprite.scale = Vector2.ONE * scale_value
			sprite.position = Vector2(120 + direction_index * column_width + column_width / 2.0,
				row_top + 192 * scale_value + 8)
			var first: Dictionary = _first_frame(family, direction, selected_action)
			sprite.offset = -Vector2(first["pivot"][0], first["pivot"][1])
			sprite.set_meta("family", family)
			sprite.set_meta("direction", direction)
			var animation := selected_action + "_" + direction
			result.add_child(sprite)
			sprite.play(animation)
			if selected_action == "walk" and preview_cadence and family != "seeker-cloak":
				sprite.speed_scale = 0.9
			if selected_action == "inspect":
				sprite.animation_finished.connect(func():
					if not paused:
						sprite.set_frame_and_progress(0, 0.0)
						sprite.play(animation))
			if sample_ms >= 0:
				sprite.pause()
				var count := sprite.sprite_frames.get_frame_count(animation)
				var frame_ms := 1000.0 / sprite.sprite_frames.get_animation_speed(animation)
				sprite.frame = posmod(int(round(sample_ms / frame_ms * 100000.0) / 100000.0), count)
			sprites.append(sprite)
			_cross(result, sprite.position)
			_label(result, Vector2(124 + direction_index * column_width, row_top + row_height - 34),
				"%d frames / pivot %s" % [sprite.sprite_frames.get_frame_count(animation), str(first["pivot"])])
	result.set_meta("sprites", sprites)
	return result


func _first_frame(family: String, direction: String, selected_action: String) -> Dictionary:
	## Look up the bound fixed canvas pivot; no per-frame recentering occurs.
	for frame in registration["frames"]:
		if frame["family"] == family and frame["direction"] == direction \
			and frame["action"] == selected_action and frame["index"] == 1:
			return frame
	return {}


func _label(parent: Node2D, position_value: Vector2, text: String) -> void:
	## Use legible labels outside the full texture rectangles.
	var label := Label.new()
	label.position = position_value
	label.text = text
	label.add_theme_color_override("font_color", Color("343c36"))
	label.add_theme_font_size_override("font_size", 14)
	parent.add_child(label)


func _cross(parent: Node2D, point: Vector2) -> void:
	## Mark the registration origin without altering sprite paint.
	for delta in [Vector2(10, 0), Vector2(0, 10)]:
		var line := Line2D.new()
		line.points = PackedVector2Array([point - delta / 2, point + delta / 2])
		line.width = 1
		line.default_color = Color("9c5344")
		parent.add_child(line)


func _refresh() -> void:
	## Rebuild only this display when its scale, action or cadence changes.
	if is_instance_valid(board):
		content.remove_child(board)
		board.queue_free()
	board = make_board(display_scale, action)
	content.custom_minimum_size = board_size(display_scale)
	content.add_child(board)
	status.text = "Idle 600 ms/frame; inspect 160 ms/frame; new walks %d ms/cycle; cloak 720 ms/cycle." \
		% (800 if preview_cadence else 720)
	if paused:
		for sprite in board.get_meta("sprites"):
			sprite.pause()


func _toggle_pause() -> void:
	## Keep synchronized manual inspection separate from gesture playback semantics.
	paused = not paused
	for sprite in board.get_meta("sprites"):
		if paused:
			sprite.pause()
		else:
			sprite.play()


func _step(delta: int) -> void:
	## Step each complete ordered sequence while holding its pivot unchanged.
	paused = true
	for sprite in board.get_meta("sprites"):
		sprite.pause()
		var count: int = sprite.sprite_frames.get_frame_count(sprite.animation)
		sprite.set_frame_and_progress(posmod(sprite.frame + delta, count), 0.0)
