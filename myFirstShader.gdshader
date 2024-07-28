shader_type canvas_item;
uniform float offset;

void fragment(){
	COLOR = texture(TEXTURE, UV); // Read from texture
	COLOR.r = (sin(TIME+offset)+1.0)*0.5f;
}