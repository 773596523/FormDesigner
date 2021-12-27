﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.Windows.Forms.Design;
using Ivytalk.DataWindow;
using Ivytalk.DataWindow.Core;
using Ivytalk.DataWindow.Core.OperationControl;
using Ivytalk.DataWindow.DesignLayer;
using Ivytalk.DataWindow.Utility;

namespace FormDesinger
{
    public partial class FromDesigner : Form
    {
        public FromDesigner()
        {
            InitializeComponent();

            BaseDataWindow dw = new BaseDataWindow();
            dw.Width = 1920;
            dw.Height = 1080;
            dw.Text = "Hello";

            designerControl1.Dispose();
            this.Controls.Remove(this.designerControl1);
            designerControl1 = new DesignerControl(dw);
            designerControl1.Dock = DockStyle.Fill;
            designerControl1.Overlayer.PropertyGrid = this.propertyGrid1;

            this.Controls.Add(designerControl1);
            designerControl1.BringToFront();


            //designerControl1.Overlayer.Init(800, 500, "新建设计");
            this.designerControl1.Overlayer.PropertyGrid = this.propertyGrid1;
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                //WS_EX_COMPOSITED
                cp.ExStyle |= 0x02000000;
                return cp;
            }
        }


        /// <summary>
        /// 拖拽控件
        /// </summary>
        private void toolMenuItems_MouseDown(object sender, MouseEventArgs e)
        {
            ToolStripItem ctrl = sender as ToolStripItem;
            if (ctrl != null)
            {
                string[] strs = {ctrl.Tag == null ? "" : ctrl.Tag.ToString(), ctrl.Text};
                DoDragDrop(strs, DragDropEffects.Copy);
            }
            else
            {
                UserControls.ToolMenuItems tool = sender as UserControls.ToolMenuItems;
                if (tool != null)
                {
                    string[] strs = {tool.Tag == null ? "" : tool.Tag.ToString(), tool.Text};
                    DoDragDrop(strs, DragDropEffects.Copy);
                }
            }
        }

        private Point LastAddToolsLocation = new Point(-1, -1);

