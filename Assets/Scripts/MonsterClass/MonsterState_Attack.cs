using MonsterState;
public class MonsterState_Attack : IMonsterState
{
    public void Enter(Monster monster) { }
    public void Update(Monster monster)
    {
        monster.AttackPlayer();
        if (!monster.IsPlayerInAttackRange())
            monster.ChangeState(new MonsterState_Chase());
    }
    public void Exit(Monster monster) { }
} 