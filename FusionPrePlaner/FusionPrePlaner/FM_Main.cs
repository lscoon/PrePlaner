﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Net;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FusionPrePlaner
{
    public partial class FM_Main : Form
    {
        DateTime dt_NextRun;
        private bool curAutoRunStat = false;



        public FM_Main()
        {
            InitializeComponent();
            this.dgv_STO.DataSource = ScrumTeamOwner.STO_FULL_LIST;
            
            this.txtInterval.DataBindings.Add("Text", Config.Instance, "PrePlanInterval", false, DataSourceUpdateMode.Never);
            this.chkAutoRun.DataBindings.Add("Checked", Config.Instance, "AutoRun", false, DataSourceUpdateMode.Never);
            this.chkRunOnStart.DataBindings.Add("Checked", Config.Instance, "RunOnStart", false, DataSourceUpdateMode.Never);

            this.txtRestApiPath.DataBindings.Add("Text", Config.Instance, "RestApi_Path", false, DataSourceUpdateMode.Never);
            this.txtUserName.DataBindings.Add("Text", Config.Instance, "UserName", false, DataSourceUpdateMode.Never);
            this.txtUserPassword.DataBindings.Add("Text", Config.Instance, "Password", false, DataSourceUpdateMode.Never);
            curAutoRunStat = Config.Instance.AutoRun;

         

        }

        private void chkSelectAllSTO_CheckedChanged(object sender, EventArgs e)
        {
            foreach (var item in ScrumTeamOwner.STO_FULL_LIST)
                item.Selected = this.chkSelectAllSTO.Checked;
            this.dgv_STO.Invalidate();
        }

  

        private delegate void deleBindDataSource(PrePlanner preplanner);
        private void BindDataSource(PrePlanner preplanner)
        {      
            dgv_AvailableIssues.DataSource = preplanner.DT_AvailIssues;
            dgv_AvailableIssues.Columns["Start FB"].Visible = false;
            dgv_AvailableIssues.Columns["End FB"].Visible = false;
            dgv_AvailableIssues.Columns["Ori Eff"].Visible = false;
            
          
            dgv_UntouchableIssues.DataSource = preplanner.DT_UntouchableIssues;
            dgv_UntouchableIssues.Columns["Start FB"].Visible = false;
            dgv_UntouchableIssues.Columns["End FB"].Visible = false;
            dgv_UntouchableIssues.Columns["Ori Eff"].Visible = false;
        }
        
        private void RefreshDgvSTOIssues()
        {
            if (dgv_STO.SelectedRows.Count > 0)
            {
                var obj = dgv_STO.SelectedRows[0].Cells["Code"].Value;
                string teamcode = obj == null ? null : obj.ToString();
                var preplanner = PrePlanner.GetPrePlannerFromTeamCode(teamcode);
         
                if (preplanner != null)
                {
                    this.Invoke(new deleBindDataSource(BindDataSource), preplanner);               
                }

                obj = dgv_STO.SelectedRows[0].Cells["col_STO"].Value;
                if(obj !=null)
                {
                    string teamname = obj.ToString();
                    for (int j = 0; j < dgv_FBList.ColumnCount; j++)
                    {

                        if (dgv_FBList.Columns[j].Name == "FB" || dgv_FBList.Columns[j].Name == "Start Date" || dgv_FBList.Columns[j].Name == "End Date")
                        {
                            dgv_FBList.Columns[j].Visible = true;
                        }
                        else if (dgv_FBList.Columns[j].Name.Contains(teamname))
                        {
                            dgv_FBList.Columns[j].Visible = true;
                        }
                        else
                        {
                            dgv_FBList.Columns[j].Visible = false;
                        }

                    }
                }
                
            }

        }

        public delegate void RefreshStatusDele(string strStatus);
        public void RefreshStatus(string strStatus)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new RefreshStatusDele(RefreshStatus), strStatus);
            }
            else
            {
                this.statusStrip1.Text = strStatus;
            }
        }

        public delegate void RefreshUIDgvSTODele();
        public void RefreshUIDgvSTO()
        {
            if(this.InvokeRequired)
            {
                this.BeginInvoke(new RefreshUIDgvSTODele(RefreshUIDgvSTO),null);
            }
            else
            {
              
                CurrencyManager cm = dgv_STO.BindingContext[dgv_STO.DataSource] as CurrencyManager;
                cm.Refresh();
            }
        }
      

        public delegate void RefreshUIDgvIssuesDele();
        public void RefreshUIDgvAvailIssues()
        {
            
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new RefreshUIDgvIssuesDele(RefreshUIDgvAvailIssues));
            }
            else
            {
           
                try
                {
                   
                    CurrencyManager cm = dgv_AvailableIssues.BindingContext[dgv_AvailableIssues.DataSource] as CurrencyManager; //bsAvail.CurrencyManager;
                    cm.Refresh();

                    
                  
                }
                catch
                {
                    //nodo
                }
            }
            
          

        }

        public delegate void RefreshUISTO_Progress_Dele();
        public void RefreshUISTO_Progress()
        {

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new RefreshUISTO_Progress_Dele(RefreshUISTO_Progress));
            }
            else
            {

                try
                {

                    // CurrencyManager cm = dgv_STO.BindingContext[dgv_STO.DataSource] as CurrencyManager; //bsAvail.CurrencyManager;
                    //cm.Refresh();
                    dgv_STO.Invalidate();
                }
                catch
                {
                    //nodo
                }
            }



        }
        
        public void RefreshUIDgvUntouchableIssues()
        {

            if (this.InvokeRequired)
            {
                this.BeginInvoke(new RefreshUIDgvIssuesDele(RefreshUIDgvUntouchableIssues));
            }
            else
            {

                try
                {
                   
                    CurrencyManager cm = dgv_UntouchableIssues.BindingContext[dgv_UntouchableIssues.DataSource] as CurrencyManager;//bsUntouch.CurrencyManager;
                    cm.Refresh();
                }
                catch
                {

                }
            }



        }

        private void dgv_STO_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dgv_STO.Columns[e.ColumnIndex].Name == "col_Preplan")
            {

                if (dgv_STO.Rows[e.RowIndex].Cells["col_Preplan"].Value.ToString() != "Run")
                {
                    MessageBox.Show("Already Running");
                }
                // else if ((STO_RUN_STAT)dgv_STO.Rows[e.RowIndex].Cells["col_runstat"].Value == STO_RUN_STAT.TO_RUN)
                else if (dgv_STO.Rows[e.RowIndex].Cells["col_Preplan"].Value.ToString() == "Run")
                {
                    var sto_name = dgv_STO.Rows[e.RowIndex].Cells["col_STO"].Value.ToString();
                    var sto = ScrumTeamOwner.GetSTO(sto_name);
                    var preplanner = PrePlanner.GetPrePlannerFromTeamCode(sto.Code);


                    preplanner.AsyncProcessSTO();

                }
            }
        }

        private void btnRunAll_Click(object sender, EventArgs e)
        {
            RunAllPrePlan();
        }
        
        private static void RunAllPrePlan()
        {
           

            foreach (var preplanner in PrePlanner.PrePlannerList)
            {
                preplanner.AsyncProcessSTO();
            }
        }

        private void FM_Main_Load(object sender, EventArgs e)
        {
            LoadFB();//add for show FB data
            RefreshDgvSTOIssues();
            if (Config.Instance.RunOnStart)
            {
                RunAllPrePlan();
            }

            timer1Sec.Start();
            if (Config.Instance.AutoRun)
            {
                dt_NextRun = DateTime.Now.AddMinutes(Config.Instance.PrePlanInterval);
            }

        }

        private void btnRunConfigApply_Click(object sender, EventArgs e)
        {
            try
            {
                Config.Instance.PrePlanInterval = int.Parse(txtInterval.Text);
                Config.Instance.AutoRun = chkAutoRun.Checked;
                Config.Instance.RunOnStart = chkRunOnStart.Checked;
                Config.Serialize(Config.Instance);
            }
            catch (System.Exception exp)
            {
                MessageBox.Show(exp.Message, "Apply Configuration Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.btnRunConfigApply.Enabled = txtInterval.Text.Trim() != Config.Instance.PrePlanInterval.ToString();
            this.btnRunConfigCancel.Enabled = this.btnRunConfigApply.Enabled;
        }

        private void btnRunConfigCancel_Click(object sender, EventArgs e)
        {
            chkAutoRun.Checked = Config.Instance.AutoRun;
            txtInterval.Text = Config.Instance.PrePlanInterval.ToString();

            this.btnRunConfigApply.Enabled = txtInterval.Text.Trim() != Config.Instance.PrePlanInterval.ToString();
            this.btnRunConfigCancel.Enabled = this.btnRunConfigApply.Enabled;
        }

       
        private void btnRestApiConfigApply_Click(object sender, EventArgs e)
        {
            try
            {
                Config.Instance.RestApi_Path = txtRestApiPath.Text.Trim();
                Config.Instance.UserName = txtUserName.Text.Trim();
                Config.Instance.Password = txtUserPassword.Text.Trim();
                Config.Serialize(Config.Instance);
            }
            catch (System.Exception exp)
            {
                MessageBox.Show(exp.Message, "Apply Configuration Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.btnRestApiConfigApply.Enabled = (!string.IsNullOrEmpty(txtUserName.Text.Trim()) && !string.IsNullOrEmpty(txtUserName.Text.Trim()) && !string.IsNullOrEmpty(txtUserName.Text.Trim()))
                                                 && (txtRestApiPath.Text.Trim() != Config.Instance.RestApi_Path.ToString()
                                                 || txtUserName.Text.Trim() != Config.Instance.UserName.ToString()
                                                 || txtUserPassword.Text.Trim() != Config.Instance.Password.ToString());
            this.btnRestApiConfigCancel.Enabled = this.btnRestApiConfigApply.Enabled;
        }
        private void btnRestApiConfigCancel_Click(object sender, EventArgs e)
        {

            txtRestApiPath.Text = Config.Instance.RestApi_Path.ToString();
            txtUserName.Text = Config.Instance.UserName.ToString();
            txtUserPassword.Text = Config.Instance.Password.ToString();


            this.btnRestApiConfigApply.Enabled = (!string.IsNullOrEmpty(txtUserName.Text.Trim()) && !string.IsNullOrEmpty(txtUserName.Text.Trim()) && !string.IsNullOrEmpty(txtUserName.Text.Trim()))
                                                 && (txtRestApiPath.Text.Trim() != Config.Instance.RestApi_Path.ToString()
                                                 || txtUserName.Text.Trim() != Config.Instance.UserName.ToString()
                                                 || txtUserPassword.Text.Trim() != Config.Instance.Password.ToString());
            this.btnRestApiConfigCancel.Enabled = this.btnRestApiConfigApply.Enabled;
        }
        private void RunConfig_Changed(object sender, EventArgs e)
        {
            int parsedSec;
            if (int.TryParse(txtInterval.Text, out parsedSec) && parsedSec > 0)
            {
                errProviderConfig.SetError(txtInterval, null);

            }
            else
            {
                errProviderConfig.SetError(txtInterval, "No less than 1 minute");


            }
            this.btnRunConfigApply.Enabled = (parsedSec > 0) && (txtInterval.Text.Trim() != Config.Instance.PrePlanInterval.ToString()
                                     || chkAutoRun.Checked != Config.Instance.AutoRun || chkRunOnStart.Checked != Config.Instance.RunOnStart);
            this.btnRunConfigCancel.Enabled = this.btnRunConfigApply.Enabled;
        }
        private void RestAPIConfig_Changed(object sender, EventArgs e)
        {
            var txtbox = sender as TextBox;
            if (string.IsNullOrEmpty(txtbox.Text.Trim())
                
                )
            {
                errProviderConfig.SetError(txtbox, "Can not be empty");

            }
            else
            {
                errProviderConfig.SetError(txtbox, null);
            }

            this.btnRestApiConfigApply.Enabled = txtRestApiPath.Text.Trim() != Config.Instance.RestApi_Path.ToString()
                                                || txtUserName.Text.Trim() != Config.Instance.UserName.ToString()
                                                || txtUserPassword.Text.Trim() != Config.Instance.Password.ToString();
            this.btnRestApiConfigCancel.Enabled = this.btnRestApiConfigApply.Enabled;
        }



        private void timer1Sec_Tick(object sender, EventArgs e)
        {
           
            if (Config.Instance.AutoRun)
            {
                if(curAutoRunStat != Config.Instance.AutoRun)
                {
                    dt_NextRun = DateTime.Now.AddMinutes(Config.Instance.PrePlanInterval);
                }
                this.txtNextRunDt.Text = this.dt_NextRun.ToString("yyyy-MM-dd HH:mm:ss");
                this.txtCountDown.Text = ((int)((this.dt_NextRun - DateTime.Now).TotalSeconds)).ToString();
                if (DateTime.Now >= this.dt_NextRun)
                {
                    this.dt_NextRun = DateTime.Now.AddMinutes(Config.Instance.PrePlanInterval);
                    RunAllPrePlan();
                }
            }
            else
            {
                this.txtNextRunDt.Text = "N/A";
                this.txtCountDown.Text = "N/A";
            }
            curAutoRunStat = Config.Instance.AutoRun;
        }
        
        private void btnTestConn_Click(object sender, EventArgs e)
        {
    
            string res = RestAPIAccess.ExecuteRestAPI_CURL(txtUserName.Text.Trim(), txtUserPassword.Text.Trim(), txtRestApiPath.Text.Trim(), "GET", "search?jql=cf[29790]=1312&startAt=0&maxResults=3");

            try
            {
                var jobj =JObject.Parse(res);
                int issuecount = 0;
                
                if(int.TryParse(jobj["total"].ToString(), out issuecount) && issuecount>0)
                {
                    MessageBox.Show("Test OK");
                }              
                else
                {
                    MessageBox.Show("Test Failed");
                }
                    
            }
            catch
            {
                MessageBox.Show("Test Failed");
            }

        }

        private void dgv_STO_SelectionChanged(object sender, EventArgs e)
        {
            RefreshDgvSTOIssues();
           
        }

       // public DataTable DT_FB;
       // public DataTable DT_Rel;

        private void LoadFB()
        {
            dgv_FBList.DataSource = null; //每次打开清空内容
            /*
            DataTable dt_xls_dates = FeatureBuild.excelToDataSet("FZM FBP tool.xlsb", "Feature Build,Start Date,End Date", "Dates");                         //调用GetData方发写上Excel文件所在的路径，这样就能获取到Excel表里面的数据了                                                                                                                              
            DataTable dt_xls_cap = FeatureBuild.excelToDataSet("FZM FBP tool.xlsb", "Capacities,FT_FZ01_Dev,FT_FZ02_Dev", "cap");                         //调用GetData方发写上Excel文件所在的路径，这样就能获取到Excel表里面的数据了        
         
            var dt_dates = FeatureBuild.FormatDataTableDates(dt_xls_dates);
            var dt_cap = FeatureBuild.FormatDataTableCap(dt_xls_cap);
            DT_Rel = FeatureBuild.FormatDataTableRelease(dt_xls_dates);

            DT_FB = FeatureBuild.MergeDataTable(dt_dates, dt_cap);
            */
            PrePlanner.load_fb();
            dgv_FBList.DataSource = PrePlanner.DT_FB;
            dgv_Release.DataSource = PrePlanner.DT_REL;
           
        }
        
    }
}
