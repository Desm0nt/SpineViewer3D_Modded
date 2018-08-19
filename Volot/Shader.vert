#version 150 core

in vec3 in_Position;
in vec3 in_Color;
in vec3 in_Norm;      // normal in 

out vec3 pass_Color;
out vec3 pass_Norm;     // normal out

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main(void) {
	mat4 modelView = viewMatrix * modelMatrix;
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(in_Position, 1.0);
	pass_Color = in_Color;
	pass_Norm = in_Norm;
}