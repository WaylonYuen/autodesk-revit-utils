
// 幾何圖形
public class Geometry {

    // 判斷優先順序與選擇順序是否一致
    bool RunWithDefaultOrder(Element e1, Element e2) =>
        GetOrderNumber(e1) >= GetOrderNumber(e2);

    // 判斷選擇物件的種類
    int GetOrderNumber(Element e) {
        switch (e.Category.Name) {
            case "Structural Framing": return 1;    //梁
            case "Structural Columns": return 2;    //柱
            case "Walls": return 3;                 //牆
            case "Floors": return 4;                //版
            default: return 0;
        }
    }

    // 執行接合
    void RunJoinGeometry(Document doc, Element e1, Element e2) {

        if (!JoinGeometryUtils.AreElementsJoined(doc, e1, e2))
            JoinGeometryUtils.JoinGeometry(doc, e1, e2);
        else {
            JoinGeometryUtils.UnjoinGeometry(doc, e1, e2);
            JoinGeometryUtils.JoinGeometry(doc, e1, e2);
        }
    }

    // 執行接合並改變接合順序
    void RunJoinGeometryAndSwitch(Document doc, Element e1, Element e2) {
        if (!JoinGeometryUtils.AreElementsJoined(doc, e1, e2))
        {
            JoinGeometryUtils.JoinGeometry(doc, e1, e2);
            JoinGeometryUtils.SwitchJoinOrder(doc, e1, e2);
        }               
        else
        {
            JoinGeometryUtils.UnjoinGeometry(doc, e1, e2);
            JoinGeometryUtils.JoinGeometry(doc, e1, e2);
            JoinGeometryUtils.SwitchJoinOrder(doc, e1, e2);
        }
    }

}