using System;
using System.Collections.Generic;
using Axiom.Core;
using Axiom.Graphics;
using Axiom.Math;
using Axiom.Input;
using Axiom.Framework.Configuration;

namespace PD2Map
{

    enum ObjectLayers
    {
        DevObjects = 1 << 0,
        StaticObjects = 1 << 1,
        ScriptedObjects = 1 << 2,
        DisabledObjects = 1 << 3,
        ScriptObjects = 1 << 4,
        MassunitObjects = 1 << 5
    }

    public class MapViewer
    {

        private Root root;
        public Camera Camera;
        private Viewport viewport;
        private RenderWindow renderWindow;
        private InputReader input;
        public SceneNode SceneRoot;
        public SceneManager Scene;
        private bool isRunning = true;
        private bool lightingEnabled = false;
        private readonly Timer keyDebounceTimer = new Timer();

        public bool Setup()
        {
            IConfigurationManager ConfigurationManager = ConfigurationManagerFactory.CreateDefault();
            this.root = new Root("PD2Map.log");
            if (!ConfigurationManager.ShowConfigDialog(this.root))
                return false;
            renderWindow = this.root.Initialize(true);
            this.input = PlatformManager.Instance.CreateInputReader();
            this.input.Initialize(renderWindow, true, true, false, true);
            Scene = this.root.CreateSceneManager("OctreeSceneManager");
            Scene.ShadowColor = new ColorEx(0.15f, 0.15f, 0.15f);
            Scene.ShadowFarDistance = 1500;
            Scene.ShadowTextureCount = 1;
            Scene.ShadowTextureSelfShadow = false;
            Scene.ShadowTechnique = ShadowTechnique.TextureModulative;
            Scene.ShadowTextureSize = 1024;
            ResourceGroupManager.Instance.AddResourceLocation("resources", "Folder");
            TextureManager.Instance.DefaultMipmapCount = 5;
            ResourceGroupManager.Instance.InitializeAllResourceGroups();
            Camera = Scene.CreateCamera("RootCamera");
            Camera.Near = 15.0f;
            Camera.Far = 15000.0f;
            Camera.FieldOfView = (float)(new Degree((Real)85.0f).InRadians);
            Camera.FixedYawAxis = new Vector3(0, 0, 1);
            //Scene.VisibilityMask = (ulong)0xffffffff;
            viewport = renderWindow.AddViewport(Camera);
            viewport.ShowShadows = false;
            this.root.FrameStarted += new EventHandler<FrameEventArgs>(FrameStarting);
            viewport.BackgroundColor = new ColorEx(0.75f, 0.75f, 1.0f);
            SceneRoot = Scene.RootSceneNode;
            Material defaultPD2Material = (Material)MaterialManager.Instance.Create("default_pd2", ResourceGroupManager.DefaultResourceGroupName);
            defaultPD2Material.ApplyDefaults();
            defaultPD2Material.Load();
            defaultPD2Material.Lighting = lightingEnabled;
            defaultPD2Material.ShadingMode = Shading.Phong;
            defaultPD2Material.Diffuse = new ColorEx(0.95f, 0.95f, 0.95f);
            defaultPD2Material.Specular = new ColorEx(1, 1, 1);
            defaultPD2Material.ReceiveShadows = false;
            Material scriptedPD2Material = (Material)MaterialManager.Instance.Create("scripted_pd2", ResourceGroupManager.DefaultResourceGroupName);
            scriptedPD2Material.ApplyDefaults();
            scriptedPD2Material.Load();
            scriptedPD2Material.Lighting = lightingEnabled;
            scriptedPD2Material.ShadingMode = Shading.Flat;
            scriptedPD2Material.Diffuse = new ColorEx(1, 0, 0);
            scriptedPD2Material.Specular = new ColorEx(1, 0, 0);
            scriptedPD2Material.ReceiveShadows = false;
            MaterialManager.Instance.Load("base_materials.material", ResourceGroupManager.DefaultResourceGroupName);
            var cubeMesh = MeshManager.Instance.Create("cube.mesh", ResourceGroupManager.DefaultResourceGroupName);
            cubeMesh.Load();
            var cylinderMesh = MeshManager.Instance.Create("cylinder.mesh", ResourceGroupManager.DefaultResourceGroupName);
            cylinderMesh.Load();
            Light sun = Scene.CreateLight("sun");
            sun.Type = LightType.Directional;
            sun.Direction = new Vector3(0.15f, 0, -1);
            sun.Diffuse = new ColorEx(1.0f, 0.95f, 0.95f);
            this.keyDebounceTimer.Start();
            return true;
        }

        public bool Render()
        {
            if (WindowEventMonitor.Instance.MessagePump != null)
            {
                WindowEventMonitor.Instance.MessagePump();
            }
            return this.root.RenderOneFrame();
        }

        void FrameStarting(object sender, FrameEventArgs args)
        {
            this.input.Capture();
            HandleKeyboard(args);
            HandleMouse(args);
        }

        void HandleKeyUp(object sender, KeyEventArgs args)
        {
            if (args.Key == KeyCodes.R)
            {
                switch (Camera.PolygonMode)
                {
                    case PolygonMode.Solid:
                        Camera.PolygonMode = PolygonMode.Wireframe;
                        break;
                    case PolygonMode.Wireframe:
                        Camera.PolygonMode = PolygonMode.Points;
                        break;
                    case PolygonMode.Points:
                        Camera.PolygonMode = PolygonMode.Solid;
                        break;
                }
            }
        }

