/// <summary>
/// ������ '��'�� ���� ��ü���� ������ ��� Ŭ����. ���� Move Ŭ������ ��ӹ޽��ϴ�.
/// </summary>
public class Move_Omok : Move
{
    /// <summary>
    /// �������� ��ġ�� ��Ÿ���� 1���� �ε���.
    /// (y, x) ��ǥ�� 'y * 19 + x'�� ���˴ϴ�.
    /// </summary>
    public int index;

    public Move_Omok(int index)
    {
        this.index = index;
    }
}