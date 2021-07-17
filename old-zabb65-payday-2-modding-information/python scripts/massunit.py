import os
import json
from struct import unpack
import Tkinter, tkFileDialog

root = Tkinter.Tk()
root.withdraw()

file_path = tkFileDialog.askopenfilename()

hoff = 0
buff = ''
units = {}

def rao(offset, size):
    return buff[offset:offset+size]

def read_instance(offset):
    position = unpack("<fff", rao(offset, 12))
    rotation = unpack("<ffff", rao(offset+12, 16))
    return position, rotation

f = open(file_path, 'rb')
buff = f.read()
f.close()
unit_count = unpack("<i", rao(0, 4))[0]
headers_offset = unpack("<i", rao(8, 4))[0]
for header in xrange(unit_count):
    header_offset = headers_offset + (32 * header)
    idstring = unpack("<q", rao(header_offset, 8))[0]
    instance_count = unpack("<i", rao(header_offset+12, 4))[0]
    instance_offset = unpack("<i", rao(header_offset+20, 4))[0]
    instances = []
    for instance in xrange(instance_count):
        instances.append(read_instance(instance_offset + (instance*28)))
    units[idstring] = instances

print units
    
