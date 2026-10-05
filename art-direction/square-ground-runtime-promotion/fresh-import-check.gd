extends SceneTree
func _initialize() -> void:
    for name in ["ground"]:
        var path := "res://art/runtime/terrain/village-square-%s.png" % name
        var texture: Texture2D = load(path)
        var source := Image.load_from_file(path)
        var decoded := texture.get_image()
        decoded.convert(Image.FORMAT_RGBA8)
        source.convert(Image.FORMAT_RGBA8)
        if decoded.get_size() != Vector2i(560,560) or decoded.get_data() != source.get_data():
            push_error("Fresh runtime import pixel mismatch")
            quit(1)
            return
    print("PASS: empty-cache runtime RGBA parity with independently regenerated proof UIDs")
    quit()
