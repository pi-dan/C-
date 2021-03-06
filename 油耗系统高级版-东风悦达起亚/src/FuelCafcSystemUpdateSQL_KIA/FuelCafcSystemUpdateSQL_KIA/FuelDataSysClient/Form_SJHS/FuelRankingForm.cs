﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;
using DevExpress.XtraBars;
using Common;
using FuelDataSysClient.Tool;
using System.Threading;
using System.Text;
using FuelDataSysClient.FuelCafc;
using DevExpress.XtraSplashScreen;
using FuelDataSysClient.SubForm;

namespace FuelDataSysClient.Form_SJHS
{
    public partial class FuelRankingForm : DevExpress.XtraBars.Ribbon.RibbonForm
    {
        Dictionary<int, string> dictInit = new Dictionary<int, string>();  //字典
        string flag = "";

        public FuelRankingForm()
        {
            InitializeComponent();
            InitDict();
            this.dtStartTime.Text = DateTime.Now.AddDays(1 - DateTime.Now.Day).ToString("yyyy/MM/dd");
            this.dtEndTime.Text = DateTime.Now.AddDays(1 - DateTime.Now.Day).AddMonths(1).AddDays(-1).ToString("yyyy/MM/dd");
        }

        //刷新
        private void barButtonItem1_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (!string.IsNullOrEmpty(flag))
            {
                if (flag == this.btnChineseCompany.Name)
                {
                    this.btnChineseCompany_Click(sender, e);
                }
                else if (flag == this.btnOtherCompany.Name)
                {
                    this.btnOtherCompany_Click(sender, e);
                }
            }

        }

