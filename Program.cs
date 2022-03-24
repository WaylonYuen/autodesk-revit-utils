//revit
//Program: 主程式

class Program {

    /// <summary>
    /// 執行外部程式
    /// </summary>
    /// <param name="commandData">指令資料</param>
    /// <param name="message">資訊</param>
    /// <param name="elements">元素(物件)</param>
    /// <returns>執行結果</returns>
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {

        UIApplication uiApp = commandData.Application;
        Document doc = uiApp.ActiveUIDocument.Document;

        // 物件集合 (選中的物件集合)
        ICollection<ElementId> selectedElIds = uiApp.ActiveUIDocument.Selection.GetElementIds();

        //string info = ""; //提示訊息

        //判斷是否有選擇物件
        if(selectedElIds.Count != 0) {
            TaskDialog.Show($"選中了: {selectedElIds.Count}個物件");
            //JoinAllGeometry(selectedElIds);
        } else {
            TaskDialog.Show($"Warning: 沒有選擇任何物件！");
        }

        return Result.Succeeded;
    }

    public void JoinAllGeometry(ICollection<ElementId> selectedElIds) {
        // 點選物件
        Selection sel = uiApp.ActiveUIDocument.Selection;
        Reference r1 = sel.PickObject(ObjectType.Element, "選擇第一個物件");
        Reference r2 = sel.PickObject(ObjectType.Element, "選擇第二個物件");

        using (Transaction trans = new Transaction(doc)) {  //獲取 & 自動釋放資源
            trans.Start("start");
            Element e1 = doc.GetElement(r1.ElementId);
            Element e2 = doc.GetElement(r2.ElementId);

            // 判斷選擇順序與優先順序是否相同
            // 若是，則直接執行接合
            // 若否，則先執行接合後再改變順序
            if (RunWithDefaultOrder(e1, e2))
                RunJoinGeometry(doc, e1, e2);
            else
                RunJoinGeometryAndSwitch(doc, e1, e2);

            trans.Commit();
        }
    }

}