using System;
using System.IO;
using System.Windows.Forms;

namespace GameLauncher;

static class Program
{
    [STAThread]
    static void Main()
    {
        try
        {
            File.WriteAllText("startup.log", "Starting...\r\n");
            ApplicationConfiguration.Initialize();
            File.WriteAllText("startup.log", "Config initialized\r\n");
            LauncherForm form = new LauncherForm();
            File.WriteAllText("startup.log", "Form created\r\n");
            Application.Run(form);
        }
        catch (Exception ex)
        {
            File.WriteAllText("startup.log", $"ERROR: {ex}\r\n");
            MessageBox.Show(ex.ToString());
        }
    }
}
