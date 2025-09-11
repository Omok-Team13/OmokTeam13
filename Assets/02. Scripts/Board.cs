using System.Collections.Generic;

/// <summary>
/// 모든 보드 게임의 기본이 되는 추상 클래스(설계도).
/// 구체적인 게임 규칙은 이 클래스를 상속받아 구현합니다.
/// </summary>
public abstract class Board
{
    // 현재 턴의 플레이어 (1 또는 2)
    protected int player;

    public Board()
    {
        // 게임은 항상 1번 플레이어부터 시작
        this.player = 1;
    }

    /// <summary>
    /// 현재 보드 상태에서 가능한 모든 수를 반환하는 메소드.
    /// 자식 클래스에서 override하여 구현합니다.
    /// </summary>
    public virtual Move[] GetMoves()
    {
        return new Move[0];
    }

    /// <summary>
    /// 특정 수를 두었을 때의 다음 보드 상태를 반환하는 추상 메소드.
    /// 자식 클래스에서 반드시 구현해야 합니다.
    /// </summary>
    /// <param name="m">둘 수 (Move 타입)</param>
    /// <returns>수가 적용된 새로운 Board 객체</returns>
    public abstract Board MakeMove(Move m);

    /// <summary>
    /// 게임이 끝났는지 확인하는 메소드.
    /// 승자가 결정되거나 무승부일 경우 게임 종료로 판단합니다.
    /// </summary>
    public virtual bool IsGameOver()
    {
        int winner = CheckWinner();
        // 승자가 0(진행 중)이 아니거나 무승부(3)인 경우 게임 종료
        return winner != 0;
    }

    /// <summary>
    /// 현재 플레이어를 반환하는 메소드.
    /// </summary>
    public virtual int GetCurrentPlayer()
    {
        return player;
    }

    /// <summary>
    /// AI가 현재 보드의 유불리를 평가하는 메소드.
    /// (1: 승리, -1: 패배, 0: 그 외)
    /// </summary>
    /// <param name="forPlayer">평가의 기준이 되는 플레이어</param>
    public virtual float Evaluate(int forPlayer)
    {
        int winner = CheckWinner();

        if (winner == 0 || winner == 3)
            return 0;
        if (winner == forPlayer)
            return 1;

        return -1;
    }

    /// <summary>
    /// 승자를 확인하는 추상 메소드 (게임 규칙의 핵심).
    /// 자식 클래스에서 반드시 구현해야 합니다.
    /// (반환값: 0: 진행중, 1: 플레이어1 승, 2: 플레이어2 승, 3: 무승부)
    /// </summary>
    public abstract int CheckWinner();
}
