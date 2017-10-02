SCREEN_SIZE = (800, 600)


import OpenGL.GL as GL
import OpenGL.GL.shaders
import ctypes
import pygame
import numpy
from vector import Vector3
from matrix44 import Matrix44
import csv
import math

# SHADERS

vertex_shader = """
#version 330
#extension GL_ARB_explicit_uniform_location : enable

layout(location=0) in vec3 position;
layout(location=1) in vec4 color;

layout(location=2) uniform mat4 model;
layout(location=3) uniform mat4 view;
layout(location=4) uniform mat4 proj;

out vec4 vcolor;

void main()
{
   gl_Position = proj * view * model * vec4(position, 1.0);
   vcolor = color;
}
"""

fragment_shader = """
#version 330
#extension GL_ARB_explicit_uniform_location : enable

layout(location=0) uniform float time;

in vec4 vcolor;

void main()
{
   float t = fract(time); 
   vec4 c = vcolor;
   gl_FragColor = vec4(c.xyz, 1.0f);
}
"""

# GLOBALS

# in data
gVertices = []
gColors = []

# MVP matrices
model = Matrix44.identity()
view = Matrix44.identity()
proj = Matrix44.identity()
view_mod = Matrix44.identity()
model_mod = Matrix44.identity()

# mouse vars
last_click_pos = [(-1, -1), (-1, -1), (-1, -1)]

# display vars
res = (1024, 1024)

# Read data into vertices
# TODO: Streamline this

with open('../data/acl_ph.txt') as csvfile:
    reader = csv.reader(csvfile, delimiter='\t')
    for row in reader:
        gVertices.append(float(row[0]) * 1)
        gVertices.append(float(row[1]) * 1)
        gVertices.append(float(row[2]) * 1)

        gColors.append(0.0)
        gColors.append(1.0)
        gColors.append(1.0)
        gColors.append(1.0)

with open('../data/acl_gt.txt') as csvfile:
    reader = csv.reader(csvfile, delimiter='\t')
    for row in reader:
        gVertices.append(float(row[0]) * 1)
        gVertices.append(float(row[1]) * 1)
        gVertices.append(float(row[2]) * 1)

        gColors.append(1.0)
        gColors.append(0.0)
        gColors.append(1.0)
        gColors.append(1.0)

gVertices = numpy.array(gVertices, dtype=numpy.float32)
gColors = numpy.array(gColors, dtype=numpy.float32)

simtime = 0.0


# functions

def create_axes_vbo(shader, minCoords, maxCoords):
    x_line = numpy.array([minCoords.x, 0.0, 0.0, maxCoords.x, 0.0, 0.0], dtype=numpy.float32)
    y_line = numpy.array([0.0, minCoords.y, 0.0, 0.0, maxCoords.y, 0.0], dtype=numpy.float32)
    z_line = numpy.array([0.0, 0.0, minCoords.z, 0.0, 0.0, maxCoords.z], dtype=numpy.float32)

    x_colors = numpy.array([1.0, 0.0, 0.0, 1.0, 1.0, 0.0, 0.0, 1.0], dtype=numpy.float32)
    y_colors = numpy.array([0.0, 1.0, 0.0, 1.0, 0.0, 1.0, 0.0, 1.0], dtype=numpy.float32)
    z_colors = numpy.array([0.0, 0.0, 1.0, 1.0, 0.0, 0.0, 1.0, 1.0], dtype=numpy.float32)

    return [create_vertex_array_object(shader, x_line, x_colors), create_vertex_array_object(shader, y_line, y_colors), create_vertex_array_object(shader, z_line, z_colors)]

def create_vertex_array_object(shader, vertices, colors):
    vertex_array_object = GL.glGenVertexArrays(1)
    GL.glBindVertexArray( vertex_array_object )

    vert_buff = create_attribute(0, 3, vertices)
    colors_buff = create_attribute(1, 4, colors)

    # Unbind the VAO first (Important)
    GL.glBindVertexArray( 0 )

    # Unbind other stuff
    GL.glDisableVertexAttribArray(0)
    GL.glDisableVertexAttribArray(1)

    GL.glBindBuffer(GL.GL_ARRAY_BUFFER, 0)

    return vertex_array_object

