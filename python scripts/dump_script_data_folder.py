import os
import json
from struct import unpack
import Tkinter, tkFileDialog

root = Tkinter.Tk()
root.withdraw()

file_folder = tkFileDialog.askdirectory()

foff = 0
soff = 0
voff = 0
qoff = 0
ioff = 0
toff = 0
buff = ''

def rao(offset, size):
    return buff[offset:offset+size]

def parse_nil(value):
    return None

def parse_false(value):
    return False

def parse_true(value):
    return True

def parse_number(value):
    return unpack("<f", rao(foff+(value*4), 4))[0]

def parse_string(value):
    offset = unpack("<i", rao(soff+(value*8)+4, 4))[0]
    out = ''
    while buff[offset] != '\0':
        out += buff[offset]
        offset += 1
    return out

def parse_vector3(value):
    return unpack("<fff", rao(voff+(value*12), 12))

def parse_quat(value):
    return unpack("<ffff", rao(qoff+(value*16), 16))

def parse_idstring(value):
    return unpack("<Q", rao(ioff+(value*8), 8))[0]

def parse_table(value):
    out = {}
    off = toff+(value*20)
    meta, item, _, items = unpack("<iiii", rao(off, 16))
    if meta >= 0:
        out['_meta'] = parse_string(meta)
    for x in xrange(item):
        key_item, value_item = unpack("<ii", rao(items+(8*x), 8))
        key = parse_item(key_item)
        value = parse_item(value_item)
        out[key] = value
    return out

parsers = [parse_nil, parse_false, parse_true, parse_number, parse_string,
           parse_vector3, parse_quat, parse_idstring, parse_table]

def parse_item(raw):
    item_type = (raw >> 24) & 0xFF
    value = raw & 0xFFFFFF
    return parsers[item_type](value)

def parse_file(file_path):
    f = open(file_path, 'rb')
    global buff, foff, soff, voff, qoff, ioff, toff
    f.seek(0, os.SEEK_SET)
    buff = f.read()
    f.close()
    foff = unpack("<i", rao(12, 4))[0]
    soff = unpack("<i", rao(28, 4))[0]
    voff = unpack("<i", rao(44, 4))[0]
    qoff = unpack("<i", rao(60, 4))[0]
    ioff = unpack("<i", rao(76, 4))[0]
    toff = unpack("<i", rao(92, 4))[0]
    structure = parse_item(unpack("<i", rao(100, 4))[0])
    out = open(file_path + ".dump", "wb")
    dat = json.dumps(structure, indent=1, sort_keys=True)
    out.write(dat)
    out.close()
    return


dump_extensions = ['.continents', '.continent', '.cover_data',
                   '.nav_data', '.world_cameras', '.world', '.mission',
                   '.world_sounds']
for root_path, subFolders, files in os.walk(file_folder):
    for filename in files:
        file_path = os.path.join(root_path, filename)
        print file_path
        file_extension = os.path.splitext(file_path)[1]
        if file_extension in dump_extensions:
            parse_file(file_path)
