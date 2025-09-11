using System.Collections.Generic;

/// <summary>
/// ��� ���� ������ �⺻�� �Ǵ� �߻� Ŭ����(���赵).
/// ��ü���� ���� ��Ģ�� �� Ŭ������ ��ӹ޾� �����մϴ�.
/// </summary>
public abstract class Board
{
    // ���� ���� �÷��̾� (1 �Ǵ� 2)
    protected int player;

    public Board()
    {
        // ������ �׻� 1�� �÷��̾���� ����
        this.player = 1;
    }

    /// <summary>
    /// ���� ���� ���¿��� ������ ��� ���� ��ȯ�ϴ� �޼ҵ�.
    /// �ڽ� Ŭ�������� override�Ͽ� �����մϴ�.
    /// </summary>
    public virtual Move[] GetMoves()
    {
        return new Move[0];
    }

    /// <summary>
    /// Ư�� ���� �ξ��� ���� ���� ���� ���¸� ��ȯ�ϴ� �߻� �޼ҵ�.
    /// �ڽ� Ŭ�������� �ݵ�� �����ؾ� �մϴ�.
    /// </summary>
    /// <param name="m">�� �� (Move Ÿ��)</param>
    /// <returns>���� ����� ���ο� Board ��ü</returns>
    public abstract Board MakeMove(Move m);

    /// <summary>
    /// ������ �������� Ȯ���ϴ� �޼ҵ�.
    /// ���ڰ� �����ǰų� ���º��� ��� ���� ����� �Ǵ��մϴ�.
    /// </summary>
    public virtual bool IsGameOver()
    {
        int winner = CheckWinner();
        // ���ڰ� 0(���� ��)�� �ƴϰų� ���º�(3)�� ��� ���� ����
        return winner != 0;
    }

    /// <summary>
    /// ���� �÷��̾ ��ȯ�ϴ� �޼ҵ�.
    /// </summary>
    public virtual int GetCurrentPlayer()
    {
        return player;
    }

    /// <summary>
    /// AI�� ���� ������ ���Ҹ��� ���ϴ� �޼ҵ�.
    /// (1: �¸�, -1: �й�, 0: �� ��)
    /// </summary>
    /// <param name="forPlayer">���� ������ �Ǵ� �÷��̾�</param>
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
    /// ���ڸ� Ȯ���ϴ� �߻� �޼ҵ� (���� ��Ģ�� �ٽ�).
    /// �ڽ� Ŭ�������� �ݵ�� �����ؾ� �մϴ�.
    /// (��ȯ��: 0: ������, 1: �÷��̾�1 ��, 2: �÷��̾�2 ��, 3: ���º�)
    /// </summary>
    public abstract int CheckWinner();
}
