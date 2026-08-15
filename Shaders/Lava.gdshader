shader_type spatial;

group_uniforms Texture;
uniform sampler2D lava_texture;
uniform float texture_scale = 1.0f;

group_uniforms Wave_Uniforms;
uniform float wave_frequency = 0.2f;
uniform float wave_amplitude = 0.5f;
uniform float wave_speed = 0.8f;

float random (vec2 uv) {
    return fract(sin(dot(uv.xy,
        vec2(12.9898,78.233))) * 43758.5453123);
}

void vertex() {
	float wave_a = sin(UV.x * texture_scale * wave_frequency + TIME * wave_speed);
	float wave_b = cos(UV.y * texture_scale * wave_frequency + TIME * (wave_speed * 0.8));
	
	float combined_wave = (wave_a+wave_b)*0.5;
	VERTEX += NORMAL * combined_wave * wave_amplitude;
}

void fragment() {
	vec2 flow = (UV * texture_scale) + vec2(TIME * 0.02, TIME * 0.01);
	ALBEDO = texture(lava_texture, flow).rgb;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
