using System.Windows.Forms;

namespace DSharpDXRastertek.Tut41.System
{
    public class DSystemConfiguration                   // 57 lines
    {
        public string Title { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        // Static Variables.
        public static bool FullScreen { get; private set; }
        public static bool VerticalSyncEnabled { get; private set; }
        public static float ScreenDepth { get; private set; }
        public static float ScreenNear { get; private set; }
        public static FormBorderStyle BorderStyle { get; private set; }
        public static string VertexShaderProfile = "vs_4_0";
        public static string PixelShaderProfile = "ps_4_0";
        public static string ShaderFilePath { get; private set; }
        public static string DataFilePath { get; private set; }
        public static string ModelFilePath { get; set; }

        // Constructors
        public DSystemConfiguration(bool fullScreen, bool vSync) : this("SharpDX Demo", fullScreen, vSync) { }
        public DSystemConfiguration(string title, bool fullScreen, bool vSync) : this(title, 800, 600, fullScreen, vSync) { }
        public DSystemConfiguration(string title, int width, int height, bool fullScreen, bool vSync)
        {
            FullScreen = fullScreen;
            Title = title;
            VerticalSyncEnabled = vSync;

            if (!FullScreen)
            {
                Width = width;
                Height = height;
            }
            else
            {
                Width = Screen.PrimaryScreen.Bounds.Width;
                Height = Screen.PrimaryScreen.Bounds.Height;
            }
        }

        // Static Constructor.
        static DSystemConfiguration()
        {
            FullScreen = false;
            VerticalSyncEnabled = false;
            ScreenDepth = 100.0f;   // 1000.0f
            ScreenNear = 1.0f;      // 0.1f
            BorderStyle = FormBorderStyle.None;

            ShaderFilePath = @"Externals\Shaders\";
            DataFilePath = @"Externals\Data\";
            ModelFilePath = @"Externals\Models\";
        }
    }
}