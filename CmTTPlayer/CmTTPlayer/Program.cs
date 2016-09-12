using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace CmTTPlayer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //获取进程的模块名称,并返回没有扩展名的模块名称 
            //获取相关名称的进程资源
            Process[] myProcess = Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule.ModuleName));

            //判断当前进程是否正在运行
            if (myProcess.Length > 1)
            {
                MessageBox.Show("当前程序已经运行", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainFrm());
            }
            
        }
    }
}
