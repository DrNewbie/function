import os
import json
from struct import unpack

file_path = ['systemmenu', 'blackmarket', 'debug', 'heist', 'hud', 'menu', 'savefile', 'subtitles']

for file_path_now in file_path:
	buff = ''
	strings = []

	def rao(offset, size):
		return buff[offset:offset+size]

	def parse_string(offset):
		out = bytearray()
		while buff[offset] != '\0':
			out.append(buff[offset])
			offset += 1
		return out

	def parse_string_item(offset):
		#print offset
		idstring, _, offset = unpack("<Qii", rao(offset, 16))
		real_string = parse_string(offset)
		return idstring, real_string
		
	file_path_now_strings = file_path_now + ".strings"
	file_path_now_lua_old = file_path_now + ".old"
	file_path_now_lua_new = file_path_now + ".lua"
	f = open(file_path_now_strings, 'rb')
	buff = f.read()
	f.close()

	sptxt = file_path_now.split('/')

	outname = str(sptxt[len(sptxt)-1]) + '.compare.txt'

	item_count, start_offset = unpack("<ii", rao(8,8))
	
	loaded_xfinal = []
	loaded_xreal_string = []
	loaded_flua_xfinal = ','
	for x in xrange(item_count):
		xidstring, xreal_string = parse_string_item(start_offset+(x*24))
		if xreal_string:
			xkeyhex = format(2**64 + xidstring, 'x')
			if xkeyhex[0] == '-':
				xkeyhex = format(2**128 + xidstring, 'x')
			if len(xkeyhex) == 17:
				xkeyhex = xkeyhex[1:17]
			if len(xkeyhex) == 16:
				xkeyhex = "LL" + xkeyhex + "LL"
				xkeyhex = "addation_" + xkeyhex[16] + xkeyhex[17] + xkeyhex[14] + xkeyhex[15] + xkeyhex[12] + xkeyhex[13] + xkeyhex[10] + xkeyhex[11] + xkeyhex[8] + xkeyhex[9] + xkeyhex[6] + xkeyhex[7] + xkeyhex[4] + xkeyhex[5] + xkeyhex[2] + xkeyhex[3]
			else:
				xkeyhex = "id_" + xkeyhex
			if xkeyhex:
				xfinal = xkeyhex
				xreal_string = str(xreal_string).encode('string_escape')
				loaded_xfinal.append(xfinal)
				loaded_xreal_string.insert(loaded_xfinal.index(xfinal), xreal_string)
	
	if os.path.isfile(file_path_now_lua_old):
		f = open(file_path_now_lua_old, 'r')
	else:
		f = None
	fe = open("---NEW---" + file_path_now_lua_new, 'w')
	print file_path_now_lua_old
	if fe:
		if f:
			for line in f:
				find_bool = 0
				spline = line.split('=')
				if len(spline) == 3:
					spidx3 = spline[2]
					spidx2 = spline[1]
					spidx = spline[0]
					spidx = spidx.replace("[\"", '')
					spidx = spidx.replace('\"]', '')
					spidx = str(spidx)
					loaded_flua_xfinal = loaded_flua_xfinal + "," + spidx

		for _xfinal in loaded_xfinal:
			if _xfinal not in loaded_flua_xfinal:
				_xreal_string = loaded_xreal_string[loaded_xfinal.index(_xfinal)]
				if _xreal_string and _xreal_string != "":
					_xout = "[\"" + _xfinal + "\"] = \"" + _xreal_string + "\", \n"
					if _xout.find('= \" \",') == -1:
						fe.write(_xout)
	if f:
		f.close()
	if fe:
		fe.close()