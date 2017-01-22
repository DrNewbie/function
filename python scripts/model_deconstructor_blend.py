import os
import bpy
import bmesh
from bmesh.types import BMVert
from struct import unpack

file_path = ""

buff = ''

def rao(offset, size):
    return buff[offset:offset+size]

def read_string(offset):
    out = ''
    while buff[offset] != 0:
        out += chr(buff[offset])
        offset += 1
    return out, offset+1

def parse_section_header(offset):
    return unpack("<III", rao(offset, 12))

def parse_file():
    out_sections = []
    file_size = 0
    section_count = unpack("<i", rao(0, 4))[0]
    current_offset = 4
    if section_count == -1:
        file_size, section_count = unpack("<ii", rao(4, 8))
        current_offset += 8
    for x in range(section_count):
        pieces = parse_section_header(current_offset)
        out_sections.append((current_offset, pieces[0], pieces[1], pieces[2]))
        current_offset += pieces[2] + 12
    return out_sections

def parse_author(offset, size, section_id):
    unknown = unpack("<q", rao(offset, 8))[0]
    email, next_offset = read_string(offset+8)
    source_file, next_offset = read_string(next_offset)
    unknown2 = unpack("<i", rao(next_offset, 4))[0]
    return ('Author', section_id, unknown, email, source_file, unknown2)

def parse_material_group(offset, size, section_id):
    count = unpack("<i", rao(offset, 4))[0]
    items = []
    for x in range(count):
        items.append(unpack("<i", rao(offset + 4 + (x*4), 4))[0])
    return ('Material Group', section_id, count, items)

def parse_animation_data(offset, size, section_id):
    unknown1, unknown2, unknown3, count = unpack("<qiii", rao(offset, 20))
    items = []
    for x in range(count):
        items.append(unpack("<f", roa(offset + 20 + (x*4), 4))[0])
    return ('Animation Data', section_id, unknown1, unknown2, unknown3, count, items)

def parse_geometry(offset, size, section_id):
    cur_offset = offset
    # 1 is verts, 7 is uvs, 2 is normals, 20 is unkown, 21 is unknown
    size_index = [0,4,8,12,16,4,4,8,12]
    count1, count2 = unpack("<ii", rao(offset, 8))
    cur_offset += 8
    headers = []
    calc_size = 0
    for x in range(count2):
        item_size, item_type = unpack("<ii", rao(cur_offset, 8))
        calc_size += size_index[item_size]
        headers.append((item_size,item_type))
        cur_offset += 8
    verts = []
    uvs = []
    normals = []
    for header in headers:
        if header[1] == 1:
            for x in range(count1):
                verts.append(unpack("<fff", rao(cur_offset, 12)))
                cur_offset += 12
        elif header[1] == 7:
            for x in range(count1):
                u,v = unpack("<ff", rao(cur_offset, 8))
                cur_offset += 8
                uvs.append((u, -v))
        elif header[1] == 2:
            for x in range(count1):
                normals.append(unpack("<fff", rao(cur_offset, 12)))
                cur_offset += 12
        else:
            cur_offset += size_index[header[0]] * count1
    return ('Geometry', section_id, count1, count2, headers, count1*calc_size, verts, uvs, normals)

def parse_topology(offset, size, section_id):
    cur_offset = offset
    unknown1, count1 = unpack("<ii", rao(offset, 8))
    cur_offset += 8
    facelist = []
    for x in range(int(count1/3)):
        facelist.append(unpack("<HHH", rao(cur_offset, 6)))
        cur_offset += 6
    count2 = unpack("<i", rao(offset+8+count1*2, 4))[0]
    items2 = unpack("<"+count2*"b", rao(offset+8+count1*2+4, count2))
    unknown2 = unpack("<q", rao(offset+8+4+count2+count1*2, 8))[0]
    return ("Topology", section_id, unknown1, count1, facelist, count2, items2, unknown2)

def parse_material(offset, size, section_id):
    cur_offset = offset
    unknown1 = unpack("<Q", rao(offset, 8))[0]
    cur_offset += 8
    return ('Material', section_id)

