import os
import json
from struct import unpack
import Tkinter, tkFileDialog

root = Tkinter.Tk()
root.withdraw()

file_path = tkFileDialog.askopenfilename()

buff = ''
strings = []

def rao(offset, size):
    return buff[offset:offset+size]

def parse_string(offset):
    out = ''
    while buff[offset] != '\0':
        out += buff[offset]
        offset += 1
    return out

def parse_string_item(offset):
    print offset
    idstring, _, offset = unpack("<qii", rao(offset, 16))
    real_string = parse_string(offset)
    return idstring, real_string

f = open(file_path, 'rb')
buff = f.read()
f.close()
item_count, start_offset = unpack("<ii", rao(8,8))
for x in xrange(item_count):
    strings.append(parse_string_item(start_offset+(x*24)))
print strings


