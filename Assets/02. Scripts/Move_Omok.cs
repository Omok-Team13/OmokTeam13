/// <summary>
/// 오목의 '수'에 대한 구체적인 정보를 담는 클래스. 공용 Move 클래스를 상속받습니다.
/// </summary>
public class Move_Omok : Move
{
    /// <summary>
    /// 보드판의 위치를 나타내는 1차원 인덱스.
    /// (y, x) 좌표는 'y * 19 + x'로 계산됩니다.
    /// </summary>
    public int index;

    public Move_Omok(int index)
    {
        this.index = index;
    }
}