extends SceneTree
func _initialize():
    for name in ['OpeningRegionPositionalAudioController', 'PositionalAudioRuntimeSmokeCheck', 'PositionalSoundCueHistoryPanel', 'PositionalAudioRuntimeProof']:
        var file = FileAccess.open('res://' + name + '.cs.uid', FileAccess.WRITE)
        file.store_string(ResourceUID.id_to_text(ResourceUID.create_id()) + '\n')
    quit()
