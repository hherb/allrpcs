extends SceneTree


func _initialize() -> void:
	var assets = JSON.parse_string(FileAccess.get_file_as_string("res://assets.json"))
	var uids := {}
	for asset in assets:
		for key in ["proof_texture", "runtime_texture"]:
			var path: String = "res://" + asset[key].trim_prefix("game/")
			var texture: CompressedTexture2D = load(path)
			var source := Image.load_from_file(path)
			if texture == null or source == null:
				push_error("Cold source/import missing: " + path)
				quit(1)
				return
			var decoded := texture.get_image()
			if decoded == null:
				push_error("Cold texture decoding failed: " + path)
				quit(1)
				return
			decoded.convert(Image.FORMAT_RGBA8)
			source.convert(Image.FORMAT_RGBA8)
			var uid := ResourceLoader.get_resource_uid(path)
			if decoded.get_size() != Vector2i(asset["size"][0], asset["size"][1]) \
				or decoded.get_data() != source.get_data() or uid == ResourceUID.INVALID_ID \
				or ResourceUID.text_to_id(ResourceUID.id_to_text(uid)) != uid or uids.has(uid):
				push_error("Cold RGBA/dimensions/canonical UID mismatch: " + path)
				quit(1)
				return
			uids[uid] = true
	print("PASS: 32 exact cold RGBA imports; 16 independent proof UIDs")
	quit()
