using System;
using System.Data;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using System.IO;


namespace CmTTPlayer
{
    public partial class MainFrm : Form
    {    
        #region 全局变量
        internal AnchorStyles StopAanhor = AnchorStyles.None;
        /// <summary>
        /// 实例化Media播放类
        /// </summary>
        protected readonly static Media media = new Media();

        /// <summary>
        /// 当前应用程序目录
        /// </summary>
        private readonly static string startupPath = Application.StartupPath;

        /// <summary>
        /// 自增长序号
        /// </summary>
        private static int number = 1;

        /// <summary>
        /// 当前播放曲目索引
        /// </summary>
        private static int changeValue = 0;

        /// <summary>
        /// 当前播放曲目自增长序号
        /// </summary>
        private static string changeItemText = "1";

        /// <summary>
        /// 淡入加载主窗体
        /// </summary>
        private static  Timer tmrShow = new Timer();
        #endregion
        
        public MainFrm()
        {
            InitializeComponent();
            axWindowsMediaPlayer1.Ctlcontrols.stop();
           
        }

        #region 全局常量
        const int WM_SYSCOMMAND = 0x112;
        const int SC_CLOSE = 0xF060;
        const int SC_MINIMIZE = 0xF020;
        const int SC_MAXIMIZE = 0xF030;
        #endregion        

        #region 应用程序初始化事件
        /// <summary>
        /// 重写窗体标题栏按钮事件
        /// </summary>
        /// <param name="m"></param>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)                     //如果触发的是系统按钮消息
            {
                if (m.WParam.ToInt32() == SC_MINIMIZE)      //如果触发最小化按钮事件
                {
                    this.Visible = false;
                    notifyIcon1.Visible = true;
                    return;
                }
                else if (m.WParam.ToInt32() == SC_CLOSE)    //如果触发关闭按钮事件
                {
                    Environment.Exit(0);
                    return;
                }
            }
            base.WndProc(ref m);
        }

        /// <summary>
        /// 初始化窗体数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frm_Load(object sender, EventArgs e)
        {
            
            //设置皮肤解锁密码
            //this.skinEngine1.SkinPassword = "cjdz2009";

            //设置窗体界面皮肤
            this.skinEngine2.SkinFile = startupPath + "\\Skin\\wave1.SSK";

            //获取当前歌唱者图片
           // this.pbSinger.Image = Image.FromFile(startupPath + @"\\images\default.jpg");

            tmrShow.Tick += new EventHandler(this.tmrShow_Tick);
            tmrShow.Enabled = true;

            //设置程序标题栏图标
            this.Icon = Icon.ExtractAssociatedIcon(startupPath + "\\CmTTPlayer.ico");
            
            //设置程序最小化右下角图标
            this.notifyIcon1.Icon = Icon.ExtractAssociatedIcon(startupPath + "\\CmTTPlayer.ico");
            
            //初始化Media声道为立体声
            media.SetAudioSource(Media.AudioSource.H);

            OpenPlayList();
        }
        #endregion        

        #region 打开/保存/删除曲目文件

        /// <summary>
        /// 打开曲目列表
        /// </summary>
        private void OpenPlayList()
        {
            XmlDocument xmlDoc = new XmlDocument();

            //加载Xml文件
            xmlDoc.Load(startupPath + "\\PlayList\\MusicFile.xml");

            //匹配曲目列表
            XmlNode root = xmlDoc.SelectSingleNode("musiclist");//查找<musiclist>

            //获取所有曲目列表
            XmlNodeList xnl = root.ChildNodes;

            for (int i = 0; i < xnl.Count; i++)
            {
                ListViewItem item = new ListViewItem(number.ToString());

                //依次顺序为：曲目名称|曲目长度
                item.SubItems.AddRange(new string[] { root.ChildNodes[i].Attributes["genre"].Value, root.ChildNodes[i].Attributes["lenght"].Value });

                //设置曲目绝对路径
                item.Tag = root.ChildNodes[i].Attributes["filename"].Value;

                lvMusicList.Items.Add(item);

                number++;

                //处理消息队列
                Application.DoEvents();
            }
        }

