﻿#version 150 core

in vec3 in_Position;

uniform mat4 projectionMatrix;
uniform mat4 viewMatrix;
uniform mat4 modelMatrix;

void main(void) 
{
	vec4 pos = projectionMatrix * viewMatrix * modelMatrix * vec4(in_Position, 1.0);
	gl_Position = pos;
	gl_PointSize = (1.0 - pos.z * 500 / pos.w) * 64.0;// 500: scale factor, 64.0: size factor
}