extends SceneTree

func _initialize() -> void:
	var result := []
	for index in range(34):
		var uid := ResourceUID.create_id()
		var value := ResourceUID.id_to_text(uid)
		if ResourceUID.text_to_id(value) != uid:
			quit(1)
			return
		result.append(value)
	var file := FileAccess.open('/tmp/allrpcs-dressing-uids.json', FileAccess.WRITE)
	file.store_string(JSON.stringify(result))
	file.close()
	print('PASS: 34 canonical distinct native Godot UIDs')
	quit()
