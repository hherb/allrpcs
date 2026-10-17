@tool
extends EditorPlugin

var shipping_export := preload("export_plugin.gd").new()

func _enter_tree() -> void:
	add_export_plugin(shipping_export)

func _exit_tree() -> void:
	remove_export_plugin(shipping_export)
	shipping_export.restore_autoloads()