        /// <summary>
        /// 添加控件
        /// <para>外部控件</para>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolMenuItems_adds_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog of = new OpenFileDialog())
            {
                of.Filter = "net程序集(*.dll)|*.dll|net exe|*.exe";
                if (of.ShowDialog() == DialogResult.OK)
                {
                    Assembly assem = null;
                    try
                    {
                        assem = Assembly.LoadFile(of.FileName);
                    }
                    catch
                    {
                        MessageBox.Show("不可识别程序集！");
                    }

                    if (assem != null)
                    {
                        using (AddControlDialog add = new AddControlDialog())
                        {
                            add.Assembly = assem;
                            if (add.ShowDialog() == DialogResult.OK)
                            {
                                //ToolStripMenuItem i = new ToolStripMenuItem(add.CtrlName);
                                //i.Tag = add.FullName + "/" + of.FileName;
                                ////toolStrip1.Items.Insert(toolStrip1.Items.Count - 1, i);
                                //i.MouseDown += new MouseEventHandler(toolStripButton10_MouseDown);
                                //添加菜单
                                UserControls.ToolMenuItems toolMenuItems = new UserControls.ToolMenuItems();
                                toolMenuItems.ToolImage = Properties.Resources.tools_settings_24px;
                                toolMenuItems.ToolName = add.FullName;
                                toolMenuItems.ToolTag = add.FullName + "/" + of.FileName;
                                toolMenuItems.ToolTip = "从外部加载的.net控件\n位置:" + of.FileName;
                                toolMenuItems.Size = new Size(136, 22);
                                toolMenuItems.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                                toolMenuItems.MouseDown += new MouseEventHandler(toolMenuItems_MouseDown);
                                if (LastAddToolsLocation == new Point(-1, -1))
                                    LastAddToolsLocation = new Point(toolMenuItems_adds.Location.X, toolMenuItems_adds.Location.Y + 23);
                                else
                                    LastAddToolsLocation = new Point(LastAddToolsLocation.X, LastAddToolsLocation.Y + 23);
                                toolMenuItems.Location = LastAddToolsLocation;
                                this.panel_tools_cus.Controls.Add(toolMenuItems); //从Panel容器中添加
                                toolMenuItems.BringToFront();
                            }
                        }
                    }
                }
            }
        }

        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (designerControl1.Overlayer.Controls.Count == 0)
            {
                MessageBox.Show("没有任何控件,不能保存.", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Title = "请选择保存路径与文件名";
            fileDialog.Filter = "文本文件|*.txt|所有文件|*.*"; //设置要选择的文件的类型
            fileDialog.FileName = "*.txt";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = fileDialog.FileName;
                //string text = new Core.ControlSave(designerControl1.Overlayer).Save();
                //
                //System.IO.File.WriteAllText(fileName, text, Encoding.UTF8);

                DataWindowAnalysis.SerializationControls(designerControl1.Overlayer.DesignerForm, fileName);
            }
        }

        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (designerControl1.Overlayer.Controls.Count != 0)
            {
                DialogResult dr = MessageBox.Show("当前设计将被覆盖,确定新建吗?\n建议先保存当前设计",
                    "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dr == DialogResult.Cancel)
                    return;
            }

            designerControl1.Overlayer.Init(800, 500, "新建设计");
            //designerControl1._hostFrame.Controls.Clear();
            //designerControl1._hostFrame.Text = "新建设计";
            //designerControl1._hostFrame.Width = 800;
            //designerControl1._hostFrame.Height = 500;
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (designerControl1.Overlayer.Controls.Count != 0)
            {
                DialogResult dr = MessageBox.Show("当前设计将被覆盖,确定新建吗?\n建议先保存当前设计",
                    "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (dr == DialogResult.Cancel)
                    return;
            }

            designerControl1.Overlayer.Init(800, 500, "新建设计");
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "请选择文本文件";
            fileDialog.Filter = "文本文件|*.txt|所有文件|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = fileDialog.FileName;
                designerControl1.Overlayer.DesignerFormText = "自由型设计";
                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Items.Add("编辑SQL");
                designerControl1.ContextMenuStrip = menu;

                string source = System.IO.File.ReadAllText(fileName);
                //Core.OldAnalysis old = new Core.OldAnalysis(designerControl1._overlayer);
                //old.of_LoaddwText(source);
                Core.MyAnalysis anl = new Core.MyAnalysis(designerControl1.Overlayer);
                anl.Load(source);
                return;
            }
        }

        //微调
        private void tool_auto_Click(object sender, EventArgs e)
        {
            designerControl1.Overlayer.ControlAlignFineTune();
        }

        //左对齐
        private void toolStrip_center_left_Click(object sender, EventArgs e)
        {
            designerControl1.Overlayer.ControlAlign(AlignType.Left);
        }

        //上对齐
        private void toolStrip_center_top_Click(object sender, EventArgs e)
        {
            designerControl1.Overlayer.ControlAlign(AlignType.Top);
        }

        private void 设计器最大化ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitter1.SplitPosition = 0;
            splitter2.SplitPosition = 0;
        }

        private void 还原ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            splitter1.SplitPosition = 138;
            splitter2.SplitPosition = 239;
        }

        private void formToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = false;
            fileDialog.Title = "请选择文本文件";
            fileDialog.Filter = "文本文件|*.txt|所有文件|*.*";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                BaseDataWindow frm = new BaseDataWindow();

                DesignerControl dc = new DesignerControl();
                dc.Dock = DockStyle.Fill;
                frm.Controls.Add(dc);

                string fileName = fileDialog.FileName;
                dc.Overlayer.DesignerFormText = "自由型设计";
                ContextMenuStrip menu = new ContextMenuStrip();
                menu.Items.Add("编辑SQL");
                dc.ContextMenuStrip = menu;

                string source = System.IO.File.ReadAllText(fileName);
                //Core.OldAnalysis old = new Core.OldAnalysis(designerControl1._overlayer);
                //old.of_LoaddwText(source);
                Core.MyAnalysis anl = new Core.MyAnalysis(dc.Overlayer);
                anl.Load(source);

                frm.Show();
                return;
            }
        }

        private void toolMenuItems_label_Load(object sender, EventArgs e)
        {
        }
    }
}