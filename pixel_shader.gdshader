shader_type canvas_item;

uniform sampler2D pixel_texture;

void fragment() {
    COLOR = texture(pixel_texture,UV);
}
