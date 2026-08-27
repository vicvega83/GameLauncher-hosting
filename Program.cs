using System;
using System.IO;
using System.Windows.Forms;

namespace GameLauncher;

static class Program
{
    [STAThread]
    static void Main()
    {
        string log = "startup.log";
        try
        {
            File.WriteAllText(log, "Starting...\r\n");
            ApplicationConfiguration.Initialize();
            File.AppendAllText(log, "Config initialized\r\n");
            var form = new LauncherForm();
            File.AppendAllText(log, "Form created\r\n");
            Application.Run(form);
            File.AppendAllText(log, "Form closed\r\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(log, $"FATAL: {ex.GetType().Name}: {ex.Message}\r\n");
            File.AppendAllText(log, ex.ToString() + "\r\n");
            MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
