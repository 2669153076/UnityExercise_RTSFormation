using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Controller : MonoBehaviour
{
    private bool mouseIsDown = false;

    private LineRenderer lineRenderer;

    private Vector3 mouseDownPoint;   //鼠标按下位置 矩形左上角
    private Vector3 rightUpPoint;   //矩形右上角
    private Vector3 rightDownPoint; //矩形右下角
    private Vector3 leftDownPoint;  //矩形左下角

    private RaycastHit hitInfo;
    private Vector3 mouseDownWorldPos;

    private Vector3 frontPos = Vector3.zero;    //上一次鼠标右键点击位置
    private float soldierOffset = 5;    //士兵间隔距离

    private List<SoldierObj> soldiers = new List<SoldierObj>(); //选择的士兵对象

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        SelSoldiers();
        ControlSoldiersMove();
    }

    /// <summary>
    /// 选中士兵
    /// </summary>
    private void SelSoldiers()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDownPoint = Input.mousePosition;
            mouseIsDown = true;

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000, 1 << LayerMask.NameToLayer("Ground")))
            {
                mouseDownWorldPos = hitInfo.point;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            mouseIsDown = false;
            //将线段的点设置为0
            lineRenderer.positionCount = 0;

            frontPos = Vector3.zero;    //每一次选中士兵 将上一次位置初始化

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo, 1000, 1 << LayerMask.NameToLayer("Ground")))
            {
                //范围检测使用的中心点
                Vector3 centerPos = new Vector3((hitInfo.point.x + mouseDownWorldPos.x) / 2, 1, (hitInfo.point.z + mouseDownWorldPos.z) / 2);
                Vector3 halfExtents = new Vector3(Mathf.Abs(hitInfo.point.x - mouseDownWorldPos.x) / 2, 1, Mathf.Abs(hitInfo.point.z - mouseDownWorldPos.z) / 2);
                //得到盒内所有的碰撞器
                Collider[] colliders = Physics.OverlapBox(centerPos, halfExtents);
                for (int i = 0; i < colliders.Length&&soldiers.Count<12; i++)
                {
                    SoldierObj obj = colliders[i].GetComponent<SoldierObj>();
                    if (obj != null)
                    {
                        obj.SetSelSelf(true);
                        soldiers.Add(obj);
                    }
                }

                soldiers.Sort((a, b) =>
                {
                    if (a.soldierType < b.soldierType)
                        return -1;
                    else if (a.soldierType == b.soldierType)
                        return 0;
                    else 
                        return 1;
                });

            }
        }

        if (mouseIsDown)
        {
            //清空选择的敌人
            for (int i = 0; i < soldiers.Count; i++)
            {
                soldiers[i].SetSelSelf(false);
            }
            soldiers.Clear();

            mouseDownPoint.z = 5;

            rightDownPoint = Input.mousePosition;
            rightDownPoint.z = 5;

            rightUpPoint.x = rightDownPoint.x;
            rightUpPoint.y = mouseDownPoint.y;
            rightUpPoint.z = 5;

            leftDownPoint.x = mouseDownPoint.x;
            leftDownPoint.y = rightDownPoint.y;
            leftDownPoint.z = 5;

            lineRenderer.positionCount = 4;
            lineRenderer.SetPosition(0, Camera.main.ScreenToWorldPoint(mouseDownPoint));
            lineRenderer.SetPosition(1, Camera.main.ScreenToWorldPoint(rightUpPoint));
            lineRenderer.SetPosition(2, Camera.main.ScreenToWorldPoint(rightDownPoint));
            lineRenderer.SetPosition(3, Camera.main.ScreenToWorldPoint(leftDownPoint));

        }
    }

    /// <summary>
    /// 移动士兵
    /// </summary>
    private void ControlSoldiersMove()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (soldiers.Count == 0)
                return;

            if(Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition),out hitInfo, 1000, 1 << LayerMask.NameToLayer("Ground")))
            {
                //(hitInfo.point - soldiers[1].transform.position).normalized;
                if (Vector3.Angle((hitInfo.point - soldiers[1].transform.position).normalized, soldiers[1].transform.forward) > 60)
                {
                    soldiers.Sort((a, b) =>
                    {
                        if (a.soldierType < b.soldierType)
                            return -1;
                        else if (a.soldierType == b.soldierType)
                        {
                            if (Vector3.Distance(a.transform.position, hitInfo.point) <= Vector3.Distance(b.transform.position, hitInfo.point))
                            {
                                return  -1;
                            }
                            else
                            {
                                return 1;
                            }
                        }
                        else
                            return 1;
                    });
                }


                List<Vector3> targetPosList = GetTargetPos(hitInfo.point);

                for (int i = 0; i < soldiers.Count; i++)
                {
                    soldiers[i].Move(targetPosList[i]);
                }
            }
        }
    }

    /// <summary>
    /// 计算阵型点位
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    private List<Vector3> GetTargetPos(Vector3 targetPos)
    {
        //需要计算目标点的 面朝向和右朝向
        Vector3 curForward = Vector3.zero;
        Vector3 curRight = Vector3.zero;

        //同一批士兵
        if (frontPos != Vector3.zero)
        {
            //已经移动过 有上一次位置
            curForward = (targetPos - frontPos).normalized;
        }
        else
        {
            //第一次移动 将第一个士兵位置设置为初始位置
            curForward = (targetPos - soldiers[0].transform.position).normalized;
        }

        curRight = Quaternion.Euler(0, 90, 0) * curForward;

        List<Vector3> targetPosList = new List<Vector3>();

        switch (soldiers.Count)
        {
            case 1:
                targetPosList.Add(targetPos); 
                break;
            case 2:
                targetPosList.Add(targetPos - curRight * soldierOffset / 2);
                targetPosList.Add(targetPos + curRight * soldierOffset / 2);
                break;
            case 3:
                targetPosList.Add(targetPos - curRight * soldierOffset );
                targetPosList.Add(targetPos);
                targetPosList.Add(targetPos + curRight * soldierOffset );
                break;
            case 4:
                targetPosList.Add(targetPos - curRight * soldierOffset/2 + curForward * soldierOffset / 2);
                targetPosList.Add(targetPos + curRight * soldierOffset/2 + curForward * soldierOffset / 2);
                targetPosList.Add(targetPos - curRight * soldierOffset/2 - curForward * soldierOffset / 2);
                targetPosList.Add(targetPos + curRight * soldierOffset/2 - curForward * soldierOffset / 2);
                break;
            case 5:
                targetPosList.Add(targetPos - curRight * soldierOffset + curForward * soldierOffset / 2);
                targetPosList.Add(targetPos+ curForward * soldierOffset / 2);
                targetPosList.Add(targetPos + curRight * soldierOffset  + curForward * soldierOffset / 2);
                targetPosList.Add(targetPos - curRight * soldierOffset  - curForward * soldierOffset / 2);
                targetPosList.Add(targetPos + curRight * soldierOffset  - curForward * soldierOffset/2 );
                break;
            case 6:
                targetPosList.Add(targetPos - curRight * soldierOffset + curForward * soldierOffset / 2);
                targetPosList.Add(targetPos + curForward * soldierOffset / 2);
                targetPosList.Add(targetPos + curRight * soldierOffset + curForward * soldierOffset / 2);
                targetPosList.Add(targetPos - curRight * soldierOffset - curForward * soldierOffset / 2);
                targetPosList.Add(targetPos - curForward * soldierOffset / 2);
                targetPosList.Add(targetPos + curRight * soldierOffset - curForward * soldierOffset / 2);
                break;
            case 7:
                targetPosList.Add(targetPos - curRight * soldierOffset  + curForward * soldierOffset );
                targetPosList.Add(targetPos + curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset  + curForward * soldierOffset );
                targetPosList.Add(targetPos - curRight * soldierOffset);
                targetPosList.Add(targetPos );
                targetPosList.Add(targetPos + curRight * soldierOffset);
                targetPosList.Add(targetPos - curForward * soldierOffset );
                break;
            case 8:
                targetPosList.Add(targetPos - curRight * soldierOffset + curForward * soldierOffset);
                targetPosList.Add(targetPos + curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset + curForward * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset);
                targetPosList.Add(targetPos);
                targetPosList.Add(targetPos + curRight * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset-curForward*soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset-curForward*soldierOffset);
                break;
            case 9:
                targetPosList.Add(targetPos - curRight * soldierOffset + curForward * soldierOffset);
                targetPosList.Add(targetPos + curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset + curForward * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset);
                targetPosList.Add(targetPos);
                targetPosList.Add(targetPos + curRight * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset - curForward * soldierOffset);
                targetPosList.Add(targetPos - curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset - curForward * soldierOffset);
                break;
            case 10:
                targetPosList.Add(targetPos - curRight * soldierOffset*1.5f + curForward * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset  / 2 + curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset  / 2 + curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset*1.5f + curForward * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset * 1.5f);
                targetPosList.Add(targetPos - curRight * soldierOffset * 0.5f);
                targetPosList.Add(targetPos + curRight * soldierOffset * 0.5f);
                targetPosList.Add(targetPos + curRight * soldierOffset * 1.5f);
                targetPosList.Add(targetPos - curRight * soldierOffset*1.5f - curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset*1.5f - curForward * soldierOffset);
                break;
            case 11:

                targetPosList.Add(targetPos - curRight * soldierOffset * 1.5f + curForward * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset / 2 + curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset / 2 + curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset * 1.5f + curForward * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset * 1.5f);
                targetPosList.Add(targetPos - curRight * soldierOffset * 0.5f);
                targetPosList.Add(targetPos + curRight * soldierOffset * 0.5f);
                targetPosList.Add(targetPos + curRight * soldierOffset * 1.5f);
                targetPosList.Add(targetPos - curRight * soldierOffset * 1.5f - curForward * soldierOffset);
                targetPosList.Add(targetPos - curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset * 1.5f - curForward * soldierOffset);
                break;
            case 12:

                targetPosList.Add(targetPos - curRight * soldierOffset * 1.5f + curForward * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset / 2 + curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset / 2 + curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset * 1.5f + curForward * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset * 1.5f);
                targetPosList.Add(targetPos - curRight * soldierOffset * 0.5f);
                targetPosList.Add(targetPos + curRight * soldierOffset * 0.5f);
                targetPosList.Add(targetPos + curRight * soldierOffset * 1.5f);
                targetPosList.Add(targetPos - curRight * soldierOffset * 1.5f - curForward * soldierOffset);
                targetPosList.Add(targetPos - curRight * soldierOffset * 0.5f - curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset * 0.5f - curForward * soldierOffset);
                targetPosList.Add(targetPos + curRight * soldierOffset * 1.5f - curForward * soldierOffset);
                break;
        }


        frontPos = targetPos;
        return targetPosList;
    }
}
