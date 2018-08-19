#version 330 compatibility

in vec3 in_Position;
in vec3 in_Color;
in vec3 in_Norm;      // normal in 

varying vec3 N;
varying vec3 v;

out vec3 pass_Color;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main(void) {

	N = normalize(gl_NormalMatrix * in_Norm);
	mat4 modelView = modelMatrix * viewMatrix;
	v = vec3(modelView * vec4(in_Position, 0));
	gl_Position = projectionMatrix * viewMatrix * modelMatrix * vec4(in_Position, 1.0);
	pass_Color = in_Color;
}