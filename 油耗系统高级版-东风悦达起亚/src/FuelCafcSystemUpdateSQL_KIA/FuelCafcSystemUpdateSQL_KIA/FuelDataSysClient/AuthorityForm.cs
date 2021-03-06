﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using DevExpress.XtraBars;
using DevExpress.XtraGrid.Views.BandedGrid;
using DevExpress.XtraTreeList.Nodes;
using System.Xml;
using FuelDataSysClient.Properties;
using FuelDataSysClient.Tool;

namespace FuelDataSysClient
{
    public partial class AuthorityForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        //string conn = String.Format("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={0})(PORT={1}))(CONNECT_DATA=(SERVICE_NAME={2})));User Id={3};Password={4}", "10.8.11.118", "1521", "FUELKIASYS", "scott", "00x0");

        public AuthorityForm()
        {
            InitializeComponent();
            BandedGridView view = bandedGridView1 as BandedGridView;
            //view.OptionsView.ShowColumnHeaders = false;
            view.OptionsView.ShowGroupPanel = false;
            view.OptionsBehavior.Editable = false;
            view.OptionsView.ColumnAutoWidth = false;
        }

        /// <summary>
        ///  查询用户信息
        /// </summary>
        /// <returns></returns>
        private DataTable QueryUserData()
        {
            string sql = @"SELECT ID AS NO, USERNAME AS 用户名, NAME AS 姓名, PHONE AS 电话,CASE STATUS WHEN 1 THEN '正常' ELSE '禁用' END AS 状态 FROM SYS_USERINFO ORDER BY ID";
            DataSet dsLocal = OracleHelper.ExecuteDataSet(OracleHelper.conn, sql, null);
            return dsLocal.Tables[0];
        }

        /// <summary>
        /// 加载用户信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem5_ItemClick(object sender, ItemClickEventArgs e)
        {
            ResfurbishData();
            DrawRowIndicator(this.bandedGridView1, 30); 
        }

        private DataTable QueryTreeData()
        {
            DataTable dtNew = new DataTable();
            //添加列
            dtNew.Columns.Add("NO");
            dtNew.Columns.Add("PNO", typeof(int));
            dtNew.Columns.Add("NodeName");

            DataRow drNew0 = dtNew.NewRow();
            drNew0["NO"] = 1;
            drNew0["PNO"] = 0 ;
            drNew0["NodeName"] = "父节点";
            dtNew.Rows.Add(drNew0);

            DataRow drNew1 = dtNew.NewRow();
            drNew1["NO"] = 2;
            drNew1["PNO"] = 1;
            drNew1["NodeName"] = "本地数据管理";
            dtNew.Rows.Add(drNew1);

            //DataRow drNew2 = dtNew.NewRow();
            //drNew2["NO"] = 3;
            //drNew2["PNO"] = 1;
            //drNew2["NodeName"] = "单条数据录入";
            //drNew2["Remark"] = "";
            //dtNew.Rows.Add(drNew2);

            //DataRow drNew3 = dtNew.NewRow();
            //drNew3["NO"] = 4;
            //drNew3["PNO"] = 1;
            //drNew3["NodeName"] = "批量导入";
            //drNew3["Remark"] = "";
            //dtNew.Rows.Add(drNew3);

            //DataRow drNew4 = dtNew.NewRow();
            //drNew4["NO"] = 5;
            //drNew4["PNO"] = 1;
            //drNew4["NodeName"] = "数据比对";
            //drNew4["Remark"] = "";
            //dtNew.Rows.Add(drNew4);

            return dtNew;
        }

