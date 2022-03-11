using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
using LM_SKJSD.SKD;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LM_SKJSD
{
    [HotUpdate]
    public class InsertSKD : AbstractDynamicFormPlugIn
    {
        /// <summary>
        /// 动态表单加载
        /// </summary>
        /// <param name="e"></param>
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            List<string> list = this.View.OpenParameter.GetCustomParameter("PTDH") as List<string>;

            string FBILLNO = Convert.ToString(this.View.OpenParameter.GetCustomParameter("fbillno"));//单据编号    
            //把值赋到文本框内
            this.Model.SetValue("F_ora_CollectionNo", FBILLNO);

            for (int j = 0; j < list.Count; j++)
            {
                //根据平台单号 匹配对应  应收单  平台单号
                string sql = string.Format(@"select 
                                         F_ORA_PTDH 
                                        , r1.FBILLNO 
                                        , r1.FID as YSDFID
                                        , r2.FID as YSDENTRYFID
                                        , r2.FSEQ
                                        , r2.FMATERIALID 
                                        , r2.FALLAMOUNTFOR as YSDAMOUNT
                                        , r1.FALLAMOUNTFOR as SUMYSDAMOUNT
                                        , r.FID as SKJSDFID
                                        , re.F_ORA_AMOUNTCOLLECTION 
                                        , c.FNUMBER
                                        , c.FCUSTID
                                        from t_AR_receivable r1
                                        join t_AR_receivableEntry r2 on r1.FID = r2.FID
                                        join T_LMYD_RECEIVEBILLENTRY re on re.F_ORA_PLATFORMNUM = r1.F_ORA_PTDH
                                        join T_LMYD_RECEIVEBILL r on r.FID = re.FID
                                        join T_BD_CUSTOMER_L cl on cl.FNAME = re.F_ORA_CUSTOMER
                                        join T_BD_CUSTOMER c on c.FCUSTID = cl.FCUSTID
                                        where r1.F_ORA_PTDH = '{0}'", list[j]);

                DataSet ds = DBServiceHelper.ExecuteDataSet(this.Context, sql);
                DataTable dt = ds.Tables[0];
                //当匹配到对应 应收单
                if (dt.Rows.Count > 0)
                {
                    Entity entity = this.View.BillBusinessInfo.GetEntity("F_ora_Entity");
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        this.View.Model.CreateNewEntryRow(entity, i);//新增一行
                        string ptdh = dt.Rows[i]["F_ORA_PTDH"].ToString();//平台单号
                        string YSDFbillNo = dt.Rows[i]["FBILLNO"].ToString();//应收单单据编号
                        string YSDFID = dt.Rows[i]["YSDFID"].ToString();//应收单单据头内码
                        string YSDEntryFID = dt.Rows[i]["YSDENTRYFID"].ToString();//应收单明细内码
                        string YSDFSEQ = dt.Rows[i]["FSEQ"].ToString();//应收单明细行号
                        string YSDMaterial = dt.Rows[i]["FMATERIALID"].ToString();//应收单物料编码
                        string YSDAMOUNT = dt.Rows[i]["YSDAMOUNT"].ToString();//应收单金额
                        string SUMYSDAMOUNT = dt.Rows[i]["SUMYSDAMOUNT"].ToString();//应收单总金额
                        string SKJSDAMOUNT = dt.Rows[i]["F_ORA_AMOUNTCOLLECTION"].ToString();//收款结算单金额
                        string SKJSDFID = dt.Rows[i]["SKJSDFID"].ToString();//收款结算内码
                        string KH = dt.Rows[i]["FNUMBER"].ToString();//客户
                        string KHJ = dt.Rows[i]["FCUSTID"].ToString();//基础资料客户



                        this.Model.SetValue("F_ora_PTDH", ptdh, i);//平台单号
                        this.Model.SetValue("F_ora_YSDH", YSDFbillNo, i);//应收单单据编号
                        this.Model.SetValue("F_ora_YSDBillNo", YSDFID, i);//应收单单据头内码
                        this.Model.SetValue("F_ora_YSDEntryBillNo", YSDEntryFID, i);//应收单明细内码
                        this.Model.SetValue("F_ora_YSDEntryNo", YSDFSEQ, i);//应收单明细行号
                        this.Model.SetValue("F_ora_YSDMaterial", YSDMaterial, i);//应收单物料编码
                        this.Model.SetValue("F_ora_YSDAmount", YSDAMOUNT, i);//应收单金额
                        this.Model.SetValue("F_ora_SKJSDBillNo", SKJSDFID, i);//收款结算内码
                        this.Model.SetValue("F_ora_KH", KH, i);//客户
                        this.Model.SetValue("F_ora_KHJ", KHJ, i);//客户
                        this.Model.SetValue("F_ora_AllocationSKDAmount", YSDAMOUNT, i);


                        int line = dt.Rows.Count;//多少行
                        decimal differ1 = Convert.ToDecimal(SUMYSDAMOUNT) - (Convert.ToDecimal(SUMYSDAMOUNT) - Convert.ToDecimal(YSDAMOUNT)); //总金额 - 一条明细金额  60

                        //收款结算单金额 > 应收单总金额  多余的金额放在单据头 多余金额 上
                        if (Convert.ToDecimal(SKJSDAMOUNT) > Convert.ToDecimal(SUMYSDAMOUNT))
                        {
                            decimal ExcessAmount;
                            decimal dec;
                            ExcessAmount = Convert.ToDecimal(SKJSDAMOUNT) - Convert.ToDecimal(SUMYSDAMOUNT);//多余金额
                            //decimal decim = ExcessAmount + dec;
                            this.Model.SetValue("F_ora_AllocationSKDAmount", YSDAMOUNT, i);//分配收款单金额
                            this.Model.SetValue("F_ora_ExcessAmount", ExcessAmount);//分配多余金额
                        }
                        //收款结算单金额 < 应收单金额   方法是
                        else
                        {
                            //当为最后一条明细的时候 给他赋值 
                            if (line == i + 1)
                            {
                                decimal DifferAmount = Convert.ToDecimal(SUMYSDAMOUNT) - Convert.ToDecimal(SKJSDAMOUNT);//应收单金额 - 收款结算单金额 
                                decimal dec = differ1 - DifferAmount;  //最后一行60 -1 两张单相差金额
                                this.Model.SetValue("F_ora_AllocationSKDAmount", dec, line - 1);//分配收款单金额 为最后一行的时候
                            }
                        }
                        decimal deci = Convert.ToDecimal(this.Model.GetValue("F_ora_AllocationSKDAmount", i));
                        //如果分配收款单金额大于0
                        if (deci > 0)
                        {
                            this.Model.SetValue("F_ora_CreateCheckBox", true, i);
                        }
                        else
                        {
                            this.Model.SetValue("F_ora_CreateCheckBox", false, i);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 点击生成收款单
        /// </summary>
        /// <param name="e"></param>
        public override void AfterButtonClick(AfterButtonClickEventArgs e)
        {
            base.AfterButtonClick(e);
            string YSDAMOUNT = "";
            string kh = "";
            string SKJSDNM = "";
            decimal DYJE = Convert.ToDecimal(this.View.Model.GetValue("F_ora_ExcessAmount"));
            //点击生成收款单按钮
            if (e.Key.EqualsIgnoreCase("F_ora_CreateBtn"))
            {
                if (DYJE > 0)
                {
                    JObject joe = new JObject();
                    joe.Add("FNOTAXAMOUNTFOR", DYJE);//多余金额
                    //生成其他应收单
                    WebApiResultHelper result1 = new WebApiResultHelper(QTYSDCreate.JsonAdd2(joe.ToString(), "admin", "666666", Context.DBId, "https://47.112.137.109/K3cloud/"));
                    if (result1.IsSuccess)
                    {
                        this.View.ShowMessage("生成其他应收单成功");
                    }
                    else
                    {
                        this.View.ShowMessage("生成其他应收单失败");
                    }

                }
                int idx = this.View.Model.GetEntryRowCount("F_ora_Entity");
                int index = this.View.Model.GetEntryRowCount("F_ora_Entity");
                JObject jo = new JObject();
                JArray jar = new JArray();
                List<string> list = new List<string>();
                for (int i = 0; i < index; i++)
                {
                    bool check = Convert.ToBoolean(this.View.Model.GetValue("F_ora_CreateCheckBox", i));
                    //勾选了生成应收单复选框
                    if (check == true)
                    {
                        YSDAMOUNT = this.View.Model.GetValue("F_ora_YSDAmount", i).ToString(); //应收单金额
                        kh = this.View.Model.GetValue("F_ora_KH", i).ToString();//客户
                        SKJSDNM = this.View.Model.GetValue("F_ora_SKJSDBillNo", i).ToString();//

                        JArray entryRows = new JArray();

                        // 把单据体行集合，添加到model中，以单据体Key为标识
                        string entityKey = "FRECEIVEBILLENTRY";

                        JObject entitymodel = new JObject();

                        entitymodel.Add("FRECTOTALAMOUNTFOR", YSDAMOUNT);//金额
                        entitymodel.Add("F_ORA_SKJSDENTRYBILLNO", SKJSDNM);//收款结算单内码
                        jar.Add(entitymodel);
                    }
                }
                jo.Add("FCONTACTUNIT", kh); //往来单位  客户
                                            //收款单
                WebApiResultHelper result = new WebApiResultHelper(SKDInventory.JsonAdd(jo.ToString(), "admin", "666666", Context.DBId, "https://47.112.137.109/K3cloud/", jar));
                if (result.IsSuccess)
                {
                    this.View.ShowMessage("生成收款单成功");
                }
                else
                {
                    this.View.ShowMessage("生成收款单成功");
                }
            }
        }
    }
}
