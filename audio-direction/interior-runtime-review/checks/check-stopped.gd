extends SceneTree

# Probe the ordinary stopped standalone scene through its installed native players.
func _initialize() -> void:
	call_deferred("_check")

func _check() -> void:
	var scene: Node = load("res://InteriorAudioRuntimeProof.tscn").instantiate()
	root.add_child(scene)
	await process_frame
	await process_frame
	if scene.get_node("OpeningRegionInteriorAudioSource").get("Automatic"):
		push_error("Standalone interior source became automatic")
		quit(1)
		return
	for owner in ["OpeningRegionMaterialAudioController", "OpeningRegionPositionalAudioController", "OpeningRegionFootstepAudioController", "OpeningRegionMusicAudioController"]:
		for player in scene.get_node(owner).get_children():
			if player is AudioStreamPlayer or player is AudioStreamPlayer2D:
				if player.playing:
					push_error("Standalone proof autoplay: " + str(player.name))
					quit(1)
					return
	print("PASS: standalone interior proof source is nonautomatic; every installed bed/contact/reply/music player starts stopped")
	quit(0)
