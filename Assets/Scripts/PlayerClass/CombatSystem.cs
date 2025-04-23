using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [SerializeField] private Animator animator;
    // Z 키 공격
    public void PerformAttack1()
    {
        if (animator != null)
        {
            // 애니메이션 트리거 발동
            animator.SetTrigger("Attack1");
            Debug.Log("Attack1 performed!"); // 작동 확인용 로그

            // 여기에 실제 데미지 판정, 이펙트 생성 등의 로직 추가
            // 예: 근처의 적 감지 및 데미지 적용
        }
        else { LogAnimatorWarning("Attack1"); }
    }

    // X 키 공격
    public void PerformAttack2()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack2"); // "Attack2" 애니메이션 트리거
            Debug.Log("Attack2 performed!");
            // 여기에 실제 Attack2 데미지/로직 추가
        }
        else { LogAnimatorWarning("Attack2"); }
    }

    // C 키 공격
    public void PerformAttack3()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack3"); // "Attack3" 애니메이션 트리거
            Debug.Log("Attack3 performed!");
            // 여기에 실제 Attack3 데미지/로직 추가
        }
        else { LogAnimatorWarning("Attack3"); }
    }

    // V 키 공격
    public void PerformAttack4()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack4"); // "Attack4" 애니메이션 트리거
            Debug.Log("Attack4 performed!");
            // 여기에 실제 Attack4 데미지/로직 추가
        }
        else { LogAnimatorWarning("Attack4"); }
    }

    private void LogAnimatorWarning(string attackName)
    {
        Debug.LogWarning($"Animator not found on CombatSystem. Cannot trigger {attackName} animation.");
    }

    // 추후 다른 공격 타입이나 스킬을 위한 메서드 추가 가능
    // public void PerformAttack2() { ... }
    // public void PerformSkill(string skillName) { ... }
} 