        void HandleKeyboard(FrameEventArgs args)
        {
            if (this.input.IsKeyPressed(KeyCodes.Escape))
            {
                Quit();
            }
            if (this.input.IsKeyPressed(KeyCodes.D1))
            {
                if (this.keyDebounceTimer.Milliseconds > 350)
                {
                    this.Scene.VisibilityMask ^= (int)ObjectLayers.StaticObjects;
                    this.keyDebounceTimer.Reset();
                }
            }
            if (this.input.IsKeyPressed(KeyCodes.D2))
            {
                if (this.keyDebounceTimer.Milliseconds > 350)
                {
                    this.Scene.VisibilityMask ^= (int)ObjectLayers.DevObjects;
                    this.keyDebounceTimer.Reset();
                }
            }
            if (this.input.IsKeyPressed(KeyCodes.D3))
            {
                if (this.keyDebounceTimer.Milliseconds > 350)
                {
                    this.Scene.VisibilityMask ^= (int)ObjectLayers.DisabledObjects;
                    this.keyDebounceTimer.Reset();
                }
            }
            if (this.input.IsKeyPressed(KeyCodes.D4))
            {
                if (this.keyDebounceTimer.Milliseconds > 350)
                {
                    this.Scene.VisibilityMask ^= (int)ObjectLayers.ScriptedObjects;
                    this.keyDebounceTimer.Reset();
                }
            }
            if (this.input.IsKeyPressed(KeyCodes.D5))
            {
                if (this.keyDebounceTimer.Milliseconds > 350)
                {
                    this.Scene.VisibilityMask ^= (int)ObjectLayers.ScriptObjects;
                    this.keyDebounceTimer.Reset();
                }
            }
            if (this.input.IsKeyPressed(KeyCodes.D6))
            {
                if (this.keyDebounceTimer.Milliseconds > 350)
                {
                    this.Scene.VisibilityMask ^= (int)ObjectLayers.MassunitObjects;
                    this.keyDebounceTimer.Reset();
                }
            }
            if (this.input.IsKeyPressed(KeyCodes.F))
            {
                if (this.keyDebounceTimer.Microseconds > 350)
                {
                    Material m = Scene.GetMaterial("default_pd2");
                    m.Lighting = !lightingEnabled;
                    lightingEnabled = !lightingEnabled;
                    this.keyDebounceTimer.Reset();
                }
            }
            var movementVector = new Vector3();
            if (this.input.IsKeyPressed(KeyCodes.W))
            {
                movementVector.z += -500 * args.TimeSinceLastFrame;
                Console.WriteLine("X: " + movementVector.x.ToString() + " Y: " + movementVector.y.ToString() + " Z: " + movementVector.z.ToString());
            }
            if (this.input.IsKeyPressed(KeyCodes.S))
            {
                movementVector.z += 500 * args.TimeSinceLastFrame;
                Console.WriteLine("X: " + movementVector.x.ToString() + " Y: " + movementVector.y.ToString() + " Z: " + movementVector.z.ToString());
            }
            if (this.input.IsKeyPressed(KeyCodes.A))
            {
                movementVector.x += -500 * args.TimeSinceLastFrame;
                Console.WriteLine("X: " + movementVector.x.ToString() + " Y: " + movementVector.y.ToString() + " Z: " + movementVector.z.ToString());
            }
            if (this.input.IsKeyPressed(KeyCodes.D))
            {
                movementVector.x += 500 * args.TimeSinceLastFrame;
                Console.WriteLine("X: " + movementVector.x.ToString() + " Y: " + movementVector.y.ToString() + " Z: " + movementVector.z.ToString());
            }
            if (this.input.IsKeyPressed(KeyCodes.E))
            {
                movementVector.y += 500 * args.TimeSinceLastFrame;
                Console.WriteLine("X: " + movementVector.x.ToString() + " Y: " + movementVector.y.ToString() + " Z: " + movementVector.z.ToString());
            }
            if (this.input.IsKeyPressed(KeyCodes.C))
            {
                movementVector.y += -500 * args.TimeSinceLastFrame;
                Console.WriteLine("X: " + movementVector.x.ToString() + " Y: " + movementVector.y.ToString() + " Z: " + movementVector.z.ToString());
            }
            if (this.input.IsKeyPressed(KeyCodes.LeftShift))
            {
                movementVector *= 5.0f;
            }
            if (this.input.IsKeyPressed(KeyCodes.LeftControl))
            {
                movementVector /= 5.0f;
            }
            Camera.MoveRelative(movementVector);
        }

        void HandleMouse(FrameEventArgs args)
        {
            if(this.input.IsMousePressed(MouseButtons.Right))
            {
                Camera.Yaw(-this.input.RelativeMouseX);
                Camera.Pitch(-this.input.RelativeMouseY);
            }
            if (this.input.IsMousePressed(MouseButtons.Left) && (this.keyDebounceTimer.Microseconds > 350))
            {
                RaySceneQuery query = Scene.CreateRayQuery();
                query.Ray = new Ray(Camera.Position, Camera.Direction);
                query.MaxResults = 5;
                query.SortByDistance = true;
                IList<RaySceneQueryResultEntry> results = query.Execute();
                foreach (RaySceneQueryResultEntry result in results)
                {
                    var obj = (Entity)result.SceneObject;
                    obj.MaterialName = "scripted_pd2";
                    Console.WriteLine("Object: {0}, at Position {1}", (string)result.SceneObject.UserData, result.SceneObject.ParentSceneNode.DerivedPosition);
                }
                Console.WriteLine("===========================================");
                this.keyDebounceTimer.Reset();
            }
        }

        public bool ShouldRun()
        {
            return isRunning;
        }

        public void Quit()
        {
            isRunning = false;
        }

        public void Shutdown()
        {
            this.root.Shutdown();
        }
    }
}
