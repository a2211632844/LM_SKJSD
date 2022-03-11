using Kingdee.BOS.Core.Bill.PlugIn;
using Kingdee.BOS.Core.DynamicForm;
using Kingdee.BOS.Core.DynamicForm.PlugIn;
using Kingdee.BOS.Core.DynamicForm.PlugIn.Args;
using Kingdee.BOS.Orm.DataEntity;
using Kingdee.BOS.Util;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (e.BarItemKey == "tbCreateSKDBtn")
            {
                string fbillno = this.View.Model.GetValue("FBillNo").ToString();//单据编号
                int idx = this.View.Model.GetEntryCurrentRowIndex("FEntity");
                string ptdh = this.View.Model.GetValue("F_ORA_PLATFORMNUM", idx).ToString();//平台单号

                //打开动态表单
                DynamicFormShowParameter param = new DynamicFormShowParameter();
                param.FormId = "kbb78adf23dfa49c6afaaf4183e5cbad8";//批量生成收款单 动态表单

                param.CustomParams.Add("PTDH", ptdh); //平台单号
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
                        //this.View.ShowErrMessage("条码值为空");
                    }

                }
                ));
            }
        }
    }
}