        /// <summary>
        /// 打开音乐文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiOpenMusic_Click(object sender, EventArgs e)
        {
            try
            {
                using (OpenFileDialog ofd = new OpenFileDialog())
                {
                    //允许多选
                    ofd.Multiselect = true;
                    //文件格式
                    ofd.Filter = "音乐文件|*.mp3;*.wma;*.wav;";
                    //对话框标题
                    ofd.Title = "打开媒体文件";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        //加载Xml文件
                        xmlDoc.Load(startupPath + "\\PlayList\\MusicFile.xml");
                        XmlNode root = xmlDoc.SelectSingleNode("musiclist");//查找<musiclist>
                        //遍历加载当前选中文件
                        foreach (string fileName in ofd.FileNames)
                        {
                            //打开音乐设备
                            media.OpenMusic(fileName, this.Handle);
                            //添加ListView项
                            ListViewItem item = new ListViewItem(number.ToString());
                            item.SubItems.AddRange(new string[] { Path.GetFileName(fileName), media.GetMusicLengthString() });
                            item.Tag = fileName;
                            lvMusicList.Items.Add(item);

                            XmlElement xe1 = xmlDoc.CreateElement("name");//创建一个<book>节点 
                            xe1.SetAttribute("genre", Path.GetFileName(fileName));//设置该节点genre属性 
                            xe1.SetAttribute("filename", fileName);//设置该节点filename属性
                            xe1.SetAttribute("lenght", media.GetMusicLengthString());//设置该节点filename属性
                            number++;
                            root.AppendChild(xe1);//添加到<bookstore>节点中                         
                            //处理消息队列
                            Application.DoEvents();
                        }
                        xmlDoc.Save(startupPath + "\\PlayList\\MusicFile.xml");
                    }
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 添加本地文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>        
        private void tsmiOpenFolderMusic_Click(object sender, EventArgs e)
        {
            try
            {
                //打开文件夹选择对话框
                using (FolderBrowserDialog fbd = new FolderBrowserDialog())
                {
                    if (fbd.ShowDialog() == DialogResult.OK)
                    {
                        DirectoryInfo dInfo = new DirectoryInfo(fbd.SelectedPath);
                        //遍历该文件夹
                        GetFolder(dInfo);
                    }
                    this.btnPlayOrPause.Enabled = true;
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 递归文件夹
        /// </summary>
        /// <param name="dInfo"></param>
        private void GetFolder(DirectoryInfo dInfo)
        {
            try
            {
                //显示其中文件
                GetFile(dInfo);

                //遍历文件夹中的文件夹
                foreach (DirectoryInfo dir in dInfo.GetDirectories())
                {
                    //递归遍历该文件夹
                    GetFolder(dir);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// 遍历文件夹中的文件
        /// </summary>
        /// <param name="dInfo"></param>
        private void GetFile(DirectoryInfo dInfo)
        {
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
               
                //加载Xml文件
                xmlDoc.Load(startupPath + "\\PlayList\\MusicFile.xml");

                XmlNode root = xmlDoc.SelectSingleNode("musiclist");//查找<musiclist>
                foreach (FileInfo fileName in dInfo.GetFiles())
                {
                    //获取文件扩展名
                    string extension = Path.GetExtension(fileName.FullName);

                    //判断是否为音乐文件
                    if (extension.ToLower() == ".mp3" || extension.ToLower() == ".wma" || extension.ToLower() == ".wav")
                    {
                        media.OpenMusic(fileName.FullName, this.Handle);

                        ListViewItem item = new ListViewItem(number.ToString());
                        //添加到播放列表
                        item.SubItems.AddRange(new string[] { Path.GetFileName(fileName.FullName), media.GetMusicLengthString().Remove(0, 3) });
                        item.Tag = fileName.FullName;
                        lvMusicList.Items.Add(item);

                        XmlElement xe1 = xmlDoc.CreateElement("name");//创建一个<book>节点 
                        xe1.SetAttribute("genre", Path.GetFileName(fileName.FullName));//设置该节点genre属性 
                        xe1.SetAttribute("filename", fileName.FullName);//设置该节点filename属性
                        xe1.SetAttribute("lenght", media.GetMusicLengthString().Remove(0, 3));   //设置该节点lenght属性
                        root.AppendChild(xe1);//添加到<bookstore>节点中       
                        number++;
                    }
                    Application.DoEvents();
                }
                xmlDoc.Save(startupPath + "\\PlayList\\MusicFile.xml");
            }
            catch (Exception exp)
            {
                throw exp;
            } 
        }

        /// <summary>
        /// 删除选中歌曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiDeleteChangeMusic_Click(object sender, EventArgs e)
        {
            try
            {
                //判断是否至少选中一项
                if (lvMusicList.SelectedItems.Count == 0)
                {
                    MessageBox.Show("请选择您要删除的歌曲!", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else
                {
                    //获取选中歌曲数目
                    int result = this.lvMusicList.SelectedItems.Count;
                    //循环删除选中歌曲
                    for (int i = 0; i < result; i++)
                    {
                        this.lvMusicList.SelectedItems[i].Remove();
                    }
                    SaveXmlFile();
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 重新保存歌曲列表
        /// </summary>
        private void SaveXmlFile()
        {
            XmlDocument xmlDoc = new XmlDocument();
            //加载Xml文件
            xmlDoc.Load(startupPath + "\\PlayList\\MusicFile.xml");
            XmlNode root = xmlDoc.SelectSingleNode("musiclist");//查找<musiclist>
            //清空原来所有歌曲
            root.RemoveAll();
            for (int i = 0; i < this.lvMusicList.Items.Count; i++)
            {
                XmlElement xe1 = xmlDoc.CreateElement("name");//创建一个<book>节点 
                xe1.SetAttribute("genre", this.lvMusicList.Items[i].SubItems[1].Text);//设置该节点genre属性 
                xe1.SetAttribute("filename", this.lvMusicList.Items[i].Tag.ToString());//设置该节点filename属性
                xe1.SetAttribute("lenght", this.lvMusicList.Items[i].SubItems[2].Text);//设置该节点lenght属性
                root.AppendChild(xe1);//添加到<bookstore>节点中       
            }
            //保存歌曲列表
            xmlDoc.Save(startupPath + "\\PlayList\\MusicFile.xml");
        }

        /// <summary>
        /// 清空当前列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiClearList_Click(object sender, EventArgs e)
        {
            XmlDocument xmlDoc = new XmlDocument();
            //加载Xml文件
            xmlDoc.Load(startupPath + "\\PlayList\\MusicFile.xml");
            XmlNode root = xmlDoc.SelectSingleNode("musiclist");//查找<musiclist>
            //删除所有歌曲列表
            root.RemoveAll();
            //保存歌曲列表
            xmlDoc.Save(startupPath + "\\PlayList\\MusicFile.xml");
            //清空列表
            this.lvMusicList.Items.Clear();

            number = 1;
        }

        /// <summary>
        /// 触发删除键,删除选中歌曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvMusicList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)       //如果触发Delete删除键则删除当前选中歌曲
                tsmiDeleteChangeMusic_Click(null, null);
        }

        /// <summary>
        /// 删除重复歌曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmDeleteRepeat_Click(object sender, EventArgs e)
        {
            //冒泡排序法——删除重复歌曲
            for (int i = 0; i < this.lvMusicList.Items.Count; i++)            //控制比较多少轮
            {
                //将重复元素删除
                for (int j = 0; j < this.lvMusicList.Items.Count; j++)
                {
                    //如果两个比较的元素相同并且不是同一元素,执行删除操作
                    if (this.lvMusicList.Items[i].Tag.ToString() == this.lvMusicList.Items[j].Tag.ToString() && i != j)
                    {
                        this.lvMusicList.Items[j].Remove();
                        //判断当前播放歌曲是否存在于删除之列,如果存在设置当前播放曲目为第一首
                        if (changeValue == j)
                            changeValue = 0;
                    }
                }
            }
            SaveXmlFile();
        }
        #endregion

        #region 操作曲目播放
        /// <summary>
        /// 上一曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPre_Click(object sender, EventArgs e)
        {
            try
            {
                //还原当前曲目播放状态
                lvMusicList.Items[changeValue].SubItems[0].Text = changeItemText;
                if (changeValue <= 0)
                    changeValue = lvMusicList.Items.Count - 1;
                else
                    changeValue--;
                //初始化播放曲目
                GetDownOrUp(sender, e);
                //按钮状态
                GetPlayButton();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 下一曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnLast_Click(object sender, EventArgs e)
        {
            try
            {
                //还原当前曲目播放状态
                lvMusicList.Items[changeValue].SubItems[0].Text = changeItemText;
                if (changeValue >= lvMusicList.Items.Count - 1)
                    changeValue = 0;
                else
                    changeValue++;
                //初始化播放曲目
                GetDownOrUp(sender, e);
                //按钮状态
                GetPlayButton();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 播放暂停
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPlayOrPause_Click(object sender, EventArgs e)
        {
            try
            {
                if (lvMusicList.SelectedItems.Count > 0)
                {
                    if (btnPlayOrPause.Text.Equals("开始播放"))
                    {
                        media.PlayMusic();
                        //设置当前音量
                        pictureBox3.BackgroundImage = pictureBox15.BackgroundImage;
                       
                        trackBar1_Scroll(sender, e);
                        GetPlayButton();
                    }
                    else
                    {
                        GetPauseButton();
                        media.PauseMusic();
                        pictureBox3.BackgroundImage = pictureBox10.BackgroundImage;
                        
                    }
                }
                else
                    MessageBox.Show("请选中要播放的文件", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 停止播放
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, EventArgs e)
        {
            try
            {
                //停止播放
                media.CloseMusic();
                //还原当前正在播放曲目标识
                lvMusicList.Items[changeValue].SubItems[0].Text = changeItemText;
                //设置控件状态
                GetStopButton();
                pictureBox5.BackgroundImage = pictureBox16.BackgroundImage;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 设置进度
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                //设置音乐当前时间
                this.tbProgress.Value = media.GetMusicPos();
                //设置音乐的当前进度
                this.lblDuration.Text = media.GetMusicPosString();
                
                //判断是否已经播放完成
                if (this.tbProgress.Value == this.tbProgress.Maximum)
                {
                    if (this.lvMusicList.Items.Count > 0)
                    {
                        //还原播放标识编号
                        this.lvMusicList.Items[changeValue].SubItems[0].Text = changeItemText;

                        //如果播放模式为循环播放
                        if (this.tsmCyclePlayer.CheckState == CheckState.Checked)
                        {
                            //判断当前播放曲目是否为最后一首,如果是返回第一曲,否则播放下一曲
                            if (changeValue >= this.lvMusicList.Items.Count - 1)
                                changeValue = 0;
                            else
                                changeValue++;

                            GetDownOrUp(sender, e);
                        }
                        else if (this.tsmOrderPlayer.CheckState == CheckState.Checked)  //如果播放模式为顺序播放
                        {
                            //判断当前播放曲目是否为最后一首,如果是停止播放,否则播放下一曲
                            if (changeValue >= this.lvMusicList.Items.Count - 1)
                            {
                                //停止播放
                                btnStop_Click(null, null);
                                this.timer1.Stop();
                            }
                            else
                            {
                                changeValue++;
                                GetDownOrUp(sender, e);
                            }
                        }
                        else if (this.tsmiRandomPlayer.CheckState == CheckState.Checked)    //如果播放模式为随机播放
                        {
                            //生成播放列表数量以内的随机数作为播放曲目
                            changeValue = new Random().Next(this.lvMusicList.Items.Count - 1);
                            GetDownOrUp(sender, e);
                        }
                        
                    }
                    else
                    {
                        //停止播放
                        btnStop_Click(null, null);
                    }
                }
            }
            catch (Exception exp)
            {
                //停止播放
                btnStop_Click(null, null);
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        /// <summary>
        /// 双击音乐文件列表播放　
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvMusicList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                lvMusicList.Items[changeValue].SubItems[0].Text = changeItemText;
                changeValue = lvMusicList.SelectedIndices[0];                
                GetDownOrUp(sender, e);
                pictureBox3.BackgroundImage = pictureBox15.BackgroundImage;
                pictureBox5.BackgroundImage = pictureBox20.BackgroundImage;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            GC.Collect();
        }

        /// <summary>
        /// 初始化播放曲目并开始播放
        /// </summary>
        private void GetDownOrUp(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists(lvMusicList.Items[changeValue].Tag.ToString()))
                {
                    //将当前播放曲目序号保存
                    changeItemText = lvMusicList.Items[changeValue].SubItems[0].Text;
                    //将当前播放曲目标识为正在播放
                    lvMusicList.Items[changeValue].SubItems[0].Text = "★";
                    //设置当前播放曲目地址
                    media.OpenMusic(lvMusicList.Items[changeValue].Tag.ToString(), this.Handle);                    
                    //开始播放
                    media.PlayMusic();
                    //设置进度条长度
                    tbProgress.Maximum = media.GetMusicLength();
                    //设置曲目标题
                    lblMusicName.Text = lvMusicList.Items[changeValue].SubItems[1].Text;
                    //设置当前音量
                    trackBar1_Scroll(null, null);

                    //播放状态下的按钮
                    GetPlayButton();
                }
                else
                {
                    MessageBox.Show("媒体文件读取失败,请确定当前播放的文件是否存在.", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
        }
        #endregion

        #region 控件状态设置
        /// <summary>
        /// 播放状态下控件
        /// </summary>
        private void GetPlayButton()
        {
            btnPlayOrPause.Text = "暂停播放";
            btnPre.Enabled = true;
            btnLast.Enabled = true;
            btnPlayOrPause.Enabled = true;
            btnStop.Enabled = true;
            timer1.Enabled = true;
            //设置当前音量
            trackBar1_Scroll(null, null);
        }

        /// <summary>
        /// 暂停状态下的控件
        /// </summary>
        private void GetPauseButton()
        {
            btnPlayOrPause.Text = "开始播放";
            timer1.Enabled = false;
            btnPlayOrPause.Enabled = true;
        }

        /// <summary>
        /// 停止状态下的控件
        /// </summary>
        private void GetStopButton()
        {
            this.timer1.Enabled = false;
            this.btnStop.Enabled = false;
            this.btnPlayOrPause.Enabled = false;
            this.btnLast.Enabled = false;
            this.btnPre.Enabled = false;
            this.tbProgress.Value = 0;
            this.lblDuration.Text = "00:00:00";
            this.lblMusicName.Text = "暂无音乐文件";
            changeValue = 0;
        }

        /// <summary>
        /// 播放顺序菜单状态
        /// </summary>
        private void GetPlayerState()
        {
            this.tsmCyclePlayer.CheckState = CheckState.Unchecked;
            this.tsmSinglePlayer.CheckState = CheckState.Unchecked;
            this.tsmOrderPlayer.CheckState = CheckState.Unchecked;
            this.tsmiRandomPlayer.CheckState = CheckState.Unchecked;
        }

        /// <summary>
        /// 淡入加载主窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrShow_Tick(object sender, EventArgs e)
        {
            this.Opacity += 0.1;
            if (this.Opacity >= 1)
            {
                tmrShow.Dispose();     //销毁时间控件
            }
        }

        /// <summary>
        /// 双击状态栏图标还原窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Visible = true;
            this.notifyIcon1.Visible = false;
        }

        /// <summary>
        /// 退出应用程序
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Environment.Exit(0);
        }

        /// <summary>
        /// 播放选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmPlayer_Click(object sender, EventArgs e)
        {
            GetPlayerState();
            ((ToolStripMenuItem)sender).CheckState = CheckState.Checked;
        }

        #endregion
        
        #region 声道/音量管理
        /// <summary>
        /// 声道菜单选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmSound_Click(object sender, EventArgs e)
        {
            GetTsmSound();
            ((ToolStripMenuItem)sender).CheckState = CheckState.Checked;
            SetAudioSource();
        }

        /// <summary>
        /// 设置音乐声道
        /// </summary>
        private void SetAudioSource()
        {
            if (this.tsmCenterSound.CheckState == CheckState.Checked)
                media.SetAudioSource(Media.AudioSource.H);
            else if (this.tsmLeftSound.CheckState == CheckState.Checked)
                media.SetAudioSource(Media.AudioSource.L);
            else if (this.tsmRightSound.CheckState == CheckState.Checked)
                media.SetAudioSource(Media.AudioSource.R);
        }

        /// <summary>
        /// 还原声道选择状态
        /// </summary>
        private void GetTsmSound()
        {
            this.tsmCenterSound.CheckState = CheckState.Unchecked;
            this.tsmLeftSound.CheckState = CheckState.Unchecked;
            this.tsmRightSound.CheckState = CheckState.Unchecked;
        }

        /// <summary>
        /// 设置音量大小
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbProgress_Scroll(object sender, EventArgs e)
        {
            //设置当前播放曲目进度
            media.SetMusicPos(this.tbProgress.Value);
            //继续播放曲目
            media.PlayMusic();
            //设置音量为当前音量游标
            media.SetValume(this.trackBar1.Value);
        }

        /// <summary>
        /// 设置音量
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            media.SetValume(this.trackBar1.Value);
        }

        /// <summary>
        /// 设置静音
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void chkAudioOnOff_CheckedChanged(object sender, EventArgs e)
        {
            if (chkAudioOnOff.Checked)
                media.SetAudioOnOff(true);
            else
                media.SetAudioOnOff(false);
        }
        #endregion

        #region 屏幕收缩功能
        /// <summary>
        /// 通过时间控件判断当前窗体位置,如果处于屏幕边缘则隐藏到屏幕中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmrStretch_Tick(object sender, EventArgs e)
        {
            if (this.Bounds.Contains(Cursor.Position))
            {
                switch (this.StopAanhor)
                {
                    case AnchorStyles.Top:
                        this.Location = new Point(this.Location.X, 0);
                        break;
                    case AnchorStyles.Left:
                        this.Location = new Point(0, this.Location.Y);
                        break;
                    case AnchorStyles.Right:
                        this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - this.Width, this.Location.Y);
                        break;
                }
            }
            else
            {
                switch (this.StopAanhor)
                {
                    case AnchorStyles.Top:
                        this.Location = new Point(this.Location.X, (this.Height - 3) * (-1));
                        break;
                    case AnchorStyles.Left:
                        this.Location = new Point((-1) * (this.Width - 3), this.Location.Y);
                        break;
                    case AnchorStyles.Right:
                        this.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 3, this.Location.Y);
                        break;
                }
                this.tmrStretch.Enabled = false;
            }            
        }

        /// <summary>
        /// 判断当前窗体是否在屏幕的边缘
        /// </summary>
        private void mStopAnhor()
        {
            if (this.Top <= 0)                      //如果窗体上边缘超出屏幕上边缘范围时
            {
                StopAanhor = AnchorStyles.Top;
                this.tmrStretch.Enabled = true;
            }
            else if (this.Left <= 0)                //如果窗体左边缘超出屏幕左边缘范围时
            {
                StopAanhor = AnchorStyles.Left;
                this.tmrStretch.Enabled = true;
            }
            else if (this.Left >= Screen.PrimaryScreen.Bounds.Width - this.Width)   //如果窗体右边缘超出屏幕右边缘范围时
            {
                StopAanhor = AnchorStyles.Right;
                this.tmrStretch.Enabled = true;
            }
            else
                StopAanhor = AnchorStyles.None;
        }

        /// <summary>
        /// 窗体坐标改变时判断当前窗体是否位于窗体边缘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frm_LocationChanged(object sender, EventArgs e)
        {
            this.mStopAnhor();
        }

        /// <summary>
        /// 当鼠标进入窗体可视范围时判断当前窗体是否位于窗体边缘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Frm_MouseEnter(object sender, EventArgs e)
        {
            this.mStopAnhor();
        }

        /// <summary>
        /// 显示/隐藏歌曲列表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnShowMusicList_Click(object sender, EventArgs e)
        {
            if (this.btnShowMusicList.Text == "∧")
            {
                this.btnShowMusicList.Text = "∨";
                this.Height = 155;
                this.Width = 300;
                //statusStrip1.Dispose();
                //pnlSearch.Dispose();
                //lvMusicList.Dispose();
                statusStrip1.Dock = System.Windows.Forms.DockStyle.Top;
                panel2.Dock = System.Windows.Forms.DockStyle.Top;
                tbProgress.Width = 280;
                tbProgress.Dock = System.Windows.Forms.DockStyle.Bottom;
            }
            else
            {
                this.btnShowMusicList.Text = "∧";
                this.Height = 573;
                this.Width = 614;
                panel2.Dock = System.Windows.Forms.DockStyle.None;
                statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
                tbProgress.Width = 571;
                tbProgress.Dock = System.Windows.Forms.DockStyle.None ;
            }
        }
        #endregion

        #region 歌曲查找
        /// <summary>
        /// 取消查找歌曲并隐藏查找面板
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCannelSearch_Click(object sender, EventArgs e)
        {
            this.txtSearchWord.Text = string.Empty;
            this.pnlSearch.Visible = false;
        }

        /// <summary>
        /// 快速查找歌曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tsmiSearchMusic_Click(object sender, EventArgs e)
        {
            this.pnlSearch.Visible = true;
        }

        /// <summary>
        /// 键盘输入时动态查找歌曲
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtSearchWord_KeyUp(object sender, KeyEventArgs e)
        {
            //遍历歌曲清除选中歌曲
            for (int i = 0; i < this.lvMusicList.Items.Count; i++)
            {
                this.lvMusicList.Items[i].Selected = false;
            }

            if (this.txtSearchWord.Text == string.Empty)
                return;
            else
            {
                for (int i = 0; i < this.lvMusicList.Items.Count; i++)
                {
                    if (this.lvMusicList.Items[i].SubItems[1].Text.Contains(this.txtSearchWord.Text))
                    {
                        this.lvMusicList.Items[i].Selected = true;
                    }
                }
            }
        }        
        #endregion        

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            btnPre_Click(sender, e);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            btnPlayOrPause_Click(sender, e);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            btnLast_Click(sender, e);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            btnStop_Click(sender, e);
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            btnShowMusicList_Click(sender, e);
            pictureBox6.BackgroundImage = pictureBox17.BackgroundImage;


        }

        private void pictureBox2_MouseEnter(object sender, EventArgs e)
        {
            pictureBox2.BackgroundImage = pictureBox7.BackgroundImage;
        }

        private void pictureBox2_MouseLeave(object sender, EventArgs e)
        {
            pictureBox2.BackgroundImage = pictureBox8.BackgroundImage;
        }

        private void pictureBox3_MouseEnter(object sender, EventArgs e)
        {
            pictureBox3.BackgroundImage = pictureBox9.BackgroundImage;
        }

        private void pictureBox3_MouseLeave(object sender, EventArgs e)
        {
            if (btnPlayOrPause.Text.Equals("开始播放"))
            {
                pictureBox3.BackgroundImage = pictureBox10.BackgroundImage;
            }
            else
            {
                pictureBox3.BackgroundImage = pictureBox15.BackgroundImage;
 
            }
        }

        private void pictureBox4_MouseEnter(object sender, EventArgs e)
        {
            pictureBox4.BackgroundImage = pictureBox11.BackgroundImage;
        }

        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            pictureBox4.BackgroundImage = pictureBox12.BackgroundImage;
        }

        private void pictureBox5_MouseEnter(object sender, EventArgs e)
        {
            pictureBox5.BackgroundImage = pictureBox13.BackgroundImage;
        }

        private void pictureBox5_MouseLeave(object sender, EventArgs e)
        {
            if (pictureBox5.BackgroundImage == pictureBox20.BackgroundImage)
            { pictureBox5.BackgroundImage = pictureBox20.BackgroundImage; }
            else
            {
                pictureBox5.BackgroundImage = pictureBox16.BackgroundImage;
            }
        }

        private void pictureBox6_MouseEnter(object sender, EventArgs e)
        {
            pictureBox6.BackgroundImage = pictureBox19.BackgroundImage;
        }

        private void pictureBox6_MouseLeave(object sender, EventArgs e)
        {
            if (this.btnShowMusicList.Text == "∧")
            { pictureBox6.BackgroundImage = pictureBox18.BackgroundImage; }
            else
            {
                pictureBox6.BackgroundImage = pictureBox17.BackgroundImage;
 
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            label2.Visible = true;
            axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            label2.Visible = false;
            axWindowsMediaPlayer1.Ctlcontrols.pause();
        }

        private void label2_Click(object sender, EventArgs e)
        {

            
        }

        private void tbProgress_MouseMove(object sender, MouseEventArgs e)
        {
            
        }

        private void label2_MouseClick(object sender, MouseEventArgs e)
        {
            
        }

        private void label2_MouseEnter(object sender, EventArgs e)
        {
            
        }

        private void label2_MouseLeave(object sender, EventArgs e)
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void button1_MouseLeave(object sender, EventArgs e)
        {
            //axWindowsMediaPlayer1.Ctlcontrols.pause();
        }

        private void button1_MouseEnter(object sender, EventArgs e)
        {
            //axWindowsMediaPlayer1.Ctlcontrols.play();
        }

        private void button1_MouseMove(object sender, MouseEventArgs e)
        {
            
        }
    }
}