        /// <summary>
        /// 加载菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem6_ItemClick(object sender, ItemClickEventArgs e)
        {
            this.treeList1.DataSource = ReadMenusXmlData();// GetMenus();
            this.treeList1.KeyFieldName = "ID";
            treeList1.ParentFieldName = "ParentID";
            //treeList1.Columns[0].Caption = "菜单1";
            ////显示复选框  
            treeList1.OptionsView.ShowCheckBoxes = true;
            //展开树结构
            treeList1.ExpandAll();

            //根据用户ID查询权限菜单
            lstCheckedMenuID.Clear();
            DataRow dr = this.gridBand1.View.GetFocusedDataRow();
            if (dr != null)
            {
                string sql = @"SELECT AUTHORITY FROM SYS_USERINFO WHERE ID=" + dr["NO"].ToString() + "";
                DataSet ds = OracleHelper.ExecuteDataSet(OracleHelper.conn, sql, null);
                string authority = ds.Tables[0].Rows[0][0].ToString();
                string[] arr = authority.Split(',');
                for (int i = 0; i < arr.Length; i++)
                {
                    if (!string.IsNullOrEmpty(arr[i].ToString()))
                    {
                        lstCheckedMenuID.Add(arr[i].ToString());
                    }
                    if (arr.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(arr[i].ToString()))
                        {
                            lstCheckedMenuID.Add("1");
                        }
                    }
                }
                RecursionCheckedNodes(treeList1.Nodes);
            }
        }

        /// <summary>
        /// 测试数据（菜单）
        /// </summary>
        /// <returns></returns>
        public List<MenuModel> GetMenus()
        {
            List<MenuModel> list = new List<MenuModel>();

            MenuModel model1 = new MenuModel();
            model1.ID = 1;
            model1.MenuName = "首页";
            model1.OrderID = 1;
            model1.ParentID = 0;
            list.Add(model1);

            MenuModel model2 = new MenuModel();
            model2.ID = 2;
            model2.MenuName = "首页资讯";
            model2.OrderID = 1;
            model2.ParentID = 1;
            list.Add(model2);

            MenuModel model3 = new MenuModel();
            model3.ID = 3;
            model3.MenuName = "首页图片";
            model3.OrderID = 2;
            model3.ParentID = 1;
            list.Add(model3);

            MenuModel model4 = new MenuModel();
            model4.ID = 4;
            model4.MenuName = "首页置顶图片";
            model4.OrderID = 1;
            model4.ParentID = 3;
            list.Add(model4);

            MenuModel model5 = new MenuModel();
            model5.ID = 5;
            model5.MenuName = "首页1";
            model5.OrderID = 1;
            model5.ParentID = 0;
            list.Add(model5);

            return list;
        }

        /// <summary>
        /// 保存菜单权限
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem4_ItemClick(object sender, ItemClickEventArgs e)
        {
            DataRow dr = this.gridBand1.View.GetFocusedDataRow();
            if (dr != null)
            {
                lstCheckedMenuID.Clear();
                strCheckedMenuID = string.Empty;
                if (treeList1.Nodes.Count > 0)
                {
                    foreach (TreeListNode root in treeList1.Nodes)
                    {
                        GetCheckedOfficeID(root);
                    }
                }
                string userId = dr["NO"].ToString();

                string upSql = string.Format(@"UPDATE SYS_USERINFO SET AUTHORITY='{0}' WHERE ID={1}", strCheckedMenuID, userId);
                int count = OracleHelper.ExecuteNonQuery(OracleHelper.conn, upSql, null);
                if (count > 0)
                {
                    MessageBox.Show("操作成功！", "提示");
                }
            }
            else 
            {
                MessageBox.Show("请选择一条用户信息！", "提示");
            }
        }

        //选中ID集合
        private List<string> lstCheckedMenuID = new List<string>();  
        private string strCheckedMenuID = string.Empty;

        // 获取选择状态的数据主键ID集合  
        private void GetCheckedOfficeID(TreeListNode parentNode)
        {
            if (parentNode.Nodes.Count == 0)
            {
                return;//递归终止  
            }

            foreach (TreeListNode node in parentNode.Nodes)
            {
                if (node.CheckState == CheckState.Checked)
                {
                    MenuModel model = treeList1.GetDataRecordByNode(node) as MenuModel;
                    if (model != null)
                    {
                        string id = model.ID.ToString();
                        lstCheckedMenuID.Add(id);
                        strCheckedMenuID += id + ",";
                    }

                }
                GetCheckedOfficeID(node);
            }
        }

        //触发选择节点事件
        private void treeList1_AfterCheckNode(object sender, DevExpress.XtraTreeList.NodeEventArgs e)
        {
            SetCheckedChildNodes(e.Node, e.Node.CheckState);
            SetCheckedParentNodes(e.Node, e.Node.CheckState);  
        }

        //选择某一节点时,该节点的子节点全部选择  取消某一节点时,该节点的子节点全部取消选择  
        private void SetCheckedChildNodes(TreeListNode node, CheckState check)
        {
            //for (int i = 0; i < node.Nodes.Count; i++)
            //{
            //    node.Nodes[i].CheckState = check;
            //    SetCheckedChildNodes(node.Nodes[i], check);
            //}
        }

        // 某节点的子节点全部选择时,该节点选择   某节点的子节点未全部选择时,该节点不选择  
        private void SetCheckedParentNodes(TreeListNode node, CheckState check)
        {
            if (node.ParentNode != null)
            {

                //CheckState parentCheckState = node.ParentNode.CheckState;
                //CheckState nodeCheckState;
                //for (int i = 0; i < node.ParentNode.Nodes.Count; i++)
                //{
                //    nodeCheckState = (CheckState)node.ParentNode.Nodes[i].CheckState;
                //    if (!check.Equals(nodeCheckState))//只要任意一个与其选中状态不一样即父节点状态不全选  
                //    {
                //        parentCheckState = CheckState.Unchecked;
                //        break;
                //    }
                //    parentCheckState = check;//否则（该节点的兄弟节点选中状态都相同），则父节点选中状态为该节点的选中状态  
                //}

                //node.ParentNode.CheckState = parentCheckState;
                node.ParentNode.CheckState = check;
                SetCheckedParentNodes(node.ParentNode, check);//遍历上级节点  
            }
        }

        /// <summary>
        /// 双击用户，打开菜单权限
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void gridControl1_DoubleClick(object sender, EventArgs e)
        {
            //ResfurbishData();
            lstCheckedMenuID.Clear();
            this.treeList1.DataSource = ReadMenusXmlData();// GetMenus();
            this.treeList1.KeyFieldName = "ID";
            treeList1.ParentFieldName = "ParentID";
            //treeList1.Columns[0].Caption = "菜单1";
            ////显示复选框  
            treeList1.OptionsView.ShowCheckBoxes = true;
            //展开树结构
            treeList1.ExpandAll();

            //根据用户ID查询权限菜单
            DataRow dr = this.gridBand1.View.GetFocusedDataRow();
            if (dr != null)
            {
                string sql = @"SELECT AUTHORITY FROM SYS_USERINFO WHERE ID=" + dr["NO"].ToString() + "";
                DataSet ds = OracleHelper.ExecuteDataSet(OracleHelper.conn, sql, null);
                string authority = ds.Tables[0].Rows[0][0].ToString();
                string[] arr = authority.Split(',');
                for (int i = 0; i < arr.Length; i++)
                {
                    if (!string.IsNullOrEmpty(arr[i].ToString()))
                    {
                        lstCheckedMenuID.Add(arr[i].ToString());
                    }
                    if (arr.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(arr[i].ToString()))
                        {
                            lstCheckedMenuID.Add("1");
                        }
                    }
                }
                RecursionCheckedNodes(treeList1.Nodes);
            }
        }

        /// <summary>
        /// 打开添加用户窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            UserInfoForm uf = new UserInfoForm();
            uf.StartPosition = FormStartPosition.CenterScreen;
            uf.Text = "新增用户信息";
            uf.ShowDialog();
            ResfurbishData();
        }

        /// <summary>
        /// 读取XML
        /// </summary>
        /// <returns></returns>
        public List<MenuModel> ReadMenusXmlData() 
        {
            string path = Application.StartupPath + Settings.Default["MenuDataUrl"];
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(path);
            XmlNode xn = xmlDoc.SelectSingleNode("MenuData");
            XmlNodeList xnl = xn.ChildNodes;
            List<MenuModel> listModel = new List<MenuModel>();
            foreach (XmlNode nodel in xnl)
            {
                MenuModel model = new MenuModel();
                XmlElement xe = (XmlElement)nodel;
                XmlNodeList xnll = xe.ChildNodes;
                model.ID = Int32.Parse(xnll.Item(0).InnerText);
                model.MenuName = xnll.Item(1).InnerText;
                model.ParentID = Int32.Parse(xnll.Item(2).InnerText);
                model.OrderID = Int32.Parse(xnll.Item(3).InnerText);
                listModel.Add(model);
            }
            return listModel;
        }

        /// <summary>
        /// 修改用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem2_ItemClick_1(object sender, ItemClickEventArgs e)
        {
            DataRow dr = this.gridBand1.View.GetFocusedDataRow();
            if (dr != null)
            {
                string no = dr["NO"].ToString();
                UserInfoForm uif = new UserInfoForm(no);
                uif.StartPosition = FormStartPosition.CenterScreen;
                uif.Text = "修改用户信息";
                uif.ShowDialog();
                ResfurbishData();
            }
            else 
            {
                MessageBox.Show("请选择一条用户信息！", "提示");
            }
        }

        /// <summary>
        /// 遍历菜单并选中
        /// </summary>
        /// <param name="Nodes"></param>
        private void RecursionCheckedNodes(TreeListNodes Nodes)
        {
            foreach (TreeListNode node in Nodes)
            {
                string id = node.GetValue("ID").ToString();
                if (lstCheckedMenuID.Contains(id))
                {
                    node.Checked = true;
                }

                if (node.Nodes.Count > 0)
                {
                    RecursionCheckedNodes(node.Nodes);
                }
            }
        }

        public void ResfurbishData() 
        {
            this.gridControl1.DataSource = null;
            //this.bandedGridView1.BestFitColumns();
            //this.bandedGridView1.Columns.Clear();
            //gridBand1.Columns.Clear();
            this.gridControl1.DataSource = QueryUserData();
            //this.gridControl1.RefreshDataSource();

            

            this.bandedGridView1.Columns[0].Visible = false;
            //测试（自定义列宽）
            int width = this.groupBox1.Width;
            for (int i = 0; i < this.bandedGridView1.Columns.Count; i++)
            {
                this.bandedGridView1.Columns[i].Width = width / (this.bandedGridView1.Columns.Count - 1);
            }
                
            lstCheckedMenuID.Clear();
            this.treeList1.DataSource = ReadMenusXmlData();// GetMenus();
            this.treeList1.KeyFieldName = "ID";
            treeList1.ParentFieldName = "ParentID";
            //treeList1.Columns[0].Caption = "菜单1";
            ////显示复选框  
            treeList1.OptionsView.ShowCheckBoxes = true;
            //展开树结构
            treeList1.ExpandAll();

            //根据用户ID查询权限菜单
            DataRow dr = this.gridBand1.View.GetFocusedDataRow();
            if (dr != null)
            {
                string sql = @"SELECT AUTHORITY FROM SYS_USERINFO WHERE ID=" + dr["NO"].ToString() + "";
                DataSet ds = OracleHelper.ExecuteDataSet(OracleHelper.conn, sql, null);
                string authority = ds.Tables[0].Rows[0][0].ToString();
                string[] arr = authority.Split(',');
                for (int i = 0; i < arr.Length; i++)
                {
                    if (!string.IsNullOrEmpty(arr[i].ToString()))
                    {
                        lstCheckedMenuID.Add(arr[i].ToString());
                    }
                    if (arr.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(arr[i].ToString()))
                        {
                            lstCheckedMenuID.Add("1");
                        }
                    }
                }
                RecursionCheckedNodes(treeList1.Nodes);
            }
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void barButtonItem3_ItemClick(object sender, ItemClickEventArgs e)
        {
            DataRow dr = this.gridBand1.View.GetFocusedDataRow();
            if (dr != null)
            {
                DialogResult dialog = MessageBox.Show("确认删除用户【" + dr["用户名"].ToString() + "】吗？", "提示", MessageBoxButtons.OKCancel);
                if (dialog == DialogResult.OK)
                {
                    string sql = @"DELETE FROM SYS_USERINFO WHERE ID=" + dr["NO"].ToString() + "";
                    int count = OracleHelper.ExecuteNonQuery(OracleHelper.conn, sql, null);
                    if (count > 0)
                    {
                        ResfurbishData();
                        MessageBox.Show("操作成功！", "提示");
                    }
                }
            }
            else
            {
                MessageBox.Show("请选择一条用户信息！", "提示");
            }
        }

        private void bandedGridView1_CustomDrawRowIndicator(object sender, DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventArgs e)
        {
            if (e.Info.IsRowIndicator && e.RowHandle > -1)
            {
                e.Info.DisplayText = (e.RowHandle + 1).ToString();
            }  
        }

        /// <summary>
        /// GridView  显示行号   设置行号列的宽度
        /// </summary>
        /// <param name="gv">GridView 控件名称</param>
        /// <param name="width">行号列的宽度 如果为null或为0 默认为30</param>
        public void DrawRowIndicator(DevExpress.XtraGrid.Views.Grid.GridView gv, int width)
        {
            gv.CustomDrawRowIndicator += new DevExpress.XtraGrid.Views.Grid.RowIndicatorCustomDrawEventHandler(bandedGridView1_CustomDrawRowIndicator);
            if (width != null)
            {
                if (width != 0)
                {
                    gv.IndicatorWidth = width;
                }
                else
                {
                    gv.IndicatorWidth = 30;
                }
            }
            else
            {
                gv.IndicatorWidth = 30;
            }

        }

    }

    public class MenuModel
    {
        #region 字段属性

        private int id;
        /// <summary>
        /// Gets or sets the menu ID.
        /// </summary>
        public int ID
        {
            get { return id; }
            set { id = value; }
        }

        private int parentID;
        /// <summary>
        /// Gets or sets the parent ID.
        /// </summary>
        /// <value>The parent ID.</value>
        public int ParentID
        {
            get { return parentID; }
            set { parentID = value; }
        }

        private int orderID;
        /// <summary>
        /// Gets or sets the order ID.
        /// </summary>
        /// <value>The order ID.</value>
        public int OrderID
        {
            get { return orderID; }
            set { orderID = value; }
        }

        private string menuName;
        /// <summary>
        /// Gets or sets the name of the menu.
        /// </summary>
        /// <value>The name of the menu.</value>
        public string MenuName
        {
            get { return menuName; }
            set { menuName = value; }
        }

        #endregion

        public MenuModel() { }

        protected MenuModel(MenuModel model)
        {
            this.id = model.id;
            this.menuName = model.menuName;
            this.orderID = model.orderID;
            this.parentID = model.parentID;
        }
    }
}