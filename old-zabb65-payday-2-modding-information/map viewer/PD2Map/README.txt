http://slimdx.googlecode.com/files/SlimDX%20Runtime%20.NET%202.0%20%28January%202012%29.msi

You will need to install the .NET 2.0 version of SlimDX before you will be able to run this map viewer. A link to the installer is above.

To view a map choose the folder you extracted the game assets into, then pick the world.world file in the main map folder you want, not the one in the world subfolder. Pick your graphics options and click okay. Wait a little bit while all of the resources are loaded from disk.
  Controls:
  W, A, S, D - Forwards, Left, Back, Right
  E, C - Up, Down
  Escape - Quit
  1 - Toggle visibility of static objects. Defaults to on.
  2 - Toggle visibility of development objects. Defaults to on.
  6 - Toggle visibility of brushes. Defaults to on.
  F - Toggles lighting on and off. Defaults to disabled.
  Right mouse button - Hold down to rotate the camera.
  Right mouse button - Hold down to rotate the camera.
The map viewer currently does not fully understand orientation matrices in models, so models that use them may appear in the wrong orientation when displayed.
Everything is in a horrifying mess of colors because without material support, it was very very difficult to tell objects apart. I'm aware that I am using sections of the model files in unintended ways. This was originally a bug, but offered a good means of visualizing model geometry. Using the normals as vertex colors would also have worked. Models that are pure white are simply models without those sections. I may fix this in the future by adding support for materials.