def create_attribute(slot, num_components, data, usage = GL.GL_STATIC_DRAW):
    # Generate buffers to hold our attribute
    vertex_buffer = GL.glGenBuffers(1)
    GL.glBindBuffer(GL.GL_ARRAY_BUFFER, vertex_buffer)

    # Send the data over to the buffer
    GL.glBufferData(GL.GL_ARRAY_BUFFER, data.nbytes, data, usage)

    GL.glBindBuffer(GL.GL_ARRAY_BUFFER, vertex_buffer)
    GL.glEnableVertexAttribArray(slot)
    GL.glVertexAttribPointer(slot, num_components, GL.GL_FLOAT, False, 0, ctypes.c_void_p(0))

    return vertex_buffer



def display(shader, vertex_array_object, time, glMode, vert_len):
    GL.glUseProgram(shader)

    GL.glUniform1f(0,time)
    modelUniform = GL.glGetUniformLocation(shader, 'model')
    GL.glUniformMatrix4fv(modelUniform, 1, GL.GL_FALSE, numpy.array((model_mod * model).to_opengl(), numpy.float32))
    viewUniform = GL.glGetUniformLocation(shader, 'view')
    GL.glUniformMatrix4fv(viewUniform, 1, GL.GL_FALSE, numpy.array((view_mod * view).to_opengl(), numpy.float32))
    projUniform = GL.glGetUniformLocation(shader, 'proj')
    GL.glUniformMatrix4fv(projUniform, 1, GL.GL_FALSE, numpy.array(proj.to_opengl(), numpy.float32))

    GL.glBindVertexArray( vertex_array_object )
    GL.glPointSize(3 * 1024 / res[0])
    
    GL.glDrawArrays(glMode, 0, vert_len)
    GL.glBindVertexArray( 0 )

    GL.glUseProgram(0)

#TODO: TEST THIS ROUTINE
def scale_to_01_cube(arr):
    i = 0
    tmpverts = []
    pmin = pmax = None
    for start in range(arr.shape[0]/3):
        i = start * 3
        v = Vector3(arr[i], arr[i+1], arr[i+2])
        tmpverts.append(v)
        if pmin is None:
            pmin = v
            continue

        if pmax is None:
            pmax = v
            continue

        if pmin[0] > v[0]:
            pmin[0] = v[0]
        if pmin[1] > v[1]:
            pmin[1] = v[1]
        if pmin[2] > v[2]:
            pmin[2] = v[2]

        if pmax[0] < v[0]:
            pmax[0] = v[0]
        if pmax[1] < v[1]:
            pmax[1] = v[1]
        if pmax[2] < v[2]:
            pmax[2] = v[2]


    diff = pmax - pmin
    biggest = max(diff.as_tuple())

    assert(biggest > 0)
    scaling = 1.0/biggest

    # Put all values into positive range
    # Then scale into [0,1]
    for v in tmpverts:
        v += pmin
        v.scale(scaling)

    # put the data back into the original array
    for start in range(arr.shape[0]/3):
        v = tmpverts[start]
        i = start * 3
        arr[i] = v[0]
        arr[i+1] = v[1]
        arr[i+2] = v[2]

def screen_to_arcball(screen_vec):
    ret = Vector3()
    ret.x = -(screen_vec[0] / float(res[0]) * 2.0 - 1.0)
    ret.y = -(screen_vec[1] / float(res[1]) * 2.0 - 1.0)
    length2 = ret.x * ret.x + ret.y * ret.y
    if (length2 <= 1):
        ret.z = math.sqrt(1 - length2)
    else:
        ret.x /= math.sqrt(length2)
        ret.y /= math.sqrt(length2)

    return ret

