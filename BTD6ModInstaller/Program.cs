using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ModInstaller
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try
            {
                Form mainForm = new MainForm();
                Assembly assembly = Assembly.GetExecutingAssembly();
                using (var iconStream = assembly.GetManifestResourceStream("BTD6Mods.Icon.ico"))
                {
                    mainForm.Icon = new Icon(iconStream);
                }
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}