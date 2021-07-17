"""
This script imports PayDay 2 Model File format files to Blender.

Usage:
Execute this script from the "File->Import" menu and choose a .model file to
open.

"""

import os
import bpy
import bmesh
from bmesh.types import BMVert
from struct import unpack


class Pd2ModelImport:

	# Definition of sections type/tag :
	
    animation_data_tag                = int('5DC011B8', 16)    # Animation data
    author_tag                        = int('7623C465', 16)    # Author tag
    material_group_tag                = int('29276B1D', 16)    # Material Group
    material_tag                      = int('3C54609C', 16)    # Material
    object3D_tag                      = int('0FFCD100', 16)    # Object3D
    model_data_tag                    = int('62212D88', 16)    # Model data
    geometry_tag                      = int('7AB072D3', 16)    # Geometry
    topology_tag                      = int('4C507A13', 16)    # Topology
    passthroughGP_tag                 = int('E3A3B1CA', 16)    # PassthroughGP
    topologyIP_tag                    = int('03B634BD', 16)    # TopologyIP
    quatLinearRotationController_tag  = int('648A206C', 16)    # QuatLinearRotationController
    quatBezRotationController_tag     = int('197345A5', 16)    # QuatBezRotationController
    skinbones_tag                     = int('65CC1825', 16)    # SkinBones
    bones_tag                         = int('2EB43C77', 16)    # Bones
	# section_unknown1			      = int('803BA1FF', 16)    # ?   Ex: in str_vehicle_van_player.model
	# section_unknown2                = int('8C12A526', 16)    # ?   Ex: in str_vehicle_van_player.model

	

    def __init__(self):
        #constructor, do initialisation and stuff
        self.buff=''

    def read(self, file_path):

        try:
            f = open(file_path, "rb")
        except IOError as e:
            print("({})".format(e))

        print('import file : %s' % file_path)
        self.buff = f.read()
        f.close()


        # Unpack 4 bytes to interpret as a "little endian int" at offset 0x0 of buff
        # section_count = unpack("<i", self.buff[0:4])[0]
        # print('section count (1) : %d' % section_count)

        sections = self.parse_file()
        parsed_sections = {}

        for section in sections:

            if section[1] == self.animation_data_tag:
                print("animation_data_tag")
                parsed_sections[section[2]] = self.parse_animation_data(section[0]+12, section[3], section[2])

            elif section[1] == self.author_tag:
                print("author_tag")
                parsed_sections[section[2]] = self.parse_author(section[0]+12, section[3], section[2])

            elif section[1] == self.material_group_tag:
                print("material_group_tag")
                parsed_sections[section[2]] = self.parse_material_group(section[0]+12, section[3], section[2])

            elif section[1] == self.material_tag:
                print("material_tag")
                parsed_sections[section[2]] = self.parse_material(section[0]+12, section[3], section[2])

            elif section[1] == self.object3D_tag:
                print("object3D_tag")
                parsed_sections[section[2]] = self.parse_object3d(section[0]+12, section[3], section[2])

            elif section[1] == self.geometry_tag:
                print("geometry_tag")
                parsed_sections[section[2]] = self.parse_geometry(section[0]+12, section[3], section[2])

            elif section[1] == self.model_data_tag:
                print("geometry_tag")
                parsed_sections[section[2]] = self.parse_model_data(section[0]+12, section[3], section[2])

            elif section[1] == self.topology_tag:
                print("topology_tag")
                parsed_sections[section[2]] = self.parse_topology(section[0]+12, section[3], section[2])
 
            elif section[1] == self.passthroughGP_tag:
                print("passthroughGP_tag")
                parsed_sections[section[2]] = self.parse_passthrough_gp(section[0]+12, section[3], section[2])

            elif section[1] == self.topologyIP_tag:
                print("topologyIP_tag")
                parsed_sections[section[2]] = self.parse_topology_ip(section[0]+12, section[3], section[2])

            elif section[1] == self.quatLinearRotationController_tag:
                print("quatLinearRotationController_tag    /!\    No parser defined   /!\ ")
                #parsed_sections[section[2]] =

            elif section[1] == self.quatBezRotationController_tag:
                print("quatBezRotationController_tag    /!\    No parser defined   /!\ ")
                #parsed_sections[section[2]] =

            elif section[1] == self.skinbones_tag:
                print("skinbones_tag    /!\    No parser defined   /!\ ")
                #parsed_sections[section[2]] =

            elif section[1] == self.bones_tag:
                print("bones_tag    /!\    No parser defined   /!\ ")
                #parsed_sections[section[2]] =

            else:
                print("Unknown section tag %d" % section[1] )



        for section in sections:

            if section[1] == self.model_data_tag:
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
                self.build_model(model_id, verts, uvs, normals, faces)

        print("Import done")
		
		
    def rao(self, offset, size):
        return self.buff[offset:offset+size]

    def read_string(self, offset):
        out = ''
        while self.buff[offset] != 0:
            out += chr(self.buff[offset])
            offset += 1
        return out, offset+1

    #read 3 unsigned int (little endian) at address "offset"
    def parse_section_header(self, offset):
        return unpack("<III", self.rao(offset, 12))

    def parse_file(self):
        out_sections = []
        file_size = 0

        section_count = unpack("<i", self.rao(0, 4))[0]

        current_offset = 4

        if section_count == -1:
            file_size, section_count = unpack("<ii", self.rao(4, 8))
            current_offset += 8
            print("file_size: %d bytes, section count : %d" % (file_size, section_count))

        # Sections headers contains : uint32 section_type  // Uses one of the below tags. Tags are assigned to serializable objects within the Diesel engine.
        #                             uint32 section_id    // Appears to be a random, but unique value assigned to the section. Unknown if these have any requirements or meanings.
        #                             uint32 section size
        for x in range(section_count):
            pieces = self.parse_section_header(current_offset)
            out_sections.append((current_offset, pieces[0], pieces[1], pieces[2]))
            print("current_offset %d, p0: %u, p1: %u, p2: %u" % (current_offset, pieces[0], pieces[1], pieces[2]))
            current_offset += pieces[2] + 12
            print("next_offset %d" % current_offset)


        return out_sections

    def parse_author(self, offset, size, section_id):
        unknown = unpack("<q", self.rao(offset, 8))[0]
        email, next_offset = self.read_string(offset+8)
        source_file, next_offset = self.read_string(next_offset)
        unknown2 = unpack("<i", self.rao(next_offset, 4))[0]
        return ('Author', section_id, unknown, email, source_file, unknown2)

    def parse_material_group(self, offset, size, section_id):
        count = unpack("<i", self.rao(offset, 4))[0]
        items = []
        for x in range(count):
            items.append(unpack("<i", self.rao(offset + 4 + (x*4), 4))[0])
        return ('Material Group', section_id, count, items)

    def parse_animation_data(self, offset, size, section_id):
        unknown1, unknown2, unknown3, count = unpack("<qiii", self.rao(offset, 20))
        items = []
        for x in range(count):
            items.append(unpack("<f", roa(offset + 20 + (x*4), 4))[0])
        return ('Animation Data', section_id, unknown1, unknown2, unknown3, count, items)

    def parse_geometry(self, offset, size, section_id):
        cur_offset = offset
        # 1 is verts, 7 is uvs, 2 is normals, 20 is unkown, 21 is unknown
        size_index = [0,4,8,12,16,4,4,8,12]
        count1, count2 = unpack("<ii", self.rao(offset, 8))
        cur_offset += 8
        headers = []
        calc_size = 0
        for x in range(count2):
            item_size, item_type = unpack("<ii", self.rao(cur_offset, 8))
            calc_size += size_index[item_size]
            headers.append((item_size,item_type))
            cur_offset += 8
        verts = []
        uvs = []
        normals = []
        for header in headers:
            if header[1] == 1:
                for x in range(count1):
                    verts.append(unpack("<fff", self.rao(cur_offset, 12)))
                    cur_offset += 12
            elif header[1] == 7:
                for x in range(count1):
                    u,v = unpack("<ff", self.rao(cur_offset, 8))
                    cur_offset += 8
                    uvs.append((u, -v))
            elif header[1] == 2:
                for x in range(count1):
                    normals.append(unpack("<fff", self.rao(cur_offset, 12)))
                    cur_offset += 12
            else:
                cur_offset += size_index[header[0]] * count1
        return ('Geometry', section_id, count1, count2, headers, count1*calc_size, verts, uvs, normals)

    def parse_topology(self, offset, size, section_id):
        cur_offset = offset
        unknown1, count1 = unpack("<ii", self.rao(offset, 8))
        cur_offset += 8
        facelist = []
        for x in range(int(count1/3)):
            facelist.append(unpack("<HHH", self.rao(cur_offset, 6)))
            cur_offset += 6
        count2 = unpack("<i", self.rao(offset+8+count1*2, 4))[0]
        items2 = unpack("<"+count2*"b", self.rao(offset+8+count1*2+4, count2))
        unknown2 = unpack("<q", self.rao(offset+8+4+count2+count1*2, 8))[0]
        return ("Topology", section_id, unknown1, count1, facelist, count2, items2, unknown2)

    def parse_material(self, offset, size, section_id):
        cur_offset = offset
        unknown1 = unpack("<Q", self.rao(offset, 8))[0]
        cur_offset += 8
        return ('Material', section_id)

    def parse_object3d(self, offset, size, section_id):
        cur_offset = offset
        unknown1, count = unpack("<Qi", self.rao(offset, 12))
        cur_offset += 12
        items = []
        for x in range(count):
            items.append(unpack("<iii", self.rao(cur_offset, 12)))
            cur_offset += 12
        int_count = 64/4
        rotation_matrix = unpack("<"+int(int_count)*"f", self.rao(cur_offset, 64))
        cur_offset += 64
        position = unpack("<fff", self.rao(cur_offset, 12))
        cur_offset += 12
        unknown4 = unpack("<i", self.rao(cur_offset, 4))[0]
        return ('Object3D', section_id, unknown1, count, items, rotation_matrix, position, unknown4)

    def parse_topology_ip(self, offset, size, section_id):
        topology_section_id = unpack("<i", self.rao(offset, 4))[0]
        return ('TopologyIP', section_id, topology_section_id)

    def parse_passthrough_gp(self, offset, size, section_id):
        geometry_section, facelist_section = unpack("<ii", self.rao(offset, 8))
        return ('PassthroughGP', section_id, geometry_section, facelist_section)

    def parse_model_data(self, offset, size, section_id):
        cur_offset = offset
        unknown1, count = unpack("<Qi", self.rao(offset, 12))
        cur_offset += 12
        items = []
        for x in range(count):
            items.append(unpack("<iii", self.rao(cur_offset, 12)))
            cur_offset += 12
        int_count = 64/4
        rotation_matrix = unpack("<"+int(int_count)*"f", self.rao(cur_offset, 64))
        cur_offset += 64
        position = unpack("<fff", self.rao(cur_offset, 12))
        cur_offset += 12
        unknown4 = unpack("<i", self.rao(cur_offset, 4))[0]
        cur_offset += 4
        object3d = ('Object3D', unknown1, count, items, rotation_matrix, position, unknown4)
        version = unpack("<i", self.rao(cur_offset, 4))[0]
        cur_offset += 4
        if version == 6:
            unknown5 = unpack("<fff", self.rao(cur_offset, 12))
            cur_offset += 12
            unknown6 = unpack("<fff", self.rao(cur_offset, 12))
            cur_offset += 12
            unknown7, unknown8 = unpack("<ii", self.rao(cur_offset, 8))
            return ('Model', section_id, object3d, version, unknown5, unknown6, unknown7, unknown8)
        else:
            a, b, count2 = unpack("<iii", self.rao(cur_offset, 12))
            cur_offset += 12
            items2 = []
            for x in range(count2):
                items2.append(unpack("<iiii", self.rao(cur_offset, 16)))
                cur_offset += 16
            return ('Model', section_id, object3d, version, a, b, count2, items2)

    def build_model(self, name, verts, uvs, normals, faces):
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
                try:
                    bm.faces.new([bm.verts[face[0]], bm.verts[face[1]], bm.verts[face[2]]])
                except:
                    print("Face allready exist")

            bm.to_mesh(mesh)
            ob = bpy.data.objects.new(name, mesh)
            bpy.context.scene.objects.link(ob)
            mesh.update()

