using System;

/// <summary>
/// 오목에서의 수를 나타내는 구체적인 클래스입니다. (좌표 포함)
/// [Serializable] 속성은 이 객체를 저장하거나 네트워크로 전송할 수 있게 해줍니다.
/// </summary>
[Serializable]
public class Move_Omok : Move
{
    public int x;
    public int y;

    public Move_Omok(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
