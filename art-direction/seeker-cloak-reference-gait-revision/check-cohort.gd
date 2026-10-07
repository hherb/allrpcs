extends SceneTree
func _initialize():
 call_deferred("check")
func check():
 var proof = load("res://CharacterAnimationImportProofReferenceGaitCloak.tscn").instantiate()
 root.add_child(proof)
 proof.visible = false
 var viewport = SubViewport.new()
 viewport.render_target_update_mode = SubViewport.UPDATE_ALWAYS
 root.add_child(viewport)
 for scale_value in [1, 2]:
  viewport.size = proof.board_size(scale_value)
  var board = proof.make_board(scale_value, "walk", 270)
  viewport.add_child(board)
  var sprites = board.get_meta("sprites")
  if sprites.size() != 40:
   push_error("Wrong full cohort size")
   quit(1)
   return
  for sprite in sprites:
   var expected = "res://art/seeker-cloak-reference-gait-revision/walk.tres" if sprite.get_meta("family") == "seeker-cloak" else "res://art/character-animation-import-proof/animations/" + sprite.get_meta("family") + ".tres"
   if sprite.sprite_frames.resource_path != expected or sprite.sprite_frames.get_frame_count(sprite.animation) != (16 if sprite.get_meta("family") == "seeker-cloak" and sprite.get_meta("direction") in ["west", "east"] else 8):
    push_error("Unexpected cohort frame replacement")
    quit(1)
    return
  await process_frame
  await RenderingServer.frame_post_draw
  viewport.get_texture().get_image().save_png("/Users/hherb/src/allrpcs/art-direction/seeker-cloak-reference-gait-revision/captures/full-cohort-%dx.png" % scale_value)
  viewport.remove_child(board)
  board.queue_free()
 print("PASS: corrected cloak with all nine unchanged families at native/2x scale")
 quit()
