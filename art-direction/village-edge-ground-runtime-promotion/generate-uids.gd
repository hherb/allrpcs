extends SceneTree
func _initialize() -> void:
    var ids := []
    for i in range(4):
        ids.append(ResourceUID.id_to_text(ResourceUID.create_id()))
    var file := FileAccess.open(ProjectSettings.globalize_path("res://").path_join("../art-direction/village-edge-ground-runtime-promotion/native-uids.json"), FileAccess.WRITE)
    file.store_string(JSON.stringify(ids) + "\n")
    file.close()
    quit()
