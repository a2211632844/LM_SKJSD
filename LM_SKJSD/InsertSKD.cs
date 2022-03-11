using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.Metadata.EntityElement;
using Kingdee.BOS.ServiceHelper;
using Kingdee.BOS.Util;
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
        public override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            string PTDH = Convert.ToString(this.View.OpenParameter.GetCustomParameter("ptdh"));//平台单号    
            string FBILLNO = Convert.ToString(this.View.OpenParameter.GetCustomParameter("fbillno"));//单据编号    
            //把值赋到文本框内
            this.Model.SetValue("F_ora_CollectionNo", FBILLNO);

            //根据平台单号 匹配对应  应收单  平台单号
            string sql = string.Format(@"select 
                                         F_ORA_PTDH as 平台单号
                                        , r1.FBILLNO as 应收单单据编号
                                        , r1.FID as 应收单单据头内码
                                        , r2.FID as 应收单明细内码
                                        , r2.FSEQ as 应收单明细行号
                                        , r2.FMATERIALID as 应收单物料编码
                                        , r2.FALLAMOUNTFOR as 应收单金额
                                        , r.FID as 收款结算单内码
                                        , re.F_ORA_AMOUNTCOLLECTION as 收款结算单金额
                                        from t_AR_receivable r1
                                        join t_AR_receivableEntry r2 on r1.FID = r2.FID
                                        join T_LMYD_RECEIVEBILLENTRY re on re.F_ORA_PLATFORMNUM = r1.F_ORA_PTDH
                                        join T_LMYD_RECEIVEBILL r on r.FID = re.FID
                                        where r1.F_ORA_PTDH = '{0}'",PTDH);

            DataSet ds = DBServiceHelper.ExecuteDataSet(this.Context, sql);
            DataTable dt = ds.Tables[0];
            //当匹配到对应 应收单
            if (dt.Rows.Count > 0)
            {
                Entity entity = this.View.BillBusinessInfo.GetEntity("F_ora_Entity");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string ptdh = dt.Rows[i]["F_ORA_PTDH"].ToString();//平台单号
                    //string YSD
                    this.View.Model.CreateNewEntryRow(entity, -1);

                    this.Model.SetValue("F_ora_PTDH", ptdh, i);//平台单号
                }

            }

        }
    }
}
