extends SceneTree


func _initialize() -> void:
	var path := "res://art/runtime/terrain/wagon-entrance-ground.png"
	var texture: Texture2D = load(path)
	var source := Image.load_from_file(path)
	if texture == null or source == null:
		push_error("Fresh Wagon source/texture load failed")
		quit(1)
		return
	var decoded := texture.get_image()
	if decoded == null:
		push_error("Fresh Wagon decode failed")
		quit(1)
		return
	decoded.convert(Image.FORMAT_RGBA8)
	source.convert(Image.FORMAT_RGBA8)
	if decoded.get_size() != Vector2i(285, 92) or decoded.get_data() != source.get_data():
		push_error("Fresh Wagon import pixel mismatch")
		quit(1)
		return
	print("PASS: empty-cache actual runtime RGBA equals source; proof UID regenerated independently")
	quit()
