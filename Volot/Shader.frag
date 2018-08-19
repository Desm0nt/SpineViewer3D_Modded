#version 150 core
in vec3 pass_Color;
in vec3 pass_Norm;
out vec4 out_Color;
out vec3 out_Norm;

void main(void) {

	out_Color = vec4(pass_Color, 1.0);
	out_Norm = pass_Norm;
}