def parse_object3d(offset, size, section_id):
    cur_offset = offset
    unknown1, count = unpack("<Qi", rao(offset, 12))
    cur_offset += 12
    items = []
    for x in range(count):
        items.append(unpack("<iii", rao(cur_offset, 12)))
        cur_offset += 12
    int_count = 64/4
    rotation_matrix = unpack("<"+int(int_count)*"f", rao(cur_offset, 64))
    cur_offset += 64
    position = unpack("<fff", rao(cur_offset, 12))
    cur_offset += 12
    unknown4 = unpack("<i", rao(cur_offset, 4))[0]
    return ('Object3D', section_id, unknown1, count, items, rotation_matrix, position, unknown4)

def parse_topology_ip(offset, size, section_id):
    topology_section_id = unpack("<i", rao(offset, 4))[0]
    return ('TopologyIP', section_id, topology_section_id)

def parse_passthrough_gp(offset, size, section_id):
    geometry_section, facelist_section = unpack("<ii", rao(offset, 8))
    return ('PassthroughGP', section_id, geometry_section, facelist_section)

def parse_model_data(offset, size, section_id):
    cur_offset = offset
    unknown1, count = unpack("<Qi", rao(offset, 12))
    cur_offset += 12
    items = []
    for x in range(count):
        items.append(unpack("<iii", rao(cur_offset, 12)))
        cur_offset += 12
    int_count = 64/4
    rotation_matrix = unpack("<"+int(int_count)*"f", rao(cur_offset, 64))
    cur_offset += 64
    position = unpack("<fff", rao(cur_offset, 12))
    cur_offset += 12
    unknown4 = unpack("<i", rao(cur_offset, 4))[0]
    cur_offset += 4
    object3d = ('Object3D', unknown1, count, items, rotation_matrix, position, unknown4)
    version = unpack("<i", rao(cur_offset, 4))[0]
    cur_offset += 4
    if version == 6:
        unknown5 = unpack("<fff", rao(cur_offset, 12))
        cur_offset += 12
        unknown6 = unpack("<fff", rao(cur_offset, 12))
        cur_offset += 12
        unknown7, unknown8 = unpack("<ii", rao(cur_offset, 8))
        return ('Model', section_id, object3d, version, unknown5, unknown6, unknown7, unknown8)
    else:
        a, b, count2 = unpack("<iii", rao(cur_offset, 12))
        cur_offset += 12
        items2 = []
        for x in range(count2):
            items2.append(unpack("<iiii", rao(cur_offset, 16)))
            cur_offset += 16
        return ('Model', section_id, object3d, version, a, b, count2, items2)

def build_model(name, verts, uvs, normals, faces):
        mesh = bpy.data.meshes.new(name)
        #mesh.from_pydata(verts, [], faces)
        bm = bmesh.new()
        for vert in verts:
            bm.verts.new(vert)
        if len(normals) > 0:
            for x in range(len(verts)):
                bm.verts[x].normal.x = normals[x][0]
                bm.verts[x].normal.y = normals[x][1]
                bm.verts[x].normal.z = normals[x][2]
        for face in faces:
            bm.faces.new([bm.verts[face[0]], bm.verts[face[1]], bm.verts[face[2]]])
        bm.to_mesh(mesh)
        ob = bpy.data.objects.new(name, mesh)
        bpy.context.scene.objects.link(ob)
        mesh.update()

section_parsers = {0x7623C465: parse_author,
                   0x29276B1D: parse_material_group,
                   0x5DC011B8: parse_animation_data,
                   0x7AB072D3: parse_geometry,
                   0x4C507A13: parse_topology,
                   0x3C54609C: parse_material,
                   0x0FFCD100: parse_object3d,
                   0x03B634BD: parse_topology_ip,
                   0xE3A3B1CA: parse_passthrough_gp,
                   0x62212D88: parse_model_data}
                   #0x648A206C: parse_quaternion_linear_rotation,
                   #0x197345A5: parse_quaternion_bezeir_rotation}

f = open(file_path, "rb")
buff = f.read()
f.close()
sections = parse_file()
parsed_sections = {}
for section in sections:
    if section[1] in section_parsers:
        parsed_sections[section[2]] = section_parsers[section[1]](section[0]+12, section[3], section[2])

for section in sections:
    if section[1] == 0x62212D88:
        model_data = parsed_sections[section[2]]
        if model_data[3] == 6:
            continue
        model_id = "model-%x" % model_data[2][2]
        geometry = parsed_sections[parsed_sections[model_data[4]][2]]
        topology = parsed_sections[parsed_sections[model_data[4]][3]]
        faces = topology[4]
        verts = geometry[6]
        uvs = geometry[7]
        normals = geometry[8]
        build_model(model_id, verts, uvs, normals, faces)
