Due to lack of time, involvement in other projects, and lack of interest and advancement in the subject,
I (Zwagoth) am no longer actively working on this project. I have handed over maintenance to 
"I am not a spy..." and "GREAT BIG BUSHY BEARD".

Included in this repository are:
 - A bundle file extractor that is capable of handling file paths and extensions 
 as the game sees them.
 - A support library for the bundle file extractor, which allows it to compute 
 64bit lookup8 hashes.
 - Python scripts related to several internal file formats.
 - Text files that include research notes on file formats within the engine, not 
 the most organized, but it's what I used when developing these tools.
 - Two C# projects that are the result of the file format research that took place. 
 (Part of the map viewer project.)
 - A very very rough map viewer project that is based on the Axiom 3D engine.
 - A semi-functional model tool for modifying existing 3D objects in model files.
 - A diesel script tool to encode and decode diesel specific files. (sequence_manager, 
 environment, menu, continent, continents, mission, nav_data, cover_data, world, 
 world_cameras, world_sounds, prefhud)

Binaries may be obtained from the downloads section of the bitbucket page. You need 
the bundle tool to extract the contents of the bundle files for the map viewer.

Lua mods are outside the scope of this project and have been frowned upon by OVERKILL Software.
Do not ask for information about how to decompile, decrypt, or modify them. No assistence 
will be given.

Bundle Extractor Readme:
Place exe, hash64.dll, extensions.txt, and paths.txt in the games asset 
folder. Run executable through command line or by clicking on it. Wait a 
long time. Files should be extracted into the extract folder.
If you want to extract textures, sounds, and movies you must pass the 
"-extract_all" command line option to the program.

Other command line options include:
<bundle id> - use only a single bundle for extracting or listing files. 
        For example "pd2bundle.exe all_0" will extract only the contents 
        of all_0.
-list - List the path hashes and file names in bundles. Output is redirected 
        to the standard output. To save you must redirect it to a file. For 
        example "pd2bundle.exe -list > list.txt" will produce a list of all 
        files in all bundles.
  
  
Map Viewer Notes:
To view a map choose the folder you extracted the game assets into, then pick 
the world.world file in the main map folder you want, not the one in the world 
subfolder. Pick your graphics options and click okay. Wait a little bit while 
all of the resources are loaded from disk. When you see the background color 
change from black, the map is loaded. Right click on the display area, hold 
the mouse down and move it up a bit to orient the camera to the world.
  Controls:
  W, A, S, D - Forwards, Left, Back, Right
  E, C - Up, Down
  Escape - Quit
  1 - Toggle visibility of static objects. Defaults to on.
  2 - Toggle visibility of development objects. Defaults to on.
  F - Toggles lighting on and off. Defaults to disabled.
  Right mouse button - Hold down to rotate the camera.
The map viewer currently does not fully understand orientation matrices in models, 
so models that use them may appear in the wrong orientation when displayed.
Everything is in a horrifying mess of colors because without material support, 
it was very very difficult to tell objects apart. I'm aware that I am using 
sections of the model files in unintended ways. This was originally a bug, but 
offered a good means of visualizing model geometry. Using the normals as vertex 
colors would also have worked. Models that are pure white are simply models 
without those sections. I may fix this in the future by adding support for 
materials.

Your source code is a horrible mess!:
Yeah, I know. I was teaching myself C# again while writing the majority of this, 
and just wanted to get things written. I'll likely go back and clean it up later 
if there is enough interest.

I'd like to contact you:
You may contact me through Steam. My steam profile is http://steamcommunity.com/id/Zwagoth/
While I'm generally willing to help people, I will not assist you in cheating, 
hacking, or modifying the game outside of visual modifications. Don't ask, 
the answer is no.


Other Notes:
There are a large number of invalid xml files in the extracted contents that 
trip up the map viewer. These are as they exist in the bundle files. How the 
Diesel engine manages to parse these blatantly invalid files is beyond my 
understanding. Manually fixing these files is often required for some geometry 
to show up within the map viewer. The console window for the map viewer should 
detail what is wrong within the .object xml file and the location where it is 
broken within that file. 

License:
All work is released into the public domain. No copyright is claimed. All 
contents are provided as is with no guarantee or warranty. While not required 
in any way, it would be nice if you would credit me under the name Zwagoth.

Credits:
 - JPMod for their excellent bundle tool which I used as a research base for my 
own tool.
 - Szkaradek123 on the XeNTaX forums for the Bionic Commando model to blender 
 conversion script, which helped to clarify some of the finer points of the model 
 format for me.
