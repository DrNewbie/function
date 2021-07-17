using System;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Math;
using System.Windows.Forms;

namespace PD2Map
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            //FolderBrowserDialog folder_browser = new FolderBrowserDialog();
            //folder_browser.ShowDialog();
            DieselUnit.DieselUnit.BasePath = @"c:\Program Files (x86)\Steam\SteamApps\common\PAYDAY 2\assets\extract\";// folder_browser.SelectedPath + @"\";
            OpenFileDialog open_dialog = new OpenFileDialog();
            open_dialog.InitialDirectory = @"c:\Program Files (x86)\Steam\SteamApps\common\PAYDAY 2\assets\extract\"; // folder_browser.SelectedPath + @"\";
            open_dialog.Filter = "World Files (*.world)|*.world";
            if (open_dialog.ShowDialog() != DialogResult.OK)
                return;
            MapViewer viewer = new MapViewer();
            if (viewer.Setup())
            {
                KnownHashIndex.Load("hashes.txt");
                PD2MapLoader map_loader = new PD2MapLoader();
                map_loader.LoadMap(viewer, open_dialog.FileName);
                while (viewer.ShouldRun())
                {
                    viewer.Render();
                }
                viewer.Shutdown();
            }
        }
    }
}
