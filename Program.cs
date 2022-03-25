//revit
//Program: 主程式

using System;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.Attributes;

public class Program {

    /// <summary>
    /// 執行外部程式
    /// </summary>
    /// <param name="commandData">指令資料</param>
    /// <param name="message">資訊</param>
    /// <param name="elements">元素(物件)</param>
    /// <returns>執行結果</returns>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {

        //Var 必要參考對象
        UIApplication uiApp = commandData.Application;
        Document doc = uiApp.ActiveUIDocument.Document;

        //嘗試執行 ->
        try {

            // 物件集合 (選中的物件集合的 元素)
            var referenceCol = uiApp.ActiveUIDocument.Selection.PickObjects(ObjectType.Element, "請選擇模型構件");

            //判斷是否有選擇物件 及 超過 1個物件
            if(referenceCol.Count > 1) {
                //TaskDialog.Show("Counter", $"選中了: {referenceCol.Count}個物件"); //For debug
                JoinAllGeometry(doc, referenceCol); // 接合所有物件函數方法
            } else {
                TaskDialog.Show("Warning", "沒有選擇任何物件！");
            }

        //捕獲錯誤(防止當機)
        } catch (Exception e) {
            message = e.Message; //寫入錯誤資訊
            TaskDialog.Show("Error", message); //輸出錯誤資訊
            return ResultsFlag.Failed; //失敗
        }

        //成功
        return Result.Succeeded;
    }


    #region 物件接合方法函數

    // 判斷優先順序與選擇順序是否一致
    private bool RunWithDefaultOrder(Element e1, Element e2) =>
        GetOrderNumber(e1) >= GetOrderNumber(e2);

    // 判斷選擇物件的種類
    private int GetOrderNumber(Element e) {
        switch (e.Category.Name) {
            case "Structural Framing": return 1;    //梁
            case "Structural Columns": return 2;    //柱
            case "Walls": return 3;                 //牆
            case "Floors": return 4;                //版
            default: return 0;
        }
    }

    // 執行接合
    private void RunJoinGeometry(Document doc, Element e1, Element e2) {
        if (!JoinGeometryUtils.AreElementsJoined(doc, e1, e2)) {
            JoinGeometryUtils.JoinGeometry(doc, e1, e2);
        } else {
            JoinGeometryUtils.UnjoinGeometry(doc, e1, e2);
            JoinGeometryUtils.JoinGeometry(doc, e1, e2);
        }
    }

    // 執行接合並改變接合順序
    private void RunJoinGeometryAndSwitch(Document doc, Element e1, Element e2) {
        if (!JoinGeometryUtils.AreElementsJoined(doc, e1, e2)) {
            JoinGeometryUtils.JoinGeometry(doc, e1, e2);
            JoinGeometryUtils.SwitchJoinOrder(doc, e1, e2);
        } else {
            JoinGeometryUtils.UnjoinGeometry(doc, e1, e2);
            JoinGeometryUtils.JoinGeometry(doc, e1, e2);
            JoinGeometryUtils.SwitchJoinOrder(doc, e1, e2);
        }
    }

    #endregion


    /// <summary>
    /// 接合所有物件
    /// </summary>
    /// <param name="referenceCol"></param>
    [Transaction(TransactionMode.Manual)]
    public void JoinAllGeometry(Document doc, IList<Reference> referenceCol) {

        //元素集合(物件集合: 將參考型態轉換成元素型態的緩存集合)
        List<Element> elements = new List<Element>();

        //型態轉換並緩存到元素集合
        foreach (var reference in referenceCol) {
            Element element = doc.GetElement(reference); //獲取元素
            //如果元素存在, 才進行緩存
            if(element != null) {
                elements.Add(element); //添加元素到元素集合中
            }
        }

        using (Transaction trans = new Transaction(doc)) {  //獲取 & 自動釋放資源
            
            //開始執行任務
            trans.Start("start");

            for (int i = 0; i < elements - 1; i++) {
                var e1 = elements[i]; //預接合元素
                for (int j = i + 1; j < elements; j++) {
                    var e2 = elements[j]; //接合目標元素

                    //嘗試執行 ->
                    try{

                        // 判斷選擇順序與優先順序是否相同
                        if (RunWithDefaultOrder(e1, e2)) {
                            RunJoinGeometry(doc, e1, e2); //則直接執行接合
                        } else {
                            RunJoinGeometryAndSwitch(doc, e1, e2); //先執行接合後再改變順序
                        }

                    } catch(Exception e) {
                        //忽略錯誤(忽略無法接合的錯誤)
                    }

                }
            }

            //提交此次任務
            trans.Commit();
        }

    }

}