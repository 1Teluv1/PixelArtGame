using MonsterState;
public class MonsterState_Chase : IMonsterState
{
    public void Enter(Monster monster) { }
    public void Update(Monster monster)
    {
        monster.MoveTowardsPlayer();
        if (monster.IsPlayerInAttackRange())
            monster.ChangeState(new MonsterState_Attack());
    }
    public void Exit(Monster monster) { }
}