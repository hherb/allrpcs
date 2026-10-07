extends Node
## Disposable offline audition: copied accepted buses, no game scene or settings writes.

func _ready() -> void:
	var config_path := OS.get_environment("ALLRPCS_AUDITION_CONFIG")
	var config: Dictionary = JSON.parse_string(FileAccess.get_file_as_string(config_path))
	AudioServer.set_bus_layout(load("res://AudioBusLayout.tres"))
	AudioServer.set_bus_effect_enabled(0, 0, config.get("reduced_range", false))
	AudioServer.set_bus_effect_enabled(0, 1, config.get("mono", false))
	AudioServer.set_bus_volume_db(0, config.get("master_gain_db", 0.0))
	AudioServer.set_bus_mute(0, config.get("master_mute", false))
	var loaded: Array = []
	for track: Dictionary in config["tracks"]:
		var bus := AudioServer.get_bus_index(track["bus"])
		assert(bus > 0, "Only named sibling buses are allowed")
		AudioServer.set_bus_volume_db(bus, track.get("gain_db", 0.0))
		AudioServer.set_bus_mute(bus, track.get("mute", false))
		var stream := AudioStreamWAV.load_from_file(track["path"])
		assert(stream != null, "Could not load retained PCM")
		var player := AudioStreamPlayer.new()
		player.stream = stream
		player.bus = track["bus"]
		add_child(player)
		player.play()
		loaded.append({"bus": track["bus"], "stream_format": stream.format,
			"mix_rate": stream.mix_rate, "stereo": stream.stereo, "length": stream.get_length()})
	print("AUDITION_INPUTS ", JSON.stringify(loaded))

func _exit_tree() -> void:
	for child in get_children():
		if child is AudioStreamPlayer:
			child.stop()
			child.stream = null
