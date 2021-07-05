using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shutdowner {
    static class Program {
        static NotifyIcon nicon;
        static SettingForm sform;
        static bool survive = true;
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());

            nicon = new NotifyIcon();
            nicon.Icon = Shutdowner.Properties.Resources.Shutdowner;
            nicon.Text = "Shutdowner";
            nicon.Visible = true;
            //コンテキストメニュークリックしたときにも表示されたので止め
            //nicon.MouseClick += (s,e) => {
            //    if (e.Button == MouseButtons.Left) {
            //        if (sform == null) {//form1をcloseしてもnullにはならないのでイベントでnullにしてやる
            //            sform = new SettingForm();
            //            sform.FormClosed += (s, e) => { sform = null; };
            //        }
            //        sform.Show();
            //    }
            //};

            //右クリックメニュー
            ContextMenuStrip cms = new ContextMenuStrip();
            ToolStripMenuItem tsmi1 = new ToolStripMenuItem("Exit");//終了
            tsmi1.Click += (s, e) => {
                survive = false;
                nicon.Dispose();
                Application.Exit();
            };
            ToolStripMenuItem tsmi2 = new ToolStripMenuItem("Shutdown now!");//今すぐシャットダウン
            tsmi2.Click += (s, e) => {
                executeShutdownProcess();
            };
            ToolStripMenuItem tsmi3 = new ToolStripMenuItem("Setting");//設定画面
            tsmi3.Click += (s, e) => {
                if (sform == null) {
                    sform = new SettingForm();
                    sform.FormClosed += (s, e) => { sform = null; };
                }
                sform.Show();
            };
            cms.Items.Add(tsmi3);
            cms.Items.Add(tsmi2);
            cms.Items.Add(tsmi1);
            nicon.ContextMenuStrip = cms;

            Task.Run(async () => {
                var t0 = DateTime.Now;
                DateTime t1 = new(0);
                while (t1.Year < t0.Year) {
                    try {
                        t1 = new DateTime(t0.Year, t0.Month, t0.Day, int.Parse(Properties.Settings.Default.Hours), int.Parse(Properties.Settings.Default.Minutes), 0);
                    } catch {
                        //設定時間でエラーが出たら設定画面を出す（設定ファイルを直接いじったりしたとき用）
                        if (sform == null) {
                            sform = new SettingForm();
                            sform.FormClosed += (s, e) => { sform = null; };
                        }
                        sform.ShowDialog();
                    }
                }
                while (survive) {
                    t0 = DateTime.Now;
                    //設定時間変更確認
                    var t2= new DateTime(t0.Year, t0.Month, t0.Day, int.Parse(Properties.Settings.Default.Hours), int.Parse(Properties.Settings.Default.Minutes), 0);
                    if (t2.Hour != t1.Hour || t2.Minute != t1.Minute) {
                        t1 = t2;
                        if (Properties.Settings.Default.SettingTime > t1) t1 = t1.AddDays(1);//設定時刻を設定した時刻より早ければ、明日実行する
                    }
                    //設定時間を現在時間が超えていても、1時間以上離れていた場合は明日にする
                    if (t0 > t1 && (t0 - t1) > new TimeSpan(1, 0, 0)) t1 = t1.AddDays(1);
                    //設定時間を超えたらシャットダウン
                    if (t0 >= t1) {
                        if (executeShutdownProcess()) break;
                        else t1 = t1.AddDays(1);
                    }
                    await Task.Delay(60000);
                }
            });

            Application.Run();
        }

        static bool executeShutdownProcess() {
            //確認画面表示
            var cform = new ConfirmForm();
            if (cform.ShowDialog() == DialogResult.OK) {
                //メイン画面を持つプロセスの終了
                waitForCloseWindows();
                //Shutdown処理
                shutdown();
                //終了
                return true;
            } else {
                return false;
            }
        }

        static void waitForCloseWindows() {
            var ignoreProcesses = new[] { "TextInputHost", "ApplicationFrameHost", "SystemSettings" };//何故か起動してないCalcurator（電卓）が出てくるが放置
            var closeList = new List<Process>();
            foreach (var p in Process.GetProcesses().Where(pr=>!ignoreProcesses.Contains(pr.ProcessName))) {
                if (p.MainWindowHandle != IntPtr.Zero && !string.IsNullOrEmpty(p.MainWindowTitle)) {
                    //Debug.WriteLine(p.ProcessName);
                    p.CloseMainWindow();
                    closeList.Add(p);
                }
            }
            //5分待っても終了しない場合kill←killは良くないっぽい
            var st = DateTime.Now;
            while (closeList.Any(p => p.HasExited)) {
                if (DateTime.Now - st > new TimeSpan(0, 5, 0)) break;
                Task.Delay(1000);
            }
            //foreach (var p in closeList) {
            //    if (!p.HasExited) p.Kill();
            //}
        }
        static void shutdown() {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "shutdown.exe";
            psi.Arguments = "/s /f";
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process.Start(psi);
        }
    }
}
