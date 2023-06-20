using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using JsonFx.Json;

public static class LEHelper  {
    public static Point[] direction = { new Point(-1, 0), new Point(0, -1), new Point(1, 0), new Point(0, 1) };
    public static LEItem CurrentItem { get; set; }
    public static List<LECell> Cells { get; set; }
    public static List<LECell> conveyorList = new List<LECell>();
    public static LECell SelectedCell { get; set; }
    //public static bool didModify = false;

    public static void EndConveyorDrawing ()
    {
        if (conveyorList.Count <= 1) {
            conveyorList.Clear();
            return;
        }

        ConveyorPanel.Info beginConveyorInfo = new ConveyorPanel.Info();
        ConveyorPanel.Info endConveyorInfo = new ConveyorPanel.Info();

        // 현재 그리고 있는 수 만큼, 방향 정보를 얻어서 그린다.
        for (int i = 0; i < conveyorList.Count; i++) {
            ConveyorPanel.Info info = new ConveyorPanel.Info();
            if (i == 0) {
                info.Type = ConveyorPanel.TYPE.BEGIN;
                info.Out = GetConveyorDirection(i, false);
                info.In = (ConveyorPanel.DIRECTION)(((int)info.Out + 2) % 4);
                beginConveyorInfo = info;
            } else if (i == (conveyorList.Count - 1)) {
                info.Type = ConveyorPanel.TYPE.END;
                info.In = GetConveyorDirection(i, true);
                info.Out = (ConveyorPanel.DIRECTION)(((int)info.In + 2) % 4);
                endConveyorInfo = info;
            } else {
                info.Type = ConveyorPanel.TYPE.MIDDLE;
                info.In = GetConveyorDirection(i, true);
                info.Out = GetConveyorDirection(i, false);
            }

            conveyorList[i].DrawConveyor(info);
        }

        // 시점과 종점이 같다면 순환으로 처리한다.
        Point p = conveyorList[0].PT;
        p.x += direction[(int)beginConveyorInfo.In].x;
        p.y += direction[(int)beginConveyorInfo.In].y;

        if (p == conveyorList[conveyorList.Count - 1].PT) {
            beginConveyorInfo.Type = ConveyorPanel.TYPE.MIDDLE;
            conveyorList[0].DrawConveyor(beginConveyorInfo);
            endConveyorInfo.Type = ConveyorPanel.TYPE.MIDDLE;
            conveyorList[conveyorList.Count - 1].DrawConveyor(endConveyorInfo);
        }

        conveyorList.Clear();
    }

    static ConveyorPanel.DIRECTION GetConveyorDirection (int index, bool isIn)
    {
        if (isIn) {
            Point inDirection = conveyorList[index - 1].PT - conveyorList[index].PT;
            return (ConveyorPanel.DIRECTION)System.Array.IndexOf(direction, inDirection);
        } else {
            Point outDirection = conveyorList[index + 1].PT - conveyorList[index].PT;
            return (ConveyorPanel.DIRECTION)System.Array.IndexOf(direction, outDirection);
        }
    }
}
