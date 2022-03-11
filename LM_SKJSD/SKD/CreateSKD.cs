using Kingdee.BOS.Core;
using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace LM_SKJSD
{
    [HotUpdate]
    public class CreateSKD :  AbstractBillPlugIn
    {
        public override void AfterEntryBarItemClick(AfterBarItemClickEventArgs e)
        {
            base.AfterEntryBarItemClick(e);
            //点击生成收款单按钮
            
            if (e.BarItemKey == "tbSKDBtn")
            {
                List<string> PTDH = new List<string>();
                
                string fbillno = this.View.Model.GetValue("FBillNo").ToString();//单据编号
                int idx = this.View.Model.GetEntryCurrentRowIndex("FEntity");
                int index = this.View.Model.GetEntryRowCount("FEntity");
                for (int i = 0; i < index; i++)
                {
                    string ptdh = this.View.Model.GetValue("F_ORA_PLATFORMNUM", i).ToString();//平台单号
                    string dddh = this.View.Model.GetValue("F_ORA_DDDH", i).ToString();//多点单号
                    string gydh = this.View.Model.GetValue("F_ORA_GYDH", i).ToString();//管易单号
                    //string kh  = 
                    PTDH.Add(ptdh);
                    PTDH.Add(dddh);
                    PTDH.Add(gydh);
                }
                //打开动态表单
                DynamicFormShowParameter param = new DynamicFormShowParameter();
                param.FormId = "k1cfa88a6b557434cbbe7ca45272d7942";//批量生成收款单 动态表单  kbb78adf23dfa49c6afaaf4183e5cbad8

                //param.CustomParams.Add("PTDH",PTDH); //平台单号
                param.CustomComplexParams.Add("PTDH",PTDH);
                param.CustomParams.Add("FBILLNO", fbillno); //单据编号
                
                //传送参数到动态表单  
                this.View.ShowForm(param, new Action<FormResult>((formResult) =>
                {
                    //页面赋值
                    if (formResult != null && formResult.ReturnData != null)
                    {
                    }
                    else
                    {
                    }
                }
                ));
            }
        }
    }
}
