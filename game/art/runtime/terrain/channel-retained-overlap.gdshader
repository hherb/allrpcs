shader_type canvas_item; varying vec2 world_position; void vertex() { world_position = (MODEL_MATRIX * vec4(VERTEX, 0.0, 1.0)).xy; } void fragment() { if (world_position.x < 1664.0) { discard; } }
