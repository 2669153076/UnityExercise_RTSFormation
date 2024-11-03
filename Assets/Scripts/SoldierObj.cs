using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 士兵类型
/// </summary>
public enum E_SoldierType
{
    Hero=0,
    Grunt = 1,
    Headhunter = 2,
    Witchdoctor = 3,
    Wyvernrider = 4
}

public class SoldierObj : MonoBehaviour
{
    private Animator anim;  //动画控制器
    private NavMeshAgent agent; //寻路控制器
    private GameObject footEffect;  //脚底特效

    public E_SoldierType soldierType;   //士兵类型类型

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponentInChildren<Animator>();
        agent = this.GetComponent<NavMeshAgent>();
        footEffect = this.transform.Find("FootEffect").gameObject;

        SetSelSelf(false);
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("IsMove", agent.velocity.magnitude > 0);

    }

    /// <summary>
    /// 移动
    /// </summary>
    /// <param name="pos">目标位置</param>
    public void Move(Vector3 pos)
    {
        agent.SetDestination(pos);
    }

    /// <summary>
    /// 设置自己是否选中
    /// </summary>
    /// <param name="isSel">是否被选中</param>
    public void SetSelSelf(bool isSel)
    {
        footEffect.SetActive(isSel);
    }
}
