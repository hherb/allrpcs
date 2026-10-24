shader_type canvas_item;

// Reveal the collision-aligned painted interior without modifying source artwork.
uniform float cutaway : hint_range(0.0, 1.0) = 0.0;

void fragment() {
	COLOR.a *= 1.0 - cutaway;
}