        //导出Excel
        private void barButtonItem2_ItemClick(object sender, ItemClickEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { FileName = "油耗达标排名", Title = "导出Excel", Filter = "Excel文件(*.xlsx)|*.xlsx" };
            DialogResult dialogResult = saveFileDialog.ShowDialog(this);
            if (dialogResult == DialogResult.OK)
            {
                try
                {
                    SplashScreenManager.ShowForm(typeof(DevWaitForm));
                    ExportToExcel toExcel = new ExportToExcel();
                    DataSet ds = new DataSet();
                    if (gridControl1.DataSource != null)
                    {
                        DataTable dt1 = (DataTable)gridControl1.DataSource;
                        dt1.TableName = "计入新能源";
                        DataTable dtc1 = dt1.Copy();
                        dtc1.Columns["QCSCQY"].ColumnName = "企业名称";
                        dtc1.Columns["CAFC"].ColumnName = "实际值/达标值";
                        dtc1.Columns["RANK"].ColumnName = "排名";
                        ds.Tables.Add(dtc1);
                    }
                    if (gridControl2.DataSource != null)
                    {
                        DataTable dt2 = (DataTable)gridControl2.DataSource;
                        dt2.TableName = "不计新能源";
                        DataTable dtc2 = dt2.Copy();
                        dtc2.Columns["QCSCQY"].ColumnName = "企业名称";
                        dtc2.Columns["CAFC"].ColumnName = "实际值/达标值";
                        dtc2.Columns["RANK"].ColumnName = "排名";
                        ds.Tables.Add(dtc2);
                    }
                    toExcel.ToExcelSheet(ds, saveFileDialog.FileName);
                    MessageBox.Show("导出成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception msg)
                {
                    MessageBox.Show(msg.ToString(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    SplashScreenManager.CloseForm();
                }
            }
        }

        //国产企业排名
        private void btnChineseCompany_Click(object sender, EventArgs e)
        {
            try
            {
                SplashScreenManager.ShowForm(typeof(DevWaitForm));
                if (this.VerifyStartEndTime().Equals("OK"))
                {
                    if (Convert.ToDateTime(dtStartTime.Text).Year < 2016)
                    {
                        this.gridControl1.DataSource = get_CAFC_YEAR(true, true);
                        this.gridControl2.DataSource = get_CAFC_YEAR(false, true);
                    }
                    else
                    {
                        this.gridControl1.DataSource = get_CAFC_All(true, true);
                        this.gridControl2.DataSource = get_CAFC_All(false, true);
                    }
                    //该标记刷新使用
                    flag = this.btnChineseCompany.Name;
                }
                else
                {
                    MessageBox.Show(this.VerifyStartEndTime(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                SplashScreenManager.CloseForm();
            }
        }

        //进口企业排名
        private void btnOtherCompany_Click(object sender, EventArgs e)
        {
            try
            {
                SplashScreenManager.ShowForm(typeof(DevWaitForm));
                if (this.VerifyStartEndTime().Equals("OK"))
                {
                    if (Convert.ToDateTime(dtStartTime.Text).Year < 2016)
                    {
                        this.gridControl1.DataSource = get_CAFC_YEAR(true, false);
                        this.gridControl2.DataSource = get_CAFC_YEAR(false, false);
                    }
                    else
                    {
                        this.gridControl1.DataSource = get_CAFC_All(true, false);
                        this.gridControl2.DataSource = get_CAFC_All(false, false);
                    }
                    //该标记刷新使用
                    flag = this.btnOtherCompany.Name;
                }
                else
                {
                    MessageBox.Show(this.VerifyStartEndTime(), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            catch (Exception msg)
            {
                MessageBox.Show(msg.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                SplashScreenManager.CloseForm();
            }
        }

        //获取排名情况
        public DataTable get_CAFC_All(bool isNew, bool isLocal)
        {
            DataTable dtCAFC = new DataTable();
            DataTable dtOrder = new DataTable();
            dtCAFC.Columns.Add("QCSCQY");
            dtCAFC.Columns.Add("CAFC", typeof(decimal));
            dtCAFC.Columns.Add("RANK", typeof(Int32));
            string localFlag = String.Empty;
            string startTime = dtStartTime.Text;
            string endTime = dtEndTime.Text;
            //是否国产
            if (isLocal)
            {
                localFlag = "C";
            }
            else
            {
                localFlag = "F";
            }
            //是否计入新能源
            if (isNew)
            {
                var order_Ne = Utils.serviceCafc.GetProc_CAFC_NE_All(Utils.userId, Utils.password, localFlag, "", startTime, endTime, "");
                dtOrder = DataTableHelper.ToDataTable<CafcService.FuelCafcAndTcafcAndTcafc1>(order_Ne);
            }
            else
            {
                var order_Te = Utils.serviceCafc.GetProc_CAFC_TE_All(Utils.userId, Utils.password, localFlag, "", startTime, endTime, "");
                dtOrder = DataTableHelper.ToDataTable<CafcService.FuelCafcAndTcafcAndTcafc1>(order_Te);
            }
            foreach (DataRow item in dtOrder.Rows)
            {
                DataRow drCAFC = dtCAFC.NewRow();
                drCAFC["QCSCQY"] = item["Qcscqy"];
                decimal dbz = Math.Round(Convert.ToDecimal(item["Tcafc"]) * Convert.ToDecimal(dictInit[Convert.ToDateTime(dtStartTime.Text).Year]), 2, MidpointRounding.AwayFromZero);
                drCAFC["CAFC"] = Math.Round(((Convert.ToDecimal(item["Cafc"]) / dbz) * 100), 2, MidpointRounding.AwayFromZero);
                dtCAFC.Rows.Add(drCAFC);
            }
            DataView dv = dtCAFC.DefaultView;
            dv.Sort = "CAFC asc";
            dtCAFC = dv.ToTable();
            for (int i = 1; i <= dtCAFC.Rows.Count; i++)
            {
                dtCAFC.Rows[i - 1]["RANK"] = i;
            }
            return dtCAFC;
        }

        //初始化字典 倍数 2015 年之前标准
        private void InitDict()
        {
            dictInit.Add(2012, "1.09");
            dictInit.Add(2013, "1.06");
            dictInit.Add(2014, "1.03");
            dictInit.Add(2015, "1.00");
            dictInit.Add(2016, "1.34");
            dictInit.Add(2017, "1.28");
            dictInit.Add(2018, "1.20");
            dictInit.Add(2019, "1.10");
            dictInit.Add(2020, "1.00");
        }

        //验证查询时间
        protected string VerifyStartEndTime()
        {
            try
            {
                string dtStart = dtStartTime.Text;
                string dtEnd = dtEndTime.Text;
                if (string.IsNullOrEmpty(dtStart) || string.IsNullOrEmpty(dtEnd))
                {
                    return "请选择排名时间";
                }
                else
                {
                    DateTime startTime = Convert.ToDateTime(dtStart);
                    DateTime endTime = Convert.ToDateTime(dtEnd);
                    if (startTime > endTime)
                    {
                        return "开始时间不能大于结束时间";
                    }
                    else if (startTime.Year < 2013)
                    {
                        dtStartTime.Text = "2013/1/1";
                        dtEndTime.Text = "2014/1/1";
                    }
                    else if (startTime.Year < 2016)
                    {
                        dtStartTime.Text = startTime.Year + "/1/1";
                        dtEndTime.Text = (startTime.Year + 1) + "/1/1";
                    }
                }
            }
            catch (Exception e)
            {
                return "系统出错:" + e.Message;
            }
            return "OK";
        }

        //获取指定年份排名，2016年之前全部为不计新能源，有计入/不计入新能源；和国产/进口区分
        public DataTable get_CAFC_YEAR(bool isNew, bool isLocal)
        {
            int year = Convert.ToDateTime(dtStartTime.Text).Year;
            try
            {
                StringBuilder strWhere = new StringBuilder();//where条件
                if (isLocal)
                {
                    strWhere.Append(@" AND DOMESTIC=1 ");
                }
                else
                {
                    strWhere.Append(@" AND DOMESTIC=0 ");
                }
                if (isNew)
                {
                    strWhere.Append(@" AND STATUS=1 ");
                }
                else
                {
                    strWhere.Append(@" AND STATUS=0 ");
                }
                DataSet ds = OracleHelper.ExecuteDataSet(OracleHelper.conn, string.Format(@"select QCSCQY,round(CAFC*100 ,2) as CAFC,RANK from FUEL_RANKING WHERE YEAR ={0} {1} ORDER BY RANK ASC", year, strWhere), null);
                if (ds != null && ds.Tables[0].Rows.Count > 0)
                {
                    return ds.Tables[0];
                }
                else
                {
                    MessageBox.Show("无官方数据", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}