def handle_arcball():
    global last_click_pos
    global model_mod
    global model
    global view_mod
    global view

    # LMB = rotate arcball
    if (pygame.mouse.get_pressed()[0]):
        if (last_click_pos[0] == (-1, -1)):
            last_click_pos[0] = pygame.mouse.get_pos()

        v_new = screen_to_arcball(pygame.mouse.get_pos())
        v_old = screen_to_arcball(last_click_pos[0])

        angle = math.acos(min(1.0, v_new.dot(v_old)))
        axis = (v_new.cross(v_old)).normalize()
        model_mod = Matrix44.rotation_about_axis(axis, angle)
    else:
        if (last_click_pos[0] != (-1, -1)):
            last_click_pos[0] = (-1, -1)
            model = model_mod * model
            model_mod = Matrix44.identity()

    # MMB = translate arcball
    if (pygame.mouse.get_pressed()[1]):
        if (last_click_pos[1] == (-1, -1)):
            last_click_pos[1] = pygame.mouse.get_pos()

        dx = (pygame.mouse.get_pos()[0] - last_click_pos[1][0]) / float(res[0])
        dy = (pygame.mouse.get_pos()[1] - last_click_pos[1][1]) / float(res[1])

        view_mod = Matrix44.translation(-dx, -dy, 0)
    else:
        if (last_click_pos[1] != (-1, -1)):
            last_click_pos[1] = (-1, -1)
            view = view_mod * view
            view_mod = Matrix44.identity()

    # RMB = scale arcball (based on RMB delta y)
    if (pygame.mouse.get_pressed()[2]):
        if (last_click_pos[2] == (-1, -1)):
            last_click_pos[2] = pygame.mouse.get_pos()

        dy = (pygame.mouse.get_pos()[1] - last_click_pos[2][1]) / float(res[1])
        model_mod = Matrix44.scale(max(-dy + 1.0, 0.01))
    else:
        if (last_click_pos[2] != (-1, -1)):
            last_click_pos[2] = (-1, -1)
            model = model_mod * model
            model_mod = Matrix44.identity()

def main():
    pygame.init()
    screen = pygame.display.set_mode(res, pygame.OPENGL|pygame.DOUBLEBUF)
    pygame.display.set_caption("data renderer")
    print(GL.glGetString(GL.GL_VERSION))

    GL.glClearColor(0.5, 0.5, 0.5, 1.0)
    GL.glEnable(GL.GL_DEPTH_TEST)

    shader = OpenGL.GL.shaders.compileProgram(
        OpenGL.GL.shaders.compileShader(vertex_shader, GL.GL_VERTEX_SHADER),
        OpenGL.GL.shaders.compileShader(fragment_shader, GL.GL_FRAGMENT_SHADER)
    )

    vertex_array_object = create_vertex_array_object(shader, gVertices, gColors)
    line_vbos = create_axes_vbo(shader, Vector3.from_floats(-3.0, -3.0, -3.0), Vector3.from_floats(3.0, 3.0, 3.0))

    #convert_array_to_vec3(vertices)

    clock = pygame.time.Clock()
    global view
    view = Matrix44().translation(0.0, 0.0, -5.0)
    global proj
    proj = Matrix44.perspective_projection_fov(math.pi / 2, res[0] / float(res[1]), 0.1, 100.0)

    while True:
        for event in pygame.event.get():
            if event.type == pygame.QUIT:
                return
            if event.type == pygame.KEYUP and event.key == pygame.K_ESCAPE:
                return
        clock.tick()
        simtime = pygame.time.get_ticks() * 0.001
        handle_arcball()

        GL.glClear(GL.GL_COLOR_BUFFER_BIT | GL.GL_DEPTH_BUFFER_BIT)

        display(shader, vertex_array_object, simtime, GL.GL_POINTS, int(len(gVertices) / 3))
        display(shader, line_vbos[0], simtime, GL.GL_LINES, 2)
        display(shader, line_vbos[1], simtime, GL.GL_LINES, 2)
        display(shader, line_vbos[2], simtime, GL.GL_LINES, 2)
        pygame.display.flip()

if __name__ == '__main__':
    try:
        main()
    finally:
        pygame.quit()
